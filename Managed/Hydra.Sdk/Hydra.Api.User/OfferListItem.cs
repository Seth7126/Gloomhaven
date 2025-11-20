using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class OfferListItem : IMessage<OfferListItem>, IMessage, IEquatable<OfferListItem>, IDeepCloneable<OfferListItem>, IBufferMessage
{
	private static readonly MessageParser<OfferListItem> _parser = new MessageParser<OfferListItem>(() => new OfferListItem());

	private UnknownFieldSet _unknownFields;

	public const int ReferenceIdFieldNumber = 1;

	private string referenceId_ = "";

	public const int OfferIdFieldNumber = 2;

	private string offerId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<OfferListItem> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[12];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ReferenceId
	{
		get
		{
			return referenceId_;
		}
		set
		{
			referenceId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string OfferId
	{
		get
		{
			return offerId_;
		}
		set
		{
			offerId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public OfferListItem()
	{
	}

	[DebuggerNonUserCode]
	public OfferListItem(OfferListItem other)
		: this()
	{
		referenceId_ = other.referenceId_;
		offerId_ = other.offerId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public OfferListItem Clone()
	{
		return new OfferListItem(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as OfferListItem);
	}

	[DebuggerNonUserCode]
	public bool Equals(OfferListItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ReferenceId != other.ReferenceId)
		{
			return false;
		}
		if (OfferId != other.OfferId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ReferenceId.Length != 0)
		{
			num ^= ReferenceId.GetHashCode();
		}
		if (OfferId.Length != 0)
		{
			num ^= OfferId.GetHashCode();
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
		if (ReferenceId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ReferenceId);
		}
		if (OfferId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(OfferId);
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
		if (ReferenceId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ReferenceId);
		}
		if (OfferId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OfferId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(OfferListItem other)
	{
		if (other != null)
		{
			if (other.ReferenceId.Length != 0)
			{
				ReferenceId = other.ReferenceId;
			}
			if (other.OfferId.Length != 0)
			{
				OfferId = other.OfferId;
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
				ReferenceId = input.ReadString();
				break;
			case 18u:
				OfferId = input.ReadString();
				break;
			}
		}
	}
}
