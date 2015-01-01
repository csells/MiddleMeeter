using System;
using Xamarin.Forms;

namespace MiddleMeeter {
  class ResultsView : ContentView {
    public static readonly BindableProperty ResultsProperty =
      BindableProperty.Create<ResultsView, Place[]>(view => view.Results, null);

    public ResultsView() {
      PropertyChanged += (sender, e) => {
        if (e.PropertyName != "Results") { return; }

        if( Results == null || Results.Length == 0 ) {
          Content = null;
          return;
        }

        var section = new TableSection("Search Results");
        foreach (var result in Results) {
          var cell = new ImageCell { Text = result.Name, Detail = result.Vicinity, ImageSource = result.Icon };
          cell.Tapped += (sender2, e2) => LaunchMapApp(result);
          section.Add(cell);
        }

        Content = new TableView {
          Intent = TableIntent.Menu,
          Root = new TableRoot("Places") {
              section
            }
        };
      };
    }

    public Place[] Results {
      get { return (Place[])GetValue(ResultsProperty); }
      set { SetValue(ResultsProperty, value); }
    }

    public void LaunchMapApp(Place place) {
      // Windows Phone doesn't like ampersands in the names and the normal URI escaping doesn't help
      var name = place.Name.Replace("&", "and"); // var name = Uri.EscapeUriString(place.Name);
      var loc = string.Format("{0},{1}", place.Location.Latitude, place.Location.Longitude);
      var addr = Uri.EscapeUriString(place.Vicinity);

      var request = Device.OnPlatform(
        // iOS doesn't like %s or spaces in their URLs, so manually replace spaces with +s
        string.Format("http://maps.apple.com/maps?q={0}&sll={1}", name.Replace(' ', '+'), loc),

        // pass the address to Android if we have it
        string.Format("geo:0,0?q={0}({1})", string.IsNullOrWhiteSpace(addr) ? loc : addr, name),

        // WinPhone
        string.Format("bingmaps:?cp={0}&q={1}", loc, name)
      );

      Device.OpenUri(new Uri(request));
    }

  }

  class ResultsPage : ContentPage {
    public ResultsPage(ResultsView content) {
      Title = "Places";
      Content = content;
    }

  }
}
