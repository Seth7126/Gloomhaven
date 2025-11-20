using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class SessionTeamResult : IMessage<SessionTeamResult>, IMessage, IEquatable<SessionTeamResult>, IDeepCloneable<SessionTeamResult>, IBufferMessage
{
	private static readonly MessageParser<SessionTeamResult> _parser = new MessageParser<SessionTeamResult>(() => new SessionTeamResult());

	private UnknownFieldSet _unknownFields;

	public const int ResultsFieldNumber = 1;

	private static readonly FieldCodec<UserTeamResult> _repeated_results_codec = FieldCodec.ForMessage(10u, UserTeamResult.Parser);

	private readonly RepeatedField<UserTeamResult> results_ = new RepeatedField<UserTeamResult>();

	[DebuggerNonUserCode]
	public static MessageParser<SessionTeamResult> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<UserTeamResult> Results => results_;

	[DebuggerNonUserCode]
	public SessionTeamResult()
	{
	}

	[DebuggerNonUserCode]
	public SessionTeamResult(SessionTeamResult other)
		: this()
	{
		results_ = other.results_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SessionTeamResult Clone()
	{
		return new SessionTeamResult(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SessionTeamResult);
	}

	[DebuggerNonUserCode]
	public bool Equals(SessionTeamResult other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!results_.Equals(other.results_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= results_.GetHashCode();
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
		results_.WriteTo(ref output, _repeated_results_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += results_.CalculateSize(_repeated_results_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SessionTeamResult other)
	{
		if (other != null)
		{
			results_.Add(other.results_);
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
				results_.AddEntriesFrom(ref input, _repeated_results_codec);
			}
		}
	}
}
