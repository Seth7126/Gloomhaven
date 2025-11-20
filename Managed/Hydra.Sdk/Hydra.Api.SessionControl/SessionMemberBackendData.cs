using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class SessionMemberBackendData : IMessage<SessionMemberBackendData>, IMessage, IEquatable<SessionMemberBackendData>, IDeepCloneable<SessionMemberBackendData>, IBufferMessage
{
	private static readonly MessageParser<SessionMemberBackendData> _parser = new MessageParser<SessionMemberBackendData>(() => new SessionMemberBackendData());

	private UnknownFieldSet _unknownFields;

	public const int KeyInfoFieldNumber = 1;

	private KeyContainer keyInfo_;

	[DebuggerNonUserCode]
	public static MessageParser<SessionMemberBackendData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MemberEventReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public KeyContainer KeyInfo
	{
		get
		{
			return keyInfo_;
		}
		set
		{
			keyInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SessionMemberBackendData()
	{
	}

	[DebuggerNonUserCode]
	public SessionMemberBackendData(SessionMemberBackendData other)
		: this()
	{
		keyInfo_ = ((other.keyInfo_ != null) ? other.keyInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SessionMemberBackendData Clone()
	{
		return new SessionMemberBackendData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SessionMemberBackendData);
	}

	[DebuggerNonUserCode]
	public bool Equals(SessionMemberBackendData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(KeyInfo, other.KeyInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (keyInfo_ != null)
		{
			num ^= KeyInfo.GetHashCode();
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
		if (keyInfo_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(KeyInfo);
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
		if (keyInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(KeyInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SessionMemberBackendData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.keyInfo_ != null)
		{
			if (keyInfo_ == null)
			{
				KeyInfo = new KeyContainer();
			}
			KeyInfo.MergeFrom(other.KeyInfo);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (keyInfo_ == null)
			{
				KeyInfo = new KeyContainer();
			}
			input.ReadMessage(KeyInfo);
		}
	}
}
