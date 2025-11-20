using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class UserTransactionItem : IMessage<UserTransactionItem>, IMessage, IEquatable<UserTransactionItem>, IDeepCloneable<UserTransactionItem>, IBufferMessage
{
	private static readonly MessageParser<UserTransactionItem> _parser = new MessageParser<UserTransactionItem>(() => new UserTransactionItem());

	private UnknownFieldSet _unknownFields;

	public const int StateUidFieldNumber = 1;

	private string stateUid_ = "";

	public const int StateOpTypeFieldNumber = 2;

	private UserStateOpType stateOpType_ = UserStateOpType.Unknown;

	public const int PrevValueFieldNumber = 3;

	private StateValue prevValue_;

	public const int CurrentValueFieldNumber = 4;

	private StateValue currentValue_;

	public const int ExtendedInfoFieldNumber = 5;

	private static readonly MapField<string, string>.Codec _map_extendedInfo_codec = new MapField<string, string>.Codec(FieldCodec.ForString(10u, ""), FieldCodec.ForString(18u, ""), 42u);

	private readonly MapField<string, string> extendedInfo_ = new MapField<string, string>();

	[DebuggerNonUserCode]
	public static MessageParser<UserTransactionItem> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[17];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string StateUid
	{
		get
		{
			return stateUid_;
		}
		set
		{
			stateUid_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UserStateOpType StateOpType
	{
		get
		{
			return stateOpType_;
		}
		set
		{
			stateOpType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public StateValue PrevValue
	{
		get
		{
			return prevValue_;
		}
		set
		{
			prevValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	public StateValue CurrentValue
	{
		get
		{
			return currentValue_;
		}
		set
		{
			currentValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	public MapField<string, string> ExtendedInfo => extendedInfo_;

	[DebuggerNonUserCode]
	public UserTransactionItem()
	{
	}

	[DebuggerNonUserCode]
	public UserTransactionItem(UserTransactionItem other)
		: this()
	{
		stateUid_ = other.stateUid_;
		stateOpType_ = other.stateOpType_;
		prevValue_ = ((other.prevValue_ != null) ? other.prevValue_.Clone() : null);
		currentValue_ = ((other.currentValue_ != null) ? other.currentValue_.Clone() : null);
		extendedInfo_ = other.extendedInfo_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserTransactionItem Clone()
	{
		return new UserTransactionItem(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserTransactionItem);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserTransactionItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (StateUid != other.StateUid)
		{
			return false;
		}
		if (StateOpType != other.StateOpType)
		{
			return false;
		}
		if (!object.Equals(PrevValue, other.PrevValue))
		{
			return false;
		}
		if (!object.Equals(CurrentValue, other.CurrentValue))
		{
			return false;
		}
		if (!ExtendedInfo.Equals(other.ExtendedInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (StateUid.Length != 0)
		{
			num ^= StateUid.GetHashCode();
		}
		if (StateOpType != UserStateOpType.Unknown)
		{
			num ^= StateOpType.GetHashCode();
		}
		if (prevValue_ != null)
		{
			num ^= PrevValue.GetHashCode();
		}
		if (currentValue_ != null)
		{
			num ^= CurrentValue.GetHashCode();
		}
		num ^= ExtendedInfo.GetHashCode();
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
		if (StateUid.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(StateUid);
		}
		if (StateOpType != UserStateOpType.Unknown)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)StateOpType);
		}
		if (prevValue_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(PrevValue);
		}
		if (currentValue_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(CurrentValue);
		}
		extendedInfo_.WriteTo(ref output, _map_extendedInfo_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (StateUid.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StateUid);
		}
		if (StateOpType != UserStateOpType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)StateOpType);
		}
		if (prevValue_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PrevValue);
		}
		if (currentValue_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CurrentValue);
		}
		num += extendedInfo_.CalculateSize(_map_extendedInfo_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserTransactionItem other)
	{
		if (other == null)
		{
			return;
		}
		if (other.StateUid.Length != 0)
		{
			StateUid = other.StateUid;
		}
		if (other.StateOpType != UserStateOpType.Unknown)
		{
			StateOpType = other.StateOpType;
		}
		if (other.prevValue_ != null)
		{
			if (prevValue_ == null)
			{
				PrevValue = new StateValue();
			}
			PrevValue.MergeFrom(other.PrevValue);
		}
		if (other.currentValue_ != null)
		{
			if (currentValue_ == null)
			{
				CurrentValue = new StateValue();
			}
			CurrentValue.MergeFrom(other.CurrentValue);
		}
		extendedInfo_.Add(other.extendedInfo_);
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
				StateUid = input.ReadString();
				break;
			case 16u:
				StateOpType = (UserStateOpType)input.ReadEnum();
				break;
			case 26u:
				if (prevValue_ == null)
				{
					PrevValue = new StateValue();
				}
				input.ReadMessage(PrevValue);
				break;
			case 34u:
				if (currentValue_ == null)
				{
					CurrentValue = new StateValue();
				}
				input.ReadMessage(CurrentValue);
				break;
			case 42u:
				extendedInfo_.AddEntriesFrom(ref input, _map_extendedInfo_codec);
				break;
			}
		}
	}
}
