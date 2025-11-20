using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure;

public sealed class ServiceAttribute : IMessage<ServiceAttribute>, IMessage, IEquatable<ServiceAttribute>, IDeepCloneable<ServiceAttribute>, IBufferMessage
{
	private static readonly MessageParser<ServiceAttribute> _parser = new MessageParser<ServiceAttribute>(() => new ServiceAttribute());

	private UnknownFieldSet _unknownFields;

	public const int VersionFieldNumber = 1;

	private ServiceVersion version_;

	public const int AccessRoleFieldNumber = 2;

	private string accessRole_ = "";

	public const int ScopeFieldNumber = 3;

	private string scope_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ServiceAttribute> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => OptionsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServiceVersion Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string AccessRole
	{
		get
		{
			return accessRole_;
		}
		set
		{
			accessRole_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Scope
	{
		get
		{
			return scope_;
		}
		set
		{
			scope_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ServiceAttribute()
	{
	}

	[DebuggerNonUserCode]
	public ServiceAttribute(ServiceAttribute other)
		: this()
	{
		version_ = ((other.version_ != null) ? other.version_.Clone() : null);
		accessRole_ = other.accessRole_;
		scope_ = other.scope_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServiceAttribute Clone()
	{
		return new ServiceAttribute(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServiceAttribute);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServiceAttribute other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Version, other.Version))
		{
			return false;
		}
		if (AccessRole != other.AccessRole)
		{
			return false;
		}
		if (Scope != other.Scope)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (version_ != null)
		{
			num ^= Version.GetHashCode();
		}
		if (AccessRole.Length != 0)
		{
			num ^= AccessRole.GetHashCode();
		}
		if (Scope.Length != 0)
		{
			num ^= Scope.GetHashCode();
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
		if (version_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Version);
		}
		if (AccessRole.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(AccessRole);
		}
		if (Scope.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Scope);
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
		if (version_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Version);
		}
		if (AccessRole.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AccessRole);
		}
		if (Scope.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Scope);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServiceAttribute other)
	{
		if (other == null)
		{
			return;
		}
		if (other.version_ != null)
		{
			if (version_ == null)
			{
				Version = new ServiceVersion();
			}
			Version.MergeFrom(other.Version);
		}
		if (other.AccessRole.Length != 0)
		{
			AccessRole = other.AccessRole;
		}
		if (other.Scope.Length != 0)
		{
			Scope = other.Scope;
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
				if (version_ == null)
				{
					Version = new ServiceVersion();
				}
				input.ReadMessage(Version);
				break;
			case 18u:
				AccessRole = input.ReadString();
				break;
			case 26u:
				Scope = input.ReadString();
				break;
			}
		}
	}
}
