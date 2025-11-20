using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Manatee.Json.Pointer;

namespace Manatee.Json.Schema;

public static class JsonSchemaOptions
{
	private interface IErrorCollectionCondition
	{
		bool ShouldExcludeChildErrors(IJsonSchemaKeyword keyword, SchemaValidationContext context);
	}

	private class LocationErrorCollectionCondition : IErrorCollectionCondition
	{
		public JsonPointer Location { get; private set; }

		public LocationErrorCollectionCondition(JsonPointer location)
		{
			Location = location;
		}

		public bool ShouldExcludeChildErrors(IJsonSchemaKeyword keyword, SchemaValidationContext context)
		{
			return context.RelativeLocation.IsChildOf(Location);
		}
	}

	private class KeywordErrorCollectionCondition : IErrorCollectionCondition
	{
		public Type Type { get; private set; }

		public KeywordErrorCollectionCondition(Type type)
		{
			Type = type;
		}

		public bool ShouldExcludeChildErrors(IJsonSchemaKeyword keyword, SchemaValidationContext context)
		{
			return Type.IsInstanceOfType(keyword);
		}
	}

	private static readonly List<IErrorCollectionCondition> _errorCollectionConditions;

	private static Func<string, string>? _download;

	public static Func<string, string> Download
	{
		get
		{
			return _BasicDownload;
		}
		set
		{
			_download = value;
		}
	}

	public static bool ValidateFormatKeyword { get; set; }

	public static bool AllowUnknownFormats { get; set; }

	public static SchemaValidationOutputFormat OutputFormat { get; set; }

	public static RefResolutionStrategy RefResolution { get; set; }

	public static Uri DefaultBaseUri { get; set; }

	internal static bool ConfigureForTestOutput { get; set; }

	static JsonSchemaOptions()
	{
		ValidateFormatKeyword = true;
		AllowUnknownFormats = true;
		OutputFormat = SchemaValidationOutputFormat.Flag;
		RefResolution = RefResolutionStrategy.ProcessSiblingId;
		DefaultBaseUri = new Uri("manatee://json-schema/", UriKind.Absolute);
		_errorCollectionConditions = new List<IErrorCollectionCondition>();
	}

	private static string _BasicDownload(string path)
	{
		Console.WriteLine(path);
		Uri uri = new Uri(path);
		switch (uri.Scheme)
		{
		case "http":
		case "https":
			return new HttpClient().GetStringAsync(uri).Result;
		case "file":
			return File.ReadAllText(Uri.UnescapeDataString(uri.AbsolutePath));
		case "manatee":
			return null;
		default:
			throw new Exception("URI scheme '" + uri.Scheme + "' is not supported.  Only HTTP(S) and local file system URIs are allowed.");
		}
	}

	public static void IgnoreErrorsForChildren<T>() where T : IJsonSchemaKeyword
	{
		_errorCollectionConditions.Add(new KeywordErrorCollectionCondition(typeof(T)));
	}

	public static void IgnoreErrorsForChildren(JsonPointer location)
	{
		_errorCollectionConditions.Add(new LocationErrorCollectionCondition(location));
	}

	public static bool ShouldReportChildErrors(IJsonSchemaKeyword keyword, SchemaValidationContext context)
	{
		return !_errorCollectionConditions.Any((IErrorCollectionCondition c) => c.ShouldExcludeChildErrors(keyword, context));
	}
}
