using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class GetTournamentsResponse : IMessage<GetTournamentsResponse>, IMessage, IEquatable<GetTournamentsResponse>, IDeepCloneable<GetTournamentsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetTournamentsResponse> _parser = new MessageParser<GetTournamentsResponse>(() => new GetTournamentsResponse());

	private UnknownFieldSet _unknownFields;

	public const int TournamentsFieldNumber = 1;

	private static readonly FieldCodec<TournamentData> _repeated_tournaments_codec = FieldCodec.ForMessage(10u, TournamentData.Parser);

	private readonly RepeatedField<TournamentData> tournaments_ = new RepeatedField<TournamentData>();

	public const int TotalCountFieldNumber = 2;

	private int totalCount_;

	[DebuggerNonUserCode]
	public static MessageParser<GetTournamentsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[38];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<TournamentData> Tournaments => tournaments_;

	[DebuggerNonUserCode]
	public int TotalCount
	{
		get
		{
			return totalCount_;
		}
		set
		{
			totalCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetTournamentsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetTournamentsResponse(GetTournamentsResponse other)
		: this()
	{
		tournaments_ = other.tournaments_.Clone();
		totalCount_ = other.totalCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetTournamentsResponse Clone()
	{
		return new GetTournamentsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetTournamentsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetTournamentsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!tournaments_.Equals(other.tournaments_))
		{
			return false;
		}
		if (TotalCount != other.TotalCount)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= tournaments_.GetHashCode();
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
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
		tournaments_.WriteTo(ref output, _repeated_tournaments_codec);
		if (TotalCount != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(TotalCount);
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
		num += tournaments_.CalculateSize(_repeated_tournaments_codec);
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetTournamentsResponse other)
	{
		if (other != null)
		{
			tournaments_.Add(other.tournaments_);
			if (other.TotalCount != 0)
			{
				TotalCount = other.TotalCount;
			}
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
				tournaments_.AddEntriesFrom(ref input, _repeated_tournaments_codec);
				break;
			case 16u:
				TotalCount = input.ReadInt32();
				break;
			}
		}
	}
}
