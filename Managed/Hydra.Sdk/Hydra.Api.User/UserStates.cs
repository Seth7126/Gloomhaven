using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class UserStates : IMessage<UserStates>, IMessage, IEquatable<UserStates>, IDeepCloneable<UserStates>, IBufferMessage
{
	private static readonly MessageParser<UserStates> _parser = new MessageParser<UserStates>(() => new UserStates());

	private UnknownFieldSet _unknownFields;

	public const int LastTransactionIdFieldNumber = 1;

	private long lastTransactionId_;

	public const int ItemsFieldNumber = 2;

	private static readonly FieldCodec<UserState> _repeated_items_codec = FieldCodec.ForMessage(18u, UserState.Parser);

	private readonly RepeatedField<UserState> items_ = new RepeatedField<UserState>();

	[DebuggerNonUserCode]
	public static MessageParser<UserStates> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[13];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long LastTransactionId
	{
		get
		{
			return lastTransactionId_;
		}
		set
		{
			lastTransactionId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<UserState> Items => items_;

	[DebuggerNonUserCode]
	public UserStates()
	{
	}

	[DebuggerNonUserCode]
	public UserStates(UserStates other)
		: this()
	{
		lastTransactionId_ = other.lastTransactionId_;
		items_ = other.items_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserStates Clone()
	{
		return new UserStates(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserStates);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserStates other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LastTransactionId != other.LastTransactionId)
		{
			return false;
		}
		if (!items_.Equals(other.items_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (LastTransactionId != 0)
		{
			num ^= LastTransactionId.GetHashCode();
		}
		num ^= items_.GetHashCode();
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
		if (LastTransactionId != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(LastTransactionId);
		}
		items_.WriteTo(ref output, _repeated_items_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (LastTransactionId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(LastTransactionId);
		}
		num += items_.CalculateSize(_repeated_items_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserStates other)
	{
		if (other != null)
		{
			if (other.LastTransactionId != 0)
			{
				LastTransactionId = other.LastTransactionId;
			}
			items_.Add(other.items_);
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
			case 8u:
				LastTransactionId = input.ReadInt64();
				break;
			case 18u:
				items_.AddEntriesFrom(ref input, _repeated_items_codec);
				break;
			}
		}
	}
}
