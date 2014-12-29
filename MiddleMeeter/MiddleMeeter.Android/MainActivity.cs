using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Labs.Services;
using Xamarin.Forms.Labs;

namespace MiddleMeeter.Droid {
  [Activity(Label = "MiddleMeeter", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
  public class MainActivity : AndroidActivity {
    protected override void OnCreate(Bundle bundle) {
      base.OnCreate(bundle);

      // DONE: force dependency injection
      new Xamarin.Forms.Labs.Droid.Services.Geolocation.Geolocator();

      Xamarin.Forms.Forms.Init(this, bundle);

      SetPage(App.GetMainPage());
    }
  }
}

