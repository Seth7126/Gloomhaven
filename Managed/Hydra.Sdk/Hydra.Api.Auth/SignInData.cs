using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.Auth;

public sealed class SignInData : IMessage<SignInData>, IMessage, IEquatable<SignInData>, IDeepCloneable<SignInData>, IBufferMessage
{
	private static readonly MessageParser<SignInData> _parser = new MessageParser<SignInData>(() => new SignInData());

	private UnknownFieldSet _unknownFields;

	public const int TitleFieldNumber = 1;

	private TitleIdentity title_;

	public const int VersionsFieldNumber = 2;

	private static readonly FieldCodec<ServiceIdentity> _repeated_versions_codec = FieldCodec.ForMessage(18u, ServiceIdentity.Parser);

	private readonly RepeatedField<ServiceIdentity> versions_ = new RepeatedField<ServiceIdentity>();

	public const int BuildVersionFieldNumber = 3;

	private string buildVersion_ = "";

	public const int BuildPlatformFieldNumber = 4;

	private Platform buildPlatform_ = Platform.Unknown;

	public const int RuntimePlatformFieldNumber = 5;

	private PlatformDetails runtimePlatform_;

	[DebuggerNonUserCode]
	public static MessageParser<SignInData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public RepeatedField<ServiceIdentity> Versions => versions_;

	[DebuggerNonUserCode]
	public string BuildVersion
	{
		get
		{
			return buildVersion_;
		}
		set
		{
			buildVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public Platform BuildPlatform
	{
		get
		{
			return buildPlatform_;
		}
		set
		{
			buildPlatform_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PlatformDetails RuntimePlatform
	{
		get
		{
			return runtimePlatform_;
		}
		set
		{
			runtimePlatform_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SignInData()
	{
	}

	[DebuggerNonUserCode]
	public SignInData(SignInData other)
		: this()
	{
		title_ = ((other.title_ != null) ? other.title_.Clone() : null);
		versions_ = other.versions_.Clone();
		buildVersion_ = other.buildVersion_;
		buildPlatform_ = other.buildPlatform_;
		runtimePlatform_ = ((other.runtimePlatform_ != null) ? other.runtimePlatform_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInData Clone()
	{
		return new SignInData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInData);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Title, other.Title))
		{
			return false;
		}
		if (!versions_.Equals(other.versions_))
		{
			return false;
		}
		if (BuildVersion != other.BuildVersion)
		{
			return false;
		}
		if (BuildPlatform != other.BuildPlatform)
		{
			return false;
		}
		if (!object.Equals(RuntimePlatform, other.RuntimePlatform))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (title_ != null)
		{
			num ^= Title.GetHashCode();
		}
		num ^= versions_.GetHashCode();
		if (BuildVersion.Length != 0)
		{
			num ^= BuildVersion.GetHashCode();
		}
		if (BuildPlatform != Platform.Unknown)
		{
			num ^= BuildPlatform.GetHashCode();
		}
		if (runtimePlatform_ != null)
		{
			num ^= RuntimePlatform.GetHashCode();
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
		if (title_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Title);
		}
		versions_.WriteTo(ref output, _repeated_versions_codec);
		if (BuildVersion.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(BuildVersion);
		}
		if (BuildPlatform != Platform.Unknown)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)BuildPlatform);
		}
		if (runtimePlatform_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(RuntimePlatform);
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
		if (title_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Title);
		}
		num += versions_.CalculateSize(_repeated_versions_codec);
		if (BuildVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(BuildVersion);
		}
		if (BuildPlatform != Platform.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BuildPlatform);
		}
		if (runtimePlatform_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(RuntimePlatform);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.title_ != null)
		{
			if (title_ == null)
			{
				Title = new TitleIdentity();
			}
			Title.MergeFrom(other.Title);
		}
		versions_.Add(other.versions_);
		if (other.BuildVersion.Length != 0)
		{
			BuildVersion = other.BuildVersion;
		}
		if (other.BuildPlatform != Platform.Unknown)
		{
			BuildPlatform = other.BuildPlatform;
		}
		if (other.runtimePlatform_ != null)
		{
			if (runtimePlatform_ == null)
			{
				RuntimePlatform = new PlatformDetails();
			}
			RuntimePlatform.MergeFrom(other.RuntimePlatform);
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
				if (title_ == null)
				{
					Title = new TitleIdentity();
				}
				input.ReadMessage(Title);
				break;
			case 18u:
				versions_.AddEntriesFrom(ref input, _repeated_versions_codec);
				break;
			case 26u:
				BuildVersion = input.ReadString();
				break;
			case 32u:
				BuildPlatform = (Platform)input.ReadEnum();
				break;
			case 42u:
				if (runtimePlatform_ == null)
				{
					RuntimePlatform = new PlatformDetails();
				}
				input.ReadMessage(RuntimePlatform);
				break;
			}
		}
	}
}
