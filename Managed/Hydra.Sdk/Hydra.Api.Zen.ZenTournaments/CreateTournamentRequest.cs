using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class CreateTournamentRequest : IMessage<CreateTournamentRequest>, IMessage, IEquatable<CreateTournamentRequest>, IDeepCloneable<CreateTournamentRequest>, IBufferMessage
{
	private static readonly MessageParser<CreateTournamentRequest> _parser = new MessageParser<CreateTournamentRequest>(() => new CreateTournamentRequest());

	private UnknownFieldSet _unknownFields;

	public const int CreateDataFieldNumber = 1;

	private TournamentCreateData createData_;

	[DebuggerNonUserCode]
	public static MessageParser<CreateTournamentRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public TournamentCreateData CreateData
	{
		get
		{
			return createData_;
		}
		set
		{
			createData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public CreateTournamentRequest()
	{
	}

	[DebuggerNonUserCode]
	public CreateTournamentRequest(CreateTournamentRequest other)
		: this()
	{
		createData_ = ((other.createData_ != null) ? other.createData_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CreateTournamentRequest Clone()
	{
		return new CreateTournamentRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CreateTournamentRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(CreateTournamentRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(CreateData, other.CreateData))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (createData_ != null)
		{
			num ^= CreateData.GetHashCode();
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
		if (createData_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(CreateData);
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
		if (createData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CreateData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CreateTournamentRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.createData_ != null)
		{
			if (createData_ == null)
			{
				CreateData = new TournamentCreateData();
			}
			CreateData.MergeFrom(other.CreateData);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (createData_ == null)
			{
				CreateData = new TournamentCreateData();
			}
			input.ReadMessage(CreateData);
		}
	}
}
