using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class InviteData : IMessage<InviteData>, IMessage, IEquatable<InviteData>, IDeepCloneable<InviteData>, IBufferMessage
{
	private static readonly MessageParser<InviteData> _parser = new MessageParser<InviteData>(() => new InviteData());

	private UnknownFieldSet _unknownFields;

	public const int InviteIdFieldNumber = 1;

	private string inviteId_ = "";

	public const int PartyIdFieldNumber = 2;

	private string partyId_ = "";

	public const int UserIdFromFieldNumber = 3;

	private string userIdFrom_ = "";

	public const int UserIdToFieldNumber = 4;

	private string userIdTo_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<InviteData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => InviteDataReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string InviteId
	{
		get
		{
			return inviteId_;
		}
		set
		{
			inviteId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string PartyId
	{
		get
		{
			return partyId_;
		}
		set
		{
			partyId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string UserIdFrom
	{
		get
		{
			return userIdFrom_;
		}
		set
		{
			userIdFrom_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string UserIdTo
	{
		get
		{
			return userIdTo_;
		}
		set
		{
			userIdTo_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public InviteData()
	{
	}

	[DebuggerNonUserCode]
	public InviteData(InviteData other)
		: this()
	{
		inviteId_ = other.inviteId_;
		partyId_ = other.partyId_;
		userIdFrom_ = other.userIdFrom_;
		userIdTo_ = other.userIdTo_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public InviteData Clone()
	{
		return new InviteData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as InviteData);
	}

	[DebuggerNonUserCode]
	public bool Equals(InviteData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (InviteId != other.InviteId)
		{
			return false;
		}
		if (PartyId != other.PartyId)
		{
			return false;
		}
		if (UserIdFrom != other.UserIdFrom)
		{
			return false;
		}
		if (UserIdTo != other.UserIdTo)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (InviteId.Length != 0)
		{
			num ^= InviteId.GetHashCode();
		}
		if (PartyId.Length != 0)
		{
			num ^= PartyId.GetHashCode();
		}
		if (UserIdFrom.Length != 0)
		{
			num ^= UserIdFrom.GetHashCode();
		}
		if (UserIdTo.Length != 0)
		{
			num ^= UserIdTo.GetHashCode();
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
		if (InviteId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(InviteId);
		}
		if (PartyId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(PartyId);
		}
		if (UserIdFrom.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(UserIdFrom);
		}
		if (UserIdTo.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(UserIdTo);
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
		if (InviteId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(InviteId);
		}
		if (PartyId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PartyId);
		}
		if (UserIdFrom.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserIdFrom);
		}
		if (UserIdTo.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserIdTo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(InviteData other)
	{
		if (other != null)
		{
			if (other.InviteId.Length != 0)
			{
				InviteId = other.InviteId;
			}
			if (other.PartyId.Length != 0)
			{
				PartyId = other.PartyId;
			}
			if (other.UserIdFrom.Length != 0)
			{
				UserIdFrom = other.UserIdFrom;
			}
			if (other.UserIdTo.Length != 0)
			{
				UserIdTo = other.UserIdTo;
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
				InviteId = input.ReadString();
				break;
			case 18u:
				PartyId = input.ReadString();
				break;
			case 26u:
				UserIdFrom = input.ReadString();
				break;
			case 34u:
				UserIdTo = input.ReadString();
				break;
			}
		}
	}
}
