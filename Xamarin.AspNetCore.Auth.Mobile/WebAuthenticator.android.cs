using Android.Content;
using Android.Support.CustomTabs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.AspNetCore.Auth.Mobile
{
	internal class WebAuthenticator
	{
		static bool isWaiting = false;
		static TaskCompletionSource<AuthResult> tcsResponse = null;
		static Uri uri = null;

		public static CustomTabsActivityManager CustomTabsActivityManager { get; private set; }
		public static global::Android.App.Activity ParentActivity { get; set; }
		public static Uri RedirectUri { get; private set; }

		public static void Cancelled()
			=> tcsResponse?.TrySetCanceled();

		public static void Failed(Exception ex)
			=> tcsResponse.TrySetException(ex);

		public static void Succeeded(AuthResult response)
			=> tcsResponse?.TrySetResult(response);

		public static Task<AuthResult> ResponseTask
			=> tcsResponse?.Task;

		public static bool Callback(Intent intent)
		{
			if (!isWaiting)
				return false;

			isWaiting = false;

			try
			{
				var intentUri = new Uri(intent.Data.ToString());

				// Only handle schemes we expect
				if (!WebUtils.CanHandleCallback(RedirectUri, intentUri))
				{
					Failed(new NullReferenceException("Invalid Redirect URI"));
					return false;
				}

				Succeeded(AuthResult.FromUri(intentUri));
				return true;
			}
			catch (Exception ex)
			{
				Failed(ex);
				return false;
			}
		}

		internal static Task<AuthResult> NavigateAsync(Uri navigateUri, Uri redirectUri, global::Android.App.Activity parentActivity)
		{
			isWaiting = true;

			tcsResponse = new TaskCompletionSource<AuthResult>();
			uri = navigateUri;
			ParentActivity = parentActivity;
			RedirectUri = redirectUri;

			CustomTabsActivityManager = new CustomTabsActivityManager(ParentActivity);
			CustomTabsActivityManager.NavigationEvent += CustomTabsActivityManager_NavigationEvent;
			CustomTabsActivityManager.CustomTabsServiceConnected += CustomTabsActivityManager_CustomTabsServiceConnected;

			if (!CustomTabsActivityManager.BindService())
			{
				// Fall back to opening the system browser if necessary
				var browserIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("http://www.google.com"));
				parentActivity.StartActivity(browserIntent);
			}

			return WebAuthenticator.ResponseTask;
		}

		static void CustomTabsActivityManager_CustomTabsServiceConnected(ComponentName name, CustomTabsClient client)
		{
			var builder = new CustomTabsIntent.Builder(CustomTabsActivityManager.Session)
												  .SetShowTitle(true);

			var customTabsIntent = builder.Build();
			customTabsIntent.Intent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.NoHistory | ActivityFlags.NewTask);

			CustomTabsHelper.AddKeepAliveExtra(ParentActivity, customTabsIntent.Intent);

			customTabsIntent.LaunchUrl(ParentActivity, global::Android.Net.Uri.Parse(uri.OriginalString));
		}

		static void CustomTabsActivityManager_NavigationEvent(int navigationEvent, global::Android.OS.Bundle extras)
		{
			Console.WriteLine($"CustomTabs.NavigationEvent: {navigationEvent}");
		}
	}
}
