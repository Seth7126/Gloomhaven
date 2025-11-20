using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.SessionControl;

public sealed class CreateSessionRequest : IMessage<CreateSessionRequest>, IMessage, IEquatable<CreateSessionRequest>, IDeepCloneable<CreateSessionRequest>, IBufferMessage
{
	private static readonly MessageParser<CreateSessionRequest> _parser = new MessageParser<CreateSessionRequest>(() => new CreateSessionRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int DataCenterIdFieldNumber = 2;

	private string dataCenterId_ = "";

	public const int ClientVersionFieldNumber = 3;

	private string clientVersion_ = "";

	public const int ServerDataFieldNumber = 4;

	private string serverData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<CreateSessionRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[0];

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
	public string DataCenterId
	{
		get
		{
			return dataCenterId_;
		}
		set
		{
			dataCenterId_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public string ServerData
	{
		get
		{
			return serverData_;
		}
		set
		{
			serverData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public CreateSessionRequest()
	{
	}

	[DebuggerNonUserCode]
	public CreateSessionRequest(CreateSessionRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		dataCenterId_ = other.dataCenterId_;
		clientVersion_ = other.clientVersion_;
		serverData_ = other.serverData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CreateSessionRequest Clone()
	{
		return new CreateSessionRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CreateSessionRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(CreateSessionRequest other)
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
		if (DataCenterId != other.DataCenterId)
		{
			return false;
		}
		if (ClientVersion != other.ClientVersion)
		{
			return false;
		}
		if (ServerData != other.ServerData)
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
		if (DataCenterId.Length != 0)
		{
			num ^= DataCenterId.GetHashCode();
		}
		if (ClientVersion.Length != 0)
		{
			num ^= ClientVersion.GetHashCode();
		}
		if (ServerData.Length != 0)
		{
			num ^= ServerData.GetHashCode();
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
		if (DataCenterId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(DataCenterId);
		}
		if (ClientVersion.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ClientVersion);
		}
		if (ServerData.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(ServerData);
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
		if (DataCenterId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DataCenterId);
		}
		if (ClientVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientVersion);
		}
		if (ServerData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CreateSessionRequest other)
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
		if (other.DataCenterId.Length != 0)
		{
			DataCenterId = other.DataCenterId;
		}
		if (other.ClientVersion.Length != 0)
		{
			ClientVersion = other.ClientVersion;
		}
		if (other.ServerData.Length != 0)
		{
			ServerData = other.ServerData;
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
				DataCenterId = input.ReadString();
				break;
			case 26u:
				ClientVersion = input.ReadString();
				break;
			case 34u:
				ServerData = input.ReadString();
				break;
			}
		}
	}
}
