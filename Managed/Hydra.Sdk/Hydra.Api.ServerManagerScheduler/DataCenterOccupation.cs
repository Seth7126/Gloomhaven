using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public sealed class DataCenterOccupation : IMessage<DataCenterOccupation>, IMessage, IEquatable<DataCenterOccupation>, IDeepCloneable<DataCenterOccupation>, IBufferMessage
{
	private static readonly MessageParser<DataCenterOccupation> _parser = new MessageParser<DataCenterOccupation>(() => new DataCenterOccupation());

	private UnknownFieldSet _unknownFields;

	public const int DataCenterIdFieldNumber = 1;

	private string dataCenterId_ = "";

	public const int OccupationFieldNumber = 2;

	private int occupation_;

	[DebuggerNonUserCode]
	public static MessageParser<DataCenterOccupation> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => DataCenterOccupationReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int Occupation
	{
		get
		{
			return occupation_;
		}
		set
		{
			occupation_ = value;
		}
	}

	[DebuggerNonUserCode]
	public DataCenterOccupation()
	{
	}

	[DebuggerNonUserCode]
	public DataCenterOccupation(DataCenterOccupation other)
		: this()
	{
		dataCenterId_ = other.dataCenterId_;
		occupation_ = other.occupation_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public DataCenterOccupation Clone()
	{
		return new DataCenterOccupation(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as DataCenterOccupation);
	}

	[DebuggerNonUserCode]
	public bool Equals(DataCenterOccupation other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DataCenterId != other.DataCenterId)
		{
			return false;
		}
		if (Occupation != other.Occupation)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (DataCenterId.Length != 0)
		{
			num ^= DataCenterId.GetHashCode();
		}
		if (Occupation != 0)
		{
			num ^= Occupation.GetHashCode();
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
		if (DataCenterId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(DataCenterId);
		}
		if (Occupation != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Occupation);
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
		if (DataCenterId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DataCenterId);
		}
		if (Occupation != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Occupation);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(DataCenterOccupation other)
	{
		if (other != null)
		{
			if (other.DataCenterId.Length != 0)
			{
				DataCenterId = other.DataCenterId;
			}
			if (other.Occupation != 0)
			{
				Occupation = other.Occupation;
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
				DataCenterId = input.ReadString();
				break;
			case 16u:
				Occupation = input.ReadInt32();
				break;
			}
		}
	}
}
