using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Errors;

public sealed class ErrorCodeDesc : IMessage<ErrorCodeDesc>, IMessage, IEquatable<ErrorCodeDesc>, IDeepCloneable<ErrorCodeDesc>, IBufferMessage
{
	private static readonly MessageParser<ErrorCodeDesc> _parser = new MessageParser<ErrorCodeDesc>(() => new ErrorCodeDesc());

	private UnknownFieldSet _unknownFields;

	public const int ErrorTextFieldNumber = 1;

	private string errorText_ = "";

	public const int ErrorSeverityFieldNumber = 2;

	private ErrorSeverity errorSeverity_ = ErrorSeverity.Undefined;

	public const int ErrorCategoryFieldNumber = 3;

	private ErrorCategory errorCategory_ = ErrorCategory.Fatal;

	[DebuggerNonUserCode]
	public static MessageParser<ErrorCodeDesc> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ErrorCodeDescReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ErrorText
	{
		get
		{
			return errorText_;
		}
		set
		{
			errorText_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ErrorSeverity ErrorSeverity
	{
		get
		{
			return errorSeverity_;
		}
		set
		{
			errorSeverity_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ErrorCategory ErrorCategory
	{
		get
		{
			return errorCategory_;
		}
		set
		{
			errorCategory_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ErrorCodeDesc()
	{
	}

	[DebuggerNonUserCode]
	public ErrorCodeDesc(ErrorCodeDesc other)
		: this()
	{
		errorText_ = other.errorText_;
		errorSeverity_ = other.errorSeverity_;
		errorCategory_ = other.errorCategory_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ErrorCodeDesc Clone()
	{
		return new ErrorCodeDesc(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ErrorCodeDesc);
	}

	[DebuggerNonUserCode]
	public bool Equals(ErrorCodeDesc other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ErrorText != other.ErrorText)
		{
			return false;
		}
		if (ErrorSeverity != other.ErrorSeverity)
		{
			return false;
		}
		if (ErrorCategory != other.ErrorCategory)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ErrorText.Length != 0)
		{
			num ^= ErrorText.GetHashCode();
		}
		if (ErrorSeverity != ErrorSeverity.Undefined)
		{
			num ^= ErrorSeverity.GetHashCode();
		}
		if (ErrorCategory != ErrorCategory.Fatal)
		{
			num ^= ErrorCategory.GetHashCode();
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
		if (ErrorText.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ErrorText);
		}
		if (ErrorSeverity != ErrorSeverity.Undefined)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ErrorSeverity);
		}
		if (ErrorCategory != ErrorCategory.Fatal)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)ErrorCategory);
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
		if (ErrorText.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ErrorText);
		}
		if (ErrorSeverity != ErrorSeverity.Undefined)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ErrorSeverity);
		}
		if (ErrorCategory != ErrorCategory.Fatal)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ErrorCategory);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ErrorCodeDesc other)
	{
		if (other != null)
		{
			if (other.ErrorText.Length != 0)
			{
				ErrorText = other.ErrorText;
			}
			if (other.ErrorSeverity != ErrorSeverity.Undefined)
			{
				ErrorSeverity = other.ErrorSeverity;
			}
			if (other.ErrorCategory != ErrorCategory.Fatal)
			{
				ErrorCategory = other.ErrorCategory;
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
				ErrorText = input.ReadString();
				break;
			case 16u:
				ErrorSeverity = (ErrorSeverity)input.ReadEnum();
				break;
			case 24u:
				ErrorCategory = (ErrorCategory)input.ReadEnum();
				break;
			}
		}
	}
}
