using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Rating;

public sealed class UpdateRatingsUserRequest : IMessage<UpdateRatingsUserRequest>, IMessage, IEquatable<UpdateRatingsUserRequest>, IDeepCloneable<UpdateRatingsUserRequest>, IBufferMessage
{
	private static readonly MessageParser<UpdateRatingsUserRequest> _parser = new MessageParser<UpdateRatingsUserRequest>(() => new UpdateRatingsUserRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int SessionIdFieldNumber = 2;

	private string sessionId_ = "";

	public const int RatingIdFieldNumber = 3;

	private string ratingId_ = "";

	public const int UpdateFieldNumber = 4;

	private RatingUpdate update_;

	[DebuggerNonUserCode]
	public static MessageParser<UpdateRatingsUserRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[15];

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
	public RatingUpdate Update
	{
		get
		{
			return update_;
		}
		set
		{
			update_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UpdateRatingsUserRequest()
	{
	}

	[DebuggerNonUserCode]
	public UpdateRatingsUserRequest(UpdateRatingsUserRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		sessionId_ = other.sessionId_;
		ratingId_ = other.ratingId_;
		update_ = ((other.update_ != null) ? other.update_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UpdateRatingsUserRequest Clone()
	{
		return new UpdateRatingsUserRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UpdateRatingsUserRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UpdateRatingsUserRequest other)
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
		if (!object.Equals(Update, other.Update))
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
		if (update_ != null)
		{
			num ^= Update.GetHashCode();
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
		if (update_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Update);
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
		if (SessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SessionId);
		}
		if (RatingId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RatingId);
		}
		if (update_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Update);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UpdateRatingsUserRequest other)
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
		if (other.update_ != null)
		{
			if (update_ == null)
			{
				Update = new RatingUpdate();
			}
			Update.MergeFrom(other.Update);
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
			case 18u:
				SessionId = input.ReadString();
				break;
			case 26u:
				RatingId = input.ReadString();
				break;
			case 34u:
				if (update_ == null)
				{
					Update = new RatingUpdate();
				}
				input.ReadMessage(Update);
				break;
			}
		}
	}
}
