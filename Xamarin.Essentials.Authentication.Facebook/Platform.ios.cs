using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Xamarin.Essentials.Authentication.Facebook
{
	public static partial class Platform
	{
		public static string AppId { get; }

		public static string[] Permissions { get; }

		public static bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
			=> global::Facebook.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);

		public static bool FinishedLaunching(UIApplication app, NSDictionary options)
			=> global::Facebook.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);

		public static void Init(string facebookAuthenticationScheme = "Facebook")
			=> Essentials.Authentication.Platform.RegisterAuthenticator<FacebookNativeAuthenticator>(facebookAuthenticationScheme);

		public static void SignOut()
			=> FacebookNativeAuthenticator.SignOut();
	}
}
