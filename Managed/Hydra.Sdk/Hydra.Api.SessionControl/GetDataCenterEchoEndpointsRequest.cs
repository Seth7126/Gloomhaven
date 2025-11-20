using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class GetDataCenterEchoEndpointsRequest : IMessage<GetDataCenterEchoEndpointsRequest>, IMessage, IEquatable<GetDataCenterEchoEndpointsRequest>, IDeepCloneable<GetDataCenterEchoEndpointsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetDataCenterEchoEndpointsRequest> _parser = new MessageParser<GetDataCenterEchoEndpointsRequest>(() => new GetDataCenterEchoEndpointsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ClientVersionFieldNumber = 1;

	private string clientVersion_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetDataCenterEchoEndpointsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[14];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public GetDataCenterEchoEndpointsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetDataCenterEchoEndpointsRequest(GetDataCenterEchoEndpointsRequest other)
		: this()
	{
		clientVersion_ = other.clientVersion_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetDataCenterEchoEndpointsRequest Clone()
	{
		return new GetDataCenterEchoEndpointsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetDataCenterEchoEndpointsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetDataCenterEchoEndpointsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ClientVersion != other.ClientVersion)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ClientVersion.Length != 0)
		{
			num ^= ClientVersion.GetHashCode();
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
		if (ClientVersion.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ClientVersion);
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
		if (ClientVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientVersion);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetDataCenterEchoEndpointsRequest other)
	{
		if (other != null)
		{
			if (other.ClientVersion.Length != 0)
			{
				ClientVersion = other.ClientVersion;
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				ClientVersion = input.ReadString();
			}
		}
	}
}
