using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.Auth;

public sealed class SignInToolRequest : IMessage<SignInToolRequest>, IMessage, IEquatable<SignInToolRequest>, IDeepCloneable<SignInToolRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInToolRequest> _parser = new MessageParser<SignInToolRequest>(() => new SignInToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int VersionsFieldNumber = 1;

	private static readonly FieldCodec<ServiceIdentity> _repeated_versions_codec = FieldCodec.ForMessage(10u, ServiceIdentity.Parser);

	private readonly RepeatedField<ServiceIdentity> versions_ = new RepeatedField<ServiceIdentity>();

	public const int SecretFieldNumber = 2;

	private string secret_ = "";

	public const int TitleFieldNumber = 3;

	private TitleIdentity title_;

	[DebuggerNonUserCode]
	public static MessageParser<SignInToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[37];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<ServiceIdentity> Versions => versions_;

	[DebuggerNonUserCode]
	[Obsolete]
	public string Secret
	{
		get
		{
			return secret_;
		}
		set
		{
			secret_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TitleIdentity Title
	{
		get
		{
			return title_;
		}
		set
		{
			title_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SignInToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInToolRequest(SignInToolRequest other)
		: this()
	{
		versions_ = other.versions_.Clone();
		secret_ = other.secret_;
		title_ = ((other.title_ != null) ? other.title_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInToolRequest Clone()
	{
		return new SignInToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInToolRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!versions_.Equals(other.versions_))
		{
			return false;
		}
		if (Secret != other.Secret)
		{
			return false;
		}
		if (!object.Equals(Title, other.Title))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= versions_.GetHashCode();
		if (Secret.Length != 0)
		{
			num ^= Secret.GetHashCode();
		}
		if (title_ != null)
		{
			num ^= Title.GetHashCode();
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
		versions_.WriteTo(ref output, _repeated_versions_codec);
		if (Secret.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Secret);
		}
		if (title_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Title);
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
		num += versions_.CalculateSize(_repeated_versions_codec);
		if (Secret.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Secret);
		}
		if (title_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Title);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInToolRequest other)
	{
		if (other == null)
		{
			return;
		}
		versions_.Add(other.versions_);
		if (other.Secret.Length != 0)
		{
			Secret = other.Secret;
		}
		if (other.title_ != null)
		{
			if (title_ == null)
			{
				Title = new TitleIdentity();
			}
			Title.MergeFrom(other.Title);
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
				versions_.AddEntriesFrom(ref input, _repeated_versions_codec);
				break;
			case 18u:
				Secret = input.ReadString();
				break;
			case 26u:
				if (title_ == null)
				{
					Title = new TitleIdentity();
				}
				input.ReadMessage(Title);
				break;
			}
		}
	}
}
