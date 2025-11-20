using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.SessionControl;

public sealed class PendingSession : IMessage<PendingSession>, IMessage, IEquatable<PendingSession>, IDeepCloneable<PendingSession>, IBufferMessage
{
	private static readonly MessageParser<PendingSession> _parser = new MessageParser<PendingSession>(() => new PendingSession());

	private UnknownFieldSet _unknownFields;

	public const int AuthEndpointFieldNumber = 1;

	private EndpointInfo authEndpoint_;

	public const int HydraAuthTicketFieldNumber = 2;

	private string hydraAuthTicket_ = "";

	public const int ServerTokenFieldNumber = 3;

	private string serverToken_ = "";

	public const int VersionFieldNumber = 4;

	private string version_ = "";

	public const int GameSessionIdFieldNumber = 5;

	private string gameSessionId_ = "";

	public const int TitleIdFieldNumber = 6;

	private string titleId_ = "";

	public const int ExcludedDsmIdsFieldNumber = 7;

	private static readonly FieldCodec<string> _repeated_excludedDsmIds_codec = FieldCodec.ForString(58u);

	private readonly RepeatedField<string> excludedDsmIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<PendingSession> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PendingSessionReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public EndpointInfo AuthEndpoint
	{
		get
		{
			return authEndpoint_;
		}
		set
		{
			authEndpoint_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string HydraAuthTicket
	{
		get
		{
			return hydraAuthTicket_;
		}
		set
		{
			hydraAuthTicket_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ServerToken
	{
		get
		{
			return serverToken_;
		}
		set
		{
			serverToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string GameSessionId
	{
		get
		{
			return gameSessionId_;
		}
		set
		{
			gameSessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

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
	public RepeatedField<string> ExcludedDsmIds => excludedDsmIds_;

	[DebuggerNonUserCode]
	public PendingSession()
	{
	}

	[DebuggerNonUserCode]
	public PendingSession(PendingSession other)
		: this()
	{
		authEndpoint_ = ((other.authEndpoint_ != null) ? other.authEndpoint_.Clone() : null);
		hydraAuthTicket_ = other.hydraAuthTicket_;
		serverToken_ = other.serverToken_;
		version_ = other.version_;
		gameSessionId_ = other.gameSessionId_;
		titleId_ = other.titleId_;
		excludedDsmIds_ = other.excludedDsmIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PendingSession Clone()
	{
		return new PendingSession(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PendingSession);
	}

	[DebuggerNonUserCode]
	public bool Equals(PendingSession other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(AuthEndpoint, other.AuthEndpoint))
		{
			return false;
		}
		if (HydraAuthTicket != other.HydraAuthTicket)
		{
			return false;
		}
		if (ServerToken != other.ServerToken)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		if (GameSessionId != other.GameSessionId)
		{
			return false;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (!excludedDsmIds_.Equals(other.excludedDsmIds_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (authEndpoint_ != null)
		{
			num ^= AuthEndpoint.GetHashCode();
		}
		if (HydraAuthTicket.Length != 0)
		{
			num ^= HydraAuthTicket.GetHashCode();
		}
		if (ServerToken.Length != 0)
		{
			num ^= ServerToken.GetHashCode();
		}
		if (Version.Length != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (GameSessionId.Length != 0)
		{
			num ^= GameSessionId.GetHashCode();
		}
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		num ^= excludedDsmIds_.GetHashCode();
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
		if (authEndpoint_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(AuthEndpoint);
		}
		if (HydraAuthTicket.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(HydraAuthTicket);
		}
		if (ServerToken.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ServerToken);
		}
		if (Version.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(Version);
		}
		if (GameSessionId.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(GameSessionId);
		}
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(TitleId);
		}
		excludedDsmIds_.WriteTo(ref output, _repeated_excludedDsmIds_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (authEndpoint_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(AuthEndpoint);
		}
		if (HydraAuthTicket.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(HydraAuthTicket);
		}
		if (ServerToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerToken);
		}
		if (Version.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Version);
		}
		if (GameSessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(GameSessionId);
		}
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		num += excludedDsmIds_.CalculateSize(_repeated_excludedDsmIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PendingSession other)
	{
		if (other == null)
		{
			return;
		}
		if (other.authEndpoint_ != null)
		{
			if (authEndpoint_ == null)
			{
				AuthEndpoint = new EndpointInfo();
			}
			AuthEndpoint.MergeFrom(other.AuthEndpoint);
		}
		if (other.HydraAuthTicket.Length != 0)
		{
			HydraAuthTicket = other.HydraAuthTicket;
		}
		if (other.ServerToken.Length != 0)
		{
			ServerToken = other.ServerToken;
		}
		if (other.Version.Length != 0)
		{
			Version = other.Version;
		}
		if (other.GameSessionId.Length != 0)
		{
			GameSessionId = other.GameSessionId;
		}
		if (other.TitleId.Length != 0)
		{
			TitleId = other.TitleId;
		}
		excludedDsmIds_.Add(other.excludedDsmIds_);
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
				if (authEndpoint_ == null)
				{
					AuthEndpoint = new EndpointInfo();
				}
				input.ReadMessage(AuthEndpoint);
				break;
			case 18u:
				HydraAuthTicket = input.ReadString();
				break;
			case 26u:
				ServerToken = input.ReadString();
				break;
			case 34u:
				Version = input.ReadString();
				break;
			case 42u:
				GameSessionId = input.ReadString();
				break;
			case 50u:
				TitleId = input.ReadString();
				break;
			case 58u:
				excludedDsmIds_.AddEntriesFrom(ref input, _repeated_excludedDsmIds_codec);
				break;
			}
		}
	}
}
