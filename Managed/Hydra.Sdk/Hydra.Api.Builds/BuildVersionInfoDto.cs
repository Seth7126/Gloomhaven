using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Builds;

public sealed class BuildVersionInfoDto : IMessage<BuildVersionInfoDto>, IMessage, IEquatable<BuildVersionInfoDto>, IDeepCloneable<BuildVersionInfoDto>, IBufferMessage
{
	private static readonly MessageParser<BuildVersionInfoDto> _parser = new MessageParser<BuildVersionInfoDto>(() => new BuildVersionInfoDto());

	private UnknownFieldSet _unknownFields;

	public const int BuildVersionFieldNumber = 1;

	private string buildVersion_ = "";

	public const int AttributesFieldNumber = 2;

	private static readonly MapField<string, string>.Codec _map_attributes_codec = new MapField<string, string>.Codec(FieldCodec.ForString(10u, ""), FieldCodec.ForString(18u, ""), 18u);

	private readonly MapField<string, string> attributes_ = new MapField<string, string>();

	public const int PinnedFieldNumber = 3;

	private bool pinned_;

	public const int CreatedTimeFieldNumber = 4;

	private Timestamp createdTime_;

	public const int IdFieldNumber = 5;

	private string id_ = "";

	public const int PackIdFieldNumber = 6;

	private string packId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<BuildVersionInfoDto> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildsGroupContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string BuildVersion
	{
		get
		{
			return buildVersion_;
		}
		set
		{
			buildVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public MapField<string, string> Attributes => attributes_;

	[DebuggerNonUserCode]
	public bool Pinned
	{
		get
		{
			return pinned_;
		}
		set
		{
			pinned_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Timestamp CreatedTime
	{
		get
		{
			return createdTime_;
		}
		set
		{
			createdTime_ = value;
		}
	}

	[DebuggerNonUserCode]
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
	public string PackId
	{
		get
		{
			return packId_;
		}
		set
		{
			packId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public BuildVersionInfoDto()
	{
	}

	[DebuggerNonUserCode]
	public BuildVersionInfoDto(BuildVersionInfoDto other)
		: this()
	{
		buildVersion_ = other.buildVersion_;
		attributes_ = other.attributes_.Clone();
		pinned_ = other.pinned_;
		createdTime_ = ((other.createdTime_ != null) ? other.createdTime_.Clone() : null);
		id_ = other.id_;
		packId_ = other.packId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public BuildVersionInfoDto Clone()
	{
		return new BuildVersionInfoDto(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as BuildVersionInfoDto);
	}

	[DebuggerNonUserCode]
	public bool Equals(BuildVersionInfoDto other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BuildVersion != other.BuildVersion)
		{
			return false;
		}
		if (!Attributes.Equals(other.Attributes))
		{
			return false;
		}
		if (Pinned != other.Pinned)
		{
			return false;
		}
		if (!object.Equals(CreatedTime, other.CreatedTime))
		{
			return false;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (PackId != other.PackId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (BuildVersion.Length != 0)
		{
			num ^= BuildVersion.GetHashCode();
		}
		num ^= Attributes.GetHashCode();
		if (Pinned)
		{
			num ^= Pinned.GetHashCode();
		}
		if (createdTime_ != null)
		{
			num ^= CreatedTime.GetHashCode();
		}
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (PackId.Length != 0)
		{
			num ^= PackId.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (BuildVersion.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(BuildVersion);
		}
		attributes_.WriteTo(ref output, _map_attributes_codec);
		if (Pinned)
		{
			output.WriteRawTag(24);
			output.WriteBool(Pinned);
		}
		if (createdTime_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(CreatedTime);
		}
		if (Id.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(Id);
		}
		if (PackId.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(PackId);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (BuildVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(BuildVersion);
		}
		num += attributes_.CalculateSize(_map_attributes_codec);
		if (Pinned)
		{
			num += 2;
		}
		if (createdTime_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CreatedTime);
		}
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Id);
		}
		if (PackId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PackId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(BuildVersionInfoDto other)
	{
		if (other == null)
		{
			return;
		}
		if (other.BuildVersion.Length != 0)
		{
			BuildVersion = other.BuildVersion;
		}
		attributes_.Add(other.attributes_);
		if (other.Pinned)
		{
			Pinned = other.Pinned;
		}
		if (other.createdTime_ != null)
		{
			if (createdTime_ == null)
			{
				CreatedTime = new Timestamp();
			}
			CreatedTime.MergeFrom(other.CreatedTime);
		}
		if (other.Id.Length != 0)
		{
			Id = other.Id;
		}
		if (other.PackId.Length != 0)
		{
			PackId = other.PackId;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
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
				BuildVersion = input.ReadString();
				break;
			case 18u:
				attributes_.AddEntriesFrom(ref input, _map_attributes_codec);
				break;
			case 24u:
				Pinned = input.ReadBool();
				break;
			case 34u:
				if (createdTime_ == null)
				{
					CreatedTime = new Timestamp();
				}
				input.ReadMessage(CreatedTime);
				break;
			case 42u:
				Id = input.ReadString();
				break;
			case 50u:
				PackId = input.ReadString();
				break;
			}
		}
	}
}
