using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User.Zen;

public sealed class DiscountOffer : IMessage<DiscountOffer>, IMessage, IEquatable<DiscountOffer>, IDeepCloneable<DiscountOffer>, IBufferMessage
{
	private static readonly MessageParser<DiscountOffer> _parser = new MessageParser<DiscountOffer>(() => new DiscountOffer());

	private UnknownFieldSet _unknownFields;

	public const int OfferFieldNumber = 1;

	private OfferListItem offer_;

	public const int PriceFieldNumber = 2;

	private Int64Value price_;

	public const int DiscountsFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_discounts_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> discounts_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<DiscountOffer> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenEconomyApiContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public OfferListItem Offer
	{
		get
		{
			return offer_;
		}
		set
		{
			offer_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Int64Value Price
	{
		get
		{
			return price_;
		}
		set
		{
			price_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> Discounts => discounts_;

	[DebuggerNonUserCode]
	public DiscountOffer()
	{
	}

	[DebuggerNonUserCode]
	public DiscountOffer(DiscountOffer other)
		: this()
	{
		offer_ = ((other.offer_ != null) ? other.offer_.Clone() : null);
		price_ = ((other.price_ != null) ? other.price_.Clone() : null);
		discounts_ = other.discounts_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public DiscountOffer Clone()
	{
		return new DiscountOffer(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as DiscountOffer);
	}

	[DebuggerNonUserCode]
	public bool Equals(DiscountOffer other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Offer, other.Offer))
		{
			return false;
		}
		if (!object.Equals(Price, other.Price))
		{
			return false;
		}
		if (!discounts_.Equals(other.discounts_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (offer_ != null)
		{
			num ^= Offer.GetHashCode();
		}
		if (price_ != null)
		{
			num ^= Price.GetHashCode();
		}
		num ^= discounts_.GetHashCode();
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
		if (offer_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Offer);
		}
		if (price_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Price);
		}
		discounts_.WriteTo(ref output, _repeated_discounts_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (offer_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Offer);
		}
		if (price_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Price);
		}
		num += discounts_.CalculateSize(_repeated_discounts_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(DiscountOffer other)
	{
		if (other == null)
		{
			return;
		}
		if (other.offer_ != null)
		{
			if (offer_ == null)
			{
				Offer = new OfferListItem();
			}
			Offer.MergeFrom(other.Offer);
		}
		if (other.price_ != null)
		{
			if (price_ == null)
			{
				Price = new Int64Value();
			}
			Price.MergeFrom(other.Price);
		}
		discounts_.Add(other.discounts_);
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
				if (offer_ == null)
				{
					Offer = new OfferListItem();
				}
				input.ReadMessage(Offer);
				break;
			case 18u:
				if (price_ == null)
				{
					Price = new Int64Value();
				}
				input.ReadMessage(Price);
				break;
			case 26u:
				discounts_.AddEntriesFrom(ref input, _repeated_discounts_codec);
				break;
			}
		}
	}
}
