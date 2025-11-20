using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GooglePlay;

public sealed class GooglePlayPurchaseInfo : IMessage<GooglePlayPurchaseInfo>, IMessage, IEquatable<GooglePlayPurchaseInfo>, IDeepCloneable<GooglePlayPurchaseInfo>, IBufferMessage
{
	private static readonly MessageParser<GooglePlayPurchaseInfo> _parser = new MessageParser<GooglePlayPurchaseInfo>(() => new GooglePlayPurchaseInfo());

	private UnknownFieldSet _unknownFields;

	public const int PurchaseTypeFieldNumber = 1;

	private GooglePlayPurchaseType purchaseType_ = GooglePlayPurchaseType.Test;

	public const int OrderIdFieldNumber = 2;

	private string orderId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GooglePlayPurchaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GooglePlayContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public GooglePlayPurchaseType PurchaseType
	{
		get
		{
			return purchaseType_;
		}
		set
		{
			purchaseType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string OrderId
	{
		get
		{
			return orderId_;
		}
		set
		{
			orderId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GooglePlayPurchaseInfo()
	{
	}

	[DebuggerNonUserCode]
	public GooglePlayPurchaseInfo(GooglePlayPurchaseInfo other)
		: this()
	{
		purchaseType_ = other.purchaseType_;
		orderId_ = other.orderId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GooglePlayPurchaseInfo Clone()
	{
		return new GooglePlayPurchaseInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GooglePlayPurchaseInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(GooglePlayPurchaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PurchaseType != other.PurchaseType)
		{
			return false;
		}
		if (OrderId != other.OrderId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PurchaseType != GooglePlayPurchaseType.Test)
		{
			num ^= PurchaseType.GetHashCode();
		}
		if (OrderId.Length != 0)
		{
			num ^= OrderId.GetHashCode();
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
		if (PurchaseType != GooglePlayPurchaseType.Test)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)PurchaseType);
		}
		if (OrderId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(OrderId);
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
		if (PurchaseType != GooglePlayPurchaseType.Test)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)PurchaseType);
		}
		if (OrderId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OrderId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GooglePlayPurchaseInfo other)
	{
		if (other != null)
		{
			if (other.PurchaseType != GooglePlayPurchaseType.Test)
			{
				PurchaseType = other.PurchaseType;
			}
			if (other.OrderId.Length != 0)
			{
				OrderId = other.OrderId;
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
				PurchaseType = (GooglePlayPurchaseType)input.ReadEnum();
				break;
			case 18u:
				OrderId = input.ReadString();
				break;
			}
		}
	}
}
