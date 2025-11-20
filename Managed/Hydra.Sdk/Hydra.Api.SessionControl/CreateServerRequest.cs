using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class CreateServerRequest : IMessage<CreateServerRequest>, IMessage, IEquatable<CreateServerRequest>, IDeepCloneable<CreateServerRequest>, IBufferMessage
{
	private static readonly MessageParser<CreateServerRequest> _parser = new MessageParser<CreateServerRequest>(() => new CreateServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int ServerTokenFieldNumber = 1;

	private string serverToken_ = "";

	public const int ClientVersionFieldNumber = 2;

	private string clientVersion_ = "";

	public const int CreateDataFieldNumber = 3;

	private ServerBrowsingSessionData createData_;

	public const int ServerInfoFieldNumber = 4;

	private ServerInfo serverInfo_;

	[DebuggerNonUserCode]
	public static MessageParser<CreateServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[27];

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
	public ServerBrowsingSessionData CreateData
	{
		get
		{
			return createData_;
		}
		set
		{
			createData_ = value;
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
	public CreateServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public CreateServerRequest(CreateServerRequest other)
		: this()
	{
		serverToken_ = other.serverToken_;
		clientVersion_ = other.clientVersion_;
		createData_ = ((other.createData_ != null) ? other.createData_.Clone() : null);
		serverInfo_ = ((other.serverInfo_ != null) ? other.serverInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CreateServerRequest Clone()
	{
		return new CreateServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CreateServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(CreateServerRequest other)
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
		if (ClientVersion != other.ClientVersion)
		{
			return false;
		}
		if (!object.Equals(CreateData, other.CreateData))
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
		if (ClientVersion.Length != 0)
		{
			num ^= ClientVersion.GetHashCode();
		}
		if (createData_ != null)
		{
			num ^= CreateData.GetHashCode();
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
		if (ClientVersion.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ClientVersion);
		}
		if (createData_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(CreateData);
		}
		if (serverInfo_ != null)
		{
			output.WriteRawTag(34);
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
		if (ClientVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientVersion);
		}
		if (createData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CreateData);
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
	public void MergeFrom(CreateServerRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ServerToken.Length != 0)
		{
			ServerToken = other.ServerToken;
		}
		if (other.ClientVersion.Length != 0)
		{
			ClientVersion = other.ClientVersion;
		}
		if (other.createData_ != null)
		{
			if (createData_ == null)
			{
				CreateData = new ServerBrowsingSessionData();
			}
			CreateData.MergeFrom(other.CreateData);
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
				ClientVersion = input.ReadString();
				break;
			case 26u:
				if (createData_ == null)
				{
					CreateData = new ServerBrowsingSessionData();
				}
				input.ReadMessage(CreateData);
				break;
			case 34u:
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
