using Refractored.Xam.Settings;
using System;
using System.ComponentModel;
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
    SearchMode mode;
    Place[] results;

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

}
