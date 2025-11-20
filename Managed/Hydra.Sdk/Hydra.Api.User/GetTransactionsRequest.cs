using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User;

public sealed class GetTransactionsRequest : IMessage<GetTransactionsRequest>, IMessage, IEquatable<GetTransactionsRequest>, IDeepCloneable<GetTransactionsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetTransactionsRequest> _parser = new MessageParser<GetTransactionsRequest>(() => new GetTransactionsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int FromTransactionIdFieldNumber = 2;

	private long fromTransactionId_;

	public const int CountFieldNumber = 3;

	private long count_;

	[DebuggerNonUserCode]
	public static MessageParser<GetTransactionsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext Context
	{
		get
		{
			return context_;
		}
		set
		{
			context_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long FromTransactionId
	{
		get
		{
			return fromTransactionId_;
		}
		set
		{
			fromTransactionId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetTransactionsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetTransactionsRequest(GetTransactionsRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		fromTransactionId_ = other.fromTransactionId_;
		count_ = other.count_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetTransactionsRequest Clone()
	{
		return new GetTransactionsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetTransactionsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetTransactionsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Context, other.Context))
		{
			return false;
		}
		if (FromTransactionId != other.FromTransactionId)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (context_ != null)
		{
			num ^= Context.GetHashCode();
		}
		if (FromTransactionId != 0)
		{
			num ^= FromTransactionId.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		if (FromTransactionId != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(FromTransactionId);
		}
		if (Count != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt64(Count);
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
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		if (FromTransactionId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FromTransactionId);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Count);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetTransactionsRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new UserContext();
			}
			Context.MergeFrom(other.Context);
		}
		if (other.FromTransactionId != 0)
		{
			FromTransactionId = other.FromTransactionId;
		}
		if (other.Count != 0)
		{
			Count = other.Count;
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
				if (context_ == null)
				{
					Context = new UserContext();
				}
				input.ReadMessage(Context);
				break;
			case 16u:
				FromTransactionId = input.ReadInt64();
				break;
			case 24u:
				Count = input.ReadInt64();
				break;
			}
		}
	}
}
