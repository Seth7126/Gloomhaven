using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.AbstractData;

public sealed class GetDataRequest : IMessage<GetDataRequest>, IMessage, IEquatable<GetDataRequest>, IDeepCloneable<GetDataRequest>, IBufferMessage
{
	private static readonly MessageParser<GetDataRequest> _parser = new MessageParser<GetDataRequest>(() => new GetDataRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int KeyContainerNamesFieldNumber = 2;

	private static readonly FieldCodec<AbstractDataKeyContainerNames> _repeated_keyContainerNames_codec = FieldCodec.ForMessage(18u, AbstractDataKeyContainerNames.Parser);

	private readonly RepeatedField<AbstractDataKeyContainerNames> keyContainerNames_ = new RepeatedField<AbstractDataKeyContainerNames>();

	[DebuggerNonUserCode]
	public static MessageParser<GetDataRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext Context
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
	public GetDataRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetDataRequest(GetDataRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		keyContainerNames_ = other.keyContainerNames_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetDataRequest Clone()
	{
		return new GetDataRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetDataRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetDataRequest other)
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
	public void MergeFrom(GetDataRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new UserContext();
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
					Context = new UserContext();
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
