using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GameConfiguration;

public sealed class ComponentDataResult : IMessage<ComponentDataResult>, IMessage, IEquatable<ComponentDataResult>, IDeepCloneable<ComponentDataResult>, IBufferMessage
{
	private static readonly MessageParser<ComponentDataResult> _parser = new MessageParser<ComponentDataResult>(() => new ComponentDataResult());

	private UnknownFieldSet _unknownFields;

	public const int ComponentSnapshotFieldNumber = 1;

	private ConfigurationComponentSnapshot componentSnapshot_;

	public const int DataResultFieldNumber = 2;

	private ByteString dataResult_ = ByteString.Empty;

	public const int DataTypeFieldNumber = 3;

	private ComponentDataType dataType_ = ComponentDataType.Json;

	[DebuggerNonUserCode]
	public static MessageParser<ComponentDataResult> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationContractsReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ConfigurationComponentSnapshot ComponentSnapshot
	{
		get
		{
			return componentSnapshot_;
		}
		set
		{
			componentSnapshot_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString DataResult
	{
		get
		{
			return dataResult_;
		}
		set
		{
			dataResult_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ComponentDataType DataType
	{
		get
		{
			return dataType_;
		}
		set
		{
			dataType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ComponentDataResult()
	{
	}

	[DebuggerNonUserCode]
	public ComponentDataResult(ComponentDataResult other)
		: this()
	{
		componentSnapshot_ = ((other.componentSnapshot_ != null) ? other.componentSnapshot_.Clone() : null);
		dataResult_ = other.dataResult_;
		dataType_ = other.dataType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ComponentDataResult Clone()
	{
		return new ComponentDataResult(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ComponentDataResult);
	}

	[DebuggerNonUserCode]
	public bool Equals(ComponentDataResult other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ComponentSnapshot, other.ComponentSnapshot))
		{
			return false;
		}
		if (DataResult != other.DataResult)
		{
			return false;
		}
		if (DataType != other.DataType)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (componentSnapshot_ != null)
		{
			num ^= ComponentSnapshot.GetHashCode();
		}
		if (DataResult.Length != 0)
		{
			num ^= DataResult.GetHashCode();
		}
		if (DataType != ComponentDataType.Json)
		{
			num ^= DataType.GetHashCode();
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
		if (componentSnapshot_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ComponentSnapshot);
		}
		if (DataResult.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(DataResult);
		}
		if (DataType != ComponentDataType.Json)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)DataType);
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
		if (componentSnapshot_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ComponentSnapshot);
		}
		if (DataResult.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(DataResult);
		}
		if (DataType != ComponentDataType.Json)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DataType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ComponentDataResult other)
	{
		if (other == null)
		{
			return;
		}
		if (other.componentSnapshot_ != null)
		{
			if (componentSnapshot_ == null)
			{
				ComponentSnapshot = new ConfigurationComponentSnapshot();
			}
			ComponentSnapshot.MergeFrom(other.ComponentSnapshot);
		}
		if (other.DataResult.Length != 0)
		{
			DataResult = other.DataResult;
		}
		if (other.DataType != ComponentDataType.Json)
		{
			DataType = other.DataType;
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
				if (componentSnapshot_ == null)
				{
					ComponentSnapshot = new ConfigurationComponentSnapshot();
				}
				input.ReadMessage(ComponentSnapshot);
				break;
			case 18u:
				DataResult = input.ReadBytes();
				break;
			case 24u:
				DataType = (ComponentDataType)input.ReadEnum();
				break;
			}
		}
	}
}
