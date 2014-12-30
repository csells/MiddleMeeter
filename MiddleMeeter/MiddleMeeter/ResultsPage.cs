using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace MiddleMeeter {
  class ResultsPage : ContentPage {
    public ResultsPage(Place[] results) {
      Title = "Places";

      var section = new TableSection();
      foreach (var result in results) {
        var cell = new ImageCell { Text = result.Name, Detail = result.Vicinity, ImageSource = result.Icon };
        cell.Tapped += (sender, e) => LaunchMapApp(result);
        section.Add(cell);
      }

      Content = new TableView {
        Intent = TableIntent.Menu,
        Root = new TableRoot("Places") {
          section
        }
      };

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
}
