using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class GetSessionMemberEventsResponse : IMessage<GetSessionMemberEventsResponse>, IMessage, IEquatable<GetSessionMemberEventsResponse>, IDeepCloneable<GetSessionMemberEventsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetSessionMemberEventsResponse> _parser = new MessageParser<GetSessionMemberEventsResponse>(() => new GetSessionMemberEventsResponse());

	private UnknownFieldSet _unknownFields;

	public const int EventsFieldNumber = 1;

	private static readonly FieldCodec<SessionMemberEvent> _repeated_events_codec = FieldCodec.ForMessage(10u, SessionMemberEvent.Parser);

	private readonly RepeatedField<SessionMemberEvent> events_ = new RepeatedField<SessionMemberEvent>();

	public const int LastEventIdFieldNumber = 2;

	private long lastEventId_;

	[DebuggerNonUserCode]
	public static MessageParser<GetSessionMemberEventsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[9];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<SessionMemberEvent> Events => events_;

	[DebuggerNonUserCode]
	public long LastEventId
	{
		get
		{
			return lastEventId_;
		}
		set
		{
			lastEventId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetSessionMemberEventsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetSessionMemberEventsResponse(GetSessionMemberEventsResponse other)
		: this()
	{
		events_ = other.events_.Clone();
		lastEventId_ = other.lastEventId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetSessionMemberEventsResponse Clone()
	{
		return new GetSessionMemberEventsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetSessionMemberEventsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetSessionMemberEventsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!events_.Equals(other.events_))
		{
			return false;
		}
		if (LastEventId != other.LastEventId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= events_.GetHashCode();
		if (LastEventId != 0)
		{
			num ^= LastEventId.GetHashCode();
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
		events_.WriteTo(ref output, _repeated_events_codec);
		if (LastEventId != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(LastEventId);
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
		num += events_.CalculateSize(_repeated_events_codec);
		if (LastEventId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(LastEventId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetSessionMemberEventsResponse other)
	{
		if (other != null)
		{
			events_.Add(other.events_);
			if (other.LastEventId != 0)
			{
				LastEventId = other.LastEventId;
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
				events_.AddEntriesFrom(ref input, _repeated_events_codec);
				break;
			case 16u:
				LastEventId = input.ReadInt64();
				break;
			}
		}
	}
}
