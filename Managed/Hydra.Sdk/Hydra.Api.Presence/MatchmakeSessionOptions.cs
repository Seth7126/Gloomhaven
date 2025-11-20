using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class MatchmakeSessionOptions : IMessage<MatchmakeSessionOptions>, IMessage, IEquatable<MatchmakeSessionOptions>, IDeepCloneable<MatchmakeSessionOptions>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeSessionOptions> _parser = new MessageParser<MatchmakeSessionOptions>(() => new MatchmakeSessionOptions());

	private UnknownFieldSet _unknownFields;

	public const int PlaylistIdFieldNumber = 1;

	private string playlistId_ = "";

	public const int PingsFieldNumber = 2;

	private static readonly FieldCodec<DCPing> _repeated_pings_codec = FieldCodec.ForMessage(18u, DCPing.Parser);

	private readonly RepeatedField<DCPing> pings_ = new RepeatedField<DCPing>();

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeSessionOptions> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string PlaylistId
	{
		get
		{
			return playlistId_;
		}
		set
		{
			playlistId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<DCPing> Pings => pings_;

	[DebuggerNonUserCode]
	public MatchmakeSessionOptions()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionOptions(MatchmakeSessionOptions other)
		: this()
	{
		playlistId_ = other.playlistId_;
		pings_ = other.pings_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionOptions Clone()
	{
		return new MatchmakeSessionOptions(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeSessionOptions);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeSessionOptions other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlaylistId != other.PlaylistId)
		{
			return false;
		}
		if (!pings_.Equals(other.pings_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PlaylistId.Length != 0)
		{
			num ^= PlaylistId.GetHashCode();
		}
		num ^= pings_.GetHashCode();
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
		if (PlaylistId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PlaylistId);
		}
		pings_.WriteTo(ref output, _repeated_pings_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (PlaylistId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PlaylistId);
		}
		num += pings_.CalculateSize(_repeated_pings_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeSessionOptions other)
	{
		if (other != null)
		{
			if (other.PlaylistId.Length != 0)
			{
				PlaylistId = other.PlaylistId;
			}
			pings_.Add(other.pings_);
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
				PlaylistId = input.ReadString();
				break;
			case 18u:
				pings_.AddEntriesFrom(ref input, _repeated_pings_codec);
				break;
			}
		}
	}
}
