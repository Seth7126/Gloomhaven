using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class UserTransactionsUpdate : IMessage<UserTransactionsUpdate>, IMessage, IEquatable<UserTransactionsUpdate>, IDeepCloneable<UserTransactionsUpdate>, IBufferMessage
{
	private static readonly MessageParser<UserTransactionsUpdate> _parser = new MessageParser<UserTransactionsUpdate>(() => new UserTransactionsUpdate());

	private UnknownFieldSet _unknownFields;

	public const int TransactionsFieldNumber = 1;

	private static readonly FieldCodec<UserTransaction> _repeated_transactions_codec = FieldCodec.ForMessage(10u, UserTransaction.Parser);

	private readonly RepeatedField<UserTransaction> transactions_ = new RepeatedField<UserTransaction>();

	[DebuggerNonUserCode]
	public static MessageParser<UserTransactionsUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[15];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<UserTransaction> Transactions => transactions_;

	[DebuggerNonUserCode]
	public UserTransactionsUpdate()
	{
	}

	[DebuggerNonUserCode]
	public UserTransactionsUpdate(UserTransactionsUpdate other)
		: this()
	{
		transactions_ = other.transactions_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserTransactionsUpdate Clone()
	{
		return new UserTransactionsUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserTransactionsUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserTransactionsUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!transactions_.Equals(other.transactions_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= transactions_.GetHashCode();
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
		transactions_.WriteTo(ref output, _repeated_transactions_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += transactions_.CalculateSize(_repeated_transactions_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserTransactionsUpdate other)
	{
		if (other != null)
		{
			transactions_.Add(other.transactions_);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				transactions_.AddEntriesFrom(ref input, _repeated_transactions_codec);
			}
		}
	}
}
