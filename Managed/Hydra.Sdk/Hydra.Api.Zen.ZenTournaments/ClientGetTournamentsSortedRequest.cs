using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientGetTournamentsSortedRequest : IMessage<ClientGetTournamentsSortedRequest>, IMessage, IEquatable<ClientGetTournamentsSortedRequest>, IDeepCloneable<ClientGetTournamentsSortedRequest>, IBufferMessage
{
	private static readonly MessageParser<ClientGetTournamentsSortedRequest> _parser = new MessageParser<ClientGetTournamentsSortedRequest>(() => new ClientGetTournamentsSortedRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int FilterPropertiesFieldNumber = 2;

	private static readonly FieldCodec<KeyValues> _repeated_filterProperties_codec = FieldCodec.ForMessage(18u, KeyValues.Parser);

	private readonly RepeatedField<KeyValues> filterProperties_ = new RepeatedField<KeyValues>();

	public const int FilterTagsFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_filterTags_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> filterTags_ = new RepeatedField<string>();

	public const int FilterPlayedFieldNumber = 4;

	private TournamentFilterPlayed filterPlayed_ = TournamentFilterPlayed.None;

	public const int FilterPlayLimitFieldNumber = 5;

	private TournamentFilterPlayLimit filterPlayLimit_ = TournamentFilterPlayLimit.None;

	public const int FilterTimeLeftFieldNumber = 6;

	private TournamentFilterTimeLeft filterTimeLeft_;

	public const int FilterStateFieldNumber = 7;

	private TournamentFilterState filterState_ = TournamentFilterState.None;

	public const int SortByFieldNumber = 8;

	private TournamentSortBy sortBy_;

	public const int StartPositionFieldNumber = 9;

	private int startPosition_;

	public const int ResultsCountFieldNumber = 10;

	private int resultsCount_;

	public const int FilterTournamentTypeFieldNumber = 11;

	private TournamentType filterTournamentType_ = TournamentType.None;

	public const int FilterPasswordFieldNumber = 12;

	private TournamentFilterPasswordProtected filterPassword_ = TournamentFilterPasswordProtected.None;

	[DebuggerNonUserCode]
	public static MessageParser<ClientGetTournamentsSortedRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[22];

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
	public RepeatedField<KeyValues> FilterProperties => filterProperties_;

	[DebuggerNonUserCode]
	public RepeatedField<string> FilterTags => filterTags_;

	[DebuggerNonUserCode]
	public TournamentFilterPlayed FilterPlayed
	{
		get
		{
			return filterPlayed_;
		}
		set
		{
			filterPlayed_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentFilterPlayLimit FilterPlayLimit
	{
		get
		{
			return filterPlayLimit_;
		}
		set
		{
			filterPlayLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentFilterTimeLeft FilterTimeLeft
	{
		get
		{
			return filterTimeLeft_;
		}
		set
		{
			filterTimeLeft_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentFilterState FilterState
	{
		get
		{
			return filterState_;
		}
		set
		{
			filterState_ = value;
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
	public TournamentFilterPasswordProtected FilterPassword
	{
		get
		{
			return filterPassword_;
		}
		set
		{
			filterPassword_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsSortedRequest()
	{
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsSortedRequest(ClientGetTournamentsSortedRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		filterProperties_ = other.filterProperties_.Clone();
		filterTags_ = other.filterTags_.Clone();
		filterPlayed_ = other.filterPlayed_;
		filterPlayLimit_ = other.filterPlayLimit_;
		filterTimeLeft_ = ((other.filterTimeLeft_ != null) ? other.filterTimeLeft_.Clone() : null);
		filterState_ = other.filterState_;
		sortBy_ = ((other.sortBy_ != null) ? other.sortBy_.Clone() : null);
		startPosition_ = other.startPosition_;
		resultsCount_ = other.resultsCount_;
		filterTournamentType_ = other.filterTournamentType_;
		filterPassword_ = other.filterPassword_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsSortedRequest Clone()
	{
		return new ClientGetTournamentsSortedRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientGetTournamentsSortedRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientGetTournamentsSortedRequest other)
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
		if (!filterProperties_.Equals(other.filterProperties_))
		{
			return false;
		}
		if (!filterTags_.Equals(other.filterTags_))
		{
			return false;
		}
		if (FilterPlayed != other.FilterPlayed)
		{
			return false;
		}
		if (FilterPlayLimit != other.FilterPlayLimit)
		{
			return false;
		}
		if (!object.Equals(FilterTimeLeft, other.FilterTimeLeft))
		{
			return false;
		}
		if (FilterState != other.FilterState)
		{
			return false;
		}
		if (!object.Equals(SortBy, other.SortBy))
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
		if (FilterPassword != other.FilterPassword)
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
		num ^= filterProperties_.GetHashCode();
		num ^= filterTags_.GetHashCode();
		if (FilterPlayed != TournamentFilterPlayed.None)
		{
			num ^= FilterPlayed.GetHashCode();
		}
		if (FilterPlayLimit != TournamentFilterPlayLimit.None)
		{
			num ^= FilterPlayLimit.GetHashCode();
		}
		if (filterTimeLeft_ != null)
		{
			num ^= FilterTimeLeft.GetHashCode();
		}
		if (FilterState != TournamentFilterState.None)
		{
			num ^= FilterState.GetHashCode();
		}
		if (sortBy_ != null)
		{
			num ^= SortBy.GetHashCode();
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
		if (FilterPassword != TournamentFilterPasswordProtected.None)
		{
			num ^= FilterPassword.GetHashCode();
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
		filterProperties_.WriteTo(ref output, _repeated_filterProperties_codec);
		filterTags_.WriteTo(ref output, _repeated_filterTags_codec);
		if (FilterPlayed != TournamentFilterPlayed.None)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)FilterPlayed);
		}
		if (FilterPlayLimit != TournamentFilterPlayLimit.None)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)FilterPlayLimit);
		}
		if (filterTimeLeft_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(FilterTimeLeft);
		}
		if (FilterState != TournamentFilterState.None)
		{
			output.WriteRawTag(56);
			output.WriteEnum((int)FilterState);
		}
		if (sortBy_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(SortBy);
		}
		if (StartPosition != 0)
		{
			output.WriteRawTag(72);
			output.WriteInt32(StartPosition);
		}
		if (ResultsCount != 0)
		{
			output.WriteRawTag(80);
			output.WriteInt32(ResultsCount);
		}
		if (FilterTournamentType != TournamentType.None)
		{
			output.WriteRawTag(88);
			output.WriteEnum((int)FilterTournamentType);
		}
		if (FilterPassword != TournamentFilterPasswordProtected.None)
		{
			output.WriteRawTag(96);
			output.WriteEnum((int)FilterPassword);
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
		num += filterProperties_.CalculateSize(_repeated_filterProperties_codec);
		num += filterTags_.CalculateSize(_repeated_filterTags_codec);
		if (FilterPlayed != TournamentFilterPlayed.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FilterPlayed);
		}
		if (FilterPlayLimit != TournamentFilterPlayLimit.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FilterPlayLimit);
		}
		if (filterTimeLeft_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(FilterTimeLeft);
		}
		if (FilterState != TournamentFilterState.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FilterState);
		}
		if (sortBy_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(SortBy);
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
		if (FilterPassword != TournamentFilterPasswordProtected.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FilterPassword);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientGetTournamentsSortedRequest other)
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
		filterProperties_.Add(other.filterProperties_);
		filterTags_.Add(other.filterTags_);
		if (other.FilterPlayed != TournamentFilterPlayed.None)
		{
			FilterPlayed = other.FilterPlayed;
		}
		if (other.FilterPlayLimit != TournamentFilterPlayLimit.None)
		{
			FilterPlayLimit = other.FilterPlayLimit;
		}
		if (other.filterTimeLeft_ != null)
		{
			if (filterTimeLeft_ == null)
			{
				FilterTimeLeft = new TournamentFilterTimeLeft();
			}
			FilterTimeLeft.MergeFrom(other.FilterTimeLeft);
		}
		if (other.FilterState != TournamentFilterState.None)
		{
			FilterState = other.FilterState;
		}
		if (other.sortBy_ != null)
		{
			if (sortBy_ == null)
			{
				SortBy = new TournamentSortBy();
			}
			SortBy.MergeFrom(other.SortBy);
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
		if (other.FilterPassword != TournamentFilterPasswordProtected.None)
		{
			FilterPassword = other.FilterPassword;
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
				filterProperties_.AddEntriesFrom(ref input, _repeated_filterProperties_codec);
				break;
			case 26u:
				filterTags_.AddEntriesFrom(ref input, _repeated_filterTags_codec);
				break;
			case 32u:
				FilterPlayed = (TournamentFilterPlayed)input.ReadEnum();
				break;
			case 40u:
				FilterPlayLimit = (TournamentFilterPlayLimit)input.ReadEnum();
				break;
			case 50u:
				if (filterTimeLeft_ == null)
				{
					FilterTimeLeft = new TournamentFilterTimeLeft();
				}
				input.ReadMessage(FilterTimeLeft);
				break;
			case 56u:
				FilterState = (TournamentFilterState)input.ReadEnum();
				break;
			case 66u:
				if (sortBy_ == null)
				{
					SortBy = new TournamentSortBy();
				}
				input.ReadMessage(SortBy);
				break;
			case 72u:
				StartPosition = input.ReadInt32();
				break;
			case 80u:
				ResultsCount = input.ReadInt32();
				break;
			case 88u:
				FilterTournamentType = (TournamentType)input.ReadEnum();
				break;
			case 96u:
				FilterPassword = (TournamentFilterPasswordProtected)input.ReadEnum();
				break;
			}
		}
	}
}
