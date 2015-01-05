using Refractored.Xam.Settings;
using System;
using Xamarin.Forms;

namespace MiddleMeeter {
  class SearchPage : ContentPage {
    SearchModel model = new SearchModel();
    ActivityIndicator activity = new ActivityIndicator { WidthRequest = 100, HorizontalOptions = LayoutOptions.End };
    Label status = new Label { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.Start };
    ResultsView resultsView = new ResultsView();
    Geocoding gc = new Geocoding();
    Func<View> portraitView;
    Func<View> landscapeView;

    public SearchPage() {
      // reading values saved during the last session (or setting defaults)
      // NOTE: for iOS, if this happens in the wrong place, data binding fails
      // 2015-01-04 16:02:53.739 MiddleMeeteriOS[1758:673574] Binding: 'TheirLocation' property not found on 'MiddleMeeter.SearchModel', target property: 'Xamarin.Forms.Entry.Text'
      // 2015-01-04 16:02:53.860 MiddleMeeteriOS[1758:673574] Binding: 'Mode' property not found on 'MiddleMeeter.SearchModel', target property: 'Xamarin.Forms.Picker.SelectedIndex'
      model.TheirLocation = CrossSettings.Current.GetValueOrDefault("theirLocation", "");
      model.Mode = CrossSettings.Current.GetValueOrDefault("mode", SearchMode.food);

      Title = "Search";
      Padding = 20;
      BindingContext = model;

      var yourLocationLabel = new Label { Text = "Your Location:", VerticalOptions = LayoutOptions.Center };
      var theirLocationLabel = new Label { Text = "Their Location:", VerticalOptions = LayoutOptions.Center };
      var pickerLabel = new Label { Text = "Mode:", VerticalOptions = LayoutOptions.Center };

      var yourLocationButton = new Button { Text = "⊕", FontSize = 30, HorizontalOptions = LayoutOptions.End };
      yourLocationButton.Clicked += yourLocationButton_Clicked;

      var searchButton = new Button { Text = "Search", HorizontalOptions = LayoutOptions.End, WidthRequest = 200 };
      searchButton.Clicked += searchButton_Clicked;

      var picker = new Picker { Title = "Mode" };
      foreach (var mode in new string[] { "coffee", "food", "drinks" }) {
        picker.Items.Add(mode);
      }
      picker.SetBinding(Picker.SelectedIndexProperty, new Binding("Mode", converter: new ModeConverter()));

      var yourLocation = new Entry { Placeholder = "your location", HorizontalOptions = LayoutOptions.Fill };
      yourLocation.SetBinding(Entry.TextProperty, new Binding("YourLocation"));

      var theirLocation = new Entry { Placeholder = "their location" };
      theirLocation.SetBinding(Entry.TextProperty, new Binding("TheirLocation", BindingMode.TwoWay));

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

      var statusAndActivity = new StackLayout {
        Orientation = StackOrientation.Horizontal,
        Children = { status, activity },
      };

      portraitView = () => new StackLayout {
        Children = {
          yourLocationLabel,
          yourLocationEntryAndButton,
          theirLocationLabel,
          theirLocation,
          pickerLabel,
          picker,
          searchButton,
          statusAndActivity,
          deviceResultsView,
        }
      };

      landscapeView = () => new Grid {
        Children = {
          yourLocationLabel.GridRowCol(0, 0),
          yourLocationEntryAndButton.GridRowCol(0, 1),

          theirLocationLabel.GridRowCol(1, 0),
          theirLocation.GridRowCol(1, 1),
          
          pickerLabel.GridRowCol(2, 0),
          picker.GridRowCol(2, 1),
          
          searchButton.GridRowCol(3, 0).GridColSpan(2),
          statusAndActivity.GridRowCol(4, 0).GridColSpan(2),
          deviceResultsView.GridRowCol(5, 0).GridColSpan(2),
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
          new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
        },
      };

      SizeChanged += (sender, e) => Content = IsPortrait(this) ? portraitView() : landscapeView();
    }

    static bool IsPortrait(Page p) { return p.Width < p.Height; }

    async void yourLocationButton_Clicked(object sender, EventArgs e) {
      try {
        activity.IsRunning = true;
        status.Text = "finding your location...";

        var loc = await gc.GetCurrentLocationAsync();
        var addr = await gc.GetAddressForLocationAsync(loc);
        model.YourLocation = addr;
        status.Text = "";
      }
      catch (Exception ex) {
        status.Text = "Can't get your location: " + ex.Message;
      }
      finally {
        activity.IsRunning = false;
      }
    }

    async void searchButton_Clicked(object sender, EventArgs e) {
      try {
        activity.IsRunning = true;
        status.Text = "searching...";

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

        status.Text = "";
      }
      catch (Exception ex) {
        status.Text = "Check your network connection: " + ex.Message;
      }
      finally {
        activity.IsRunning = false;
      }
    }

  }

}
