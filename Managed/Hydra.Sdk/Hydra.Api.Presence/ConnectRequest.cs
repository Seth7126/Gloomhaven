using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.Push;

namespace Hydra.Api.Presence;

public sealed class ConnectRequest : IMessage<ConnectRequest>, IMessage, IEquatable<ConnectRequest>, IDeepCloneable<ConnectRequest>, IBufferMessage
{
	private static readonly MessageParser<ConnectRequest> _parser = new MessageParser<ConnectRequest>(() => new ConnectRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int PushTokenFieldNumber = 2;

	private PushToken pushToken_;

	public const int ClientVersionFieldNumber = 3;

	private string clientVersion_ = "";

	public const int StaticPropertyFieldNumber = 4;

	private string staticProperty_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ConnectRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[0];

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
	public string ClientVersion
	{
		get
		{
			return clientVersion_;
		}
		set
		{
			clientVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string StaticProperty
	{
		get
		{
			return staticProperty_;
		}
		set
		{
			staticProperty_ = ProtoPreconditions.CheckNotNull(value, "value");
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
		pushToken_ = ((other.pushToken_ != null) ? other.pushToken_.Clone() : null);
		clientVersion_ = other.clientVersion_;
		staticProperty_ = other.staticProperty_;
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
		if (!object.Equals(PushToken, other.PushToken))
		{
			return false;
		}
		if (ClientVersion != other.ClientVersion)
		{
			return false;
		}
		if (StaticProperty != other.StaticProperty)
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
		if (pushToken_ != null)
		{
			num ^= PushToken.GetHashCode();
		}
		if (ClientVersion.Length != 0)
		{
			num ^= ClientVersion.GetHashCode();
		}
		if (StaticProperty.Length != 0)
		{
			num ^= StaticProperty.GetHashCode();
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
		if (pushToken_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(PushToken);
		}
		if (ClientVersion.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ClientVersion);
		}
		if (StaticProperty.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(StaticProperty);
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
		if (pushToken_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PushToken);
		}
		if (ClientVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientVersion);
		}
		if (StaticProperty.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StaticProperty);
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
		if (other.pushToken_ != null)
		{
			if (pushToken_ == null)
			{
				PushToken = new PushToken();
			}
			PushToken.MergeFrom(other.PushToken);
		}
		if (other.ClientVersion.Length != 0)
		{
			ClientVersion = other.ClientVersion;
		}
		if (other.StaticProperty.Length != 0)
		{
			StaticProperty = other.StaticProperty;
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
				if (pushToken_ == null)
				{
					PushToken = new PushToken();
				}
				input.ReadMessage(PushToken);
				break;
			case 26u:
				ClientVersion = input.ReadString();
				break;
			case 34u:
				StaticProperty = input.ReadString();
				break;
			}
		}
	}
}
