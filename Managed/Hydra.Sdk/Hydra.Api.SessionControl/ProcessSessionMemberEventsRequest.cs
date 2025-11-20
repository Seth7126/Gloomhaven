using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.SessionControl;

public sealed class ProcessSessionMemberEventsRequest : IMessage<ProcessSessionMemberEventsRequest>, IMessage, IEquatable<ProcessSessionMemberEventsRequest>, IDeepCloneable<ProcessSessionMemberEventsRequest>, IBufferMessage
{
	private static readonly MessageParser<ProcessSessionMemberEventsRequest> _parser = new MessageParser<ProcessSessionMemberEventsRequest>(() => new ProcessSessionMemberEventsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ServerContextFieldNumber = 1;

	private ServerContext serverContext_;

	public const int ListFieldNumber = 2;

	private static readonly FieldCodec<SessionMemberEventResult> _repeated_list_codec = FieldCodec.ForMessage(18u, SessionMemberEventResult.Parser);

	private readonly RepeatedField<SessionMemberEventResult> list_ = new RepeatedField<SessionMemberEventResult>();

	[DebuggerNonUserCode]
	public static MessageParser<ProcessSessionMemberEventsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[10];

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
	public RepeatedField<SessionMemberEventResult> List => list_;

	[DebuggerNonUserCode]
	public ProcessSessionMemberEventsRequest()
	{
	}

	[DebuggerNonUserCode]
	public ProcessSessionMemberEventsRequest(ProcessSessionMemberEventsRequest other)
		: this()
	{
		serverContext_ = ((other.serverContext_ != null) ? other.serverContext_.Clone() : null);
		list_ = other.list_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ProcessSessionMemberEventsRequest Clone()
	{
		return new ProcessSessionMemberEventsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ProcessSessionMemberEventsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ProcessSessionMemberEventsRequest other)
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
		if (!list_.Equals(other.list_))
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
		num ^= list_.GetHashCode();
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
		list_.WriteTo(ref output, _repeated_list_codec);
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
		num += list_.CalculateSize(_repeated_list_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ProcessSessionMemberEventsRequest other)
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
		list_.Add(other.list_);
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
			case 18u:
				list_.AddEntriesFrom(ref input, _repeated_list_codec);
				break;
			}
		}
	}
}
