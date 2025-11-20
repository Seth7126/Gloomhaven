using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.AbstractData;

public sealed class GetDataServerRequest : IMessage<GetDataServerRequest>, IMessage, IEquatable<GetDataServerRequest>, IDeepCloneable<GetDataServerRequest>, IBufferMessage
{
	private static readonly MessageParser<GetDataServerRequest> _parser = new MessageParser<GetDataServerRequest>(() => new GetDataServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ServerContext context_;

	public const int KeyContainerNamesFieldNumber = 2;

	private static readonly FieldCodec<AbstractDataKeyContainerNames> _repeated_keyContainerNames_codec = FieldCodec.ForMessage(18u, AbstractDataKeyContainerNames.Parser);

	private readonly RepeatedField<AbstractDataKeyContainerNames> keyContainerNames_ = new RepeatedField<AbstractDataKeyContainerNames>();

	[DebuggerNonUserCode]
	public static MessageParser<GetDataServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerContext Context
	{
		get
		{
			return context_;
		}
		set
		{
			context_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<AbstractDataKeyContainerNames> KeyContainerNames => keyContainerNames_;

	[DebuggerNonUserCode]
	public GetDataServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetDataServerRequest(GetDataServerRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		keyContainerNames_ = other.keyContainerNames_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetDataServerRequest Clone()
	{
		return new GetDataServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetDataServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetDataServerRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Context, other.Context))
		{
			return false;
		}
		if (!keyContainerNames_.Equals(other.keyContainerNames_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (context_ != null)
		{
			num ^= Context.GetHashCode();
		}
		num ^= keyContainerNames_.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		keyContainerNames_.WriteTo(ref output, _repeated_keyContainerNames_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		num += keyContainerNames_.CalculateSize(_repeated_keyContainerNames_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetDataServerRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new ServerContext();
			}
			Context.MergeFrom(other.Context);
		}
		keyContainerNames_.Add(other.keyContainerNames_);
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
				if (context_ == null)
				{
					Context = new ServerContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				keyContainerNames_.AddEntriesFrom(ref input, _repeated_keyContainerNames_codec);
				break;
			}
		}
	}
}
