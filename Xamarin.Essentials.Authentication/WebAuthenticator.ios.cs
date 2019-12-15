using Foundation;
using SafariServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials.Authentication
{
	public static class WebAuthenticator
	{
		static TaskCompletionSource<AuthResult> tcsAuth;
		static UIViewController currentViewController;
		static Uri redirectUri;
		static bool isWaiting = false;

		internal static async Task<AuthResult> NavigateAsync(Uri uri, Uri redirectUri)
		{
			isWaiting = true;

			tcsAuth = new TaskCompletionSource<AuthResult>();
			WebAuthenticator.redirectUri = redirectUri;
			
			try
			{
				var scheme = redirectUri.Scheme;

				if (!VerifyHasUrlSchemeOrDoesntRequire(scheme))
				{
					tcsAuth.TrySetException(new InvalidOperationException("You must register your URL Scheme handler in your app's Info.plist!"));
					return await tcsAuth.Task;
				}

				if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
				{
					var sf = new SFAuthenticationSession(new NSUrl(uri.OriginalString), scheme,
						(callbackUrl, Error) =>
						{
							if (Error == null)
								UrlOpened(callbackUrl);
							else
								tcsAuth.TrySetException(new Exception($"SFAuthenticationSession Error: {Error.ToString()}"));
						}
					);
					sf.Start();
					return await tcsAuth.Task;
				}

				if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
				{
					var controller = new SFSafariViewController(new NSUrl(uri.OriginalString), false)
					{
						Delegate = new NativeSFSafariViewControllerDelegate
						{
							DidFinishHandler = delegate
							{
								// Cancel our task if it wasn't already marked as completed
								if (!(tcsAuth?.Task?.IsCompleted ?? true))
									tcsAuth.TrySetException(new OperationCanceledException());

								Console.WriteLine("Finished");
							}
						},
					};
					currentViewController = controller;
					await Platform.PresentingViewController.PresentViewControllerAsync(controller, true);
					return await tcsAuth.Task;
				}

				var opened = UIApplication.SharedApplication.OpenUrl(uri);
				if (!opened)
					tcsAuth.TrySetException(new Exception("Error opening Safari"));
			}
			catch (Exception ex)
			{
				tcsAuth.TrySetException(ex);
			}

			return await tcsAuth.Task;
		}

		internal static bool UrlOpened(Uri uri)
		{
			// If we aren't waiting on a task, don't handle the url
			if (tcsAuth?.Task?.IsCompleted ?? true)
				return false;

			try
			{
				// If we can't handle the url, don't
				if (!WebUtils.CanHandleCallback(redirectUri, uri))
					return false;

				currentViewController?.DismissViewControllerAsync(true);
				currentViewController = null;

				tcsAuth.TrySetResult(AuthResult.FromUri(uri));
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return false;
		}

		public static bool VerifyHasUrlSchemeOrDoesntRequire(string scheme)
		{
			//ios11+ uses sfAuthenticationSession which handles its own url routing
			if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
				return true;

			var cleansed = scheme.Replace("://", "");
			var schemes = GetCFBundleURLSchemes().ToList();
			return schemes.Any(x => x.Equals(cleansed, StringComparison.InvariantCultureIgnoreCase));
		}

		static IEnumerable<string> GetCFBundleURLSchemes()
		{
			NSObject nsobj = null;
			if (!NSBundle.MainBundle.InfoDictionary.TryGetValue((NSString)"CFBundleURLTypes", out nsobj))
				yield return null;
			var array = nsobj as NSArray;
			for (nuint i = 0; i < (array?.Count ?? 0); i++)
			{
				var d = array.GetItem<NSDictionary>(i);
				if (!d?.TryGetValue((NSString)"CFBundleURLSchemes", out nsobj) ?? false)
					yield return null;
				var a = nsobj as NSArray;
				var urls = ConvertToIEnumerable<NSString>(a).Select(x => x.ToString()).ToArray();
				foreach (var url in urls)
					yield return url;
			}
		}

		static IEnumerable<T> ConvertToIEnumerable<T>(NSArray array) where T : class, ObjCRuntime.INativeObject
		{
			for (nuint i = 0; i < array.Count; i++)
				yield return array.GetItem<T>(i);
		}


		class NativeSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
		{
			public Action<SFSafariViewController> DidFinishHandler { get; set; }

			public override void DidFinish(SFSafariViewController controller)
			{
				DidFinishHandler?.Invoke(controller);
			}
		}
	}
}
