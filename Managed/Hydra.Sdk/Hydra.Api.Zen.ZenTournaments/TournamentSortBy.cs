using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentSortBy : IMessage<TournamentSortBy>, IMessage, IEquatable<TournamentSortBy>, IDeepCloneable<TournamentSortBy>, IBufferMessage
{
	private static readonly MessageParser<TournamentSortBy> _parser = new MessageParser<TournamentSortBy>(() => new TournamentSortBy());

	private UnknownFieldSet _unknownFields;

	public const int SortFieldFieldNumber = 1;

	private TournamentSortField sortField_ = TournamentSortField.None;

	public const int SortOrderFieldNumber = 2;

	private TournamentSortOrder sortOrder_ = TournamentSortOrder.None;

	[DebuggerNonUserCode]
	public static MessageParser<TournamentSortBy> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[21];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public TournamentSortField SortField
	{
		get
		{
			return sortField_;
		}
		set
		{
			sortField_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentSortOrder SortOrder
	{
		get
		{
			return sortOrder_;
		}
		set
		{
			sortOrder_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentSortBy()
	{
	}

	[DebuggerNonUserCode]
	public TournamentSortBy(TournamentSortBy other)
		: this()
	{
		sortField_ = other.sortField_;
		sortOrder_ = other.sortOrder_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentSortBy Clone()
	{
		return new TournamentSortBy(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentSortBy);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentSortBy other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SortField != other.SortField)
		{
			return false;
		}
		if (SortOrder != other.SortOrder)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (SortField != TournamentSortField.None)
		{
			num ^= SortField.GetHashCode();
		}
		if (SortOrder != TournamentSortOrder.None)
		{
			num ^= SortOrder.GetHashCode();
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
		if (SortField != TournamentSortField.None)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)SortField);
		}
		if (SortOrder != TournamentSortOrder.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)SortOrder);
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
		if (SortField != TournamentSortField.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)SortField);
		}
		if (SortOrder != TournamentSortOrder.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)SortOrder);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentSortBy other)
	{
		if (other != null)
		{
			if (other.SortField != TournamentSortField.None)
			{
				SortField = other.SortField;
			}
			if (other.SortOrder != TournamentSortOrder.None)
			{
				SortOrder = other.SortOrder;
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
			case 8u:
				SortField = (TournamentSortField)input.ReadEnum();
				break;
			case 16u:
				SortOrder = (TournamentSortOrder)input.ReadEnum();
				break;
			}
		}
	}
}
