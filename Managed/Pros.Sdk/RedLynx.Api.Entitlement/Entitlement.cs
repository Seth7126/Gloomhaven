using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using RedLynx.Api.Localization;

namespace RedLynx.Api.Entitlement;

public sealed class Entitlement : IMessage<Entitlement>, IMessage, IEquatable<Entitlement>, IDeepCloneable<Entitlement>, IBufferMessage
{
	private static readonly MessageParser<Entitlement> _parser = new MessageParser<Entitlement>(() => new Entitlement());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private string id_ = "";

	public const int NameFieldNumber = 2;

	private string name_ = "";

	public const int UINameFieldNumber = 3;

	private static readonly FieldCodec<LocalizedString> _repeated_uIName_codec = FieldCodec.ForMessage(26u, LocalizedString.Parser);

	private readonly RepeatedField<LocalizedString> uIName_ = new RepeatedField<LocalizedString>();

	public const int UIDescFieldNumber = 4;

	private static readonly FieldCodec<LocalizedString> _repeated_uIDesc_codec = FieldCodec.ForMessage(34u, LocalizedString.Parser);

	private readonly RepeatedField<LocalizedString> uIDesc_ = new RepeatedField<LocalizedString>();

	public const int PictureUrlFieldNumber = 5;

	private string pictureUrl_ = "";

	public const int EntitlementTypeFieldNumber = 6;

	private EntitlementType entitlementType_;

	public const int EntitlementStatusFieldNumber = 7;

	private EntitlementStatus entitlementStatus_;

	public const int CreatedFieldNumber = 8;

	private Timestamp created_;

	public const int UpdatedFieldNumber = 9;

	private Timestamp updated_;

	public const int ConsumeLimitFieldNumber = 10;

	private int consumeLimit_;

	public const int ConsumeCountFieldNumber = 11;

	private int consumeCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<Entitlement> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => EntitlementContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LocalizedString> UIName => uIName_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LocalizedString> UIDesc => uIDesc_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string PictureUrl
	{
		get
		{
			return pictureUrl_;
		}
		set
		{
			pictureUrl_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EntitlementType EntitlementType
	{
		get
		{
			return entitlementType_;
		}
		set
		{
			entitlementType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EntitlementStatus EntitlementStatus
	{
		get
		{
			return entitlementStatus_;
		}
		set
		{
			entitlementStatus_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Timestamp Created
	{
		get
		{
			return created_;
		}
		set
		{
			created_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Timestamp Updated
	{
		get
		{
			return updated_;
		}
		set
		{
			updated_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ConsumeLimit
	{
		get
		{
			return consumeLimit_;
		}
		set
		{
			consumeLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ConsumeCount
	{
		get
		{
			return consumeCount_;
		}
		set
		{
			consumeCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Entitlement()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Entitlement(Entitlement other)
		: this()
	{
		id_ = other.id_;
		name_ = other.name_;
		uIName_ = other.uIName_.Clone();
		uIDesc_ = other.uIDesc_.Clone();
		pictureUrl_ = other.pictureUrl_;
		entitlementType_ = other.entitlementType_;
		entitlementStatus_ = other.entitlementStatus_;
		created_ = ((other.created_ != null) ? other.created_.Clone() : null);
		updated_ = ((other.updated_ != null) ? other.updated_.Clone() : null);
		consumeLimit_ = other.consumeLimit_;
		consumeCount_ = other.consumeCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Entitlement Clone()
	{
		return new Entitlement(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as Entitlement);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(Entitlement other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (!uIName_.Equals(other.uIName_))
		{
			return false;
		}
		if (!uIDesc_.Equals(other.uIDesc_))
		{
			return false;
		}
		if (PictureUrl != other.PictureUrl)
		{
			return false;
		}
		if (EntitlementType != other.EntitlementType)
		{
			return false;
		}
		if (EntitlementStatus != other.EntitlementStatus)
		{
			return false;
		}
		if (!object.Equals(Created, other.Created))
		{
			return false;
		}
		if (!object.Equals(Updated, other.Updated))
		{
			return false;
		}
		if (ConsumeLimit != other.ConsumeLimit)
		{
			return false;
		}
		if (ConsumeCount != other.ConsumeCount)
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
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
		num ^= uIName_.GetHashCode();
		num ^= uIDesc_.GetHashCode();
		if (PictureUrl.Length != 0)
		{
			num ^= PictureUrl.GetHashCode();
		}
		if (EntitlementType != EntitlementType.Unknown)
		{
			num ^= EntitlementType.GetHashCode();
		}
		if (EntitlementStatus != EntitlementStatus.Unknown)
		{
			num ^= EntitlementStatus.GetHashCode();
		}
		if (created_ != null)
		{
			num ^= Created.GetHashCode();
		}
		if (updated_ != null)
		{
			num ^= Updated.GetHashCode();
		}
		if (ConsumeLimit != 0)
		{
			num ^= ConsumeLimit.GetHashCode();
		}
		if (ConsumeCount != 0)
		{
			num ^= ConsumeCount.GetHashCode();
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
		if (Id.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Id);
		}
		if (Name.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Name);
		}
		uIName_.WriteTo(ref output, _repeated_uIName_codec);
		uIDesc_.WriteTo(ref output, _repeated_uIDesc_codec);
		if (PictureUrl.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(PictureUrl);
		}
		if (EntitlementType != EntitlementType.Unknown)
		{
			output.WriteRawTag(48);
			output.WriteEnum((int)EntitlementType);
		}
		if (EntitlementStatus != EntitlementStatus.Unknown)
		{
			output.WriteRawTag(56);
			output.WriteEnum((int)EntitlementStatus);
		}
		if (created_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(Created);
		}
		if (updated_ != null)
		{
			output.WriteRawTag(74);
			output.WriteMessage(Updated);
		}
		if (ConsumeLimit != 0)
		{
			output.WriteRawTag(80);
			output.WriteInt32(ConsumeLimit);
		}
		if (ConsumeCount != 0)
		{
			output.WriteRawTag(88);
			output.WriteInt32(ConsumeCount);
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
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Id);
		}
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		num += uIName_.CalculateSize(_repeated_uIName_codec);
		num += uIDesc_.CalculateSize(_repeated_uIDesc_codec);
		if (PictureUrl.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PictureUrl);
		}
		if (EntitlementType != EntitlementType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EntitlementType);
		}
		if (EntitlementStatus != EntitlementStatus.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EntitlementStatus);
		}
		if (created_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Created);
		}
		if (updated_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Updated);
		}
		if (ConsumeLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ConsumeLimit);
		}
		if (ConsumeCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ConsumeCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(Entitlement other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id.Length != 0)
		{
			Id = other.Id;
		}
		if (other.Name.Length != 0)
		{
			Name = other.Name;
		}
		uIName_.Add(other.uIName_);
		uIDesc_.Add(other.uIDesc_);
		if (other.PictureUrl.Length != 0)
		{
			PictureUrl = other.PictureUrl;
		}
		if (other.EntitlementType != EntitlementType.Unknown)
		{
			EntitlementType = other.EntitlementType;
		}
		if (other.EntitlementStatus != EntitlementStatus.Unknown)
		{
			EntitlementStatus = other.EntitlementStatus;
		}
		if (other.created_ != null)
		{
			if (created_ == null)
			{
				Created = new Timestamp();
			}
			Created.MergeFrom(other.Created);
		}
		if (other.updated_ != null)
		{
			if (updated_ == null)
			{
				Updated = new Timestamp();
			}
			Updated.MergeFrom(other.Updated);
		}
		if (other.ConsumeLimit != 0)
		{
			ConsumeLimit = other.ConsumeLimit;
		}
		if (other.ConsumeCount != 0)
		{
			ConsumeCount = other.ConsumeCount;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				Id = input.ReadString();
				break;
			case 18u:
				Name = input.ReadString();
				break;
			case 26u:
				uIName_.AddEntriesFrom(ref input, _repeated_uIName_codec);
				break;
			case 34u:
				uIDesc_.AddEntriesFrom(ref input, _repeated_uIDesc_codec);
				break;
			case 42u:
				PictureUrl = input.ReadString();
				break;
			case 48u:
				EntitlementType = (EntitlementType)input.ReadEnum();
				break;
			case 56u:
				EntitlementStatus = (EntitlementStatus)input.ReadEnum();
				break;
			case 66u:
				if (created_ == null)
				{
					Created = new Timestamp();
				}
				input.ReadMessage(Created);
				break;
			case 74u:
				if (updated_ == null)
				{
					Updated = new Timestamp();
				}
				input.ReadMessage(Updated);
				break;
			case 80u:
				ConsumeLimit = input.ReadInt32();
				break;
			case 88u:
				ConsumeCount = input.ReadInt32();
				break;
			}
		}
	}
}
