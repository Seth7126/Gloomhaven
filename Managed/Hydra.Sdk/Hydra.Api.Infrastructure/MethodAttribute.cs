using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure;

public sealed class MethodAttribute : IMessage<MethodAttribute>, IMessage, IEquatable<MethodAttribute>, IDeepCloneable<MethodAttribute>, IBufferMessage
{
	private static readonly MessageParser<MethodAttribute> _parser = new MessageParser<MethodAttribute>(() => new MethodAttribute());

	private UnknownFieldSet _unknownFields;

	public const int AccessRoleFieldNumber = 1;

	private string accessRole_ = "";

	public const int ScopeFieldNumber = 2;

	private string scope_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<MethodAttribute> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => OptionsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public MethodAttribute()
	{
	}

	[DebuggerNonUserCode]
	public MethodAttribute(MethodAttribute other)
		: this()
	{
		accessRole_ = other.accessRole_;
		scope_ = other.scope_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MethodAttribute Clone()
	{
		return new MethodAttribute(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MethodAttribute);
	}

	[DebuggerNonUserCode]
	public bool Equals(MethodAttribute other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
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
		if (AccessRole.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(AccessRole);
		}
		if (Scope.Length != 0)
		{
			output.WriteRawTag(18);
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
	public void MergeFrom(MethodAttribute other)
	{
		if (other != null)
		{
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
				AccessRole = input.ReadString();
				break;
			case 18u:
				Scope = input.ReadString();
				break;
			}
		}
	}
}
