using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Presence;

public sealed class MatchmakeSessionCreateRequest : IMessage<MatchmakeSessionCreateRequest>, IMessage, IEquatable<MatchmakeSessionCreateRequest>, IDeepCloneable<MatchmakeSessionCreateRequest>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeSessionCreateRequest> _parser = new MessageParser<MatchmakeSessionCreateRequest>(() => new MatchmakeSessionCreateRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int OptionsFieldNumber = 2;

	private MatchmakeCreateOptions options_;

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeSessionCreateRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[42];

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
	public MatchmakeCreateOptions Options
	{
		get
		{
			return options_;
		}
		set
		{
			options_ = value;
		}
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionCreateRequest()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionCreateRequest(MatchmakeSessionCreateRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		options_ = ((other.options_ != null) ? other.options_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionCreateRequest Clone()
	{
		return new MatchmakeSessionCreateRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeSessionCreateRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeSessionCreateRequest other)
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
		if (!object.Equals(Options, other.Options))
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
		if (options_ != null)
		{
			num ^= Options.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		if (options_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Options);
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
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		if (options_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Options);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeSessionCreateRequest other)
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
		if (other.options_ != null)
		{
			if (options_ == null)
			{
				Options = new MatchmakeCreateOptions();
			}
			Options.MergeFrom(other.Options);
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
				if (context_ == null)
				{
					Context = new UserContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				if (options_ == null)
				{
					Options = new MatchmakeCreateOptions();
				}
				input.ReadMessage(Options);
				break;
			}
		}
	}
}
