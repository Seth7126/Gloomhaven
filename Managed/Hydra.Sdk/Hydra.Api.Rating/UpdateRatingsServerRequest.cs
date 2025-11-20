using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Rating;

public sealed class UpdateRatingsServerRequest : IMessage<UpdateRatingsServerRequest>, IMessage, IEquatable<UpdateRatingsServerRequest>, IDeepCloneable<UpdateRatingsServerRequest>, IBufferMessage
{
	private static readonly MessageParser<UpdateRatingsServerRequest> _parser = new MessageParser<UpdateRatingsServerRequest>(() => new UpdateRatingsServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ServerContext context_;

	public const int RatingIdFieldNumber = 2;

	private string ratingId_ = "";

	public const int UpdateFieldNumber = 3;

	private RatingUpdate update_;

	[DebuggerNonUserCode]
	public static MessageParser<UpdateRatingsServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[13];

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
	public UpdateRatingsServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public UpdateRatingsServerRequest(UpdateRatingsServerRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		ratingId_ = other.ratingId_;
		update_ = ((other.update_ != null) ? other.update_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UpdateRatingsServerRequest Clone()
	{
		return new UpdateRatingsServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UpdateRatingsServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UpdateRatingsServerRequest other)
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
		if (RatingId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(RatingId);
		}
		if (update_ != null)
		{
			output.WriteRawTag(26);
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
	public void MergeFrom(UpdateRatingsServerRequest other)
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
					Context = new ServerContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				RatingId = input.ReadString();
				break;
			case 26u:
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
