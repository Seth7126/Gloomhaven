using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.SessionControl;

public sealed class EchoEndpoint : IMessage<EchoEndpoint>, IMessage, IEquatable<EchoEndpoint>, IDeepCloneable<EchoEndpoint>, IBufferMessage
{
	private static readonly MessageParser<EchoEndpoint> _parser = new MessageParser<EchoEndpoint>(() => new EchoEndpoint());

	private UnknownFieldSet _unknownFields;

	public const int IpFieldNumber = 1;

	private string ip_ = "";

	public const int PortFieldNumber = 2;

	private int port_;

	public const int SchemeFieldNumber = 3;

	private EndpointScheme scheme_ = EndpointScheme.Secured;

	[DebuggerNonUserCode]
	public static MessageParser<EchoEndpoint> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[16];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Ip
	{
		get
		{
			return ip_;
		}
		set
		{
			ip_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int Port
	{
		get
		{
			return port_;
		}
		set
		{
			port_ = value;
		}
	}

	[DebuggerNonUserCode]
	public EndpointScheme Scheme
	{
		get
		{
			return scheme_;
		}
		set
		{
			scheme_ = value;
		}
	}

	[DebuggerNonUserCode]
	public EchoEndpoint()
	{
	}

	[DebuggerNonUserCode]
	public EchoEndpoint(EchoEndpoint other)
		: this()
	{
		ip_ = other.ip_;
		port_ = other.port_;
		scheme_ = other.scheme_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public EchoEndpoint Clone()
	{
		return new EchoEndpoint(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as EchoEndpoint);
	}

	[DebuggerNonUserCode]
	public bool Equals(EchoEndpoint other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Ip != other.Ip)
		{
			return false;
		}
		if (Port != other.Port)
		{
			return false;
		}
		if (Scheme != other.Scheme)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Ip.Length != 0)
		{
			num ^= Ip.GetHashCode();
		}
		if (Port != 0)
		{
			num ^= Port.GetHashCode();
		}
		if (Scheme != EndpointScheme.Secured)
		{
			num ^= Scheme.GetHashCode();
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
		if (Ip.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Ip);
		}
		if (Port != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Port);
		}
		if (Scheme != EndpointScheme.Secured)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)Scheme);
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
		if (Ip.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Ip);
		}
		if (Port != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Port);
		}
		if (Scheme != EndpointScheme.Secured)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Scheme);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(EchoEndpoint other)
	{
		if (other != null)
		{
			if (other.Ip.Length != 0)
			{
				Ip = other.Ip;
			}
			if (other.Port != 0)
			{
				Port = other.Port;
			}
			if (other.Scheme != EndpointScheme.Secured)
			{
				Scheme = other.Scheme;
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
				Ip = input.ReadString();
				break;
			case 16u:
				Port = input.ReadInt32();
				break;
			case 24u:
				Scheme = (EndpointScheme)input.ReadEnum();
				break;
			}
		}
	}
}
