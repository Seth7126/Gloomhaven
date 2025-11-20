using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class PresencePartyMemberUpdate : IMessage<PresencePartyMemberUpdate>, IMessage, IEquatable<PresencePartyMemberUpdate>, IDeepCloneable<PresencePartyMemberUpdate>, IBufferMessage
{
	private static readonly MessageParser<PresencePartyMemberUpdate> _parser = new MessageParser<PresencePartyMemberUpdate>(() => new PresencePartyMemberUpdate());

	private UnknownFieldSet _unknownFields;

	public const int UpdateTypeFieldNumber = 1;

	private PresencePartyMemberUpdateType updateType_ = PresencePartyMemberUpdateType.None;

	public const int MemberFieldNumber = 2;

	private PartyMember member_;

	[DebuggerNonUserCode]
	public static MessageParser<PresencePartyMemberUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PresencePartyMemberUpdateType UpdateType
	{
		get
		{
			return updateType_;
		}
		set
		{
			updateType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartyMember Member
	{
		get
		{
			return member_;
		}
		set
		{
			member_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresencePartyMemberUpdate()
	{
	}

	[DebuggerNonUserCode]
	public PresencePartyMemberUpdate(PresencePartyMemberUpdate other)
		: this()
	{
		updateType_ = other.updateType_;
		member_ = ((other.member_ != null) ? other.member_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresencePartyMemberUpdate Clone()
	{
		return new PresencePartyMemberUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresencePartyMemberUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresencePartyMemberUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UpdateType != other.UpdateType)
		{
			return false;
		}
		if (!object.Equals(Member, other.Member))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UpdateType != PresencePartyMemberUpdateType.None)
		{
			num ^= UpdateType.GetHashCode();
		}
		if (member_ != null)
		{
			num ^= Member.GetHashCode();
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
		if (UpdateType != PresencePartyMemberUpdateType.None)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)UpdateType);
		}
		if (member_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Member);
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
		if (UpdateType != PresencePartyMemberUpdateType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)UpdateType);
		}
		if (member_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Member);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresencePartyMemberUpdate other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UpdateType != PresencePartyMemberUpdateType.None)
		{
			UpdateType = other.UpdateType;
		}
		if (other.member_ != null)
		{
			if (member_ == null)
			{
				Member = new PartyMember();
			}
			Member.MergeFrom(other.Member);
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
			case 8u:
				UpdateType = (PresencePartyMemberUpdateType)input.ReadEnum();
				break;
			case 18u:
				if (member_ == null)
				{
					Member = new PartyMember();
				}
				input.ReadMessage(Member);
				break;
			}
		}
	}
}
