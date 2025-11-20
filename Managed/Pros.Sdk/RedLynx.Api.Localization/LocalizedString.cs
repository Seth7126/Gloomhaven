using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Localization;

public sealed class LocalizedString : IMessage<LocalizedString>, IMessage, IEquatable<LocalizedString>, IDeepCloneable<LocalizedString>, IBufferMessage
{
	private static readonly MessageParser<LocalizedString> _parser = new MessageParser<LocalizedString>(() => new LocalizedString());

	private UnknownFieldSet _unknownFields;

	public const int LanguageCodeFieldNumber = 1;

	private string languageCode_ = "";

	public const int CountryCodeFieldNumber = 2;

	private string countryCode_ = "";

	public const int TextFieldNumber = 3;

	private string text_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LocalizedString> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => LocalizedStringReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string LanguageCode
	{
		get
		{
			return languageCode_;
		}
		set
		{
			languageCode_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string CountryCode
	{
		get
		{
			return countryCode_;
		}
		set
		{
			countryCode_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Text
	{
		get
		{
			return text_;
		}
		set
		{
			text_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LocalizedString()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LocalizedString(LocalizedString other)
		: this()
	{
		languageCode_ = other.languageCode_;
		countryCode_ = other.countryCode_;
		text_ = other.text_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LocalizedString Clone()
	{
		return new LocalizedString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LocalizedString);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LocalizedString other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LanguageCode != other.LanguageCode)
		{
			return false;
		}
		if (CountryCode != other.CountryCode)
		{
			return false;
		}
		if (Text != other.Text)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (LanguageCode.Length != 0)
		{
			num ^= LanguageCode.GetHashCode();
		}
		if (CountryCode.Length != 0)
		{
			num ^= CountryCode.GetHashCode();
		}
		if (Text.Length != 0)
		{
			num ^= Text.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (LanguageCode.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(LanguageCode);
		}
		if (CountryCode.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(CountryCode);
		}
		if (Text.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Text);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (LanguageCode.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LanguageCode);
		}
		if (CountryCode.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CountryCode);
		}
		if (Text.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Text);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LocalizedString other)
	{
		if (other != null)
		{
			if (other.LanguageCode.Length != 0)
			{
				LanguageCode = other.LanguageCode;
			}
			if (other.CountryCode.Length != 0)
			{
				CountryCode = other.CountryCode;
			}
			if (other.Text.Length != 0)
			{
				Text = other.Text;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				LanguageCode = input.ReadString();
				break;
			case 18u:
				CountryCode = input.ReadString();
				break;
			case 26u:
				Text = input.ReadString();
				break;
			}
		}
	}
}
