using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ServerGetChallengesResponse : IMessage<ServerGetChallengesResponse>, IMessage, IEquatable<ServerGetChallengesResponse>, IDeepCloneable<ServerGetChallengesResponse>, IBufferMessage
{
	private static readonly MessageParser<ServerGetChallengesResponse> _parser = new MessageParser<ServerGetChallengesResponse>(() => new ServerGetChallengesResponse());

	private UnknownFieldSet _unknownFields;

	public const int PerUserChallengeCountersFieldNumber = 1;

	private static readonly FieldCodec<PerUserChallengeCounter> _repeated_perUserChallengeCounters_codec = FieldCodec.ForMessage(10u, PerUserChallengeCounter.Parser);

	private readonly RepeatedField<PerUserChallengeCounter> perUserChallengeCounters_ = new RepeatedField<PerUserChallengeCounter>();

	public const int ServerChallengesSettingsFieldNumber = 2;

	private ServerChallengesSettings serverChallengesSettings_;

	[DebuggerNonUserCode]
	public static MessageParser<ServerGetChallengesResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[10];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<PerUserChallengeCounter> PerUserChallengeCounters => perUserChallengeCounters_;

	[DebuggerNonUserCode]
	public ServerChallengesSettings ServerChallengesSettings
	{
		get
		{
			return serverChallengesSettings_;
		}
		set
		{
			serverChallengesSettings_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ServerGetChallengesResponse()
	{
	}

	[DebuggerNonUserCode]
	public ServerGetChallengesResponse(ServerGetChallengesResponse other)
		: this()
	{
		perUserChallengeCounters_ = other.perUserChallengeCounters_.Clone();
		serverChallengesSettings_ = ((other.serverChallengesSettings_ != null) ? other.serverChallengesSettings_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerGetChallengesResponse Clone()
	{
		return new ServerGetChallengesResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerGetChallengesResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerGetChallengesResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!perUserChallengeCounters_.Equals(other.perUserChallengeCounters_))
		{
			return false;
		}
		if (!object.Equals(ServerChallengesSettings, other.ServerChallengesSettings))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= perUserChallengeCounters_.GetHashCode();
		if (serverChallengesSettings_ != null)
		{
			num ^= ServerChallengesSettings.GetHashCode();
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
		perUserChallengeCounters_.WriteTo(ref output, _repeated_perUserChallengeCounters_codec);
		if (serverChallengesSettings_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ServerChallengesSettings);
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
		num += perUserChallengeCounters_.CalculateSize(_repeated_perUserChallengeCounters_codec);
		if (serverChallengesSettings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerChallengesSettings);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServerGetChallengesResponse other)
	{
		if (other == null)
		{
			return;
		}
		perUserChallengeCounters_.Add(other.perUserChallengeCounters_);
		if (other.serverChallengesSettings_ != null)
		{
			if (serverChallengesSettings_ == null)
			{
				ServerChallengesSettings = new ServerChallengesSettings();
			}
			ServerChallengesSettings.MergeFrom(other.ServerChallengesSettings);
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
				perUserChallengeCounters_.AddEntriesFrom(ref input, _repeated_perUserChallengeCounters_codec);
				break;
			case 18u:
				if (serverChallengesSettings_ == null)
				{
					ServerChallengesSettings = new ServerChallengesSettings();
				}
				input.ReadMessage(ServerChallengesSettings);
				break;
			}
		}
	}
}
