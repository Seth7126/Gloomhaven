using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public sealed class ResourceStatus : IMessage<ResourceStatus>, IMessage, IEquatable<ResourceStatus>, IDeepCloneable<ResourceStatus>, IBufferMessage
{
	private static readonly MessageParser<ResourceStatus> _parser = new MessageParser<ResourceStatus>(() => new ResourceStatus());

	private UnknownFieldSet _unknownFields;

	public const int UsedFieldNumber = 1;

	private long used_;

	public const int TotalFieldNumber = 2;

	private long total_;

	[DebuggerNonUserCode]
	public static MessageParser<ResourceStatus> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ResourceStatusReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Used
	{
		get
		{
			return used_;
		}
		set
		{
			used_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long Total
	{
		get
		{
			return total_;
		}
		set
		{
			total_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ResourceStatus()
	{
	}

	[DebuggerNonUserCode]
	public ResourceStatus(ResourceStatus other)
		: this()
	{
		used_ = other.used_;
		total_ = other.total_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ResourceStatus Clone()
	{
		return new ResourceStatus(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ResourceStatus);
	}

	[DebuggerNonUserCode]
	public bool Equals(ResourceStatus other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Used != other.Used)
		{
			return false;
		}
		if (Total != other.Total)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Used != 0)
		{
			num ^= Used.GetHashCode();
		}
		if (Total != 0)
		{
			num ^= Total.GetHashCode();
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
		if (Used != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Used);
		}
		if (Total != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(Total);
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
		if (Used != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Used);
		}
		if (Total != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Total);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ResourceStatus other)
	{
		if (other != null)
		{
			if (other.Used != 0)
			{
				Used = other.Used;
			}
			if (other.Total != 0)
			{
				Total = other.Total;
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
			case 8u:
				Used = input.ReadInt64();
				break;
			case 16u:
				Total = input.ReadInt64();
				break;
			}
		}
	}
}
