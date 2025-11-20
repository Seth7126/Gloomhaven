using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class ActivateSessionRequest : IMessage<ActivateSessionRequest>, IMessage, IEquatable<ActivateSessionRequest>, IDeepCloneable<ActivateSessionRequest>, IBufferMessage
{
	private static readonly MessageParser<ActivateSessionRequest> _parser = new MessageParser<ActivateSessionRequest>(() => new ActivateSessionRequest());

	private UnknownFieldSet _unknownFields;

	public const int ServerTokenFieldNumber = 1;

	private string serverToken_ = "";

	public const int ServerInfoFieldNumber = 2;

	private ServerInfo serverInfo_;

	[DebuggerNonUserCode]
	public static MessageParser<ActivateSessionRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ServerToken
	{
		get
		{
			return serverToken_;
		}
		set
		{
			serverToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ServerInfo ServerInfo
	{
		get
		{
			return serverInfo_;
		}
		set
		{
			serverInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ActivateSessionRequest()
	{
	}

	[DebuggerNonUserCode]
	public ActivateSessionRequest(ActivateSessionRequest other)
		: this()
	{
		serverToken_ = other.serverToken_;
		serverInfo_ = ((other.serverInfo_ != null) ? other.serverInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ActivateSessionRequest Clone()
	{
		return new ActivateSessionRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ActivateSessionRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ActivateSessionRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServerToken != other.ServerToken)
		{
			return false;
		}
		if (!object.Equals(ServerInfo, other.ServerInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ServerToken.Length != 0)
		{
			num ^= ServerToken.GetHashCode();
		}
		if (serverInfo_ != null)
		{
			num ^= ServerInfo.GetHashCode();
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
		if (ServerToken.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ServerToken);
		}
		if (serverInfo_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ServerInfo);
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
		if (ServerToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerToken);
		}
		if (serverInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ActivateSessionRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ServerToken.Length != 0)
		{
			ServerToken = other.ServerToken;
		}
		if (other.serverInfo_ != null)
		{
			if (serverInfo_ == null)
			{
				ServerInfo = new ServerInfo();
			}
			ServerInfo.MergeFrom(other.ServerInfo);
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
				ServerToken = input.ReadString();
				break;
			case 18u:
				if (serverInfo_ == null)
				{
					ServerInfo = new ServerInfo();
				}
				input.ReadMessage(ServerInfo);
				break;
			}
		}
	}
}
