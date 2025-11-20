using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.Push;

namespace Hydra.Api.Challenges;

public sealed class ConnectRequest : IMessage<ConnectRequest>, IMessage, IEquatable<ConnectRequest>, IDeepCloneable<ConnectRequest>, IBufferMessage
{
	private static readonly MessageParser<ConnectRequest> _parser = new MessageParser<ConnectRequest>(() => new ConnectRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int ConfigurationContextFieldNumber = 2;

	private ConfigurationContext configurationContext_;

	public const int PushTokenFieldNumber = 3;

	private PushToken pushToken_;

	[DebuggerNonUserCode]
	public static MessageParser<ConnectRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[0];

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
	public ConfigurationContext ConfigurationContext
	{
		get
		{
			return configurationContext_;
		}
		set
		{
			configurationContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PushToken PushToken
	{
		get
		{
			return pushToken_;
		}
		set
		{
			pushToken_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ConnectRequest()
	{
	}

	[DebuggerNonUserCode]
	public ConnectRequest(ConnectRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		configurationContext_ = ((other.configurationContext_ != null) ? other.configurationContext_.Clone() : null);
		pushToken_ = ((other.pushToken_ != null) ? other.pushToken_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ConnectRequest Clone()
	{
		return new ConnectRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ConnectRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ConnectRequest other)
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
		if (!object.Equals(ConfigurationContext, other.ConfigurationContext))
		{
			return false;
		}
		if (!object.Equals(PushToken, other.PushToken))
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
		if (configurationContext_ != null)
		{
			num ^= ConfigurationContext.GetHashCode();
		}
		if (pushToken_ != null)
		{
			num ^= PushToken.GetHashCode();
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
		if (configurationContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ConfigurationContext);
		}
		if (pushToken_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(PushToken);
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
		if (configurationContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ConfigurationContext);
		}
		if (pushToken_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PushToken);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ConnectRequest other)
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
		if (other.configurationContext_ != null)
		{
			if (configurationContext_ == null)
			{
				ConfigurationContext = new ConfigurationContext();
			}
			ConfigurationContext.MergeFrom(other.ConfigurationContext);
		}
		if (other.pushToken_ != null)
		{
			if (pushToken_ == null)
			{
				PushToken = new PushToken();
			}
			PushToken.MergeFrom(other.PushToken);
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
				if (configurationContext_ == null)
				{
					ConfigurationContext = new ConfigurationContext();
				}
				input.ReadMessage(ConfigurationContext);
				break;
			case 26u:
				if (pushToken_ == null)
				{
					PushToken = new PushToken();
				}
				input.ReadMessage(PushToken);
				break;
			}
		}
	}
}
