using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Rating;

public sealed class UpdateCustomRatingsUserRequest : IMessage<UpdateCustomRatingsUserRequest>, IMessage, IEquatable<UpdateCustomRatingsUserRequest>, IDeepCloneable<UpdateCustomRatingsUserRequest>, IBufferMessage
{
	private static readonly MessageParser<UpdateCustomRatingsUserRequest> _parser = new MessageParser<UpdateCustomRatingsUserRequest>(() => new UpdateCustomRatingsUserRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int SessionIdFieldNumber = 2;

	private string sessionId_ = "";

	public const int RatingIdFieldNumber = 3;

	private string ratingId_ = "";

	public const int UpdateFieldNumber = 4;

	private static readonly FieldCodec<CustomRatingUpdate> _repeated_update_codec = FieldCodec.ForMessage(34u, CustomRatingUpdate.Parser);

	private readonly RepeatedField<CustomRatingUpdate> update_ = new RepeatedField<CustomRatingUpdate>();

	[DebuggerNonUserCode]
	public static MessageParser<UpdateCustomRatingsUserRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[20];

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
	public string SessionId
	{
		get
		{
			return sessionId_;
		}
		set
		{
			sessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public RepeatedField<CustomRatingUpdate> Update => update_;

	[DebuggerNonUserCode]
	public UpdateCustomRatingsUserRequest()
	{
	}

	[DebuggerNonUserCode]
	public UpdateCustomRatingsUserRequest(UpdateCustomRatingsUserRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		sessionId_ = other.sessionId_;
		ratingId_ = other.ratingId_;
		update_ = other.update_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UpdateCustomRatingsUserRequest Clone()
	{
		return new UpdateCustomRatingsUserRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UpdateCustomRatingsUserRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UpdateCustomRatingsUserRequest other)
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
		if (SessionId != other.SessionId)
		{
			return false;
		}
		if (RatingId != other.RatingId)
		{
			return false;
		}
		if (!update_.Equals(other.update_))
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
		if (SessionId.Length != 0)
		{
			num ^= SessionId.GetHashCode();
		}
		if (RatingId.Length != 0)
		{
			num ^= RatingId.GetHashCode();
		}
		num ^= update_.GetHashCode();
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
		if (SessionId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(SessionId);
		}
		if (RatingId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(RatingId);
		}
		update_.WriteTo(ref output, _repeated_update_codec);
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
		if (SessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SessionId);
		}
		if (RatingId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RatingId);
		}
		num += update_.CalculateSize(_repeated_update_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UpdateCustomRatingsUserRequest other)
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
		if (other.SessionId.Length != 0)
		{
			SessionId = other.SessionId;
		}
		if (other.RatingId.Length != 0)
		{
			RatingId = other.RatingId;
		}
		update_.Add(other.update_);
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
			case 18u:
				SessionId = input.ReadString();
				break;
			case 26u:
				RatingId = input.ReadString();
				break;
			case 34u:
				update_.AddEntriesFrom(ref input, _repeated_update_codec);
				break;
			}
		}
	}
}
