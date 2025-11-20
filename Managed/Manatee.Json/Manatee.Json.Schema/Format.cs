using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Manatee.Json.Schema;

public class Format
{
	private static readonly Dictionary<string, Format> _lookup;

	private static readonly string[] _dateTimeFormats;

	public static Format Date { get; set; }

	public static Format DateTime { get; set; }

	public static Format Duration { get; set; }

	public static Format Email { get; set; }

	public static Format HostName { get; set; }

	public static Format Ipv4 { get; set; }

	public static Format Ipv6 { get; set; }

	public static Format Iri { get; set; }

	public static Format IriReference { get; set; }

	public static Format JsonPointer { get; set; }

	public static Format Regex { get; set; }

	public static Format RelativeJsonPointer { get; set; }

	public static Format Uri { get; set; }

	public static Format UriReference { get; set; }

	public static Format UriTemplate { get; set; }

	public static Format Uuid { get; set; }

	public string Key { get; }

	public JsonSchemaVersion SupportedBy { get; }

	public Regex? ValidationRegex { get; }

	public Func<JsonValue, bool>? ValidationFunction { get; }

	internal bool IsKnown { get; private set; } = true;

	static Format()
	{
		_dateTimeFormats = new string[9] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fK", "yyyy'-'MM'-'dd'T'HH':'mm':'ssK", "yyyy'-'MM'-'dd'T'HH':'mm':'ss" };
		_lookup = new Dictionary<string, Format>();
		Date = new Format("date", JsonSchemaVersion.Draft2019_09, "^(\\d{4})-(\\d{2})-(\\d{2})$");
		DateTime = new Format("date-time", JsonSchemaVersion.All, (JsonValue s) => s.Type != JsonValueType.String || DateTimeOffset.TryParseExact(s.String, _dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var _));
		Duration = new Format("duration", JsonSchemaVersion.Draft2019_09, "^(-?)P(?=\\d|T\\d)(?:(\\d+)Y)?(?:(\\d+)M)?(?:(\\d+)([DW]))?(?:T(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+(?:\\.\\d+)?)S)?)?$");
		Email = new Format("email", JsonSchemaVersion.All, "^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])$");
		HostName = new Format("hostname", JsonSchemaVersion.All, "^(?!.{255,})([a-zA-Z0-9-]{0,63}\\.)*([a-zA-Z0-9-]{0,63})$");
		Ipv4 = new Format("ipv4", JsonSchemaVersion.All, "^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
		Ipv6 = new Format("ipv6", JsonSchemaVersion.All, "^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]).){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]).){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$");
		IriReference = new Format("iri-reference", JsonSchemaVersion.Draft2019_09, "^(([^:/?#]+):)?(//([^/?#]*))?([^?#]*)(\\?([^#]*))?(#(.*))?");
		Iri = new Format("iri", JsonSchemaVersion.Draft2019_09, (JsonValue s) => s.Type != JsonValueType.String || System.Uri.IsWellFormedUriString(s.String, UriKind.RelativeOrAbsolute));
		JsonPointer = new Format("json-pointer", JsonSchemaVersion.Draft2019_09, "^(/(([^/~])|(~[01]))+)*/?$");
		Regex = new Format("regex", JsonSchemaVersion.All, string.Empty, isCaseSensitive: true);
		RelativeJsonPointer = new Format("relative-json-pointer", JsonSchemaVersion.Draft2019_09, "^[0-9]+#/(([^/~])|(~[01]))*$");
		UriReference = new Format("uri-reference", JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09, "^(([^:/?#]+):)?(//([^/?#]*))?([^?#]*)(\\?([^#]*))?(#(.*))?");
		UriTemplate = new Format("uri-template", JsonSchemaVersion.Draft2019_09, "^$");
		Uri = new Format("uri", JsonSchemaVersion.All, (JsonValue s) => s.Type != JsonValueType.String || System.Uri.IsWellFormedUriString(s.String, UriKind.RelativeOrAbsolute));
		Uuid = new Format("uuid", JsonSchemaVersion.Draft2019_09, "[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}");
	}

	public Format(string key, JsonSchemaVersion supportedBy, [RegexPattern] string regex, bool isCaseSensitive = false)
	{
		Key = key;
		SupportedBy = supportedBy;
		if (regex != null)
		{
			ValidationRegex = new Regex(regex, (!isCaseSensitive) ? RegexOptions.IgnoreCase : RegexOptions.None);
		}
		_lookup[key] = this;
	}

	public Format(string key, JsonSchemaVersion supportedBy, Func<JsonValue, bool> validate)
	{
		ValidationFunction = validate;
		Key = key;
		SupportedBy = supportedBy;
		_lookup[key] = this;
	}

	public bool Validate(JsonValue value)
	{
		if (ValidationRegex != null && value.Type == JsonValueType.String)
		{
			return ValidationRegex.IsMatch(value.String);
		}
		if (ValidationFunction != null)
		{
			return ValidationFunction(value);
		}
		return true;
	}

	public static Format GetFormat(string formatKey)
	{
		if (formatKey == null)
		{
			throw new ArgumentNullException("formatKey");
		}
		if (!_lookup.TryGetValue(formatKey, out Format value))
		{
			return new Format(formatKey, JsonSchemaVersion.None, (JsonValue s) => JsonSchemaOptions.AllowUnknownFormats)
			{
				IsKnown = false
			};
		}
		return value;
	}
}
