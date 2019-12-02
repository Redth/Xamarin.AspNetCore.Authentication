using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.AspNetCore.Auth.Mobile
{
	public static class Authentication
	{
		public static Task<AuthResult> AuthenticateAsync(Uri appCallbackUri, Uri serverBaseUri, string authenticationScheme, string authenticationPath = "/mobileauth")
		{
			var uri = new Uri(serverBaseUri.AbsoluteUri.TrimEnd('/') + "/" + authenticationPath.Trim('/') + "/" + authenticationScheme);

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
