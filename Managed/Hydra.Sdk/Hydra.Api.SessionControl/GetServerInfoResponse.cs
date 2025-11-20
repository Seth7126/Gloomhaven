using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class GetServerInfoResponse : IMessage<GetServerInfoResponse>, IMessage, IEquatable<GetServerInfoResponse>, IDeepCloneable<GetServerInfoResponse>, IBufferMessage
{
	private static readonly MessageParser<GetServerInfoResponse> _parser = new MessageParser<GetServerInfoResponse>(() => new GetServerInfoResponse());

	private UnknownFieldSet _unknownFields;

	public const int AcceptClientResultFieldNumber = 1;

	private AcceptClientResult acceptClientResult_;

	public const int RefreshAfterSecondsFieldNumber = 2;

	private int refreshAfterSeconds_;

	[DebuggerNonUserCode]
	public static MessageParser<GetServerInfoResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public AcceptClientResult AcceptClientResult
	{
		get
		{
			return acceptClientResult_;
		}
		set
		{
			acceptClientResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int RefreshAfterSeconds
	{
		get
		{
			return refreshAfterSeconds_;
		}
		set
		{
			refreshAfterSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetServerInfoResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetServerInfoResponse(GetServerInfoResponse other)
		: this()
	{
		acceptClientResult_ = ((other.acceptClientResult_ != null) ? other.acceptClientResult_.Clone() : null);
		refreshAfterSeconds_ = other.refreshAfterSeconds_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetServerInfoResponse Clone()
	{
		return new GetServerInfoResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetServerInfoResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetServerInfoResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(AcceptClientResult, other.AcceptClientResult))
		{
			return false;
		}
		if (RefreshAfterSeconds != other.RefreshAfterSeconds)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (acceptClientResult_ != null)
		{
			num ^= AcceptClientResult.GetHashCode();
		}
		if (RefreshAfterSeconds != 0)
		{
			num ^= RefreshAfterSeconds.GetHashCode();
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
		if (acceptClientResult_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(AcceptClientResult);
		}
		if (RefreshAfterSeconds != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(RefreshAfterSeconds);
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
		if (acceptClientResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(AcceptClientResult);
		}
		if (RefreshAfterSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshAfterSeconds);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetServerInfoResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.acceptClientResult_ != null)
		{
			if (acceptClientResult_ == null)
			{
				AcceptClientResult = new AcceptClientResult();
			}
			AcceptClientResult.MergeFrom(other.AcceptClientResult);
		}
		if (other.RefreshAfterSeconds != 0)
		{
			RefreshAfterSeconds = other.RefreshAfterSeconds;
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
				if (acceptClientResult_ == null)
				{
					AcceptClientResult = new AcceptClientResult();
				}
				input.ReadMessage(AcceptClientResult);
				break;
			case 16u:
				RefreshAfterSeconds = input.ReadInt32();
				break;
			}
		}
	}
}
