using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class GetTournamentsRequest : IMessage<GetTournamentsRequest>, IMessage, IEquatable<GetTournamentsRequest>, IDeepCloneable<GetTournamentsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetTournamentsRequest> _parser = new MessageParser<GetTournamentsRequest>(() => new GetTournamentsRequest());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int StateFilterFieldNumber = 2;

	private TournamentStateFilter stateFilter_ = TournamentStateFilter.None;

	public const int StartPositionFieldNumber = 3;

	private int startPosition_;

	public const int ResultsCountFieldNumber = 4;

	private int resultsCount_;

	public const int FilterTournamentTypeFieldNumber = 5;

	private TournamentType filterTournamentType_ = TournamentType.None;

	public const int SortByFieldNumber = 6;

	private TournamentSortBy sortBy_;

	[DebuggerNonUserCode]
	public static MessageParser<GetTournamentsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[37];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TournamentStateFilter StateFilter
	{
		get
		{
			return stateFilter_;
		}
		set
		{
			stateFilter_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int StartPosition
	{
		get
		{
			return startPosition_;
		}
		set
		{
			startPosition_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int ResultsCount
	{
		get
		{
			return resultsCount_;
		}
		set
		{
			resultsCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentType FilterTournamentType
	{
		get
		{
			return filterTournamentType_;
		}
		set
		{
			filterTournamentType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentSortBy SortBy
	{
		get
		{
			return sortBy_;
		}
		set
		{
			sortBy_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetTournamentsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetTournamentsRequest(GetTournamentsRequest other)
		: this()
	{
		titleId_ = other.titleId_;
		stateFilter_ = other.stateFilter_;
		startPosition_ = other.startPosition_;
		resultsCount_ = other.resultsCount_;
		filterTournamentType_ = other.filterTournamentType_;
		sortBy_ = ((other.sortBy_ != null) ? other.sortBy_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetTournamentsRequest Clone()
	{
		return new GetTournamentsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetTournamentsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetTournamentsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (StateFilter != other.StateFilter)
		{
			return false;
		}
		if (StartPosition != other.StartPosition)
		{
			return false;
		}
		if (ResultsCount != other.ResultsCount)
		{
			return false;
		}
		if (FilterTournamentType != other.FilterTournamentType)
		{
			return false;
		}
		if (!object.Equals(SortBy, other.SortBy))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (StateFilter != TournamentStateFilter.None)
		{
			num ^= StateFilter.GetHashCode();
		}
		if (StartPosition != 0)
		{
			num ^= StartPosition.GetHashCode();
		}
		if (ResultsCount != 0)
		{
			num ^= ResultsCount.GetHashCode();
		}
		if (FilterTournamentType != TournamentType.None)
		{
			num ^= FilterTournamentType.GetHashCode();
		}
		if (sortBy_ != null)
		{
			num ^= SortBy.GetHashCode();
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
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TitleId);
		}
		if (StateFilter != TournamentStateFilter.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)StateFilter);
		}
		if (StartPosition != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(StartPosition);
		}
		if (ResultsCount != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(ResultsCount);
		}
		if (FilterTournamentType != TournamentType.None)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)FilterTournamentType);
		}
		if (sortBy_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(SortBy);
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
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (StateFilter != TournamentStateFilter.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)StateFilter);
		}
		if (StartPosition != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(StartPosition);
		}
		if (ResultsCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResultsCount);
		}
		if (FilterTournamentType != TournamentType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FilterTournamentType);
		}
		if (sortBy_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(SortBy);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetTournamentsRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TitleId.Length != 0)
		{
			TitleId = other.TitleId;
		}
		if (other.StateFilter != TournamentStateFilter.None)
		{
			StateFilter = other.StateFilter;
		}
		if (other.StartPosition != 0)
		{
			StartPosition = other.StartPosition;
		}
		if (other.ResultsCount != 0)
		{
			ResultsCount = other.ResultsCount;
		}
		if (other.FilterTournamentType != TournamentType.None)
		{
			FilterTournamentType = other.FilterTournamentType;
		}
		if (other.sortBy_ != null)
		{
			if (sortBy_ == null)
			{
				SortBy = new TournamentSortBy();
			}
			SortBy.MergeFrom(other.SortBy);
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
				TitleId = input.ReadString();
				break;
			case 16u:
				StateFilter = (TournamentStateFilter)input.ReadEnum();
				break;
			case 24u:
				StartPosition = input.ReadInt32();
				break;
			case 32u:
				ResultsCount = input.ReadInt32();
				break;
			case 40u:
				FilterTournamentType = (TournamentType)input.ReadEnum();
				break;
			case 50u:
				if (sortBy_ == null)
				{
					SortBy = new TournamentSortBy();
				}
				input.ReadMessage(SortBy);
				break;
			}
		}
	}
}
