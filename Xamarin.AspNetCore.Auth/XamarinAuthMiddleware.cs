using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.AspNetCore.Auth
{
	public class XamarinAuthMiddleware
	{
		readonly RequestDelegate _next;

		readonly XamarinAuthOptions options;

		public XamarinAuthMiddleware(RequestDelegate next, IOptions<XamarinAuthOptions> configureOptions)
		{
			_next = next;

			options = configureOptions.Value;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (context.Request.Path.HasValue && context.Request.Path.StartsWithSegments(new PathString(options.AuthPath)))
			{
				// Get the scheme being used
				var path = context.Request.Path.Value;

				var scheme = path.Substring(path.LastIndexOf('/')).Trim('/');

				var auth = await context.AuthenticateAsync(scheme);

				if (!auth.Succeeded || auth?.Principal == null || !auth.Principal.Identities.Any(id => id.IsAuthenticated))
				{
					// Not authenticated, challenge
					await context.ChallengeAsync(scheme);
				}
				else
				{
					// Parse out items to send back to app
					var refresh = auth.Properties.GetTokenValue("refresh_token");
					var access = auth.Properties.GetTokenValue("access_token");

					long expires = -1;
					if (auth.Properties.ExpiresUtc.HasValue)
						expires = auth.Properties.ExpiresUtc.Value.ToUnixTimeSeconds();

					if (string.IsNullOrEmpty(access))
					{
						// No access token, challenge
						await context.ChallengeAsync(scheme);
					}
					else
					{
						var qs = new Dictionary<string, string>();
						qs.Add("access_token", access);

						if (!string.IsNullOrEmpty(refresh))
							qs.Add("refresh_token", refresh);
						if (expires >= 0)
							qs.Add("expires_in", expires.ToString());

						// Give the consumer of the library a chance to change the parameters
						if (options.AuthenticatedRedirectHandler != null)
							options.AuthenticatedRedirectHandler.Invoke(context, auth, qs);

						var sep = options.UseUrlFragmentForRedirectParameters ? "#" : "?";

						// Build the result url
						var url = options.CallbackUri.ToString()
							+ sep + string.Join("&", qs.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

						// Redirect to final url
						context.Response.Redirect(url);
					}
				}
			}
			else
			{
				// Not us to handle, call the next delegate/middleware in the pipeline
				await _next(context);
			}
		}
	}

	public class XamarinAuthOptions
	{
		/// <summary>
		/// Your App's callback URI that is registered to receive requests (eg: myappcustomscheme://)
		/// </summary>
		public Uri CallbackUri { get; set; }

		/// <summary>
		/// The path within the base url which will be registered to handle authentication flow requests
		/// </summary>
		public string AuthPath { get; set; } = "/mobileauth";

		/// <summary>
		/// Optional delegate called before the app callback occurs, which provides a chance to add or modify the redirect url parameters
		/// </summary>
		public AuthenticatedRedirectDelegate AuthenticatedRedirectHandler { get; set; } = null;

		/// <summary>
		/// If true (default), '#' is used to separate the URL parameters from the URL, otherwise '?' is used
		/// </summary>
		public bool UseUrlFragmentForRedirectParameters { get; set; } = true;
	}

	public delegate void AuthenticatedRedirectDelegate(HttpContext context, AuthenticateResult authenticateResult, IDictionary<string, string> redirectUriParameters);
}
