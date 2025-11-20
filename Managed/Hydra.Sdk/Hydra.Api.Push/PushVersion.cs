using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public sealed class PushVersion : IMessage<PushVersion>, IMessage, IEquatable<PushVersion>, IDeepCloneable<PushVersion>, IBufferMessage
{
	private static readonly MessageParser<PushVersion> _parser = new MessageParser<PushVersion>(() => new PushVersion());

	private UnknownFieldSet _unknownFields;

	public const int VersionTypeFieldNumber = 1;

	private PushMessageType versionType_ = PushMessageType.Undefined;

	public const int VersionFieldNumber = 2;

	private int version_;

	[DebuggerNonUserCode]
	public static MessageParser<PushVersion> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushVersionReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PushMessageType VersionType
	{
		get
		{
			return versionType_;
		}
		set
		{
			versionType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PushVersion()
	{
	}

	[DebuggerNonUserCode]
	public PushVersion(PushVersion other)
		: this()
	{
		versionType_ = other.versionType_;
		version_ = other.version_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushVersion Clone()
	{
		return new PushVersion(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushVersion);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushVersion other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (VersionType != other.VersionType)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (VersionType != PushMessageType.Undefined)
		{
			num ^= VersionType.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
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
		if (VersionType != PushMessageType.Undefined)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)VersionType);
		}
		if (Version != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Version);
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
		if (VersionType != PushMessageType.Undefined)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)VersionType);
		}
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Version);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushVersion other)
	{
		if (other != null)
		{
			if (other.VersionType != PushMessageType.Undefined)
			{
				VersionType = other.VersionType;
			}
			if (other.Version != 0)
			{
				Version = other.Version;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
			case 8u:
				VersionType = (PushMessageType)input.ReadEnum();
				break;
			case 16u:
				Version = input.ReadInt32();
				break;
			}
		}
	}
}
