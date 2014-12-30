using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MiddleMeeter.iOS {
  public class Application {
    // This is the main entry point of the application.
    static void Main(string[] args) {
      // System.Exception: Could not initialize an instance of the type 'MonoTouch.Foundation.NSUrl': the native 'initWithString:' method returned nil.
      // It is possible to ignore this condition by setting MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure to false.
      //MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure = false;

      // if you want to use a different Application Delegate class from "AppDelegate"
      // you can specify it here.
      UIApplication.Main(args, null, "AppDelegate");
    }
  }
}
