using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Push.Signaling;

namespace Hydra.Api.Push;

public sealed class PushClientMessage : IMessage<PushClientMessage>, IMessage, IEquatable<PushClientMessage>, IDeepCloneable<PushClientMessage>, IBufferMessage
{
	public enum KindOneofCase
	{
		None = 0,
		Signal = 2
	}

	private static readonly MessageParser<PushClientMessage> _parser = new MessageParser<PushClientMessage>(() => new PushClientMessage());

	private UnknownFieldSet _unknownFields;

	public const int MessageTypeFieldNumber = 1;

	private PushClientMessageType messageType_ = PushClientMessageType.Unknown;

	public const int SignalFieldNumber = 2;

	private object kind_;

	private KindOneofCase kindCase_ = KindOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<PushClientMessage> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushClientMessageReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PushClientMessageType MessageType
	{
		get
		{
			return messageType_;
		}
		set
		{
			messageType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SignalingMessage Signal
	{
		get
		{
			return (kindCase_ == KindOneofCase.Signal) ? ((SignalingMessage)kind_) : null;
		}
		set
		{
			kind_ = value;
			kindCase_ = ((value != null) ? KindOneofCase.Signal : KindOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public KindOneofCase KindCase => kindCase_;

	[DebuggerNonUserCode]
	public PushClientMessage()
	{
	}

	[DebuggerNonUserCode]
	public PushClientMessage(PushClientMessage other)
		: this()
	{
		messageType_ = other.messageType_;
		KindOneofCase kindCase = other.KindCase;
		KindOneofCase kindOneofCase = kindCase;
		if (kindOneofCase == KindOneofCase.Signal)
		{
			Signal = other.Signal.Clone();
		}
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushClientMessage Clone()
	{
		return new PushClientMessage(this);
	}

	[DebuggerNonUserCode]
	public void ClearKind()
	{
		kindCase_ = KindOneofCase.None;
		kind_ = null;
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushClientMessage);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushClientMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MessageType != other.MessageType)
		{
			return false;
		}
		if (!object.Equals(Signal, other.Signal))
		{
			return false;
		}
		if (KindCase != other.KindCase)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (MessageType != PushClientMessageType.Unknown)
		{
			num ^= MessageType.GetHashCode();
		}
		if (kindCase_ == KindOneofCase.Signal)
		{
			num ^= Signal.GetHashCode();
		}
		num ^= (int)kindCase_;
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
		if (MessageType != PushClientMessageType.Unknown)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)MessageType);
		}
		if (kindCase_ == KindOneofCase.Signal)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Signal);
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
		if (MessageType != PushClientMessageType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)MessageType);
		}
		if (kindCase_ == KindOneofCase.Signal)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Signal);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushClientMessage other)
	{
		if (other == null)
		{
			return;
		}
		if (other.MessageType != PushClientMessageType.Unknown)
		{
			MessageType = other.MessageType;
		}
		KindOneofCase kindCase = other.KindCase;
		KindOneofCase kindOneofCase = kindCase;
		if (kindOneofCase == KindOneofCase.Signal)
		{
			if (Signal == null)
			{
				Signal = new SignalingMessage();
			}
			Signal.MergeFrom(other.Signal);
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
				MessageType = (PushClientMessageType)input.ReadEnum();
				break;
			case 18u:
			{
				SignalingMessage signalingMessage = new SignalingMessage();
				if (kindCase_ == KindOneofCase.Signal)
				{
					signalingMessage.MergeFrom(Signal);
				}
				input.ReadMessage(signalingMessage);
				Signal = signalingMessage;
				break;
			}
			}
		}
	}
}
