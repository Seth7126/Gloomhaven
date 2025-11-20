using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class ServerInfo : IMessage<ServerInfo>, IMessage, IEquatable<ServerInfo>, IDeepCloneable<ServerInfo>, IBufferMessage
{
	private static readonly MessageParser<ServerInfo> _parser = new MessageParser<ServerInfo>(() => new ServerInfo());

	private UnknownFieldSet _unknownFields;

	public const int ConnectionInfoFieldNumber = 1;

	private string connectionInfo_ = "";

	public const int ServerPropertyFieldNumber = 2;

	private string serverProperty_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ServerInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MemberEventReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ConnectionInfo
	{
		get
		{
			return connectionInfo_;
		}
		set
		{
			connectionInfo_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ServerProperty
	{
		get
		{
			return serverProperty_;
		}
		set
		{
			serverProperty_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ServerInfo()
	{
	}

	[DebuggerNonUserCode]
	public ServerInfo(ServerInfo other)
		: this()
	{
		connectionInfo_ = other.connectionInfo_;
		serverProperty_ = other.serverProperty_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerInfo Clone()
	{
		return new ServerInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ConnectionInfo != other.ConnectionInfo)
		{
			return false;
		}
		if (ServerProperty != other.ServerProperty)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ConnectionInfo.Length != 0)
		{
			num ^= ConnectionInfo.GetHashCode();
		}
		if (ServerProperty.Length != 0)
		{
			num ^= ServerProperty.GetHashCode();
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
		if (ConnectionInfo.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ConnectionInfo);
		}
		if (ServerProperty.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ServerProperty);
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
		if (ConnectionInfo.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ConnectionInfo);
		}
		if (ServerProperty.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerProperty);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServerInfo other)
	{
		if (other != null)
		{
			if (other.ConnectionInfo.Length != 0)
			{
				ConnectionInfo = other.ConnectionInfo;
			}
			if (other.ServerProperty.Length != 0)
			{
				ServerProperty = other.ServerProperty;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				ConnectionInfo = input.ReadString();
				break;
			case 18u:
				ServerProperty = input.ReadString();
				break;
			}
		}
	}
}
