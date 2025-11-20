using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class PartyMember : IMessage<PartyMember>, IMessage, IEquatable<PartyMember>, IDeepCloneable<PartyMember>, IBufferMessage
{
	private static readonly MessageParser<PartyMember> _parser = new MessageParser<PartyMember>(() => new PartyMember());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int IsOwnerFieldNumber = 2;

	private bool isOwner_;

	public const int DataFieldNumber = 3;

	private string data_ = "";

	public const int StaticDataFieldNumber = 4;

	private string staticData_ = "";

	public const int SortIndexFieldNumber = 5;

	private int sortIndex_;

	public const int StateFieldNumber = 6;

	private MatchmakeState state_ = MatchmakeState.None;

	[DebuggerNonUserCode]
	public static MessageParser<PartyMember> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PartyStatusReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public bool IsOwner
	{
		get
		{
			return isOwner_;
		}
		set
		{
			isOwner_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string StaticData
	{
		get
		{
			return staticData_;
		}
		set
		{
			staticData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int SortIndex
	{
		get
		{
			return sortIndex_;
		}
		set
		{
			sortIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	public MatchmakeState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartyMember()
	{
	}

	[DebuggerNonUserCode]
	public PartyMember(PartyMember other)
		: this()
	{
		userId_ = other.userId_;
		isOwner_ = other.isOwner_;
		data_ = other.data_;
		staticData_ = other.staticData_;
		sortIndex_ = other.sortIndex_;
		state_ = other.state_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PartyMember Clone()
	{
		return new PartyMember(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PartyMember);
	}

	[DebuggerNonUserCode]
	public bool Equals(PartyMember other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		if (IsOwner != other.IsOwner)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (StaticData != other.StaticData)
		{
			return false;
		}
		if (SortIndex != other.SortIndex)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
		}
		if (IsOwner)
		{
			num ^= IsOwner.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (StaticData.Length != 0)
		{
			num ^= StaticData.GetHashCode();
		}
		if (SortIndex != 0)
		{
			num ^= SortIndex.GetHashCode();
		}
		if (State != MatchmakeState.None)
		{
			num ^= State.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserId);
		}
		if (IsOwner)
		{
			output.WriteRawTag(16);
			output.WriteBool(IsOwner);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Data);
		}
		if (StaticData.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(StaticData);
		}
		if (SortIndex != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(SortIndex);
		}
		if (State != MatchmakeState.None)
		{
			output.WriteRawTag(48);
			output.WriteEnum((int)State);
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
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		if (IsOwner)
		{
			num += 2;
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Data);
		}
		if (StaticData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StaticData);
		}
		if (SortIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SortIndex);
		}
		if (State != MatchmakeState.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PartyMember other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.IsOwner)
			{
				IsOwner = other.IsOwner;
			}
			if (other.Data.Length != 0)
			{
				Data = other.Data;
			}
			if (other.StaticData.Length != 0)
			{
				StaticData = other.StaticData;
			}
			if (other.SortIndex != 0)
			{
				SortIndex = other.SortIndex;
			}
			if (other.State != MatchmakeState.None)
			{
				State = other.State;
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
				UserId = input.ReadString();
				break;
			case 16u:
				IsOwner = input.ReadBool();
				break;
			case 26u:
				Data = input.ReadString();
				break;
			case 34u:
				StaticData = input.ReadString();
				break;
			case 40u:
				SortIndex = input.ReadInt32();
				break;
			case 48u:
				State = (MatchmakeState)input.ReadEnum();
				break;
			}
		}
	}
}
