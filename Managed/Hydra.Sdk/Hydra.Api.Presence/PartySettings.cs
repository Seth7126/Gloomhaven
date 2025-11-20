using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class PartySettings : IMessage<PartySettings>, IMessage, IEquatable<PartySettings>, IDeepCloneable<PartySettings>, IBufferMessage
{
	private static readonly MessageParser<PartySettings> _parser = new MessageParser<PartySettings>(() => new PartySettings());

	private UnknownFieldSet _unknownFields;

	public const int PartyMaxCountFieldNumber = 1;

	private int partyMaxCount_;

	public const int InviteDelegationFieldNumber = 2;

	private PartyInviteDelegation inviteDelegation_ = PartyInviteDelegation.Owner;

	public const int JoinDelegationFieldNumber = 3;

	private PartyJoinDelegation joinDelegation_ = PartyJoinDelegation.Disabled;

	public const int AllowedUserIdsFieldNumber = 4;

	private static readonly FieldCodec<string> _repeated_allowedUserIds_codec = FieldCodec.ForString(34u);

	private readonly RepeatedField<string> allowedUserIds_ = new RepeatedField<string>();

	public const int JoinableFieldNumber = 5;

	private PartyJoin joinable_ = PartyJoin.Disabled;

	public const int DisbandOnOwnerLeaveFieldNumber = 6;

	private bool disbandOnOwnerLeave_;

	[DebuggerNonUserCode]
	public static MessageParser<PartySettings> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PartyStatusReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int PartyMaxCount
	{
		get
		{
			return partyMaxCount_;
		}
		set
		{
			partyMaxCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartyInviteDelegation InviteDelegation
	{
		get
		{
			return inviteDelegation_;
		}
		set
		{
			inviteDelegation_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartyJoinDelegation JoinDelegation
	{
		get
		{
			return joinDelegation_;
		}
		set
		{
			joinDelegation_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> AllowedUserIds => allowedUserIds_;

	[DebuggerNonUserCode]
	public PartyJoin Joinable
	{
		get
		{
			return joinable_;
		}
		set
		{
			joinable_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool DisbandOnOwnerLeave
	{
		get
		{
			return disbandOnOwnerLeave_;
		}
		set
		{
			disbandOnOwnerLeave_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartySettings()
	{
	}

	[DebuggerNonUserCode]
	public PartySettings(PartySettings other)
		: this()
	{
		partyMaxCount_ = other.partyMaxCount_;
		inviteDelegation_ = other.inviteDelegation_;
		joinDelegation_ = other.joinDelegation_;
		allowedUserIds_ = other.allowedUserIds_.Clone();
		joinable_ = other.joinable_;
		disbandOnOwnerLeave_ = other.disbandOnOwnerLeave_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PartySettings Clone()
	{
		return new PartySettings(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PartySettings);
	}

	[DebuggerNonUserCode]
	public bool Equals(PartySettings other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PartyMaxCount != other.PartyMaxCount)
		{
			return false;
		}
		if (InviteDelegation != other.InviteDelegation)
		{
			return false;
		}
		if (JoinDelegation != other.JoinDelegation)
		{
			return false;
		}
		if (!allowedUserIds_.Equals(other.allowedUserIds_))
		{
			return false;
		}
		if (Joinable != other.Joinable)
		{
			return false;
		}
		if (DisbandOnOwnerLeave != other.DisbandOnOwnerLeave)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PartyMaxCount != 0)
		{
			num ^= PartyMaxCount.GetHashCode();
		}
		if (InviteDelegation != PartyInviteDelegation.Owner)
		{
			num ^= InviteDelegation.GetHashCode();
		}
		if (JoinDelegation != PartyJoinDelegation.Disabled)
		{
			num ^= JoinDelegation.GetHashCode();
		}
		num ^= allowedUserIds_.GetHashCode();
		if (Joinable != PartyJoin.Disabled)
		{
			num ^= Joinable.GetHashCode();
		}
		if (DisbandOnOwnerLeave)
		{
			num ^= DisbandOnOwnerLeave.GetHashCode();
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
		if (PartyMaxCount != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(PartyMaxCount);
		}
		if (InviteDelegation != PartyInviteDelegation.Owner)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)InviteDelegation);
		}
		if (JoinDelegation != PartyJoinDelegation.Disabled)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)JoinDelegation);
		}
		allowedUserIds_.WriteTo(ref output, _repeated_allowedUserIds_codec);
		if (Joinable != PartyJoin.Disabled)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)Joinable);
		}
		if (DisbandOnOwnerLeave)
		{
			output.WriteRawTag(48);
			output.WriteBool(DisbandOnOwnerLeave);
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
		if (PartyMaxCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PartyMaxCount);
		}
		if (InviteDelegation != PartyInviteDelegation.Owner)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)InviteDelegation);
		}
		if (JoinDelegation != PartyJoinDelegation.Disabled)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)JoinDelegation);
		}
		num += allowedUserIds_.CalculateSize(_repeated_allowedUserIds_codec);
		if (Joinable != PartyJoin.Disabled)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Joinable);
		}
		if (DisbandOnOwnerLeave)
		{
			num += 2;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PartySettings other)
	{
		if (other != null)
		{
			if (other.PartyMaxCount != 0)
			{
				PartyMaxCount = other.PartyMaxCount;
			}
			if (other.InviteDelegation != PartyInviteDelegation.Owner)
			{
				InviteDelegation = other.InviteDelegation;
			}
			if (other.JoinDelegation != PartyJoinDelegation.Disabled)
			{
				JoinDelegation = other.JoinDelegation;
			}
			allowedUserIds_.Add(other.allowedUserIds_);
			if (other.Joinable != PartyJoin.Disabled)
			{
				Joinable = other.Joinable;
			}
			if (other.DisbandOnOwnerLeave)
			{
				DisbandOnOwnerLeave = other.DisbandOnOwnerLeave;
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
				PartyMaxCount = input.ReadInt32();
				break;
			case 16u:
				InviteDelegation = (PartyInviteDelegation)input.ReadEnum();
				break;
			case 24u:
				JoinDelegation = (PartyJoinDelegation)input.ReadEnum();
				break;
			case 34u:
				allowedUserIds_.AddEntriesFrom(ref input, _repeated_allowedUserIds_codec);
				break;
			case 40u:
				Joinable = (PartyJoin)input.ReadEnum();
				break;
			case 48u:
				DisbandOnOwnerLeave = input.ReadBool();
				break;
			}
		}
	}
}
