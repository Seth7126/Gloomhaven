using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Signaling;

public sealed class SignalingMessage : IMessage<SignalingMessage>, IMessage, IEquatable<SignalingMessage>, IDeepCloneable<SignalingMessage>, IBufferMessage
{
	private static readonly MessageParser<SignalingMessage> _parser = new MessageParser<SignalingMessage>(() => new SignalingMessage());

	private UnknownFieldSet _unknownFields;

	public const int MessageIdFieldNumber = 1;

	private string messageId_ = "";

	public const int TypeFieldNumber = 2;

	private SignalingType type_ = SignalingType.SignalUnknown;

	public const int UserIdFromFieldNumber = 3;

	private string userIdFrom_ = "";

	public const int UserIdToFieldNumber = 4;

	private string userIdTo_ = "";

	public const int DataFieldNumber = 5;

	private ByteString data_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<SignalingMessage> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SignalingReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string MessageId
	{
		get
		{
			return messageId_;
		}
		set
		{
			messageId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignalingType Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = value;
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
	public ByteString Data
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
	public SignalingMessage()
	{
	}

	[DebuggerNonUserCode]
	public SignalingMessage(SignalingMessage other)
		: this()
	{
		messageId_ = other.messageId_;
		type_ = other.type_;
		userIdFrom_ = other.userIdFrom_;
		userIdTo_ = other.userIdTo_;
		data_ = other.data_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignalingMessage Clone()
	{
		return new SignalingMessage(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignalingMessage);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignalingMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MessageId != other.MessageId)
		{
			return false;
		}
		if (Type != other.Type)
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
		if (Data != other.Data)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (MessageId.Length != 0)
		{
			num ^= MessageId.GetHashCode();
		}
		if (Type != SignalingType.SignalUnknown)
		{
			num ^= Type.GetHashCode();
		}
		if (UserIdFrom.Length != 0)
		{
			num ^= UserIdFrom.GetHashCode();
		}
		if (UserIdTo.Length != 0)
		{
			num ^= UserIdTo.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
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
		if (MessageId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(MessageId);
		}
		if (Type != SignalingType.SignalUnknown)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)Type);
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
		if (Data.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteBytes(Data);
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
		if (MessageId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(MessageId);
		}
		if (Type != SignalingType.SignalUnknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Type);
		}
		if (UserIdFrom.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserIdFrom);
		}
		if (UserIdTo.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserIdTo);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignalingMessage other)
	{
		if (other != null)
		{
			if (other.MessageId.Length != 0)
			{
				MessageId = other.MessageId;
			}
			if (other.Type != SignalingType.SignalUnknown)
			{
				Type = other.Type;
			}
			if (other.UserIdFrom.Length != 0)
			{
				UserIdFrom = other.UserIdFrom;
			}
			if (other.UserIdTo.Length != 0)
			{
				UserIdTo = other.UserIdTo;
			}
			if (other.Data.Length != 0)
			{
				Data = other.Data;
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
				MessageId = input.ReadString();
				break;
			case 16u:
				Type = (SignalingType)input.ReadEnum();
				break;
			case 26u:
				UserIdFrom = input.ReadString();
				break;
			case 34u:
				UserIdTo = input.ReadString();
				break;
			case 42u:
				Data = input.ReadBytes();
				break;
			}
		}
	}
}
