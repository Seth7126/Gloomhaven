using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GooglePlay;

public sealed class GooglePlayPurchase : IMessage<GooglePlayPurchase>, IMessage, IEquatable<GooglePlayPurchase>, IDeepCloneable<GooglePlayPurchase>, IBufferMessage
{
	private static readonly MessageParser<GooglePlayPurchase> _parser = new MessageParser<GooglePlayPurchase>(() => new GooglePlayPurchase());

	private UnknownFieldSet _unknownFields;

	public const int ProductIdFieldNumber = 1;

	private string productId_ = "";

	public const int PurchaseTokenFieldNumber = 2;

	private string purchaseToken_ = "";

	public const int CurrencyCodeFieldNumber = 3;

	private string currencyCode_ = "";

	public const int NumericPriceFieldNumber = 4;

	private float numericPrice_;

	public const int SaleInstanceIdFieldNumber = 5;

	private string saleInstanceId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GooglePlayPurchase> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GooglePlayContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ProductId
	{
		get
		{
			return productId_;
		}
		set
		{
			productId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string PurchaseToken
	{
		get
		{
			return purchaseToken_;
		}
		set
		{
			purchaseToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string CurrencyCode
	{
		get
		{
			return currencyCode_;
		}
		set
		{
			currencyCode_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public float NumericPrice
	{
		get
		{
			return numericPrice_;
		}
		set
		{
			numericPrice_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string SaleInstanceId
	{
		get
		{
			return saleInstanceId_;
		}
		set
		{
			saleInstanceId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GooglePlayPurchase()
	{
	}

	[DebuggerNonUserCode]
	public GooglePlayPurchase(GooglePlayPurchase other)
		: this()
	{
		productId_ = other.productId_;
		purchaseToken_ = other.purchaseToken_;
		currencyCode_ = other.currencyCode_;
		numericPrice_ = other.numericPrice_;
		saleInstanceId_ = other.saleInstanceId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GooglePlayPurchase Clone()
	{
		return new GooglePlayPurchase(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GooglePlayPurchase);
	}

	[DebuggerNonUserCode]
	public bool Equals(GooglePlayPurchase other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ProductId != other.ProductId)
		{
			return false;
		}
		if (PurchaseToken != other.PurchaseToken)
		{
			return false;
		}
		if (CurrencyCode != other.CurrencyCode)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(NumericPrice, other.NumericPrice))
		{
			return false;
		}
		if (SaleInstanceId != other.SaleInstanceId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ProductId.Length != 0)
		{
			num ^= ProductId.GetHashCode();
		}
		if (PurchaseToken.Length != 0)
		{
			num ^= PurchaseToken.GetHashCode();
		}
		if (CurrencyCode.Length != 0)
		{
			num ^= CurrencyCode.GetHashCode();
		}
		if (NumericPrice != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(NumericPrice);
		}
		if (SaleInstanceId.Length != 0)
		{
			num ^= SaleInstanceId.GetHashCode();
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
		if (ProductId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ProductId);
		}
		if (PurchaseToken.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(PurchaseToken);
		}
		if (CurrencyCode.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(CurrencyCode);
		}
		if (NumericPrice != 0f)
		{
			output.WriteRawTag(37);
			output.WriteFloat(NumericPrice);
		}
		if (SaleInstanceId.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(SaleInstanceId);
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
		if (ProductId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ProductId);
		}
		if (PurchaseToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PurchaseToken);
		}
		if (CurrencyCode.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CurrencyCode);
		}
		if (NumericPrice != 0f)
		{
			num += 5;
		}
		if (SaleInstanceId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SaleInstanceId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GooglePlayPurchase other)
	{
		if (other != null)
		{
			if (other.ProductId.Length != 0)
			{
				ProductId = other.ProductId;
			}
			if (other.PurchaseToken.Length != 0)
			{
				PurchaseToken = other.PurchaseToken;
			}
			if (other.CurrencyCode.Length != 0)
			{
				CurrencyCode = other.CurrencyCode;
			}
			if (other.NumericPrice != 0f)
			{
				NumericPrice = other.NumericPrice;
			}
			if (other.SaleInstanceId.Length != 0)
			{
				SaleInstanceId = other.SaleInstanceId;
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
				ProductId = input.ReadString();
				break;
			case 18u:
				PurchaseToken = input.ReadString();
				break;
			case 26u:
				CurrencyCode = input.ReadString();
				break;
			case 37u:
				NumericPrice = input.ReadFloat();
				break;
			case 42u:
				SaleInstanceId = input.ReadString();
				break;
			}
		}
	}
}
