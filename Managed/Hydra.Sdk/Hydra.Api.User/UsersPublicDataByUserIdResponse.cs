using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class UsersPublicDataByUserIdResponse : IMessage<UsersPublicDataByUserIdResponse>, IMessage, IEquatable<UsersPublicDataByUserIdResponse>, IDeepCloneable<UsersPublicDataByUserIdResponse>, IBufferMessage
{
	private static readonly MessageParser<UsersPublicDataByUserIdResponse> _parser = new MessageParser<UsersPublicDataByUserIdResponse>(() => new UsersPublicDataByUserIdResponse());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private static readonly FieldCodec<UserBaseData> _repeated_data_codec = FieldCodec.ForMessage(10u, UserBaseData.Parser);

	private readonly RepeatedField<UserBaseData> data_ = new RepeatedField<UserBaseData>();

	[DebuggerNonUserCode]
	public static MessageParser<UsersPublicDataByUserIdResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<UserBaseData> Data => data_;

	[DebuggerNonUserCode]
	public UsersPublicDataByUserIdResponse()
	{
	}

	[DebuggerNonUserCode]
	public UsersPublicDataByUserIdResponse(UsersPublicDataByUserIdResponse other)
		: this()
	{
		data_ = other.data_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UsersPublicDataByUserIdResponse Clone()
	{
		return new UsersPublicDataByUserIdResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UsersPublicDataByUserIdResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(UsersPublicDataByUserIdResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!data_.Equals(other.data_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= data_.GetHashCode();
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
		data_.WriteTo(ref output, _repeated_data_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += data_.CalculateSize(_repeated_data_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UsersPublicDataByUserIdResponse other)
	{
		if (other != null)
		{
			data_.Add(other.data_);
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
				data_.AddEntriesFrom(ref input, _repeated_data_codec);
			}
		}
	}
}
