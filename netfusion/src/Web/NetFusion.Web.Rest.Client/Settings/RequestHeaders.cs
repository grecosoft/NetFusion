﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using NetFusion.Web.Common;

namespace NetFusion.Web.Rest.Client.Settings;

/// <summary>
/// HTTP Headers associated with a HTTP request.
/// </summary>
public class RequestHeaders
{
	/// <summary>
	/// The HTTP accept media-type. 
	/// </summary>
	public HeaderValue[] Accept { get; private set; }

	/// <summary>
	/// The HTTP content media-type.
	/// </summary>
	public HeaderValue ContentType { get; private set; }

	/// <summary>
	/// A named list of header values associated with the request.
	/// </summary>
	public IReadOnlyDictionary<string, HeaderValue> Values { get; }

	private readonly Dictionary<string, HeaderValue> _headers;

	/// <summary>
	/// Constructor.
	/// </summary>
	public RequestHeaders()
	{
		_headers = new Dictionary<string, HeaderValue>();
		Values = new ReadOnlyDictionary<string, HeaderValue>(_headers);
	}

	/// <summary>
	/// Adds a header value to be associated with the request.
	/// </summary>
	/// <returns>Self reference for method chaining.</returns>
	/// <param name="name">The name of the header value.</param>
	/// <param name="value">The value of the header.</param>
	public RequestHeaders Add(string name, params string[] value)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Header name not specified.", nameof(name));

		if (IsKnownHeaderName(name))
		{
			throw new ArgumentException(
				$"The HTTP Header named: {name} must be set using the specific " + 
				$"header method defined on the {GetType()} class.", 
				nameof(name));
		}

		_headers[name] = HeaderValue.WithValue(value);
		return this;
	}

	private static bool IsKnownHeaderName(string name)
	{
		return new[] { HttpHeaderNames.Accept, HttpHeaderNames.ContentType }.Contains(name);
	}

	/// <summary>
	/// Removes the header with the specified name.
	/// </summary>
	/// <param name="name">The name of the header to be removed.</param>
	public void Remove(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Header name not specified.", nameof(name));
	        
		_headers.Remove(name);
	}

	/// <summary>
	/// Removes the authentication header.
	/// </summary>
	public void RemoveAuthHeader()
	{
		Remove("Authorization");
	}

	/// <summary>
	/// Sets the Accept media-type header value.  To accept multiple media types,
	/// this method can be called multiple times.
	/// </summary>
	/// <returns>Self reference for method chaining.</returns>
	/// <param name="value">The media-type value.</param>
	/// <param name="quality">The precedence of the value compared in relation to 
	/// other accept media-type values.</param>
	public RequestHeaders AcceptMediaType(string value, double? quality = null)
	{
		var acceptValues = Accept?.ToList() ?? new List<HeaderValue>();
		acceptValues.Add(HeaderValue.WithValue(value, quality));

		Accept = acceptValues.ToArray();
		return this;
	}

	/// <summary>
	/// Sets the content media-type value.
	/// </summary>
	/// <returns>Self reference for method chaining.</returns>
	/// <param name="value">The media-type value.</param>
	public RequestHeaders ContentMediaType(string value)
	{
		ContentType = HeaderValue.WithValue(value);
		return this;
	}

	/// <summary>
	/// Sets the Authorization header to the base64 encoded username and password.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="password">The password.</param>
	/// <returns>Self reference for method chaining.</returns>
	public RequestHeaders SetBasicAuthHeader(string username, string password)
	{
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var value = Base64Encode($"{username}:{password}");
		return Add("Authorization", $"Basic {value}");
	}

	/// <summary>
	/// Set the Authorization Bearer JWT token.
	/// </summary>
	/// <param name="token">The JWT token value.</param>
	/// <returns>Self reference for method changing.</returns>
	public RequestHeaders SetAuthBearerToken(string token)
	{
        ArgumentNullException.ThrowIfNull(token);

        return Add("Authorization", $"Bearer {token}");
	}

	private static string Base64Encode(string plainText) 
	{
		var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(plainTextBytes);
	}

	internal void ApplyHeaders(HttpRequestMessage requestMessage)
	{
		if (Accept != null)
		{
			foreach(HeaderValue acceptHeader in Accept)
			{
				string acceptType = acceptHeader.Value.First();

				var mediaTypeHeader = acceptHeader.Quality == null ? 
					new MediaTypeWithQualityHeaderValue(acceptType) : 
					new MediaTypeWithQualityHeaderValue(acceptType, acceptHeader.Quality.Value);

				requestMessage.Headers.Accept.Add(mediaTypeHeader);
			}
		}

		foreach (var (key, value) in Values)
		{
			requestMessage.Headers.Add(key, value.Value);
		}
	}

	internal RequestHeaders GetMerged(RequestHeaders requestSettings)
	{
		// Check if any of the known header values have been overridden.
		var mergedHeaders = new RequestHeaders
		{
			Accept = Accept ?? requestSettings.Accept ?? new[] { HeaderValue.WithValue (InternetMediaTypes.Json) },
			ContentType = ContentType ?? requestSettings.ContentType ?? HeaderValue.WithValue(InternetMediaTypes.Json)
		};
			
		foreach (var (key, value) in requestSettings.Values)
		{
			mergedHeaders._headers[key] = value;
		}

		// Override any passed headers having a corresponding value defined
		// by the settings object being merged into.
		foreach (var (key, value) in Values)
		{
			mergedHeaders._headers[key] = value;
		}

		return mergedHeaders;
	}
}