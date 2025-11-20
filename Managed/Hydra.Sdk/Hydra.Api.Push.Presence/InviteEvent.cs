using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class InviteEvent : IMessage<InviteEvent>, IMessage, IEquatable<InviteEvent>, IDeepCloneable<InviteEvent>, IBufferMessage
{
	private static readonly MessageParser<InviteEvent> _parser = new MessageParser<InviteEvent>(() => new InviteEvent());

	private UnknownFieldSet _unknownFields;

	public const int EventTypeFieldNumber = 1;

	private InviteEventType eventType_ = InviteEventType.Unknown;

	public const int DataFieldNumber = 2;

	private InviteData data_;

	public const int SequenceIdFieldNumber = 3;

	private int sequenceId_;

	[DebuggerNonUserCode]
	public static MessageParser<InviteEvent> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public InviteEventType EventType
	{
		get
		{
			return eventType_;
		}
		set
		{
			eventType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public InviteData Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int SequenceId
	{
		get
		{
			return sequenceId_;
		}
		set
		{
			sequenceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public InviteEvent()
	{
	}

	[DebuggerNonUserCode]
	public InviteEvent(InviteEvent other)
		: this()
	{
		eventType_ = other.eventType_;
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		sequenceId_ = other.sequenceId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public InviteEvent Clone()
	{
		return new InviteEvent(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as InviteEvent);
	}

	[DebuggerNonUserCode]
	public bool Equals(InviteEvent other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EventType != other.EventType)
		{
			return false;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		if (SequenceId != other.SequenceId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (EventType != InviteEventType.Unknown)
		{
			num ^= EventType.GetHashCode();
		}
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
		}
		if (SequenceId != 0)
		{
			num ^= SequenceId.GetHashCode();
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
		if (EventType != InviteEventType.Unknown)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)EventType);
		}
		if (data_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Data);
		}
		if (SequenceId != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(SequenceId);
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
		if (EventType != InviteEventType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EventType);
		}
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		if (SequenceId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SequenceId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(InviteEvent other)
	{
		if (other == null)
		{
			return;
		}
		if (other.EventType != InviteEventType.Unknown)
		{
			EventType = other.EventType;
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new InviteData();
			}
			Data.MergeFrom(other.Data);
		}
		if (other.SequenceId != 0)
		{
			SequenceId = other.SequenceId;
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
				EventType = (InviteEventType)input.ReadEnum();
				break;
			case 18u:
				if (data_ == null)
				{
					Data = new InviteData();
				}
				input.ReadMessage(Data);
				break;
			case 24u:
				SequenceId = input.ReadInt32();
				break;
			}
		}
	}
}
