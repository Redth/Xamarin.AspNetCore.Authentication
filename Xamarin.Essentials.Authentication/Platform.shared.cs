using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials.Authentication
{
	public static partial class Platform
	{
		static Dictionary<string, Type> authenticatorOverrides = new Dictionary<string, Type>();

		public static void RegisterAuthenticator<TAuthenticator>(string authenticationScheme) where TAuthenticator : Authenticator
			=> authenticatorOverrides[authenticationScheme] = typeof(TAuthenticator);

		public static void UnregisterAuthenticator(string authenticationScheme)
			=> authenticatorOverrides.Remove(authenticationScheme);

		public static Authenticator GetAuthenticator(string authenticationScheme)
		{
			if (authenticatorOverrides.TryGetValue(authenticationScheme, out var v))
				return (Authenticator)Activator.CreateInstance(v);

			return null;
		}
	}

	public abstract class Authenticator
	{
		public abstract Task<AuthResult> AuthenticateAsync(Uri appCallbackUri, Uri serverBaseUri, string authenticationScheme, string authenticationPath);
	}
}
