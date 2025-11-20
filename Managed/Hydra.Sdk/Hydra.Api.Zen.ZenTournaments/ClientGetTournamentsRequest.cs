using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientGetTournamentsRequest : IMessage<ClientGetTournamentsRequest>, IMessage, IEquatable<ClientGetTournamentsRequest>, IDeepCloneable<ClientGetTournamentsRequest>, IBufferMessage
{
	private static readonly MessageParser<ClientGetTournamentsRequest> _parser = new MessageParser<ClientGetTournamentsRequest>(() => new ClientGetTournamentsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int FilterPropertiesFieldNumber = 2;

	private static readonly FieldCodec<KeyValue> _repeated_filterProperties_codec = FieldCodec.ForMessage(18u, KeyValue.Parser);

	private readonly RepeatedField<KeyValue> filterProperties_ = new RepeatedField<KeyValue>();

	public const int FilterTagsFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_filterTags_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> filterTags_ = new RepeatedField<string>();

	public const int StartPositionFieldNumber = 4;

	private int startPosition_;

	public const int ResultsCountFieldNumber = 5;

	private int resultsCount_;

	public const int FilterTournamentTypeFieldNumber = 6;

	private TournamentType filterTournamentType_ = TournamentType.None;

	[DebuggerNonUserCode]
	public static MessageParser<ClientGetTournamentsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[19];

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
	public RepeatedField<KeyValue> FilterProperties => filterProperties_;

	[DebuggerNonUserCode]
	public RepeatedField<string> FilterTags => filterTags_;

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
	public ClientGetTournamentsRequest()
	{
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsRequest(ClientGetTournamentsRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		filterProperties_ = other.filterProperties_.Clone();
		filterTags_ = other.filterTags_.Clone();
		startPosition_ = other.startPosition_;
		resultsCount_ = other.resultsCount_;
		filterTournamentType_ = other.filterTournamentType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsRequest Clone()
	{
		return new ClientGetTournamentsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientGetTournamentsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientGetTournamentsRequest other)
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
		if (StartPosition != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(StartPosition);
		}
		if (ResultsCount != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(ResultsCount);
		}
		if (FilterTournamentType != TournamentType.None)
		{
			output.WriteRawTag(48);
			output.WriteEnum((int)FilterTournamentType);
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
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientGetTournamentsRequest other)
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
				StartPosition = input.ReadInt32();
				break;
			case 40u:
				ResultsCount = input.ReadInt32();
				break;
			case 48u:
				FilterTournamentType = (TournamentType)input.ReadEnum();
				break;
			}
		}
	}
}
