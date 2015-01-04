using Refractored.Xam.Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Labs.Controls;

namespace MiddleMeeter {
  static class GridExtensions {
    public static View GridRowCol(this View view, int row, int col) {
      Grid.SetRow(view, row);
      Grid.SetColumn(view, col);
      return view;
    }

    public static View GridRowSpan(this View view, int span) {
      Grid.SetRowSpan(view, span);
      return view;
    }

    public static View GridColSpan(this View view, int span) {
      Grid.SetColumnSpan(view, span);
      return view;
    }
  }

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
    Geocoding gc = new Geocoding();

    public SearchPage() {
      Title = "Search";
      Padding = 20;
      BindingContext = model;

      var yourLocationLabel = new Label { Text = "Your Location:", VerticalOptions = LayoutOptions.Center };
      var theirLocationLabel = new Label { Text = "Their Location:", VerticalOptions = LayoutOptions.Center };
      var pickerLabel = new Label { Text = "Mode:", VerticalOptions = LayoutOptions.Center };

      var searchButton = new Button { Text = "Search", HorizontalOptions = LayoutOptions.End, WidthRequest = 200 };
      searchButton.Clicked += searchButton_Clicked;

      var yourLocationButton = new Image {
        Aspect = Aspect.AspectFit,
        Source = ImageSource.FromResource(this.GetType().Namespace + "." + Device.OnPlatform("iOS", "Droid", "WinPhone") + ".crosshairs.png"),
      };

      var tap = new TapGestureRecognizer();
      tap.Tapped += yourLocationButton_Clicked;
      yourLocationButton.GestureRecognizers.Add(tap);

      //var yourLocationButton = new Button { Text = "±", FontSize = 30, FontFamily = "Wingdings",  };
      //Device.OnPlatform(
      //  () => { },
      //  () => { yourLocationButton.Text = "@"; },
      //  () => { }
      //);
      //yourLocationButton.Clicked += yourLocationButton_Clicked;

      var picker = new Picker { Title = "Mode" };
      foreach (var mode in new string[] { "coffee", "food", "drinks" }) {
        picker.Items.Add(mode);
      }
      picker.SetBinding(Picker.SelectedIndexProperty, new Binding("Mode", converter: new ModeConverter()));

      var yourLocation = new Entry { Placeholder = "your location", HorizontalOptions = LayoutOptions.Fill };
      yourLocation.SetBinding(Entry.TextProperty, new Binding("YourLocation"));

      var theirLocation = new Entry { Placeholder = "their location" };
      theirLocation.SetBinding(Entry.TextProperty, new Binding("TheirLocation"));

      // disable Search button if no locations entered
      Action checkLocations = () => {
        searchButton.IsEnabled = !string.IsNullOrEmpty(yourLocation.Text) && !string.IsNullOrEmpty(theirLocation.Text);
      };
      yourLocation.TextChanged += (sender, e) => { checkLocations(); };
      theirLocation.TextChanged += (sender, e) => { checkLocations(); };
      checkLocations();

      resultsView.SetBinding(ResultsView.ResultsProperty, "Results");
      var deviceResultsView = Device.Idiom == TargetIdiom.Tablet ? resultsView : new ContentView();

      var yourLocationEntryAndButton = new Grid {
        Children = {
          yourLocation.GridRowCol(0, 0),
          yourLocationButton.GridRowCol(0, 1),
        },
        ColumnDefinitions = new ColumnDefinitionCollection {
          new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
          new ColumnDefinition { Width = GridLength.Auto },
        },
      };

      Func<View> portraitView = () => new StackLayout {
        Children = {
          yourLocationLabel,
          yourLocationEntryAndButton,
          theirLocationLabel,
          theirLocation,
          pickerLabel,
          picker,
          searchButton,
          activity,
          error,
          deviceResultsView,
        }
      };

      Func<View> landscapeView = () => new Grid {
        Children = {
          yourLocationLabel.GridRowCol(0, 0),
          yourLocationEntryAndButton.GridRowCol(0, 1),

          theirLocationLabel.GridRowCol(1, 0),
          theirLocation.GridRowCol(1, 1),
          
          pickerLabel.GridRowCol(2, 0),
          picker.GridRowCol(2, 1),
          
          searchButton.GridRowCol(3, 0).GridColSpan(2),
          activity.GridRowCol(4, 0).GridColSpan(2),
          error.GridRowCol(5, 0).GridColSpan(2),
          deviceResultsView.GridRowCol(6, 0).GridColSpan(2),
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

      SizeChanged += (sender, e) => Content = IsPortrait(this) ? portraitView() : landscapeView();
    }

    static bool IsPortrait(Page p) { return p.Width < p.Height; }

    async void PopulateLocationSuggestions(AutoCompleteView theirLocation) {
      // avoid Sugestions, which filters, and set AvailableSugestions directly.
      theirLocation.AvailableSugestions.Clear();

      var addr = theirLocation.Text;
      if (addr != null && addr.Length > 2) {
        var suggestions = await gc.GetLocationSuggestionsAsync(addr);
        foreach (var s in suggestions) { theirLocation.AvailableSugestions.Add(s); }
      }

      theirLocation.ListViewSugestions.IsVisible = theirLocation.AvailableSugestions.Count > 0;
    }

    async void yourLocationButton_Clicked(object sender, EventArgs e) {
      try {
        activity.IsRunning = true;

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

    async void searchButton_Clicked(object sender, EventArgs e) {
      try {
        activity.IsRunning = true;
        error.Text = "";

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
