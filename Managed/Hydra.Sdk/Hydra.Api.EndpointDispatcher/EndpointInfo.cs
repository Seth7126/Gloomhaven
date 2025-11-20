using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure;

namespace Hydra.Api.EndpointDispatcher;

public sealed class EndpointInfo : IMessage<EndpointInfo>, IMessage, IEquatable<EndpointInfo>, IDeepCloneable<EndpointInfo>, IBufferMessage
{
	private static readonly MessageParser<EndpointInfo> _parser = new MessageParser<EndpointInfo>(() => new EndpointInfo());

	private UnknownFieldSet _unknownFields;

	public const int NameFieldNumber = 1;

	private string name_ = "";

	public const int IpFieldNumber = 2;

	private string ip_ = "";

	public const int PortFieldNumber = 3;

	private int port_;

	public const int SchemeFieldNumber = 4;

	private EndpointScheme scheme_ = EndpointScheme.Secured;

	public const int VersionFieldNumber = 5;

	private ServiceVersion version_;

	[DebuggerNonUserCode]
	public static MessageParser<EndpointInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EndpointDispatcherContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

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
	public ServiceVersion Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public EndpointInfo()
	{
	}

	[DebuggerNonUserCode]
	public EndpointInfo(EndpointInfo other)
		: this()
	{
		name_ = other.name_;
		ip_ = other.ip_;
		port_ = other.port_;
		scheme_ = other.scheme_;
		version_ = ((other.version_ != null) ? other.version_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public EndpointInfo Clone()
	{
		return new EndpointInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as EndpointInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(EndpointInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Name != other.Name)
		{
			return false;
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
		if (!object.Equals(Version, other.Version))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
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
		if (version_ != null)
		{
			num ^= Version.GetHashCode();
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
		if (Name.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Name);
		}
		if (Ip.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Ip);
		}
		if (Port != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Port);
		}
		if (Scheme != EndpointScheme.Secured)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)Scheme);
		}
		if (version_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(Version);
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
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
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
		if (version_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Version);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(EndpointInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Name.Length != 0)
		{
			Name = other.Name;
		}
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
		if (other.version_ != null)
		{
			if (version_ == null)
			{
				Version = new ServiceVersion();
			}
			Version.MergeFrom(other.Version);
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
				Name = input.ReadString();
				break;
			case 18u:
				Ip = input.ReadString();
				break;
			case 24u:
				Port = input.ReadInt32();
				break;
			case 32u:
				Scheme = (EndpointScheme)input.ReadEnum();
				break;
			case 42u:
				if (version_ == null)
				{
					Version = new ServiceVersion();
				}
				input.ReadMessage(Version);
				break;
			}
		}
	}
}
