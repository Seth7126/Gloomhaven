using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class GetCustomDeveloperStringMsStoreResponse : IMessage<GetCustomDeveloperStringMsStoreResponse>, IMessage, IEquatable<GetCustomDeveloperStringMsStoreResponse>, IDeepCloneable<GetCustomDeveloperStringMsStoreResponse>, IBufferMessage
{
	private static readonly MessageParser<GetCustomDeveloperStringMsStoreResponse> _parser = new MessageParser<GetCustomDeveloperStringMsStoreResponse>(() => new GetCustomDeveloperStringMsStoreResponse());

	private UnknownFieldSet _unknownFields;

	public const int CustomDeveloperStringFieldNumber = 1;

	private string customDeveloperString_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetCustomDeveloperStringMsStoreResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[17];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string CustomDeveloperString
	{
		get
		{
			return customDeveloperString_;
		}
		set
		{
			customDeveloperString_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetCustomDeveloperStringMsStoreResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetCustomDeveloperStringMsStoreResponse(GetCustomDeveloperStringMsStoreResponse other)
		: this()
	{
		customDeveloperString_ = other.customDeveloperString_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetCustomDeveloperStringMsStoreResponse Clone()
	{
		return new GetCustomDeveloperStringMsStoreResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetCustomDeveloperStringMsStoreResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetCustomDeveloperStringMsStoreResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CustomDeveloperString != other.CustomDeveloperString)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (CustomDeveloperString.Length != 0)
		{
			num ^= CustomDeveloperString.GetHashCode();
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
		if (CustomDeveloperString.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(CustomDeveloperString);
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
		if (CustomDeveloperString.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CustomDeveloperString);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetCustomDeveloperStringMsStoreResponse other)
	{
		if (other != null)
		{
			if (other.CustomDeveloperString.Length != 0)
			{
				CustomDeveloperString = other.CustomDeveloperString;
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				CustomDeveloperString = input.ReadString();
			}
		}
	}
}
