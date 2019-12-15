using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials.Authentication
{
	public static class Authentication
	{
		public static Task<AuthResult> AuthenticateAsync(Uri appCallbackUri, Uri serverBaseUri, string authenticationScheme, string authenticationPath = "/mobileauth")
		{
			var uri = new Uri(serverBaseUri.AbsoluteUri.TrimEnd('/') + "/" + authenticationPath.Trim('/') + "/" + authenticationScheme);

			// See if an authenticator is registered to use instead of the default web authenticator
			var auth = Platform.GetAuthenticator(authenticationScheme);
			if (auth != null)
				return auth.AuthenticateAsync(appCallbackUri, serverBaseUri, authenticationScheme, authenticationPath);

#if __ANDROID__
			return WebAuthenticator.NavigateAsync(uri, appCallbackUri, Platform.GetCurrentActivity(false));
#elif __IOS__
			return WebAuthenticator.NavigateAsync(uri, appCallbackUri);
#elif __UWP__
			return WebAuthenticator.NavigateAsync(uri, appCallbackUri);
#endif

			return Task.FromResult(new AuthResult());
		}
	}
}
