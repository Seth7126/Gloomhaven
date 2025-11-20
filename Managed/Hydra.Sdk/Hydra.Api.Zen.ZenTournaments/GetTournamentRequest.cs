using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class GetTournamentRequest : IMessage<GetTournamentRequest>, IMessage, IEquatable<GetTournamentRequest>, IDeepCloneable<GetTournamentRequest>, IBufferMessage
{
	private static readonly MessageParser<GetTournamentRequest> _parser = new MessageParser<GetTournamentRequest>(() => new GetTournamentRequest());

	private UnknownFieldSet _unknownFields;

	public const int TournamentIdFieldNumber = 1;

	private string tournamentId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetTournamentRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[39];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TournamentId
	{
		get
		{
			return tournamentId_;
		}
		set
		{
			tournamentId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetTournamentRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetTournamentRequest(GetTournamentRequest other)
		: this()
	{
		tournamentId_ = other.tournamentId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetTournamentRequest Clone()
	{
		return new GetTournamentRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetTournamentRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetTournamentRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TournamentId != other.TournamentId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TournamentId.Length != 0)
		{
			num ^= TournamentId.GetHashCode();
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
		if (TournamentId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TournamentId);
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
		if (TournamentId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetTournamentRequest other)
	{
		if (other != null)
		{
			if (other.TournamentId.Length != 0)
			{
				TournamentId = other.TournamentId;
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				TournamentId = input.ReadString();
			}
		}
	}
}
