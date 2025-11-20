using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class PlatformDetails : IMessage<PlatformDetails>, IMessage, IEquatable<PlatformDetails>, IDeepCloneable<PlatformDetails>, IBufferMessage
{
	private static readonly MessageParser<PlatformDetails> _parser = new MessageParser<PlatformDetails>(() => new PlatformDetails());

	private UnknownFieldSet _unknownFields;

	public const int PlatformDescriptionFieldNumber = 1;

	private string platformDescription_ = "";

	public const int PlatformInternalIdFieldNumber = 2;

	private int platformInternalId_;

	[DebuggerNonUserCode]
	public static MessageParser<PlatformDetails> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string PlatformDescription
	{
		get
		{
			return platformDescription_;
		}
		set
		{
			platformDescription_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int PlatformInternalId
	{
		get
		{
			return platformInternalId_;
		}
		set
		{
			platformInternalId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PlatformDetails()
	{
	}

	[DebuggerNonUserCode]
	public PlatformDetails(PlatformDetails other)
		: this()
	{
		platformDescription_ = other.platformDescription_;
		platformInternalId_ = other.platformInternalId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PlatformDetails Clone()
	{
		return new PlatformDetails(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PlatformDetails);
	}

	[DebuggerNonUserCode]
	public bool Equals(PlatformDetails other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlatformDescription != other.PlatformDescription)
		{
			return false;
		}
		if (PlatformInternalId != other.PlatformInternalId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PlatformDescription.Length != 0)
		{
			num ^= PlatformDescription.GetHashCode();
		}
		if (PlatformInternalId != 0)
		{
			num ^= PlatformInternalId.GetHashCode();
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
		if (PlatformDescription.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PlatformDescription);
		}
		if (PlatformInternalId != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(PlatformInternalId);
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
		if (PlatformDescription.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PlatformDescription);
		}
		if (PlatformInternalId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlatformInternalId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PlatformDetails other)
	{
		if (other != null)
		{
			if (other.PlatformDescription.Length != 0)
			{
				PlatformDescription = other.PlatformDescription;
			}
			if (other.PlatformInternalId != 0)
			{
				PlatformInternalId = other.PlatformInternalId;
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
			case 10u:
				PlatformDescription = input.ReadString();
				break;
			case 16u:
				PlatformInternalId = input.ReadInt32();
				break;
			}
		}
	}
}
