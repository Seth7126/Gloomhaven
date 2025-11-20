using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.CrossSave;

public sealed class SaveSnapshot : IMessage<SaveSnapshot>, IMessage, IEquatable<SaveSnapshot>, IDeepCloneable<SaveSnapshot>, IBufferMessage
{
	private static readonly MessageParser<SaveSnapshot> _parser = new MessageParser<SaveSnapshot>(() => new SaveSnapshot());

	private UnknownFieldSet _unknownFields;

	public const int SavesFieldNumber = 1;

	private static readonly FieldCodec<SaveData> _repeated_saves_codec = FieldCodec.ForMessage(10u, SaveData.Parser);

	private readonly RepeatedField<SaveData> saves_ = new RepeatedField<SaveData>();

	public const int ContentTypeFieldNumber = 2;

	private string contentType_ = "";

	public const int DataDescriptionJsonFieldNumber = 3;

	private string dataDescriptionJson_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SaveSnapshot> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => CrossSaveContractsReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SaveData> Saves => saves_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ContentType
	{
		get
		{
			return contentType_;
		}
		set
		{
			contentType_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string DataDescriptionJson
	{
		get
		{
			return dataDescriptionJson_;
		}
		set
		{
			dataDescriptionJson_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SaveSnapshot()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SaveSnapshot(SaveSnapshot other)
		: this()
	{
		saves_ = other.saves_.Clone();
		contentType_ = other.contentType_;
		dataDescriptionJson_ = other.dataDescriptionJson_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SaveSnapshot Clone()
	{
		return new SaveSnapshot(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SaveSnapshot);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SaveSnapshot other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!saves_.Equals(other.saves_))
		{
			return false;
		}
		if (ContentType != other.ContentType)
		{
			return false;
		}
		if (DataDescriptionJson != other.DataDescriptionJson)
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
		num ^= saves_.GetHashCode();
		if (ContentType.Length != 0)
		{
			num ^= ContentType.GetHashCode();
		}
		if (DataDescriptionJson.Length != 0)
		{
			num ^= DataDescriptionJson.GetHashCode();
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
		saves_.WriteTo(ref output, _repeated_saves_codec);
		if (ContentType.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ContentType);
		}
		if (DataDescriptionJson.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(DataDescriptionJson);
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
		num += saves_.CalculateSize(_repeated_saves_codec);
		if (ContentType.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ContentType);
		}
		if (DataDescriptionJson.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DataDescriptionJson);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SaveSnapshot other)
	{
		if (other != null)
		{
			saves_.Add(other.saves_);
			if (other.ContentType.Length != 0)
			{
				ContentType = other.ContentType;
			}
			if (other.DataDescriptionJson.Length != 0)
			{
				DataDescriptionJson = other.DataDescriptionJson;
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
				saves_.AddEntriesFrom(ref input, _repeated_saves_codec);
				break;
			case 18u:
				ContentType = input.ReadString();
				break;
			case 26u:
				DataDescriptionJson = input.ReadString();
				break;
			}
		}
	}
}
