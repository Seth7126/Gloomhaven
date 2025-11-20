using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class GetTransactionsResponse : IMessage<GetTransactionsResponse>, IMessage, IEquatable<GetTransactionsResponse>, IDeepCloneable<GetTransactionsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetTransactionsResponse> _parser = new MessageParser<GetTransactionsResponse>(() => new GetTransactionsResponse());

	private UnknownFieldSet _unknownFields;

	public const int TransactionsFieldNumber = 1;

	private UserTransactionsUpdate transactions_;

	[DebuggerNonUserCode]
	public static MessageParser<GetTransactionsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserTransactionsUpdate Transactions
	{
		get
		{
			return transactions_;
		}
		set
		{
			transactions_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetTransactionsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetTransactionsResponse(GetTransactionsResponse other)
		: this()
	{
		transactions_ = ((other.transactions_ != null) ? other.transactions_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetTransactionsResponse Clone()
	{
		return new GetTransactionsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetTransactionsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetTransactionsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Transactions, other.Transactions))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (transactions_ != null)
		{
			num ^= Transactions.GetHashCode();
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
		if (transactions_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Transactions);
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
		if (transactions_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Transactions);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetTransactionsResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.transactions_ != null)
		{
			if (transactions_ == null)
			{
				Transactions = new UserTransactionsUpdate();
			}
			Transactions.MergeFrom(other.Transactions);
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
			if (transactions_ == null)
			{
				Transactions = new UserTransactionsUpdate();
			}
			input.ReadMessage(Transactions);
		}
	}
}
