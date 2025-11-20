using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class GetStandaloneSignInCodeResponse : IMessage<GetStandaloneSignInCodeResponse>, IMessage, IEquatable<GetStandaloneSignInCodeResponse>, IDeepCloneable<GetStandaloneSignInCodeResponse>, IBufferMessage
{
	private static readonly MessageParser<GetStandaloneSignInCodeResponse> _parser = new MessageParser<GetStandaloneSignInCodeResponse>(() => new GetStandaloneSignInCodeResponse());

	private UnknownFieldSet _unknownFields;

	public const int StandaloneCodeFieldNumber = 1;

	private string standaloneCode_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetStandaloneSignInCodeResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[36];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string StandaloneCode
	{
		get
		{
			return standaloneCode_;
		}
		set
		{
			standaloneCode_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetStandaloneSignInCodeResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetStandaloneSignInCodeResponse(GetStandaloneSignInCodeResponse other)
		: this()
	{
		standaloneCode_ = other.standaloneCode_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetStandaloneSignInCodeResponse Clone()
	{
		return new GetStandaloneSignInCodeResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetStandaloneSignInCodeResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetStandaloneSignInCodeResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (StandaloneCode != other.StandaloneCode)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (StandaloneCode.Length != 0)
		{
			num ^= StandaloneCode.GetHashCode();
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
		if (StandaloneCode.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(StandaloneCode);
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
		if (StandaloneCode.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StandaloneCode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetStandaloneSignInCodeResponse other)
	{
		if (other != null)
		{
			if (other.StandaloneCode.Length != 0)
			{
				StandaloneCode = other.StandaloneCode;
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
				StandaloneCode = input.ReadString();
			}
		}
	}
}
