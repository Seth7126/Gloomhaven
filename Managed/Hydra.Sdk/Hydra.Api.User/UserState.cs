using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class UserState : IMessage<UserState>, IMessage, IEquatable<UserState>, IDeepCloneable<UserState>, IBufferMessage
{
	private static readonly MessageParser<UserState> _parser = new MessageParser<UserState>(() => new UserState());

	private UnknownFieldSet _unknownFields;

	public const int StateUidFieldNumber = 1;

	private string stateUid_ = "";

	public const int StateValueFieldNumber = 2;

	private StateValue stateValue_;

	[DebuggerNonUserCode]
	public static MessageParser<UserState> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[14];

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
	public StateValue StateValue
	{
		get
		{
			return stateValue_;
		}
		set
		{
			stateValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserState()
	{
	}

	[DebuggerNonUserCode]
	public UserState(UserState other)
		: this()
	{
		stateUid_ = other.stateUid_;
		stateValue_ = ((other.stateValue_ != null) ? other.stateValue_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserState Clone()
	{
		return new UserState(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserState);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserState other)
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
		if (!object.Equals(StateValue, other.StateValue))
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
		if (stateValue_ != null)
		{
			num ^= StateValue.GetHashCode();
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
		if (StateUid.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(StateUid);
		}
		if (stateValue_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(StateValue);
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
		if (StateUid.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StateUid);
		}
		if (stateValue_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(StateValue);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserState other)
	{
		if (other == null)
		{
			return;
		}
		if (other.StateUid.Length != 0)
		{
			StateUid = other.StateUid;
		}
		if (other.stateValue_ != null)
		{
			if (stateValue_ == null)
			{
				StateValue = new StateValue();
			}
			StateValue.MergeFrom(other.StateValue);
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
				StateUid = input.ReadString();
				break;
			case 18u:
				if (stateValue_ == null)
				{
					StateValue = new StateValue();
				}
				input.ReadMessage(StateValue);
				break;
			}
		}
	}
}
