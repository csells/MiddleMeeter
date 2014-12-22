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

      //var section = new TableSection();
      //foreach (var result in results) {
      //  section.Add(new TextCell { Text = result.Name, Detail = result.Description });
      //}

      //Content = new TableView {
      //  Intent = TableIntent.Menu,
      //  Root = new TableRoot {
      //    section
      //  }
      //};

      var template = new DataTemplate(typeof(TextCell));
      template.SetBinding(TextCell.TextProperty, new Binding("Name"));
      template.SetBinding(TextCell.DetailProperty, new Binding("Description"));

      Content = new ListView {
        ItemsSource = results,
        ItemTemplate = template,
      };
    }
  }
}
