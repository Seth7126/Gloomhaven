using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Leaderboards;

public sealed class UpdateEntry : IMessage<UpdateEntry>, IMessage, IEquatable<UpdateEntry>, IDeepCloneable<UpdateEntry>, IBufferMessage
{
	private static readonly MessageParser<UpdateEntry> _parser = new MessageParser<UpdateEntry>(() => new UpdateEntry());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int ValueFieldNumber = 2;

	private double value_;

	public const int CustomDataFieldNumber = 3;

	private string customData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<UpdateEntry> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public double Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string CustomData
	{
		get
		{
			return customData_;
		}
		set
		{
			customData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UpdateEntry()
	{
	}

	[DebuggerNonUserCode]
	public UpdateEntry(UpdateEntry other)
		: this()
	{
		userId_ = other.userId_;
		value_ = other.value_;
		customData_ = other.customData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UpdateEntry Clone()
	{
		return new UpdateEntry(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UpdateEntry);
	}

	[DebuggerNonUserCode]
	public bool Equals(UpdateEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Value, other.Value))
		{
			return false;
		}
		if (CustomData != other.CustomData)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
		}
		if (Value != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Value);
		}
		if (CustomData.Length != 0)
		{
			num ^= CustomData.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserId);
		}
		if (Value != 0.0)
		{
			output.WriteRawTag(17);
			output.WriteDouble(Value);
		}
		if (CustomData.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(CustomData);
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
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		if (Value != 0.0)
		{
			num += 9;
		}
		if (CustomData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CustomData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UpdateEntry other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.Value != 0.0)
			{
				Value = other.Value;
			}
			if (other.CustomData.Length != 0)
			{
				CustomData = other.CustomData;
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
				UserId = input.ReadString();
				break;
			case 17u:
				Value = input.ReadDouble();
				break;
			case 26u:
				CustomData = input.ReadString();
				break;
			}
		}
	}
}
