using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class SetTournamentCdnDataRequest : IMessage<SetTournamentCdnDataRequest>, IMessage, IEquatable<SetTournamentCdnDataRequest>, IDeepCloneable<SetTournamentCdnDataRequest>, IBufferMessage
{
	private static readonly MessageParser<SetTournamentCdnDataRequest> _parser = new MessageParser<SetTournamentCdnDataRequest>(() => new SetTournamentCdnDataRequest());

	private UnknownFieldSet _unknownFields;

	public const int TournamentIdFieldNumber = 1;

	private string tournamentId_ = "";

	public const int CdnDataFieldNumber = 2;

	private string cdnData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SetTournamentCdnDataRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[10];

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
	public string CdnData
	{
		get
		{
			return cdnData_;
		}
		set
		{
			cdnData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SetTournamentCdnDataRequest()
	{
	}

	[DebuggerNonUserCode]
	public SetTournamentCdnDataRequest(SetTournamentCdnDataRequest other)
		: this()
	{
		tournamentId_ = other.tournamentId_;
		cdnData_ = other.cdnData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SetTournamentCdnDataRequest Clone()
	{
		return new SetTournamentCdnDataRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SetTournamentCdnDataRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SetTournamentCdnDataRequest other)
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
		if (CdnData != other.CdnData)
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
		if (CdnData.Length != 0)
		{
			num ^= CdnData.GetHashCode();
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
		if (CdnData.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(CdnData);
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
		if (CdnData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CdnData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SetTournamentCdnDataRequest other)
	{
		if (other != null)
		{
			if (other.TournamentId.Length != 0)
			{
				TournamentId = other.TournamentId;
			}
			if (other.CdnData.Length != 0)
			{
				CdnData = other.CdnData;
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
				TournamentId = input.ReadString();
				break;
			case 18u:
				CdnData = input.ReadString();
				break;
			}
		}
	}
}
