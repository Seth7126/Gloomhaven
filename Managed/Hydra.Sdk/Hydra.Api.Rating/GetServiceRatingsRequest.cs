using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class GetServiceRatingsRequest : IMessage<GetServiceRatingsRequest>, IMessage, IEquatable<GetServiceRatingsRequest>, IDeepCloneable<GetServiceRatingsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetServiceRatingsRequest> _parser = new MessageParser<GetServiceRatingsRequest>(() => new GetServiceRatingsRequest());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int RatingIdFieldNumber = 2;

	private string ratingId_ = "";

	public const int UserIdsFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_userIds_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> userIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<GetServiceRatingsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[12];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string RatingId
	{
		get
		{
			return ratingId_;
		}
		set
		{
			ratingId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> UserIds => userIds_;

	[DebuggerNonUserCode]
	public GetServiceRatingsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetServiceRatingsRequest(GetServiceRatingsRequest other)
		: this()
	{
		titleId_ = other.titleId_;
		ratingId_ = other.ratingId_;
		userIds_ = other.userIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetServiceRatingsRequest Clone()
	{
		return new GetServiceRatingsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetServiceRatingsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetServiceRatingsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (RatingId != other.RatingId)
		{
			return false;
		}
		if (!userIds_.Equals(other.userIds_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (RatingId.Length != 0)
		{
			num ^= RatingId.GetHashCode();
		}
		num ^= userIds_.GetHashCode();
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
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TitleId);
		}
		if (RatingId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(RatingId);
		}
		userIds_.WriteTo(ref output, _repeated_userIds_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (RatingId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RatingId);
		}
		num += userIds_.CalculateSize(_repeated_userIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetServiceRatingsRequest other)
	{
		if (other != null)
		{
			if (other.TitleId.Length != 0)
			{
				TitleId = other.TitleId;
			}
			if (other.RatingId.Length != 0)
			{
				RatingId = other.RatingId;
			}
			userIds_.Add(other.userIds_);
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
				TitleId = input.ReadString();
				break;
			case 18u:
				RatingId = input.ReadString();
				break;
			case 26u:
				userIds_.AddEntriesFrom(ref input, _repeated_userIds_codec);
				break;
			}
		}
	}
}
