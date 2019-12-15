using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Essentials.Authentication.Facebook
{
	public static class Platform
	{
		public static string AppId { get; set; }

		public static string[] Permissions { get; set; }


		public static void Init(string facebookAuthenticationScheme = "Facebook")
		{
			Xamarin.Essentials.Authentication.Platform.RegisterAuthenticator<FacebookNativeAuthenticator>(facebookAuthenticationScheme);
		}

		public static void SignOut()
			=> FacebookNativeAuthenticator.SignOut();
	}
}
