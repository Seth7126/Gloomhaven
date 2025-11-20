using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class SessionMemberEventResult : IMessage<SessionMemberEventResult>, IMessage, IEquatable<SessionMemberEventResult>, IDeepCloneable<SessionMemberEventResult>, IBufferMessage
{
	private static readonly MessageParser<SessionMemberEventResult> _parser = new MessageParser<SessionMemberEventResult>(() => new SessionMemberEventResult());

	private UnknownFieldSet _unknownFields;

	public const int KeyFieldNumber = 1;

	private string key_ = "";

	public const int DataFieldNumber = 2;

	private SessionMemberEventResultData data_;

	[DebuggerNonUserCode]
	public static MessageParser<SessionMemberEventResult> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MemberEventReflection.Descriptor.MessageTypes[9];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Key
	{
		get
		{
			return key_;
		}
		set
		{
			key_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SessionMemberEventResultData Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SessionMemberEventResult()
	{
	}

	[DebuggerNonUserCode]
	public SessionMemberEventResult(SessionMemberEventResult other)
		: this()
	{
		key_ = other.key_;
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SessionMemberEventResult Clone()
	{
		return new SessionMemberEventResult(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SessionMemberEventResult);
	}

	[DebuggerNonUserCode]
	public bool Equals(SessionMemberEventResult other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Key != other.Key)
		{
			return false;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Key.Length != 0)
		{
			num ^= Key.GetHashCode();
		}
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
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
		if (Key.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Key);
		}
		if (data_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Data);
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
		if (Key.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Key);
		}
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SessionMemberEventResult other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Key.Length != 0)
		{
			Key = other.Key;
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new SessionMemberEventResultData();
			}
			Data.MergeFrom(other.Data);
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
				Key = input.ReadString();
				break;
			case 18u:
				if (data_ == null)
				{
					Data = new SessionMemberEventResultData();
				}
				input.ReadMessage(Data);
				break;
			}
		}
	}
}
