﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace MobileSample.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Xamarin.Essentials.Authentication.Facebook.Platform.FinishedLaunching(app, options);
            Xamarin.Essentials.Authentication.Facebook.Platform.Init();

            Xamarin.Essentials.Authentication.Platform.Init();

            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

		public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
		{
            if (Xamarin.Essentials.Authentication.Platform.OpenUrl(app, url, options))
				return true;

			return base.OpenUrl(app, url, options);
		}

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            if (Xamarin.Essentials.Authentication.Facebook.Platform.OpenUrl(application, url, sourceApplication, annotation))
                return true;

            return base.OpenUrl(application, url, sourceApplication, annotation);
        }
    }
}
