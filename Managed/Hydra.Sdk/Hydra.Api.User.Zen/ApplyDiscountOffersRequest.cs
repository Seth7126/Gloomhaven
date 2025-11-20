using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User.Zen;

public sealed class ApplyDiscountOffersRequest : IMessage<ApplyDiscountOffersRequest>, IMessage, IEquatable<ApplyDiscountOffersRequest>, IDeepCloneable<ApplyDiscountOffersRequest>, IBufferMessage
{
	private static readonly MessageParser<ApplyDiscountOffersRequest> _parser = new MessageParser<ApplyDiscountOffersRequest>(() => new ApplyDiscountOffersRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ConfigurationContextFieldNumber = 2;

	private ConfigurationContext configurationContext_;

	public const int DiscountOffersFieldNumber = 3;

	private static readonly FieldCodec<DiscountOffer> _repeated_discountOffers_codec = FieldCodec.ForMessage(26u, DiscountOffer.Parser);

	private readonly RepeatedField<DiscountOffer> discountOffers_ = new RepeatedField<DiscountOffer>();

	[DebuggerNonUserCode]
	public static MessageParser<ApplyDiscountOffersRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenEconomyApiContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ConfigurationContext ConfigurationContext
	{
		get
		{
			return configurationContext_;
		}
		set
		{
			configurationContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<DiscountOffer> DiscountOffers => discountOffers_;

	[DebuggerNonUserCode]
	public ApplyDiscountOffersRequest()
	{
	}

	[DebuggerNonUserCode]
	public ApplyDiscountOffersRequest(ApplyDiscountOffersRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		configurationContext_ = ((other.configurationContext_ != null) ? other.configurationContext_.Clone() : null);
		discountOffers_ = other.discountOffers_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ApplyDiscountOffersRequest Clone()
	{
		return new ApplyDiscountOffersRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ApplyDiscountOffersRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ApplyDiscountOffersRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (!object.Equals(ConfigurationContext, other.ConfigurationContext))
		{
			return false;
		}
		if (!discountOffers_.Equals(other.discountOffers_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (configurationContext_ != null)
		{
			num ^= ConfigurationContext.GetHashCode();
		}
		num ^= discountOffers_.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (configurationContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ConfigurationContext);
		}
		discountOffers_.WriteTo(ref output, _repeated_discountOffers_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (configurationContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ConfigurationContext);
		}
		num += discountOffers_.CalculateSize(_repeated_discountOffers_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ApplyDiscountOffersRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		if (other.configurationContext_ != null)
		{
			if (configurationContext_ == null)
			{
				ConfigurationContext = new ConfigurationContext();
			}
			ConfigurationContext.MergeFrom(other.ConfigurationContext);
		}
		discountOffers_.Add(other.discountOffers_);
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				if (configurationContext_ == null)
				{
					ConfigurationContext = new ConfigurationContext();
				}
				input.ReadMessage(ConfigurationContext);
				break;
			case 26u:
				discountOffers_.AddEntriesFrom(ref input, _repeated_discountOffers_codec);
				break;
			}
		}
	}
}
