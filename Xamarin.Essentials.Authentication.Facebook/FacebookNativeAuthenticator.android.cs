using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace Xamarin.Essentials.Authentication.Facebook
{
	public class FacebookNativeAuthenticator : Authenticator
	{	
		static ICallbackManager callbackManager = null;

		public override async Task<AuthResult> AuthenticateAsync(Uri appCallbackUri, Uri serverBaseUri, string authenticationScheme, string authenticationPath)
		{
			var currentActivity = Xamarin.Essentials.Authentication.Platform.GetCurrentActivity();

#pragma warning disable CS0619
#pragma warning disable CS0618
			FacebookSdk.ApplicationId = Platform.AppId;
			FacebookSdk.SdkInitialize(currentActivity);
#pragma warning restore CS0619
#pragma warning restore CS0619

			LoginManager.Instance.LogOut();

			callbackManager = CallbackManagerFactory.Create();
			var loginManager = LoginManager.Instance;

			var fbHandler = new FbCallbackHandler();

			loginManager.RegisterCallback(callbackManager, fbHandler);

			fbHandler.Reset();

			var perms = new List<string>();
			if (Platform.Permissions != null && Platform.Permissions.Length > 0)
				perms.AddRange(Platform.Permissions);
			if (perms.Count <= 0)
				perms = null;

			loginManager.LogInWithReadPermissions(currentActivity, perms);

			LoginResult result = null;

			try
			{
				result = await fbHandler.Task;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return null;
			}

			DateTime? expires = null;
			long expiresIn = -1;
			if (result?.AccessToken.Expires != null)
			{
				expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(result.AccessToken.Expires.Time);
				expiresIn = (long)(expires.Value - DateTime.Now).TotalSeconds;
			}

			var a = new AuthResult();
			a.Put("expires_in", expiresIn.ToString());
			a.Put("access_token", result.AccessToken.Token);
			a.Put("user_id", result.AccessToken.UserId);
			a.Put("permissions", string.Join(" ", result.AccessToken.Permissions));

			return a;
		}

		internal static void SignOut()
			=> LoginManager.Instance.LogOut();

		class FbCallbackHandler : Java.Lang.Object, IFacebookCallback
		{
			TaskCompletionSource<LoginResult> tcs = new TaskCompletionSource<LoginResult>();

			public void Reset()
			{
				if (tcs != null && !tcs.Task.IsCompleted)
					tcs.TrySetResult(null);

				tcs = new TaskCompletionSource<LoginResult>();
			}

			public Task<LoginResult> Task
				=> tcs.Task;

			public void OnCancel()
				=> tcs.TrySetResult(null);

			public void OnError(FacebookException ex)
				=> tcs.TrySetException(new System.Exception(ex.Message));

			public void OnSuccess(Java.Lang.Object data)
				=> tcs.TrySetResult(data.JavaCast<LoginResult>());
		}
	}
}
