using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class CreateSessionResponse : IMessage<CreateSessionResponse>, IMessage, IEquatable<CreateSessionResponse>, IDeepCloneable<CreateSessionResponse>, IBufferMessage
{
	private static readonly MessageParser<CreateSessionResponse> _parser = new MessageParser<CreateSessionResponse>(() => new CreateSessionResponse());

	private UnknownFieldSet _unknownFields;

	public const int GameSessionIdFieldNumber = 1;

	private string gameSessionId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<CreateSessionResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public CreateSessionResponse()
	{
	}

	[DebuggerNonUserCode]
	public CreateSessionResponse(CreateSessionResponse other)
		: this()
	{
		gameSessionId_ = other.gameSessionId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CreateSessionResponse Clone()
	{
		return new CreateSessionResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CreateSessionResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(CreateSessionResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GameSessionId != other.GameSessionId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (GameSessionId.Length != 0)
		{
			num ^= GameSessionId.GetHashCode();
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
		if (GameSessionId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(GameSessionId);
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
		if (GameSessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(GameSessionId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CreateSessionResponse other)
	{
		if (other != null)
		{
			if (other.GameSessionId.Length != 0)
			{
				GameSessionId = other.GameSessionId;
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
				GameSessionId = input.ReadString();
			}
		}
	}
}
