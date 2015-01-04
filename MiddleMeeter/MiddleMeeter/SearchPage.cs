using Refractored.Xam.Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Labs.Controls;

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
    // reading values saved during the last session (or setting defaults)
    string theirLocation = CrossSettings.Current.GetValueOrDefault("theirLocation", "");
    SearchMode mode = CrossSettings.Current.GetValueOrDefault("mode", SearchMode.food);
    Place[] results = null;

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

    public Place[] Results {
      get { return this.results; }
      set {
        if (this.results != value) {
          this.results = value;
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
    ResultsView resultsView = new ResultsView();

    public SearchPage() {
      Title = "Search";
      Padding = 20;
      BindingContext = model;

      var yourLocationLabel = new Label { Text = "Your Location:", VerticalOptions = LayoutOptions.Center };
      var theirLocationLabel = new Label { Text = "Their Location:", VerticalOptions = LayoutOptions.Center };
      var pickerLabel = new Label { Text = "Mode:", VerticalOptions = LayoutOptions.Center };

      var button1 = new Button { Text = "Search", HorizontalOptions = LayoutOptions.End, WidthRequest = 200 };
      button1.Clicked += button1_Clicked;

      var picker = new Picker { Title = "Mode" };
      foreach (var mode in new string[] { "coffee", "food", "drinks" }) {
        picker.Items.Add(mode);
      }
      picker.SetBinding(Picker.SelectedIndexProperty, new Binding("Mode", converter: new ModeConverter()));

      var yourLocation = new Entry { Placeholder = "your location" };
      yourLocation.SetBinding(Entry.TextProperty, new Binding("YourLocation"));

      var theirLocation = new AutoCompleteView {
        Placeholder = "their location",
        ShowSearchButton = false,
        SelectedCommand = new Command(() => { }),
      };
      theirLocation.SetBinding(AutoCompleteView.TextProperty, new Binding("TheirLocation"));
      theirLocation.TextEntry.Text = model.TheirLocation;

      // disable Search button if no locations entered
      Action checkLocations = () => {
        button1.IsEnabled = !string.IsNullOrEmpty(yourLocation.Text) && !string.IsNullOrEmpty(theirLocation.Text);
      };
      yourLocation.TextChanged += (sender, e) => { checkLocations(); };
      theirLocation.PropertyChanged += (sender, e) => {
        if (e.PropertyName == "Text" &&
          !object.ReferenceEquals(theirLocation.ListViewSugestions.SelectedItem, theirLocation.Text)) {
          checkLocations();
          PopulateLocationSuggestions(theirLocation);
        }
      };
      checkLocations();

      resultsView.SetBinding(ResultsView.ResultsProperty, "Results");
      var deviceResultsView = Device.Idiom == TargetIdiom.Tablet ? resultsView : new ContentView();

      var portraitView = new StackLayout {
        Children = {
          yourLocationLabel,
          yourLocation,
          theirLocationLabel,
          theirLocation,
          pickerLabel,
          picker,
          button1,
          activity,
          error,
          deviceResultsView,
        }
      };

      var landscapeView = new Grid {
        Children = {
            GridChild(0, 0, yourLocationLabel),
            GridChild(0, 1, yourLocation),
            GridChild(1, 0, theirLocationLabel),
            GridChild(1, 1, theirLocation),
            GridChild(2, 0, pickerLabel),
            GridChild(2, 1, picker),
            GridChild(3, 0, button1),
            GridChild(4, 0, activity),
            GridChild(5, 0, error),
            GridChild(6, 0, deviceResultsView),
          },
        ColumnDefinitions = {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
          },
        RowDefinitions = {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
          },
      };

      Grid.SetColumnSpan(landscapeView.Children[6], landscapeView.ColumnDefinitions.Count);
      Grid.SetColumnSpan(landscapeView.Children[7], landscapeView.ColumnDefinitions.Count);
      Grid.SetColumnSpan(landscapeView.Children[8], landscapeView.ColumnDefinitions.Count);
      Grid.SetColumnSpan(landscapeView.Children[9], landscapeView.ColumnDefinitions.Count);

      SizeChanged += (sender, e) => Content = IsPortrait(this) ? (View)portraitView : (View)landscapeView;
    }

    static bool IsPortrait(Page p) { return p.Width < p.Height; }

    View GridChild(int row, int col, View child) {
      Grid.SetRow(child, row);
      Grid.SetColumn(child, col);
      return child;
    }

    async void PopulateLocationSuggestions(AutoCompleteView theirLocation) {
      // avoid Sugestions, which filters, and set AvailableSugestions directly.
      theirLocation.AvailableSugestions.Clear();

      var addr = theirLocation.Text;
      if (addr != null && addr.Length > 2) {
        var gc = new Geocoding();
        var suggestions = await gc.GetLocationSuggestionsAsync(addr);
        foreach (var s in suggestions) { theirLocation.AvailableSugestions.Add(s); }
      }

      theirLocation.ListViewSugestions.IsVisible = theirLocation.AvailableSugestions.Count > 0;
    }

    protected override void OnAppearing() {
      base.OnAppearing();
      Device.StartTimer(TimeSpan.FromSeconds(1), () => { LookupYourLocation(); return false; });
    }

    async void LookupYourLocation() {

      if (!string.IsNullOrWhiteSpace(model.YourLocation)) { return; }

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

        // writing settings values at an appropriate time
        CrossSettings.Current.AddOrUpdateValue("theirLocation", model.TheirLocation);
        CrossSettings.Current.AddOrUpdateValue("mode", model.Mode);

        resultsView.Results = places;
        if (Device.Idiom != TargetIdiom.Tablet) {
          await Navigation.PushAsync(new ResultsPage(resultsView));
        }
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
