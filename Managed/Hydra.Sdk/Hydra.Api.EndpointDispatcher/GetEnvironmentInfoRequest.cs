using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.EndpointDispatcher;

public sealed class GetEnvironmentInfoRequest : IMessage<GetEnvironmentInfoRequest>, IMessage, IEquatable<GetEnvironmentInfoRequest>, IDeepCloneable<GetEnvironmentInfoRequest>, IBufferMessage
{
	private static readonly MessageParser<GetEnvironmentInfoRequest> _parser = new MessageParser<GetEnvironmentInfoRequest>(() => new GetEnvironmentInfoRequest());

	private UnknownFieldSet _unknownFields;

	public const int AuthorizationFieldNumber = 1;

	private ServiceIdentity authorization_;

	public const int TitleFieldNumber = 2;

	private TitleIdentity title_;

	public const int BuildVersionFieldNumber = 3;

	private string buildVersion_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetEnvironmentInfoRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EndpointDispatcherContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServiceIdentity Authorization
	{
		get
		{
			return authorization_;
		}
		set
		{
			authorization_ = value;
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
	public GetEnvironmentInfoRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetEnvironmentInfoRequest(GetEnvironmentInfoRequest other)
		: this()
	{
		authorization_ = ((other.authorization_ != null) ? other.authorization_.Clone() : null);
		title_ = ((other.title_ != null) ? other.title_.Clone() : null);
		buildVersion_ = other.buildVersion_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetEnvironmentInfoRequest Clone()
	{
		return new GetEnvironmentInfoRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetEnvironmentInfoRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetEnvironmentInfoRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Authorization, other.Authorization))
		{
			return false;
		}
		if (!object.Equals(Title, other.Title))
		{
			return false;
		}
		if (BuildVersion != other.BuildVersion)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (authorization_ != null)
		{
			num ^= Authorization.GetHashCode();
		}
		if (title_ != null)
		{
			num ^= Title.GetHashCode();
		}
		if (BuildVersion.Length != 0)
		{
			num ^= BuildVersion.GetHashCode();
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
		if (authorization_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Authorization);
		}
		if (title_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Title);
		}
		if (BuildVersion.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(BuildVersion);
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
		if (authorization_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Authorization);
		}
		if (title_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Title);
		}
		if (BuildVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(BuildVersion);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetEnvironmentInfoRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.authorization_ != null)
		{
			if (authorization_ == null)
			{
				Authorization = new ServiceIdentity();
			}
			Authorization.MergeFrom(other.Authorization);
		}
		if (other.title_ != null)
		{
			if (title_ == null)
			{
				Title = new TitleIdentity();
			}
			Title.MergeFrom(other.Title);
		}
		if (other.BuildVersion.Length != 0)
		{
			BuildVersion = other.BuildVersion;
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
				if (authorization_ == null)
				{
					Authorization = new ServiceIdentity();
				}
				input.ReadMessage(Authorization);
				break;
			case 18u:
				if (title_ == null)
				{
					Title = new TitleIdentity();
				}
				input.ReadMessage(Title);
				break;
			case 26u:
				BuildVersion = input.ReadString();
				break;
			}
		}
	}
}
