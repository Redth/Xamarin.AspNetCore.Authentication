using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fb = global::Facebook;

namespace Xamarin.Essentials.Authentication.Facebook
{
	public class FacebookNativeAuthenticator : Authenticator
	{		
		TaskCompletionSource<AuthResult> tcsAccount;

		public override Task<AuthResult> AuthenticateAsync(Uri appCallbackUri, Uri serverBaseUri, string authenticationScheme, string authenticationPath)
		{
			Fb.CoreKit.Settings.AppId = Platform.AppId;
			tcsAccount = new TaskCompletionSource<AuthResult>();

			var loginManager = new Fb.LoginKit.LoginManager();

			var vc = Essentials.Authentication.Platform.GetCurrentViewController();

			var p = new List<string>();
			if (Platform.Permissions != null && Platform.Permissions.Length > 0)
				p.AddRange(Platform.Permissions);

			loginManager.LogIn(p.ToArray(), vc, LoginHandler);

			return tcsAccount.Task;
		}

		internal static void SignOut()
		{
			var loginManager = new Fb.LoginKit.LoginManager();
			loginManager.LogOut();
		}

		void LoginHandler(Fb.LoginKit.LoginManagerLoginResult result, NSError err)
		{
			if (result.IsCancelled)
			{
				tcsAccount?.TrySetCanceled();
				return;
			}

			var date = (DateTime)(result?.Token?.ExpirationDate ?? NSDate.DistantPast);
			var expiresIn = (long)(date - DateTime.Now).TotalSeconds;

			if (expiresIn < 0)
				expiresIn = -1;

			var permissions = new List<string>();
			foreach (var p in result.Token.Permissions.ToArray<NSString>())
				permissions.Add(p.ToString());

			var a = new AuthResult();
			a.Put("expires_in", expiresIn.ToString());
			a.Put("access_token", result.Token.TokenString);
			a.Put("user_id", result.Token.UserId);
			a.Put("permissions", string.Join(" ", permissions));

			tcsAccount?.TrySetResult(a);
		}
	}
}
