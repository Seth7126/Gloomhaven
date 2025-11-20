using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ServerSubmitChallengeCountersResponse : IMessage<ServerSubmitChallengeCountersResponse>, IMessage, IEquatable<ServerSubmitChallengeCountersResponse>, IDeepCloneable<ServerSubmitChallengeCountersResponse>, IBufferMessage
{
	private static readonly MessageParser<ServerSubmitChallengeCountersResponse> _parser = new MessageParser<ServerSubmitChallengeCountersResponse>(() => new ServerSubmitChallengeCountersResponse());

	private UnknownFieldSet _unknownFields;

	public const int FailedUserIdsFieldNumber = 1;

	private static readonly FieldCodec<string> _repeated_failedUserIds_codec = FieldCodec.ForString(10u);

	private readonly RepeatedField<string> failedUserIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<ServerSubmitChallengeCountersResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<string> FailedUserIds => failedUserIds_;

	[DebuggerNonUserCode]
	public ServerSubmitChallengeCountersResponse()
	{
	}

	[DebuggerNonUserCode]
	public ServerSubmitChallengeCountersResponse(ServerSubmitChallengeCountersResponse other)
		: this()
	{
		failedUserIds_ = other.failedUserIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerSubmitChallengeCountersResponse Clone()
	{
		return new ServerSubmitChallengeCountersResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerSubmitChallengeCountersResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerSubmitChallengeCountersResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!failedUserIds_.Equals(other.failedUserIds_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= failedUserIds_.GetHashCode();
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
		failedUserIds_.WriteTo(ref output, _repeated_failedUserIds_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += failedUserIds_.CalculateSize(_repeated_failedUserIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServerSubmitChallengeCountersResponse other)
	{
		if (other != null)
		{
			failedUserIds_.Add(other.failedUserIds_);
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
				failedUserIds_.AddEntriesFrom(ref input, _repeated_failedUserIds_codec);
			}
		}
	}
}
