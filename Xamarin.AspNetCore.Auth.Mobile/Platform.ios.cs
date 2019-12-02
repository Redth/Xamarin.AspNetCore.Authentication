using System;
using Foundation;
using UIKit;

namespace Xamarin.AspNetCore.Auth.Mobile
{
	public class Platform
	{
		public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
			=> WebAuthenticator.UrlOpened(new Uri(url.AbsoluteString));

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