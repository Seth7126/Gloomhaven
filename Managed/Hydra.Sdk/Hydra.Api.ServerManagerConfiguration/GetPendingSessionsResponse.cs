using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.SessionControl;

namespace Hydra.Api.ServerManagerConfiguration;

public sealed class GetPendingSessionsResponse : IMessage<GetPendingSessionsResponse>, IMessage, IEquatable<GetPendingSessionsResponse>, IDeepCloneable<GetPendingSessionsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetPendingSessionsResponse> _parser = new MessageParser<GetPendingSessionsResponse>(() => new GetPendingSessionsResponse());

	private UnknownFieldSet _unknownFields;

	public const int SessionsFieldNumber = 1;

	private static readonly FieldCodec<PendingSession> _repeated_sessions_codec = FieldCodec.ForMessage(10u, PendingSession.Parser);

	private readonly RepeatedField<PendingSession> sessions_ = new RepeatedField<PendingSession>();

	public const int RefreshAfterSecondsFieldNumber = 2;

	private int refreshAfterSeconds_;

	[DebuggerNonUserCode]
	public static MessageParser<GetPendingSessionsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => DevDSMConfigurationContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<PendingSession> Sessions => sessions_;

	[DebuggerNonUserCode]
	public int RefreshAfterSeconds
	{
		get
		{
			return refreshAfterSeconds_;
		}
		set
		{
			refreshAfterSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetPendingSessionsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetPendingSessionsResponse(GetPendingSessionsResponse other)
		: this()
	{
		sessions_ = other.sessions_.Clone();
		refreshAfterSeconds_ = other.refreshAfterSeconds_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetPendingSessionsResponse Clone()
	{
		return new GetPendingSessionsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetPendingSessionsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetPendingSessionsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!sessions_.Equals(other.sessions_))
		{
			return false;
		}
		if (RefreshAfterSeconds != other.RefreshAfterSeconds)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= sessions_.GetHashCode();
		if (RefreshAfterSeconds != 0)
		{
			num ^= RefreshAfterSeconds.GetHashCode();
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
		sessions_.WriteTo(ref output, _repeated_sessions_codec);
		if (RefreshAfterSeconds != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(RefreshAfterSeconds);
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
		num += sessions_.CalculateSize(_repeated_sessions_codec);
		if (RefreshAfterSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshAfterSeconds);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetPendingSessionsResponse other)
	{
		if (other != null)
		{
			sessions_.Add(other.sessions_);
			if (other.RefreshAfterSeconds != 0)
			{
				RefreshAfterSeconds = other.RefreshAfterSeconds;
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
				sessions_.AddEntriesFrom(ref input, _repeated_sessions_codec);
				break;
			case 16u:
				RefreshAfterSeconds = input.ReadInt32();
				break;
			}
		}
	}
}
