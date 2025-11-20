using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Presence;

public sealed class MatchmakeSessionJoinRequest : IMessage<MatchmakeSessionJoinRequest>, IMessage, IEquatable<MatchmakeSessionJoinRequest>, IDeepCloneable<MatchmakeSessionJoinRequest>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeSessionJoinRequest> _parser = new MessageParser<MatchmakeSessionJoinRequest>(() => new MatchmakeSessionJoinRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int SessionIdFieldNumber = 2;

	private string sessionId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeSessionJoinRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[52];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext Context
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
	public string SessionId
	{
		get
		{
			return sessionId_;
		}
		set
		{
			sessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionJoinRequest()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionJoinRequest(MatchmakeSessionJoinRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		sessionId_ = other.sessionId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionJoinRequest Clone()
	{
		return new MatchmakeSessionJoinRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeSessionJoinRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeSessionJoinRequest other)
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
		if (SessionId != other.SessionId)
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
		if (SessionId.Length != 0)
		{
			num ^= SessionId.GetHashCode();
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
		if (SessionId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(SessionId);
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
		if (SessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SessionId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeSessionJoinRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new UserContext();
			}
			Context.MergeFrom(other.Context);
		}
		if (other.SessionId.Length != 0)
		{
			SessionId = other.SessionId;
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
					Context = new UserContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				SessionId = input.ReadString();
				break;
			}
		}
	}
}
