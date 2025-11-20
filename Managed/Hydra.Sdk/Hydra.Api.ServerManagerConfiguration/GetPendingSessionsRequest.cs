using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.ServerManagerConfiguration;

public sealed class GetPendingSessionsRequest : IMessage<GetPendingSessionsRequest>, IMessage, IEquatable<GetPendingSessionsRequest>, IDeepCloneable<GetPendingSessionsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetPendingSessionsRequest> _parser = new MessageParser<GetPendingSessionsRequest>(() => new GetPendingSessionsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ToolContext context_;

	public const int ServerManagerIdFieldNumber = 2;

	private string serverManagerId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetPendingSessionsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => DevDSMConfigurationContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ToolContext Context
	{
		get
		{
			return context_;
		}
		set
		{
			context_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string ServerManagerId
	{
		get
		{
			return serverManagerId_;
		}
		set
		{
			serverManagerId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetPendingSessionsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetPendingSessionsRequest(GetPendingSessionsRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		serverManagerId_ = other.serverManagerId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetPendingSessionsRequest Clone()
	{
		return new GetPendingSessionsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetPendingSessionsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetPendingSessionsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Context, other.Context))
		{
			return false;
		}
		if (ServerManagerId != other.ServerManagerId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (context_ != null)
		{
			num ^= Context.GetHashCode();
		}
		if (ServerManagerId.Length != 0)
		{
			num ^= ServerManagerId.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		if (ServerManagerId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ServerManagerId);
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
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		if (ServerManagerId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerManagerId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetPendingSessionsRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new ToolContext();
			}
			Context.MergeFrom(other.Context);
		}
		if (other.ServerManagerId.Length != 0)
		{
			ServerManagerId = other.ServerManagerId;
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
				if (context_ == null)
				{
					Context = new ToolContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				ServerManagerId = input.ReadString();
				break;
			}
		}
	}
}
