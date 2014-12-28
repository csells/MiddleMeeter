using Xamarin.Forms;

namespace MiddleMeeter {
  class ResultsPage : ContentPage {
    public ResultsPage(Place[] results) {
      Title = "Places";

      var section = new TableSection();
      foreach (var result in results) {
        section.Add(new ImageCell { Text = result.Name, Detail = result.Vicinity, ImageSource = result.Icon });
      }

      Content = new TableView {
        Intent = TableIntent.Menu,
        Root = new TableRoot("Places") {
          section
        }
      };

    }
  }
}
