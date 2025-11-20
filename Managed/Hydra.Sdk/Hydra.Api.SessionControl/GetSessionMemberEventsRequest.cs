using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.SessionControl;

public sealed class GetSessionMemberEventsRequest : IMessage<GetSessionMemberEventsRequest>, IMessage, IEquatable<GetSessionMemberEventsRequest>, IDeepCloneable<GetSessionMemberEventsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetSessionMemberEventsRequest> _parser = new MessageParser<GetSessionMemberEventsRequest>(() => new GetSessionMemberEventsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ServerContextFieldNumber = 1;

	private ServerContext serverContext_;

	public const int LastEventIdFieldNumber = 2;

	private long lastEventId_;

	[DebuggerNonUserCode]
	public static MessageParser<GetSessionMemberEventsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerContext ServerContext
	{
		get
		{
			return serverContext_;
		}
		set
		{
			serverContext_ = value;
		}
	}

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
	public GetSessionMemberEventsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetSessionMemberEventsRequest(GetSessionMemberEventsRequest other)
		: this()
	{
		serverContext_ = ((other.serverContext_ != null) ? other.serverContext_.Clone() : null);
		lastEventId_ = other.lastEventId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetSessionMemberEventsRequest Clone()
	{
		return new GetSessionMemberEventsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetSessionMemberEventsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetSessionMemberEventsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ServerContext, other.ServerContext))
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
		if (serverContext_ != null)
		{
			num ^= ServerContext.GetHashCode();
		}
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
		if (serverContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ServerContext);
		}
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
		if (serverContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerContext);
		}
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
	public void MergeFrom(GetSessionMemberEventsRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.serverContext_ != null)
		{
			if (serverContext_ == null)
			{
				ServerContext = new ServerContext();
			}
			ServerContext.MergeFrom(other.ServerContext);
		}
		if (other.LastEventId != 0)
		{
			LastEventId = other.LastEventId;
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
				if (serverContext_ == null)
				{
					ServerContext = new ServerContext();
				}
				input.ReadMessage(ServerContext);
				break;
			case 16u:
				LastEventId = input.ReadInt64();
				break;
			}
		}
	}
}
