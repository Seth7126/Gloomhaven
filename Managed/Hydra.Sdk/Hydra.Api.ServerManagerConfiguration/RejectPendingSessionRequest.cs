using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.SessionControl;

namespace Hydra.Api.ServerManagerConfiguration;

public sealed class RejectPendingSessionRequest : IMessage<RejectPendingSessionRequest>, IMessage, IEquatable<RejectPendingSessionRequest>, IDeepCloneable<RejectPendingSessionRequest>, IBufferMessage
{
	private static readonly MessageParser<RejectPendingSessionRequest> _parser = new MessageParser<RejectPendingSessionRequest>(() => new RejectPendingSessionRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ToolContext context_;

	public const int ServerManagerIdFieldNumber = 3;

	private string serverManagerId_ = "";

	public const int SessionFieldNumber = 4;

	private PendingSession session_;

	[DebuggerNonUserCode]
	public static MessageParser<RejectPendingSessionRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => DevDSMConfigurationContractsReflection.Descriptor.MessageTypes[6];

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
	public PendingSession Session
	{
		get
		{
			return session_;
		}
		set
		{
			session_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RejectPendingSessionRequest()
	{
	}

	[DebuggerNonUserCode]
	public RejectPendingSessionRequest(RejectPendingSessionRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		serverManagerId_ = other.serverManagerId_;
		session_ = ((other.session_ != null) ? other.session_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RejectPendingSessionRequest Clone()
	{
		return new RejectPendingSessionRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RejectPendingSessionRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(RejectPendingSessionRequest other)
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
		if (!object.Equals(Session, other.Session))
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
		if (session_ != null)
		{
			num ^= Session.GetHashCode();
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
			output.WriteRawTag(26);
			output.WriteString(ServerManagerId);
		}
		if (session_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Session);
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
		if (session_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Session);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RejectPendingSessionRequest other)
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
		if (other.session_ != null)
		{
			if (session_ == null)
			{
				Session = new PendingSession();
			}
			Session.MergeFrom(other.Session);
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
			case 26u:
				ServerManagerId = input.ReadString();
				break;
			case 34u:
				if (session_ == null)
				{
					Session = new PendingSession();
				}
				input.ReadMessage(Session);
				break;
			}
		}
	}
}
