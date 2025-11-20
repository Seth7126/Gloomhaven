using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class ServerData : IMessage<ServerData>, IMessage, IEquatable<ServerData>, IDeepCloneable<ServerData>, IBufferMessage
{
	private static readonly MessageParser<ServerData> _parser = new MessageParser<ServerData>(() => new ServerData());

	private UnknownFieldSet _unknownFields;

	public const int SessionDataFieldNumber = 1;

	private ServerBrowsingSessionData sessionData_;

	public const int ServerInfoFieldNumber = 2;

	private ServerInfo serverInfo_;

	[DebuggerNonUserCode]
	public static MessageParser<ServerData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[24];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerBrowsingSessionData SessionData
	{
		get
		{
			return sessionData_;
		}
		set
		{
			sessionData_ = value;
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
	public ServerData()
	{
	}

	[DebuggerNonUserCode]
	public ServerData(ServerData other)
		: this()
	{
		sessionData_ = ((other.sessionData_ != null) ? other.sessionData_.Clone() : null);
		serverInfo_ = ((other.serverInfo_ != null) ? other.serverInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerData Clone()
	{
		return new ServerData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerData);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(SessionData, other.SessionData))
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
		if (sessionData_ != null)
		{
			num ^= SessionData.GetHashCode();
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
		if (sessionData_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(SessionData);
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
		if (sessionData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(SessionData);
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
	public void MergeFrom(ServerData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.sessionData_ != null)
		{
			if (sessionData_ == null)
			{
				SessionData = new ServerBrowsingSessionData();
			}
			SessionData.MergeFrom(other.SessionData);
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
				if (sessionData_ == null)
				{
					SessionData = new ServerBrowsingSessionData();
				}
				input.ReadMessage(SessionData);
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
