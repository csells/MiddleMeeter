using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MiddleMeeter {
  enum SearchMode {
    coffee = 0,
    food = 1,
    drinks = 2,
  }

  class ModeConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
      return (int)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
      return (SearchMode)value;
    }
  }

  class SearchModel : INotifyPropertyChanged {
    string yourLocation;
    string theirLocation;
    SearchMode mode = SearchMode.food;

    public string YourLocation {
      get { return this.yourLocation; }
      set {
        if (this.yourLocation != value) {
          this.yourLocation = value;
          NotifyPropertyChanged();
        }
      }
    }

    public string TheirLocation {
      get { return this.theirLocation; }
      set {
        if (this.theirLocation != value) {
          this.theirLocation = value;
          NotifyPropertyChanged();
        }
      }
    }

    public SearchMode Mode {
      get { return this.mode; }
      set {
        if (this.mode != value) {
          this.mode = value;
          NotifyPropertyChanged();
        }
      }
    }

    void NotifyPropertyChanged([CallerMemberName]string propertyName = "") {
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  class SearchPage : ContentPage {
    SearchModel model = new SearchModel();
    ActivityIndicator activity = new ActivityIndicator();
    Label error = new Label();

    public SearchPage() {
      Title = "Search";
      Padding = 20;
      BindingContext = model;

      var button1 = new Button { Text = "Search", HorizontalOptions = LayoutOptions.End, WidthRequest = 200 };
      button1.Clicked += button1_Clicked;

      var picker = new Picker { Title = "Mode" };
      foreach (var mode in new string[] { "coffee", "food", "drinks" }) {
        picker.Items.Add(mode);
      }
      picker.SetBinding(Picker.SelectedIndexProperty, new Binding("Mode", converter: new ModeConverter()));

      var yourLocation = new Entry { Placeholder = "your location" };
      yourLocation.SetBinding(Entry.TextProperty, new Binding("YourLocation"));

      var theirLocation = new Entry { Placeholder = "their location" };
      theirLocation.SetBinding(Entry.TextProperty, new Binding("TheirLocation"));

      // disable Search button if no locations entered
      Action checkLocations = () => {
        button1.IsEnabled = !string.IsNullOrEmpty(yourLocation.Text) && !string.IsNullOrEmpty(theirLocation.Text);
      };
      yourLocation.TextChanged += (sender, e) => checkLocations();
      theirLocation.TextChanged += (sender, e) => checkLocations();
      checkLocations();

      Content = new StackLayout {
        Children = {
          new Label { Text = "Your Location:" },
          yourLocation,
          new Label { Text = "Their Location:" },
          theirLocation,
          new Label { Text = "Mode:" },
          picker,
          button1,
          activity,
          error,
        }
      };
    }

    async protected override void OnAppearing() {
      base.OnAppearing();

      try {
        activity.IsRunning = true;

        var gc = new Geocoding();
        var loc = await gc.GetCurrentLocationAsync();
        var addr = await gc.GetAddressForLocationAsync(loc);
        model.YourLocation = addr;
      }
      catch (Exception ex) {
        error.Text = "Can't get your location: " + ex.Message;
      }
      finally {
        activity.IsRunning = false;
      }
    }

    async void button1_Clicked(object sender, EventArgs e) {
      try {
        activity.IsRunning = true;
        error.Text = "";

        var gc = new Geocoding();

        var yourGeocode = await gc.GetGeocodeForLocationAsync(model.YourLocation);
        var theirGeocode = await gc.GetGeocodeForLocationAsync(model.TheirLocation);
        var middleGeocode = gc.GetGreatCircleMidpoint(yourGeocode, theirGeocode);
        var places = await gc.GetNearbyPlacesAsync(middleGeocode, model.Mode.ToString());

        await Navigation.PushAsync(new ResultsPage(places));
      }
      catch (Exception ex) {
        error.Text = "Check your network connection: " + ex.Message;
      }
      finally {
        activity.IsRunning = false;
      }
    }

  }
}
