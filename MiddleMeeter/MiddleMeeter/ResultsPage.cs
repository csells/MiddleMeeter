using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MiddleMeeter {
  class Result {
    public string Name { get; set; }
    public string Description { get; set; }
  }

  class ResultsPage : ContentPage {
    public ResultsPage(Result[] results) {
      Title = "Results";

      var section = new TableSection();
      foreach (var result in results) {
        section.Add(new TextCell { Text = result.Name, Detail = result.Description });
      }

      Content = new TableView {
        Intent = TableIntent.Menu,
        Root = new TableRoot {
          section
        }
      };

    }
  }
}
