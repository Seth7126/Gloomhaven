using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class MemberAcceptResult : IMessage<MemberAcceptResult>, IMessage, IEquatable<MemberAcceptResult>, IDeepCloneable<MemberAcceptResult>, IBufferMessage
{
	private static readonly MessageParser<MemberAcceptResult> _parser = new MessageParser<MemberAcceptResult>(() => new MemberAcceptResult());

	private UnknownFieldSet _unknownFields;

	public const int StatusFieldNumber = 1;

	private AcceptStatus status_ = AcceptStatus.Pending;

	public const int DataFieldNumber = 2;

	private AcceptData data_;

	[DebuggerNonUserCode]
	public static MessageParser<MemberAcceptResult> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MemberEventReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public AcceptStatus Status
	{
		get
		{
			return status_;
		}
		set
		{
			status_ = value;
		}
	}

	[DebuggerNonUserCode]
	public AcceptData Data
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
	public MemberAcceptResult()
	{
	}

	[DebuggerNonUserCode]
	public MemberAcceptResult(MemberAcceptResult other)
		: this()
	{
		status_ = other.status_;
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MemberAcceptResult Clone()
	{
		return new MemberAcceptResult(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MemberAcceptResult);
	}

	[DebuggerNonUserCode]
	public bool Equals(MemberAcceptResult other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Status != other.Status)
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
		if (Status != AcceptStatus.Pending)
		{
			num ^= Status.GetHashCode();
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
		if (Status != AcceptStatus.Pending)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)Status);
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
		if (Status != AcceptStatus.Pending)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Status);
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
	public void MergeFrom(MemberAcceptResult other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Status != AcceptStatus.Pending)
		{
			Status = other.Status;
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new AcceptData();
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
			case 8u:
				Status = (AcceptStatus)input.ReadEnum();
				break;
			case 18u:
				if (data_ == null)
				{
					Data = new AcceptData();
				}
				input.ReadMessage(Data);
				break;
			}
		}
	}
}
