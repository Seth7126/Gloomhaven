using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class RefreshUserResponse : IMessage<RefreshUserResponse>, IMessage, IEquatable<RefreshUserResponse>, IDeepCloneable<RefreshUserResponse>, IBufferMessage
{
	private static readonly MessageParser<RefreshUserResponse> _parser = new MessageParser<RefreshUserResponse>(() => new RefreshUserResponse());

	private UnknownFieldSet _unknownFields;

	public const int RequestContextFieldNumber = 1;

	private HydraRequestContext requestContext_;

	public const int RefreshAfterSecondsFieldNumber = 2;

	private int refreshAfterSeconds_;

	[DebuggerNonUserCode]
	public static MessageParser<RefreshUserResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[30];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public HydraRequestContext RequestContext
	{
		get
		{
			return requestContext_;
		}
		set
		{
			requestContext_ = value;
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
	public RefreshUserResponse()
	{
	}

	[DebuggerNonUserCode]
	public RefreshUserResponse(RefreshUserResponse other)
		: this()
	{
		requestContext_ = ((other.requestContext_ != null) ? other.requestContext_.Clone() : null);
		refreshAfterSeconds_ = other.refreshAfterSeconds_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RefreshUserResponse Clone()
	{
		return new RefreshUserResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RefreshUserResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(RefreshUserResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RequestContext, other.RequestContext))
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
		if (requestContext_ != null)
		{
			num ^= RequestContext.GetHashCode();
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
		if (requestContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(RequestContext);
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
		if (requestContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(RequestContext);
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
	public void MergeFrom(RefreshUserResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.requestContext_ != null)
		{
			if (requestContext_ == null)
			{
				RequestContext = new HydraRequestContext();
			}
			RequestContext.MergeFrom(other.RequestContext);
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
				if (requestContext_ == null)
				{
					RequestContext = new HydraRequestContext();
				}
				input.ReadMessage(RequestContext);
				break;
			case 16u:
				RefreshAfterSeconds = input.ReadInt32();
				break;
			}
		}
	}
}
