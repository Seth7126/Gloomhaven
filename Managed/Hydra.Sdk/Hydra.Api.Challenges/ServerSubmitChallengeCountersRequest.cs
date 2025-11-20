using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Challenges;

public sealed class ServerSubmitChallengeCountersRequest : IMessage<ServerSubmitChallengeCountersRequest>, IMessage, IEquatable<ServerSubmitChallengeCountersRequest>, IDeepCloneable<ServerSubmitChallengeCountersRequest>, IBufferMessage
{
	private static readonly MessageParser<ServerSubmitChallengeCountersRequest> _parser = new MessageParser<ServerSubmitChallengeCountersRequest>(() => new ServerSubmitChallengeCountersRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ServerContext context_;

	public const int ReferenceIdFieldNumber = 2;

	private string referenceId_ = "";

	public const int UserOperationsFieldNumber = 3;

	private static readonly FieldCodec<ChallengeOperationList> _repeated_userOperations_codec = FieldCodec.ForMessage(26u, ChallengeOperationList.Parser);

	private readonly RepeatedField<ChallengeOperationList> userOperations_ = new RepeatedField<ChallengeOperationList>();

	public const int IsLastUpdateFieldNumber = 4;

	private bool isLastUpdate_;

	[DebuggerNonUserCode]
	public static MessageParser<ServerSubmitChallengeCountersRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[6];

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
	public string ReferenceId
	{
		get
		{
			return referenceId_;
		}
		set
		{
			referenceId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChallengeOperationList> UserOperations => userOperations_;

	[DebuggerNonUserCode]
	public bool IsLastUpdate
	{
		get
		{
			return isLastUpdate_;
		}
		set
		{
			isLastUpdate_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ServerSubmitChallengeCountersRequest()
	{
	}

	[DebuggerNonUserCode]
	public ServerSubmitChallengeCountersRequest(ServerSubmitChallengeCountersRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		referenceId_ = other.referenceId_;
		userOperations_ = other.userOperations_.Clone();
		isLastUpdate_ = other.isLastUpdate_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerSubmitChallengeCountersRequest Clone()
	{
		return new ServerSubmitChallengeCountersRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerSubmitChallengeCountersRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerSubmitChallengeCountersRequest other)
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
		if (ReferenceId != other.ReferenceId)
		{
			return false;
		}
		if (!userOperations_.Equals(other.userOperations_))
		{
			return false;
		}
		if (IsLastUpdate != other.IsLastUpdate)
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
		if (ReferenceId.Length != 0)
		{
			num ^= ReferenceId.GetHashCode();
		}
		num ^= userOperations_.GetHashCode();
		if (IsLastUpdate)
		{
			num ^= IsLastUpdate.GetHashCode();
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
		if (ReferenceId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ReferenceId);
		}
		userOperations_.WriteTo(ref output, _repeated_userOperations_codec);
		if (IsLastUpdate)
		{
			output.WriteRawTag(32);
			output.WriteBool(IsLastUpdate);
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
		if (ReferenceId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ReferenceId);
		}
		num += userOperations_.CalculateSize(_repeated_userOperations_codec);
		if (IsLastUpdate)
		{
			num += 2;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServerSubmitChallengeCountersRequest other)
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
		if (other.ReferenceId.Length != 0)
		{
			ReferenceId = other.ReferenceId;
		}
		userOperations_.Add(other.userOperations_);
		if (other.IsLastUpdate)
		{
			IsLastUpdate = other.IsLastUpdate;
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
				ReferenceId = input.ReadString();
				break;
			case 26u:
				userOperations_.AddEntriesFrom(ref input, _repeated_userOperations_codec);
				break;
			case 32u:
				IsLastUpdate = input.ReadBool();
				break;
			}
		}
	}
}
