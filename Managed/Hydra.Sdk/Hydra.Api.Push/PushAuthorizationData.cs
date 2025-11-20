using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Push;

public sealed class PushAuthorizationData : IMessage<PushAuthorizationData>, IMessage, IEquatable<PushAuthorizationData>, IDeepCloneable<PushAuthorizationData>, IBufferMessage
{
	public enum ContextOneofCase
	{
		None = 0,
		UserContext = 4,
		ServerContext = 5
	}

	private static readonly MessageParser<PushAuthorizationData> _parser = new MessageParser<PushAuthorizationData>(() => new PushAuthorizationData());

	private UnknownFieldSet _unknownFields;

	public const int GenerationFieldNumber = 1;

	private int generation_;

	public const int TokenFieldNumber = 2;

	private PushToken token_;

	public const int VersionsFieldNumber = 3;

	private static readonly FieldCodec<PushVersion> _repeated_versions_codec = FieldCodec.ForMessage(26u, PushVersion.Parser);

	private readonly RepeatedField<PushVersion> versions_ = new RepeatedField<PushVersion>();

	public const int UserContextFieldNumber = 4;

	public const int ServerContextFieldNumber = 5;

	private object context_;

	private ContextOneofCase contextCase_ = ContextOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<PushAuthorizationData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushClientDataReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int Generation
	{
		get
		{
			return generation_;
		}
		set
		{
			generation_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PushToken Token
	{
		get
		{
			return token_;
		}
		set
		{
			token_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<PushVersion> Versions => versions_;

	[DebuggerNonUserCode]
	public UserContext UserContext
	{
		get
		{
			return (contextCase_ == ContextOneofCase.UserContext) ? ((UserContext)context_) : null;
		}
		set
		{
			context_ = value;
			contextCase_ = ((value != null) ? ContextOneofCase.UserContext : ContextOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public ServerContext ServerContext
	{
		get
		{
			return (contextCase_ == ContextOneofCase.ServerContext) ? ((ServerContext)context_) : null;
		}
		set
		{
			context_ = value;
			contextCase_ = ((value != null) ? ContextOneofCase.ServerContext : ContextOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public ContextOneofCase ContextCase => contextCase_;

	[DebuggerNonUserCode]
	public PushAuthorizationData()
	{
	}

	[DebuggerNonUserCode]
	public PushAuthorizationData(PushAuthorizationData other)
		: this()
	{
		generation_ = other.generation_;
		token_ = ((other.token_ != null) ? other.token_.Clone() : null);
		versions_ = other.versions_.Clone();
		switch (other.ContextCase)
		{
		case ContextOneofCase.UserContext:
			UserContext = other.UserContext.Clone();
			break;
		case ContextOneofCase.ServerContext:
			ServerContext = other.ServerContext.Clone();
			break;
		}
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushAuthorizationData Clone()
	{
		return new PushAuthorizationData(this);
	}

	[DebuggerNonUserCode]
	public void ClearContext()
	{
		contextCase_ = ContextOneofCase.None;
		context_ = null;
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushAuthorizationData);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushAuthorizationData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Generation != other.Generation)
		{
			return false;
		}
		if (!object.Equals(Token, other.Token))
		{
			return false;
		}
		if (!versions_.Equals(other.versions_))
		{
			return false;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (!object.Equals(ServerContext, other.ServerContext))
		{
			return false;
		}
		if (ContextCase != other.ContextCase)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Generation != 0)
		{
			num ^= Generation.GetHashCode();
		}
		if (token_ != null)
		{
			num ^= Token.GetHashCode();
		}
		num ^= versions_.GetHashCode();
		if (contextCase_ == ContextOneofCase.UserContext)
		{
			num ^= UserContext.GetHashCode();
		}
		if (contextCase_ == ContextOneofCase.ServerContext)
		{
			num ^= ServerContext.GetHashCode();
		}
		num ^= (int)contextCase_;
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
		if (Generation != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(Generation);
		}
		if (token_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Token);
		}
		versions_.WriteTo(ref output, _repeated_versions_codec);
		if (contextCase_ == ContextOneofCase.UserContext)
		{
			output.WriteRawTag(34);
			output.WriteMessage(UserContext);
		}
		if (contextCase_ == ContextOneofCase.ServerContext)
		{
			output.WriteRawTag(42);
			output.WriteMessage(ServerContext);
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
		if (Generation != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Generation);
		}
		if (token_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Token);
		}
		num += versions_.CalculateSize(_repeated_versions_codec);
		if (contextCase_ == ContextOneofCase.UserContext)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (contextCase_ == ContextOneofCase.ServerContext)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerContext);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushAuthorizationData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Generation != 0)
		{
			Generation = other.Generation;
		}
		if (other.token_ != null)
		{
			if (token_ == null)
			{
				Token = new PushToken();
			}
			Token.MergeFrom(other.Token);
		}
		versions_.Add(other.versions_);
		switch (other.ContextCase)
		{
		case ContextOneofCase.UserContext:
			if (UserContext == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
			break;
		case ContextOneofCase.ServerContext:
			if (ServerContext == null)
			{
				ServerContext = new ServerContext();
			}
			ServerContext.MergeFrom(other.ServerContext);
			break;
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
			case 8u:
				Generation = input.ReadInt32();
				break;
			case 18u:
				if (token_ == null)
				{
					Token = new PushToken();
				}
				input.ReadMessage(Token);
				break;
			case 26u:
				versions_.AddEntriesFrom(ref input, _repeated_versions_codec);
				break;
			case 34u:
			{
				UserContext userContext = new UserContext();
				if (contextCase_ == ContextOneofCase.UserContext)
				{
					userContext.MergeFrom(UserContext);
				}
				input.ReadMessage(userContext);
				UserContext = userContext;
				break;
			}
			case 42u:
			{
				ServerContext serverContext = new ServerContext();
				if (contextCase_ == ContextOneofCase.ServerContext)
				{
					serverContext.MergeFrom(ServerContext);
				}
				input.ReadMessage(serverContext);
				ServerContext = serverContext;
				break;
			}
			}
		}
	}
}
