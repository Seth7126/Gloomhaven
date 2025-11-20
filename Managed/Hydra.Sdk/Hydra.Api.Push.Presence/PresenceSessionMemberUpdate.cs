using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class PresenceSessionMemberUpdate : IMessage<PresenceSessionMemberUpdate>, IMessage, IEquatable<PresenceSessionMemberUpdate>, IDeepCloneable<PresenceSessionMemberUpdate>, IBufferMessage
{
	private static readonly MessageParser<PresenceSessionMemberUpdate> _parser = new MessageParser<PresenceSessionMemberUpdate>(() => new PresenceSessionMemberUpdate());

	private UnknownFieldSet _unknownFields;

	public const int UpdateTypeFieldNumber = 2;

	private PresenceSessionMemberUpdateType updateType_ = PresenceSessionMemberUpdateType.None;

	public const int MemberDataFieldNumber = 3;

	private SessionMember memberData_;

	[DebuggerNonUserCode]
	public static MessageParser<PresenceSessionMemberUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PresenceSessionMemberUpdateType UpdateType
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
	public SessionMember MemberData
	{
		get
		{
			return memberData_;
		}
		set
		{
			memberData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresenceSessionMemberUpdate()
	{
	}

	[DebuggerNonUserCode]
	public PresenceSessionMemberUpdate(PresenceSessionMemberUpdate other)
		: this()
	{
		updateType_ = other.updateType_;
		memberData_ = ((other.memberData_ != null) ? other.memberData_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresenceSessionMemberUpdate Clone()
	{
		return new PresenceSessionMemberUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresenceSessionMemberUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresenceSessionMemberUpdate other)
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
		if (!object.Equals(MemberData, other.MemberData))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UpdateType != PresenceSessionMemberUpdateType.None)
		{
			num ^= UpdateType.GetHashCode();
		}
		if (memberData_ != null)
		{
			num ^= MemberData.GetHashCode();
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
		if (UpdateType != PresenceSessionMemberUpdateType.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)UpdateType);
		}
		if (memberData_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(MemberData);
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
		if (UpdateType != PresenceSessionMemberUpdateType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)UpdateType);
		}
		if (memberData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(MemberData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresenceSessionMemberUpdate other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UpdateType != PresenceSessionMemberUpdateType.None)
		{
			UpdateType = other.UpdateType;
		}
		if (other.memberData_ != null)
		{
			if (memberData_ == null)
			{
				MemberData = new SessionMember();
			}
			MemberData.MergeFrom(other.MemberData);
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
			case 16u:
				UpdateType = (PresenceSessionMemberUpdateType)input.ReadEnum();
				break;
			case 26u:
				if (memberData_ == null)
				{
					MemberData = new SessionMember();
				}
				input.ReadMessage(MemberData);
				break;
			}
		}
	}
}
