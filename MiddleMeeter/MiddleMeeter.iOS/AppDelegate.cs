using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Labs.Services;
using Xamarin.Forms.Labs;

namespace MiddleMeeter.iOS {
  // The UIApplicationDelegate for the application. This class is responsible for launching the 
  // User Interface of the application, as well as listening (and optionally responding) to 
  // application events from iOS.
  [Register("AppDelegate")]
  public partial class AppDelegate : UIApplicationDelegate {
    // class-level declarations
    UIWindow window;

    //
    // This method is invoked when the application has loaded and is ready to run. In this 
    // method you should instantiate the window, load the UI into it and then make the window
    // visible.
    //
    // You have 17 seconds to return from this method, or iOS will terminate your application.
    //
    public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
      // DONE: force dependency injection
      new Xamarin.Forms.Labs.iOS.Services.Geolocation.Geolocator();

      Forms.Init();

      window = new UIWindow(UIScreen.MainScreen.Bounds);

      window.RootViewController = App.GetMainPage().CreateViewController();

      window.MakeKeyAndVisible();

      return true;
    }
  }
}
