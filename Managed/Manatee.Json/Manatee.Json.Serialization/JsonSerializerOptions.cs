using System;
using Manatee.Json.Serialization.Internal;

namespace Manatee.Json.Serialization;

public class JsonSerializerOptions
{
	private static readonly IResolver _defaultResolver;

	private IResolver? _resolver;

	private Func<string, string>? _serializationNameTransform;

	private Func<string, string>? _deserializationNameTransform;

	public static readonly JsonSerializerOptions Default;

	public bool EncodeDefaultValues { get; set; }

	public InvalidPropertyKeyBehavior InvalidPropertyKeyBehavior { get; set; }

	public DateTimeSerializationFormat DateTimeSerializationFormat { get; set; }

	public string? CustomDateTimeSerializationFormat { get; set; }

	public EnumSerializationFormat EnumSerializationFormat { get; set; }

	public string FlagsEnumSeparator { get; set; } = ",";

	public bool CaseSensitiveDeserialization { get; set; }

	public TypeNameSerializationBehavior TypeNameSerializationBehavior { get; set; }

	public PropertySelectionStrategy PropertySelectionStrategy { get; set; }

	public IResolver Resolver
	{
		get
		{
			return _resolver ?? (_resolver = _defaultResolver);
		}
		set
		{
			_resolver = value;
		}
	}

	public bool AutoSerializeFields { get; set; }

	public Func<string, string> SerializationNameTransform
	{
		get
		{
			return (string s) => s;
		}
		set
		{
			_serializationNameTransform = value;
		}
	}

	public Func<string, string> DeserializationNameTransform
	{
		get
		{
			return (string s) => s;
		}
		set
		{
			_deserializationNameTransform = value;
		}
	}

	public bool OnlyExplicitProperties { get; set; }

	internal bool IncludeContentSample { get; set; }

	static JsonSerializerOptions()
	{
		_defaultResolver = new ConstructorResolver();
		Default = new JsonSerializerOptions
		{
			EncodeDefaultValues = false,
			InvalidPropertyKeyBehavior = InvalidPropertyKeyBehavior.DoNothing,
			EnumSerializationFormat = EnumSerializationFormat.AsName
		};
	}

	public JsonSerializerOptions()
	{
		PropertySelectionStrategy = PropertySelectionStrategy.ReadWriteOnly;
		TypeNameSerializationBehavior = TypeNameSerializationBehavior.OnlyForAbstractions;
	}

	public JsonSerializerOptions(JsonSerializerOptions options)
		: this()
	{
		EncodeDefaultValues = options.EncodeDefaultValues;
		InvalidPropertyKeyBehavior = options.InvalidPropertyKeyBehavior;
		DateTimeSerializationFormat = options.DateTimeSerializationFormat;
		CustomDateTimeSerializationFormat = options.CustomDateTimeSerializationFormat;
		EnumSerializationFormat = options.EnumSerializationFormat;
		FlagsEnumSeparator = options.FlagsEnumSeparator;
		CaseSensitiveDeserialization = options.CaseSensitiveDeserialization;
		TypeNameSerializationBehavior = options.TypeNameSerializationBehavior;
		PropertySelectionStrategy = options.PropertySelectionStrategy;
		Resolver = options.Resolver;
		AutoSerializeFields = options.AutoSerializeFields;
		SerializationNameTransform = options.SerializationNameTransform;
		DeserializationNameTransform = options.DeserializationNameTransform;
		OnlyExplicitProperties = options.OnlyExplicitProperties;
		IncludeContentSample = options.IncludeContentSample;
	}
}
