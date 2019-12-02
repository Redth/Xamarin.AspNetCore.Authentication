using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xamarin.AspNetCore.Auth
{
	public static class XamarinAuthExtensions
	{
		const string APPLE_ISSUER = "https://appleid.apple.com";
		const string APPLE_AUTH_URL = "https://appleid.apple.com/auth/authorize";
		const string APPLE_TOKEN_URL = "https://appleid.apple.com/auth/token";
		const string APPLE_JWT_KEYS_URL = "https://appleid.apple.com/auth/keys";

		public static IServiceCollection AddXamarinAuth(this IServiceCollection service, Action<XamarinAuthOptions> configureOptions = default)
			=> service.Configure(configureOptions ?? (o => { }));

		public static IApplicationBuilder UseXamarinAuth(this IApplicationBuilder builder)
			=> builder.UseMiddleware<XamarinAuthMiddleware>();

		public static AuthenticationBuilder AddAppleSignIn(this AuthenticationBuilder authenticationBuilder, AppleSignInOptions appleOptions, Action<OpenIdConnectOptions> configureOptions = default)
		{
			return authenticationBuilder.AddOpenIdConnect("Apple", async options =>
			{
				options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.ResponseType = "code id_token";
				options.Scope.Clear();
				options.Scope.Add("name");
				options.Scope.Add("email");
				options.ClientId = appleOptions.ServerId; // ServerId is client id
				options.CallbackPath = "/signin-apple"; // Default callback path
				options.TokenValidationParameters.ValidIssuer = APPLE_ISSUER; // Which issuer we expect
				options.SaveTokens = true;

				options.Configuration = new OpenIdConnectConfiguration
				{
					AuthorizationEndpoint = APPLE_AUTH_URL,
					TokenEndpoint = APPLE_TOKEN_URL,
				};

				options.Events.OnAuthorizationCodeReceived = context =>
				{
					context.TokenEndpointRequest.ClientSecret = GenerateSecretToken(appleOptions);
					return Task.CompletedTask;
				};

				// Get the identity token signing key we expect
				var jwks = await new HttpClient().GetStringAsync(APPLE_JWT_KEYS_URL);
				options.TokenValidationParameters.IssuerSigningKey = new JsonWebKeySet(jwks).Keys.FirstOrDefault();
			});
		}

		public static string GenerateSecretToken(AppleSignInOptions options)
		{
			var cngKey = CngKey.Import(
			  Convert.FromBase64String(CleanP8Key(options.P8Key)),
			  CngKeyBlobFormat.Pkcs8PrivateBlob);

			var jwtHandler = new JwtSecurityTokenHandler();
			var jwtToken = jwtHandler.CreateJwtSecurityToken(
				issuer: options.TeamId,
				audience: APPLE_ISSUER,
				subject: new ClaimsIdentity(new List<Claim> { new Claim("sub", options.ServerId) }),
				expires: DateTime.UtcNow.AddMinutes(5),
				issuedAt: DateTime.UtcNow,
				notBefore: DateTime.UtcNow,
				signingCredentials: new SigningCredentials(
				  new ECDsaSecurityKey(new ECDsaCng(cngKey)), SecurityAlgorithms.EcdsaSha256));

			return jwtHandler.WriteToken(jwtToken);
		}

		static string CleanP8Key(string p8Contents)
		{
			// Remove whitespace
			var tmp = Regex.Replace(p8Contents, "\\s+", string.Empty, RegexOptions.Singleline);

			// Remove `---- BEGIN PRIVATE KEY ----` bits
			tmp = Regex.Replace(tmp, "-{1,}.*?-{1,}", string.Empty, RegexOptions.Singleline);

			return tmp;
		}
	}
}
