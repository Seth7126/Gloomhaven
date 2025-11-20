using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.EndpointDispatcher;

public sealed class EnvironmentInfo : IMessage<EnvironmentInfo>, IMessage, IEquatable<EnvironmentInfo>, IDeepCloneable<EnvironmentInfo>, IBufferMessage
{
	private static readonly MessageParser<EnvironmentInfo> _parser = new MessageParser<EnvironmentInfo>(() => new EnvironmentInfo());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private string id_ = "";

	public const int StatusFieldNumber = 2;

	private EnvironmentStatus status_ = EnvironmentStatus.Unknown;

	public const int EndpointFieldNumber = 3;

	private EndpointInfo endpoint_;

	[DebuggerNonUserCode]
	public static MessageParser<EnvironmentInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EndpointDispatcherContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public EnvironmentStatus Status
	{
		get
		{
			return status_;
		}
		set
		{
			status_ = value;
		}
	}

	[DebuggerNonUserCode]
	public EndpointInfo Endpoint
	{
		get
		{
			return endpoint_;
		}
		set
		{
			endpoint_ = value;
		}
	}

	[DebuggerNonUserCode]
	public EnvironmentInfo()
	{
	}

	[DebuggerNonUserCode]
	public EnvironmentInfo(EnvironmentInfo other)
		: this()
	{
		id_ = other.id_;
		status_ = other.status_;
		endpoint_ = ((other.endpoint_ != null) ? other.endpoint_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public EnvironmentInfo Clone()
	{
		return new EnvironmentInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as EnvironmentInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(EnvironmentInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (Status != other.Status)
		{
			return false;
		}
		if (!object.Equals(Endpoint, other.Endpoint))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (Status != EnvironmentStatus.Unknown)
		{
			num ^= Status.GetHashCode();
		}
		if (endpoint_ != null)
		{
			num ^= Endpoint.GetHashCode();
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
		if (Id.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Id);
		}
		if (Status != EnvironmentStatus.Unknown)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)Status);
		}
		if (endpoint_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Endpoint);
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
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Id);
		}
		if (Status != EnvironmentStatus.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Status);
		}
		if (endpoint_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Endpoint);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(EnvironmentInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id.Length != 0)
		{
			Id = other.Id;
		}
		if (other.Status != EnvironmentStatus.Unknown)
		{
			Status = other.Status;
		}
		if (other.endpoint_ != null)
		{
			if (endpoint_ == null)
			{
				Endpoint = new EndpointInfo();
			}
			Endpoint.MergeFrom(other.Endpoint);
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
				Id = input.ReadString();
				break;
			case 16u:
				Status = (EnvironmentStatus)input.ReadEnum();
				break;
			case 26u:
				if (endpoint_ == null)
				{
					Endpoint = new EndpointInfo();
				}
				input.ReadMessage(Endpoint);
				break;
			}
		}
	}
}
