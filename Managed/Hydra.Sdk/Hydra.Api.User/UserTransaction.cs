using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.User;

public sealed class UserTransaction : IMessage<UserTransaction>, IMessage, IEquatable<UserTransaction>, IDeepCloneable<UserTransaction>, IBufferMessage
{
	private static readonly MessageParser<UserTransaction> _parser = new MessageParser<UserTransaction>(() => new UserTransaction());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int OfferIdFieldNumber = 2;

	private string offerId_ = "";

	public const int ReferenceIdFieldNumber = 3;

	private string referenceId_ = "";

	public const int TransactionItemsFieldNumber = 4;

	private static readonly FieldCodec<UserTransactionItem> _repeated_transactionItems_codec = FieldCodec.ForMessage(34u, UserTransactionItem.Parser);

	private readonly RepeatedField<UserTransactionItem> transactionItems_ = new RepeatedField<UserTransactionItem>();

	public const int ExtendedInfoFieldNumber = 5;

	private static readonly MapField<string, string>.Codec _map_extendedInfo_codec = new MapField<string, string>.Codec(FieldCodec.ForString(10u, ""), FieldCodec.ForString(18u, ""), 42u);

	private readonly MapField<string, string> extendedInfo_ = new MapField<string, string>();

	public const int CreatedAtFieldNumber = 6;

	private Timestamp createdAt_;

	[DebuggerNonUserCode]
	public static MessageParser<UserTransaction> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[16];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = value;
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
	public RepeatedField<UserTransactionItem> TransactionItems => transactionItems_;

	[DebuggerNonUserCode]
	public MapField<string, string> ExtendedInfo => extendedInfo_;

	[DebuggerNonUserCode]
	public Timestamp CreatedAt
	{
		get
		{
			return createdAt_;
		}
		set
		{
			createdAt_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserTransaction()
	{
	}

	[DebuggerNonUserCode]
	public UserTransaction(UserTransaction other)
		: this()
	{
		id_ = other.id_;
		offerId_ = other.offerId_;
		referenceId_ = other.referenceId_;
		transactionItems_ = other.transactionItems_.Clone();
		extendedInfo_ = other.extendedInfo_.Clone();
		createdAt_ = ((other.createdAt_ != null) ? other.createdAt_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserTransaction Clone()
	{
		return new UserTransaction(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserTransaction);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserTransaction other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (OfferId != other.OfferId)
		{
			return false;
		}
		if (ReferenceId != other.ReferenceId)
		{
			return false;
		}
		if (!transactionItems_.Equals(other.transactionItems_))
		{
			return false;
		}
		if (!ExtendedInfo.Equals(other.ExtendedInfo))
		{
			return false;
		}
		if (!object.Equals(CreatedAt, other.CreatedAt))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (OfferId.Length != 0)
		{
			num ^= OfferId.GetHashCode();
		}
		if (ReferenceId.Length != 0)
		{
			num ^= ReferenceId.GetHashCode();
		}
		num ^= transactionItems_.GetHashCode();
		num ^= ExtendedInfo.GetHashCode();
		if (createdAt_ != null)
		{
			num ^= CreatedAt.GetHashCode();
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
		if (Id != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Id);
		}
		if (OfferId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(OfferId);
		}
		if (ReferenceId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ReferenceId);
		}
		transactionItems_.WriteTo(ref output, _repeated_transactionItems_codec);
		extendedInfo_.WriteTo(ref output, _map_extendedInfo_codec);
		if (createdAt_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(CreatedAt);
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
		if (Id != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Id);
		}
		if (OfferId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OfferId);
		}
		if (ReferenceId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ReferenceId);
		}
		num += transactionItems_.CalculateSize(_repeated_transactionItems_codec);
		num += extendedInfo_.CalculateSize(_map_extendedInfo_codec);
		if (createdAt_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CreatedAt);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserTransaction other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0)
		{
			Id = other.Id;
		}
		if (other.OfferId.Length != 0)
		{
			OfferId = other.OfferId;
		}
		if (other.ReferenceId.Length != 0)
		{
			ReferenceId = other.ReferenceId;
		}
		transactionItems_.Add(other.transactionItems_);
		extendedInfo_.Add(other.extendedInfo_);
		if (other.createdAt_ != null)
		{
			if (createdAt_ == null)
			{
				CreatedAt = new Timestamp();
			}
			CreatedAt.MergeFrom(other.CreatedAt);
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
			case 8u:
				Id = input.ReadInt64();
				break;
			case 18u:
				OfferId = input.ReadString();
				break;
			case 26u:
				ReferenceId = input.ReadString();
				break;
			case 34u:
				transactionItems_.AddEntriesFrom(ref input, _repeated_transactionItems_codec);
				break;
			case 42u:
				extendedInfo_.AddEntriesFrom(ref input, _map_extendedInfo_codec);
				break;
			case 50u:
				if (createdAt_ == null)
				{
					CreatedAt = new Timestamp();
				}
				input.ReadMessage(CreatedAt);
				break;
			}
		}
	}
}
