using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GooglePlay;

public sealed class ValidateGooglePurchaseRequest : IMessage<ValidateGooglePurchaseRequest>, IMessage, IEquatable<ValidateGooglePurchaseRequest>, IDeepCloneable<ValidateGooglePurchaseRequest>, IBufferMessage
{
	private static readonly MessageParser<ValidateGooglePurchaseRequest> _parser = new MessageParser<ValidateGooglePurchaseRequest>(() => new ValidateGooglePurchaseRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int PurchaseFieldNumber = 2;

	private GooglePlayPurchase purchase_;

	[DebuggerNonUserCode]
	public static MessageParser<ValidateGooglePurchaseRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GooglePlayContractsReflection.Descriptor.MessageTypes[0];

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
	public GooglePlayPurchase Purchase
	{
		get
		{
			return purchase_;
		}
		set
		{
			purchase_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ValidateGooglePurchaseRequest()
	{
	}

	[DebuggerNonUserCode]
	public ValidateGooglePurchaseRequest(ValidateGooglePurchaseRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		purchase_ = ((other.purchase_ != null) ? other.purchase_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ValidateGooglePurchaseRequest Clone()
	{
		return new ValidateGooglePurchaseRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ValidateGooglePurchaseRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ValidateGooglePurchaseRequest other)
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
		if (!object.Equals(Purchase, other.Purchase))
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
		if (purchase_ != null)
		{
			num ^= Purchase.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (purchase_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Purchase);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (purchase_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Purchase);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ValidateGooglePurchaseRequest other)
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
		if (other.purchase_ != null)
		{
			if (purchase_ == null)
			{
				Purchase = new GooglePlayPurchase();
			}
			Purchase.MergeFrom(other.Purchase);
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				if (purchase_ == null)
				{
					Purchase = new GooglePlayPurchase();
				}
				input.ReadMessage(Purchase);
				break;
			}
		}
	}
}
