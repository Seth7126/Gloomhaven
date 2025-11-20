using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public sealed class VmResourceStatus : IMessage<VmResourceStatus>, IMessage, IEquatable<VmResourceStatus>, IDeepCloneable<VmResourceStatus>, IBufferMessage
{
	private static readonly MessageParser<VmResourceStatus> _parser = new MessageParser<VmResourceStatus>(() => new VmResourceStatus());

	private UnknownFieldSet _unknownFields;

	public const int CpuFieldNumber = 1;

	private ResourceStatus cpu_;

	public const int MemoryFieldNumber = 2;

	private ResourceStatus memory_;

	[DebuggerNonUserCode]
	public static MessageParser<VmResourceStatus> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ResourceStatusReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ResourceStatus Cpu
	{
		get
		{
			return cpu_;
		}
		set
		{
			cpu_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ResourceStatus Memory
	{
		get
		{
			return memory_;
		}
		set
		{
			memory_ = value;
		}
	}

	[DebuggerNonUserCode]
	public VmResourceStatus()
	{
	}

	[DebuggerNonUserCode]
	public VmResourceStatus(VmResourceStatus other)
		: this()
	{
		cpu_ = ((other.cpu_ != null) ? other.cpu_.Clone() : null);
		memory_ = ((other.memory_ != null) ? other.memory_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public VmResourceStatus Clone()
	{
		return new VmResourceStatus(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as VmResourceStatus);
	}

	[DebuggerNonUserCode]
	public bool Equals(VmResourceStatus other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Cpu, other.Cpu))
		{
			return false;
		}
		if (!object.Equals(Memory, other.Memory))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (cpu_ != null)
		{
			num ^= Cpu.GetHashCode();
		}
		if (memory_ != null)
		{
			num ^= Memory.GetHashCode();
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
		if (cpu_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Cpu);
		}
		if (memory_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Memory);
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
		if (cpu_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Cpu);
		}
		if (memory_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Memory);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(VmResourceStatus other)
	{
		if (other == null)
		{
			return;
		}
		if (other.cpu_ != null)
		{
			if (cpu_ == null)
			{
				Cpu = new ResourceStatus();
			}
			Cpu.MergeFrom(other.Cpu);
		}
		if (other.memory_ != null)
		{
			if (memory_ == null)
			{
				Memory = new ResourceStatus();
			}
			Memory.MergeFrom(other.Memory);
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
				if (cpu_ == null)
				{
					Cpu = new ResourceStatus();
				}
				input.ReadMessage(Cpu);
				break;
			case 18u:
				if (memory_ == null)
				{
					Memory = new ResourceStatus();
				}
				input.ReadMessage(Memory);
				break;
			}
		}
	}
}
