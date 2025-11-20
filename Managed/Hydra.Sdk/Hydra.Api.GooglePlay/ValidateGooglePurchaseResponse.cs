using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GooglePlay;

public sealed class ValidateGooglePurchaseResponse : IMessage<ValidateGooglePurchaseResponse>, IMessage, IEquatable<ValidateGooglePurchaseResponse>, IDeepCloneable<ValidateGooglePurchaseResponse>, IBufferMessage
{
	private static readonly MessageParser<ValidateGooglePurchaseResponse> _parser = new MessageParser<ValidateGooglePurchaseResponse>(() => new ValidateGooglePurchaseResponse());

	private UnknownFieldSet _unknownFields;

	public const int PurchaseInfoFieldNumber = 1;

	private GooglePlayPurchaseInfo purchaseInfo_;

	[DebuggerNonUserCode]
	public static MessageParser<ValidateGooglePurchaseResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GooglePlayContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public GooglePlayPurchaseInfo PurchaseInfo
	{
		get
		{
			return purchaseInfo_;
		}
		set
		{
			purchaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ValidateGooglePurchaseResponse()
	{
	}

	[DebuggerNonUserCode]
	public ValidateGooglePurchaseResponse(ValidateGooglePurchaseResponse other)
		: this()
	{
		purchaseInfo_ = ((other.purchaseInfo_ != null) ? other.purchaseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ValidateGooglePurchaseResponse Clone()
	{
		return new ValidateGooglePurchaseResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ValidateGooglePurchaseResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ValidateGooglePurchaseResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(PurchaseInfo, other.PurchaseInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (purchaseInfo_ != null)
		{
			num ^= PurchaseInfo.GetHashCode();
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
		if (purchaseInfo_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(PurchaseInfo);
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
		if (purchaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PurchaseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ValidateGooglePurchaseResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.purchaseInfo_ != null)
		{
			if (purchaseInfo_ == null)
			{
				PurchaseInfo = new GooglePlayPurchaseInfo();
			}
			PurchaseInfo.MergeFrom(other.PurchaseInfo);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (purchaseInfo_ == null)
			{
				PurchaseInfo = new GooglePlayPurchaseInfo();
			}
			input.ReadMessage(PurchaseInfo);
		}
	}
}
