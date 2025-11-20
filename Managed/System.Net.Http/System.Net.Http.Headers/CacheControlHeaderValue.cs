using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers;

/// <summary>Represents the value of the Cache-Control header.</summary>
public class CacheControlHeaderValue : ICloneable
{
	private List<NameValueHeaderValue> extensions;

	private List<string> no_cache_headers;

	private List<string> private_headers;

	/// <summary>Cache-extension tokens, each with an optional assigned value.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.A collection of cache-extension tokens each with an optional assigned value.</returns>
	public ICollection<NameValueHeaderValue> Extensions => extensions ?? (extensions = new List<NameValueHeaderValue>());

	/// <summary>The maximum age, specified in seconds, that the HTTP client is willing to accept a response. </summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time in seconds. </returns>
	public TimeSpan? MaxAge { get; set; }

	/// <summary>Whether an HTTP client is willing to accept a response that has exceeded its expiration time.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the HTTP client is willing to accept a response that has exceed the expiration time; otherwise, false.</returns>
	public bool MaxStale { get; set; }

	/// <summary>The maximum time, in seconds, an HTTP client is willing to accept a response that has exceeded its expiration time.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time in seconds.</returns>
	public TimeSpan? MaxStaleLimit { get; set; }

	/// <summary>The freshness lifetime, in seconds, that an HTTP client is willing to accept a response.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time in seconds.</returns>
	public TimeSpan? MinFresh { get; set; }

	/// <summary>Whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the origin server requires revalidation of a cache entry on any subsequent use when the entry becomes stale; otherwise, false.</returns>
	public bool MustRevalidate { get; set; }

	/// <summary>Whether an HTTP client is willing to accept a cached response.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the HTTP client is willing to accept a cached response; otherwise, false.</returns>
	public bool NoCache { get; set; }

	/// <summary>A collection of fieldnames in the "no-cache" directive in a cache-control header field on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.A collection of fieldnames.</returns>
	public ICollection<string> NoCacheHeaders => no_cache_headers ?? (no_cache_headers = new List<string>());

	/// <summary>Whether a cache must not store any part of either the HTTP request mressage or any response.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if a cache must not store any part of either the HTTP request mressage or any response; otherwise, false.</returns>
	public bool NoStore { get; set; }

	/// <summary>Whether a cache or proxy must not change any aspect of the entity-body.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if a cache or proxy must not change any aspect of the entity-body; otherwise, false.</returns>
	public bool NoTransform { get; set; }

	/// <summary>Whether a cache should either respond using a cached entry that is consistent with the other constraints of the HTTP request, or respond with a 504 (Gateway Timeout) status.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if a cache should either respond using a cached entry that is consistent with the other constraints of the HTTP request, or respond with a 504 (Gateway Timeout) status; otherwise, false.</returns>
	public bool OnlyIfCached { get; set; }

	/// <summary>Whether all or part of the HTTP response message is intended for a single user and must not be cached by a shared cache.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the HTTP response message is intended for a single user and must not be cached by a shared cache; otherwise, false.</returns>
	public bool Private { get; set; }

	/// <summary>A collection fieldnames in the "private" directive in a cache-control header field on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.A collection of fieldnames.</returns>
	public ICollection<string> PrivateHeaders => private_headers ?? (private_headers = new List<string>());

	/// <summary>Whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale for shared user agent caches.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the origin server requires revalidation of a cache entry on any subsequent use when the entry becomes stale for shared user agent caches; otherwise, false.</returns>
	public bool ProxyRevalidate { get; set; }

	/// <summary>Whether an HTTP response may be cached by any cache, even if it would normally be non-cacheable or cacheable only within a non- shared cache.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the HTTP response may be cached by any cache, even if it would normally be non-cacheable or cacheable only within a non- shared cache; otherwise, false.</returns>
	public bool Public { get; set; }

	/// <summary>The shared maximum age, specified in seconds, in an HTTP response that overrides the "max-age" directive in a cache-control header or an Expires header for a shared cache.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time in seconds.</returns>
	public TimeSpan? SharedMaxAge { get; set; }

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		CacheControlHeaderValue cacheControlHeaderValue = (CacheControlHeaderValue)MemberwiseClone();
		if (extensions != null)
		{
			cacheControlHeaderValue.extensions = new List<NameValueHeaderValue>();
			foreach (NameValueHeaderValue extension in extensions)
			{
				cacheControlHeaderValue.extensions.Add(extension);
			}
		}
		if (no_cache_headers != null)
		{
			cacheControlHeaderValue.no_cache_headers = new List<string>();
			foreach (string no_cache_header in no_cache_headers)
			{
				cacheControlHeaderValue.no_cache_headers.Add(no_cache_header);
			}
		}
		if (private_headers != null)
		{
			cacheControlHeaderValue.private_headers = new List<string>();
			foreach (string private_header in private_headers)
			{
				cacheControlHeaderValue.private_headers.Add(private_header);
			}
		}
		return cacheControlHeaderValue;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is CacheControlHeaderValue cacheControlHeaderValue))
		{
			return false;
		}
		TimeSpan? maxAge = MaxAge;
		TimeSpan? maxAge2 = cacheControlHeaderValue.MaxAge;
		if (maxAge.HasValue == maxAge2.HasValue && (!maxAge.HasValue || !(maxAge.GetValueOrDefault() != maxAge2.GetValueOrDefault())) && MaxStale == cacheControlHeaderValue.MaxStale && !(MaxStaleLimit != cacheControlHeaderValue.MaxStaleLimit))
		{
			maxAge = MinFresh;
			maxAge2 = cacheControlHeaderValue.MinFresh;
			if (maxAge.HasValue == maxAge2.HasValue && (!maxAge.HasValue || !(maxAge.GetValueOrDefault() != maxAge2.GetValueOrDefault())) && MustRevalidate == cacheControlHeaderValue.MustRevalidate && NoCache == cacheControlHeaderValue.NoCache && NoStore == cacheControlHeaderValue.NoStore && NoTransform == cacheControlHeaderValue.NoTransform && OnlyIfCached == cacheControlHeaderValue.OnlyIfCached && Private == cacheControlHeaderValue.Private && ProxyRevalidate == cacheControlHeaderValue.ProxyRevalidate && Public == cacheControlHeaderValue.Public && !(SharedMaxAge != cacheControlHeaderValue.SharedMaxAge))
			{
				if (extensions.SequenceEqual(cacheControlHeaderValue.extensions) && no_cache_headers.SequenceEqual(cacheControlHeaderValue.no_cache_headers))
				{
					return private_headers.SequenceEqual(cacheControlHeaderValue.private_headers);
				}
				return false;
			}
		}
		return false;
	}

	/// <summary>Serves as a hash function for a  <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (((((((((((((((29 * 29 + HashCodeCalculator.Calculate(extensions)) * 29 + MaxAge.GetHashCode()) * 29 + MaxStale.GetHashCode()) * 29 + MaxStaleLimit.GetHashCode()) * 29 + MinFresh.GetHashCode()) * 29 + MustRevalidate.GetHashCode()) * 29 + HashCodeCalculator.Calculate(no_cache_headers)) * 29 + NoCache.GetHashCode()) * 29 + NoStore.GetHashCode()) * 29 + NoTransform.GetHashCode()) * 29 + OnlyIfCached.GetHashCode()) * 29 + Private.GetHashCode()) * 29 + HashCodeCalculator.Calculate(private_headers)) * 29 + ProxyRevalidate.GetHashCode()) * 29 + Public.GetHashCode()) * 29 + SharedMaxAge.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" />.A <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents cache-control header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid cache-control header value information.</exception>
	public static CacheControlHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out CacheControlHeaderValue parsedValue)
	{
		parsedValue = null;
		if (input == null)
		{
			return true;
		}
		CacheControlHeaderValue cacheControlHeaderValue = new CacheControlHeaderValue();
		Lexer lexer = new Lexer(input);
		Token token;
		do
		{
			token = lexer.Scan();
			if ((Token.Type)token != Token.Type.Token)
			{
				return false;
			}
			string stringValue = lexer.GetStringValue(token);
			bool flag = false;
			switch (stringValue)
			{
			case "no-store":
				cacheControlHeaderValue.NoStore = true;
				break;
			case "no-transform":
				cacheControlHeaderValue.NoTransform = true;
				break;
			case "only-if-cached":
				cacheControlHeaderValue.OnlyIfCached = true;
				break;
			case "public":
				cacheControlHeaderValue.Public = true;
				break;
			case "must-revalidate":
				cacheControlHeaderValue.MustRevalidate = true;
				break;
			case "proxy-revalidate":
				cacheControlHeaderValue.ProxyRevalidate = true;
				break;
			case "max-stale":
			{
				cacheControlHeaderValue.MaxStale = true;
				token = lexer.Scan();
				if ((Token.Type)token != Token.Type.SeparatorEqual)
				{
					flag = true;
					break;
				}
				token = lexer.Scan();
				if ((Token.Type)token != Token.Type.Token)
				{
					return false;
				}
				TimeSpan? maxStaleLimit = lexer.TryGetTimeSpanValue(token);
				if (!maxStaleLimit.HasValue)
				{
					return false;
				}
				cacheControlHeaderValue.MaxStaleLimit = maxStaleLimit;
				break;
			}
			case "max-age":
			case "s-maxage":
			case "min-fresh":
			{
				token = lexer.Scan();
				if ((Token.Type)token != Token.Type.SeparatorEqual)
				{
					return false;
				}
				token = lexer.Scan();
				if ((Token.Type)token != Token.Type.Token)
				{
					return false;
				}
				TimeSpan? maxStaleLimit = lexer.TryGetTimeSpanValue(token);
				if (!maxStaleLimit.HasValue)
				{
					return false;
				}
				switch (stringValue.Length)
				{
				case 7:
					cacheControlHeaderValue.MaxAge = maxStaleLimit;
					break;
				case 8:
					cacheControlHeaderValue.SharedMaxAge = maxStaleLimit;
					break;
				default:
					cacheControlHeaderValue.MinFresh = maxStaleLimit;
					break;
				}
				break;
			}
			case "private":
			case "no-cache":
			{
				if (stringValue.Length == 7)
				{
					cacheControlHeaderValue.Private = true;
				}
				else
				{
					cacheControlHeaderValue.NoCache = true;
				}
				token = lexer.Scan();
				if ((Token.Type)token != Token.Type.SeparatorEqual)
				{
					flag = true;
					break;
				}
				token = lexer.Scan();
				if ((Token.Type)token != Token.Type.QuotedString)
				{
					return false;
				}
				string[] array = lexer.GetQuotedStringValue(token).Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					string item = array[i].Trim('\t', ' ');
					if (stringValue.Length == 7)
					{
						cacheControlHeaderValue.PrivateHeaders.Add(item);
						continue;
					}
					cacheControlHeaderValue.NoCache = true;
					cacheControlHeaderValue.NoCacheHeaders.Add(item);
				}
				break;
			}
			default:
			{
				string stringValue2 = lexer.GetStringValue(token);
				string value = null;
				token = lexer.Scan();
				if ((Token.Type)token == Token.Type.SeparatorEqual)
				{
					token = lexer.Scan();
					Token.Type kind = token.Kind;
					if ((uint)(kind - 2) > 1u)
					{
						return false;
					}
					value = lexer.GetStringValue(token);
				}
				else
				{
					flag = true;
				}
				cacheControlHeaderValue.Extensions.Add(NameValueHeaderValue.Create(stringValue2, value));
				break;
			}
			}
			if (!flag)
			{
				token = lexer.Scan();
			}
		}
		while ((Token.Type)token == Token.Type.SeparatorComma);
		if ((Token.Type)token != Token.Type.End)
		{
			return false;
		}
		parsedValue = cacheControlHeaderValue;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (NoStore)
		{
			stringBuilder.Append("no-store");
			stringBuilder.Append(", ");
		}
		if (NoTransform)
		{
			stringBuilder.Append("no-transform");
			stringBuilder.Append(", ");
		}
		if (OnlyIfCached)
		{
			stringBuilder.Append("only-if-cached");
			stringBuilder.Append(", ");
		}
		if (Public)
		{
			stringBuilder.Append("public");
			stringBuilder.Append(", ");
		}
		if (MustRevalidate)
		{
			stringBuilder.Append("must-revalidate");
			stringBuilder.Append(", ");
		}
		if (ProxyRevalidate)
		{
			stringBuilder.Append("proxy-revalidate");
			stringBuilder.Append(", ");
		}
		if (NoCache)
		{
			stringBuilder.Append("no-cache");
			if (no_cache_headers != null)
			{
				stringBuilder.Append("=\"");
				no_cache_headers.ToStringBuilder(stringBuilder);
				stringBuilder.Append("\"");
			}
			stringBuilder.Append(", ");
		}
		if (MaxAge.HasValue)
		{
			stringBuilder.Append("max-age=");
			stringBuilder.Append(MaxAge.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append(", ");
		}
		if (SharedMaxAge.HasValue)
		{
			stringBuilder.Append("s-maxage=");
			stringBuilder.Append(SharedMaxAge.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append(", ");
		}
		if (MaxStale)
		{
			stringBuilder.Append("max-stale");
			if (MaxStaleLimit.HasValue)
			{
				stringBuilder.Append("=");
				stringBuilder.Append(MaxStaleLimit.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			}
			stringBuilder.Append(", ");
		}
		if (MinFresh.HasValue)
		{
			stringBuilder.Append("min-fresh=");
			stringBuilder.Append(MinFresh.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append(", ");
		}
		if (Private)
		{
			stringBuilder.Append("private");
			if (private_headers != null)
			{
				stringBuilder.Append("=\"");
				private_headers.ToStringBuilder(stringBuilder);
				stringBuilder.Append("\"");
			}
			stringBuilder.Append(", ");
		}
		extensions.ToStringBuilder(stringBuilder);
		if (stringBuilder.Length > 2 && stringBuilder[stringBuilder.Length - 2] == ',' && stringBuilder[stringBuilder.Length - 1] == ' ')
		{
			stringBuilder.Remove(stringBuilder.Length - 2, 2);
		}
		return stringBuilder.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.CacheControlHeaderValue" /> class.</summary>
	public CacheControlHeaderValue()
	{
	}
}
