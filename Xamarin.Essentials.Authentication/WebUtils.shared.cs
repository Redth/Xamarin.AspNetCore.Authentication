using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Essentials.Authentication
{
	public static class WebUtils
	{
		public static string ToQueryString(this Dictionary<string, string> items)
		{
			var r = new StringBuilder();

			foreach (var i in items)
			{
				r.Append(System.Net.WebUtility.UrlEncode(i.Key));
				r.Append("=");
				r.Append(System.Net.WebUtility.UrlEncode(i.Value));
				r.Append("&");
			}

			return "?" + r.ToString().TrimEnd('&');
		}

		public static IDictionary<string, string> ParseQueryString(string url)
		{
			var d = new Dictionary<string, string>();

			if (string.IsNullOrWhiteSpace(url) || (!url.Contains("?") && !url.Contains("#")))
				return d;

			var qsStartIndex = url.IndexOf('?');
			if (qsStartIndex < 0)
				qsStartIndex = url.IndexOf('#');

			if (url.Length - 1 < qsStartIndex + 1)
				return d;

			var qs = url.Substring(qsStartIndex + 1);

			var kvps = qs.Split('&');

			if (kvps == null || !kvps.Any())
				return d;

			foreach (var kvp in kvps)
			{
				var pair = kvp.Split(new char[] { '=' }, 2);

				if (pair == null || pair.Length != 2)
					continue;

				d[pair[0]] = pair[1];
			}

			return d;
		}

		public static string Base64UrlEncode(byte[] data)
		{
			var base64 = Convert.ToBase64String(data);
			var base64Url = new StringBuilder();

			foreach (var c in base64)
			{
				if (c == '+')
					base64Url.Append('-');
				else if (c == '/')
					base64Url.Append('_');
				else if (c == '=')
					break;
				else
					base64Url.Append(c);
			}

			return base64Url.ToString();
		}

		public static byte[] Base64UrlDecode(string encoded)
		{
			var decoded = encoded;
			decoded = decoded.Replace('_', '/');
			decoded = decoded.Replace('-', '+');

			// Figure out if we need to pad with trailing = 
			switch (decoded.Length % 4)
			{
				case 0:
					break;
				case 2:
					decoded += "==";
					break;
				case 3:
					decoded += "=";
					break;
			}

			return Convert.FromBase64String(decoded);
		}

		public static string UrlEncodeRfc5894(string unencoded)
		{
			var utf8 = Encoding.UTF8.GetBytes(unencoded);
			var sb = new StringBuilder();

			for (var i = 0; i < utf8.Length; i++)
			{
				var v = utf8[i];
				if ((0x41 <= v && v <= 0x5A) || (0x61 <= v && v <= 0x7A) || (0x30 <= v && v <= 0x39) ||
					v == 0x2D || v == 0x2E || v == 0x5F || v == 0x7E)
					sb.Append((char)v);
				else
					sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "%{0:X2}", v);
			}

			return sb.ToString();
		}

		public static bool CanHandleCallback(Uri expectedUrl, Uri callbackUrl)
		{
			if (!callbackUrl.Scheme.Equals(expectedUrl.Scheme, StringComparison.OrdinalIgnoreCase))
				return false;

			if (!string.IsNullOrEmpty(expectedUrl.Host))
			{
				if (!callbackUrl.Host.Equals(expectedUrl.Host, StringComparison.OrdinalIgnoreCase))
					return false;
			}

			return true;
		}

	}
}
