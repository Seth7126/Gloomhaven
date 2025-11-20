using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Rating;

public sealed class GetRatingsServerRequest : IMessage<GetRatingsServerRequest>, IMessage, IEquatable<GetRatingsServerRequest>, IDeepCloneable<GetRatingsServerRequest>, IBufferMessage
{
	private static readonly MessageParser<GetRatingsServerRequest> _parser = new MessageParser<GetRatingsServerRequest>(() => new GetRatingsServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ServerContext context_;

	public const int RatingIdFieldNumber = 2;

	private string ratingId_ = "";

	public const int IncludeHistoryFieldNumber = 3;

	private bool includeHistory_;

	public const int UserIdsFieldNumber = 4;

	private static readonly FieldCodec<string> _repeated_userIds_codec = FieldCodec.ForString(34u);

	private readonly RepeatedField<string> userIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<GetRatingsServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[10];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerContext Context
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
	public bool IncludeHistory
	{
		get
		{
			return includeHistory_;
		}
		set
		{
			includeHistory_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> UserIds => userIds_;

	[DebuggerNonUserCode]
	public GetRatingsServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetRatingsServerRequest(GetRatingsServerRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		ratingId_ = other.ratingId_;
		includeHistory_ = other.includeHistory_;
		userIds_ = other.userIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetRatingsServerRequest Clone()
	{
		return new GetRatingsServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetRatingsServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetRatingsServerRequest other)
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
		if (RatingId != other.RatingId)
		{
			return false;
		}
		if (IncludeHistory != other.IncludeHistory)
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
		if (context_ != null)
		{
			num ^= Context.GetHashCode();
		}
		if (RatingId.Length != 0)
		{
			num ^= RatingId.GetHashCode();
		}
		if (IncludeHistory)
		{
			num ^= IncludeHistory.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		if (RatingId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(RatingId);
		}
		if (IncludeHistory)
		{
			output.WriteRawTag(24);
			output.WriteBool(IncludeHistory);
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
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		if (RatingId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RatingId);
		}
		if (IncludeHistory)
		{
			num += 2;
		}
		num += userIds_.CalculateSize(_repeated_userIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetRatingsServerRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new ServerContext();
			}
			Context.MergeFrom(other.Context);
		}
		if (other.RatingId.Length != 0)
		{
			RatingId = other.RatingId;
		}
		if (other.IncludeHistory)
		{
			IncludeHistory = other.IncludeHistory;
		}
		userIds_.Add(other.userIds_);
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
					Context = new ServerContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				RatingId = input.ReadString();
				break;
			case 24u:
				IncludeHistory = input.ReadBool();
				break;
			case 34u:
				userIds_.AddEntriesFrom(ref input, _repeated_userIds_codec);
				break;
			}
		}
	}
}
