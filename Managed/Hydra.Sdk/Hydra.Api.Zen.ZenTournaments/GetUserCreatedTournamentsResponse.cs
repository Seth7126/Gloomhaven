using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class GetUserCreatedTournamentsResponse : IMessage<GetUserCreatedTournamentsResponse>, IMessage, IEquatable<GetUserCreatedTournamentsResponse>, IDeepCloneable<GetUserCreatedTournamentsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetUserCreatedTournamentsResponse> _parser = new MessageParser<GetUserCreatedTournamentsResponse>(() => new GetUserCreatedTournamentsResponse());

	private UnknownFieldSet _unknownFields;

	public const int TournamentsFieldNumber = 1;

	private static readonly FieldCodec<TournamentData> _repeated_tournaments_codec = FieldCodec.ForMessage(10u, TournamentData.Parser);

	private readonly RepeatedField<TournamentData> tournaments_ = new RepeatedField<TournamentData>();

	[DebuggerNonUserCode]
	public static MessageParser<GetUserCreatedTournamentsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[46];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<TournamentData> Tournaments => tournaments_;

	[DebuggerNonUserCode]
	public GetUserCreatedTournamentsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetUserCreatedTournamentsResponse(GetUserCreatedTournamentsResponse other)
		: this()
	{
		tournaments_ = other.tournaments_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetUserCreatedTournamentsResponse Clone()
	{
		return new GetUserCreatedTournamentsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetUserCreatedTournamentsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetUserCreatedTournamentsResponse other)
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
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= tournaments_.GetHashCode();
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
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetUserCreatedTournamentsResponse other)
	{
		if (other != null)
		{
			tournaments_.Add(other.tournaments_);
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
				tournaments_.AddEntriesFrom(ref input, _repeated_tournaments_codec);
			}
		}
	}
}
