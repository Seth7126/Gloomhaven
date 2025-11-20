using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class GetRatingsServerResponse : IMessage<GetRatingsServerResponse>, IMessage, IEquatable<GetRatingsServerResponse>, IDeepCloneable<GetRatingsServerResponse>, IBufferMessage
{
	private static readonly MessageParser<GetRatingsServerResponse> _parser = new MessageParser<GetRatingsServerResponse>(() => new GetRatingsServerResponse());

	private UnknownFieldSet _unknownFields;

	public const int RatingsFieldNumber = 1;

	private static readonly FieldCodec<UserRating> _repeated_ratings_codec = FieldCodec.ForMessage(10u, UserRating.Parser);

	private readonly RepeatedField<UserRating> ratings_ = new RepeatedField<UserRating>();

	[DebuggerNonUserCode]
	public static MessageParser<GetRatingsServerResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[11];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<UserRating> Ratings => ratings_;

	[DebuggerNonUserCode]
	public GetRatingsServerResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetRatingsServerResponse(GetRatingsServerResponse other)
		: this()
	{
		ratings_ = other.ratings_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetRatingsServerResponse Clone()
	{
		return new GetRatingsServerResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetRatingsServerResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetRatingsServerResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ratings_.Equals(other.ratings_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= ratings_.GetHashCode();
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
		ratings_.WriteTo(ref output, _repeated_ratings_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += ratings_.CalculateSize(_repeated_ratings_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetRatingsServerResponse other)
	{
		if (other != null)
		{
			ratings_.Add(other.ratings_);
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
				ratings_.AddEntriesFrom(ref input, _repeated_ratings_codec);
			}
		}
	}
}
