using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Leaderboards;

public sealed class GetLeaderboardsResponse : IMessage<GetLeaderboardsResponse>, IMessage, IEquatable<GetLeaderboardsResponse>, IDeepCloneable<GetLeaderboardsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetLeaderboardsResponse> _parser = new MessageParser<GetLeaderboardsResponse>(() => new GetLeaderboardsResponse());

	private UnknownFieldSet _unknownFields;

	public const int ResultsFieldNumber = 1;

	private static readonly FieldCodec<LeaderboardData> _repeated_results_codec = FieldCodec.ForMessage(10u, LeaderboardData.Parser);

	private readonly RepeatedField<LeaderboardData> results_ = new RepeatedField<LeaderboardData>();

	[DebuggerNonUserCode]
	public static MessageParser<GetLeaderboardsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<LeaderboardData> Results => results_;

	[DebuggerNonUserCode]
	public GetLeaderboardsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetLeaderboardsResponse(GetLeaderboardsResponse other)
		: this()
	{
		results_ = other.results_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetLeaderboardsResponse Clone()
	{
		return new GetLeaderboardsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetLeaderboardsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetLeaderboardsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!results_.Equals(other.results_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= results_.GetHashCode();
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
		results_.WriteTo(ref output, _repeated_results_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += results_.CalculateSize(_repeated_results_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetLeaderboardsResponse other)
	{
		if (other != null)
		{
			results_.Add(other.results_);
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
				results_.AddEntriesFrom(ref input, _repeated_results_codec);
			}
		}
	}
}
