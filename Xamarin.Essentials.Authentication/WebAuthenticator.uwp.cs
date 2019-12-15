using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials.Authentication
{
	internal static class WebAuthenticator
	{
		internal static Task<AuthResult> NavigateAsync(Uri navigateUri, Uri redirectUri)
		{
			RedirectUri = redirectUri;

			tcsAccount = new TaskCompletionSource<AuthResult>();

			// TODO: Check if app is registered
			Windows.System.Launcher.LaunchUriAsync(navigateUri).AsTask();

			return tcsAccount.Task;
		}

		public static Uri RedirectUri { get; private set; }

		static TaskCompletionSource<AuthResult> tcsAccount = null;

		internal static bool Callback(Uri uri)
		{
			// Only handle the url with our callback uri scheme
			if (!uri.Scheme.Equals(RedirectUri.Scheme))
				return false;

			// Ensure we have a task waiting
			if (tcsAccount != null && !tcsAccount.Task.IsCompleted)
			{
				try
				{
					// Parse the account from the url the app opened with
					var r = AuthResult.FromUri(uri);

					// Set our account result
					tcsAccount.TrySetResult(r);
				}
				catch (Exception ex)
				{
					tcsAccount.TrySetException(ex);
				}
			}

			tcsAccount.TrySetResult(null);
			return false;
		}
	}
}
