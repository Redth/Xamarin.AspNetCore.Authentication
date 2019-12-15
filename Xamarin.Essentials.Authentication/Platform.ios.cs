using System;
using Foundation;
using UIKit;

namespace Xamarin.Essentials.Authentication
{
	public static partial class Platform
	{
		public static void Init(string appleSignInScheme = "Apple")
		{
			// Use native apple sign in if available
			if (UIDevice.CurrentDevice.CheckSystemVersion(13,0))
				RegisterAuthenticator<NativeAppleSignInAuthenticator>(appleSignInScheme);
		}

		// TODO: Make smart to call authenticator's open url maybe?
		public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
			=> WebAuthenticator.UrlOpened(new Uri(url.AbsoluteString));

		public static UIViewController GetCurrentViewController()
			=> PresentingViewController;

		internal static UIViewController PresentingViewController
		{
			get
			{
				var window = UIApplication.SharedApplication.KeyWindow;
				var vc = window.RootViewController;
				while (vc.PresentedViewController != null)
					vc = vc.PresentedViewController;
				return vc;
			}
		}

		internal static UIWindow PresentingWindow
			=> UIApplication.SharedApplication.KeyWindow;
	}
}