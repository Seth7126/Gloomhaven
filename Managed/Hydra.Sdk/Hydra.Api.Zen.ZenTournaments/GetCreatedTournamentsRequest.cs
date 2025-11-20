using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class GetCreatedTournamentsRequest : IMessage<GetCreatedTournamentsRequest>, IMessage, IEquatable<GetCreatedTournamentsRequest>, IDeepCloneable<GetCreatedTournamentsRequest>, IBufferMessage
{
	private static readonly MessageParser<GetCreatedTournamentsRequest> _parser = new MessageParser<GetCreatedTournamentsRequest>(() => new GetCreatedTournamentsRequest());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int UserIdFieldNumber = 2;

	private string userId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetCreatedTournamentsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[41];

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
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetCreatedTournamentsRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetCreatedTournamentsRequest(GetCreatedTournamentsRequest other)
		: this()
	{
		titleId_ = other.titleId_;
		userId_ = other.userId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetCreatedTournamentsRequest Clone()
	{
		return new GetCreatedTournamentsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetCreatedTournamentsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetCreatedTournamentsRequest other)
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
		if (UserId != other.UserId)
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
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(UserId);
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
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetCreatedTournamentsRequest other)
	{
		if (other != null)
		{
			if (other.TitleId.Length != 0)
			{
				TitleId = other.TitleId;
			}
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
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
				TitleId = input.ReadString();
				break;
			case 18u:
				UserId = input.ReadString();
				break;
			}
		}
	}
}
