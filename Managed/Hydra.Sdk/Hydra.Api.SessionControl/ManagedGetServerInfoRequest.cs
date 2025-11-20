using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.SessionControl;

public sealed class ManagedGetServerInfoRequest : IMessage<ManagedGetServerInfoRequest>, IMessage, IEquatable<ManagedGetServerInfoRequest>, IDeepCloneable<ManagedGetServerInfoRequest>, IBufferMessage
{
	private static readonly MessageParser<ManagedGetServerInfoRequest> _parser = new MessageParser<ManagedGetServerInfoRequest>(() => new ManagedGetServerInfoRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int SCContextFieldNumber = 2;

	private string sCContext_ = "";

	public const int GameSessionIdFieldNumber = 3;

	private string gameSessionId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ManagedGetServerInfoRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string SCContext
	{
		get
		{
			return sCContext_;
		}
		set
		{
			sCContext_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string GameSessionId
	{
		get
		{
			return gameSessionId_;
		}
		set
		{
			gameSessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ManagedGetServerInfoRequest()
	{
	}

	[DebuggerNonUserCode]
	public ManagedGetServerInfoRequest(ManagedGetServerInfoRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		sCContext_ = other.sCContext_;
		gameSessionId_ = other.gameSessionId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ManagedGetServerInfoRequest Clone()
	{
		return new ManagedGetServerInfoRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ManagedGetServerInfoRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ManagedGetServerInfoRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (SCContext != other.SCContext)
		{
			return false;
		}
		if (GameSessionId != other.GameSessionId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (SCContext.Length != 0)
		{
			num ^= SCContext.GetHashCode();
		}
		if (GameSessionId.Length != 0)
		{
			num ^= GameSessionId.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (SCContext.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(SCContext);
		}
		if (GameSessionId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(GameSessionId);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (SCContext.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SCContext);
		}
		if (GameSessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(GameSessionId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ManagedGetServerInfoRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		if (other.SCContext.Length != 0)
		{
			SCContext = other.SCContext;
		}
		if (other.GameSessionId.Length != 0)
		{
			GameSessionId = other.GameSessionId;
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				SCContext = input.ReadString();
				break;
			case 26u:
				GameSessionId = input.ReadString();
				break;
			}
		}
	}
}
