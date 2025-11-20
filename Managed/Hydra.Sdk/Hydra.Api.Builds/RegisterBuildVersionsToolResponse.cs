using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Builds.Common;

namespace Hydra.Api.Builds;

public sealed class RegisterBuildVersionsToolResponse : IMessage<RegisterBuildVersionsToolResponse>, IMessage, IEquatable<RegisterBuildVersionsToolResponse>, IDeepCloneable<RegisterBuildVersionsToolResponse>, IBufferMessage
{
	private static readonly MessageParser<RegisterBuildVersionsToolResponse> _parser = new MessageParser<RegisterBuildVersionsToolResponse>(() => new RegisterBuildVersionsToolResponse());

	private UnknownFieldSet _unknownFields;

	public const int FailedVersionsFieldNumber = 1;

	private static readonly FieldCodec<string> _repeated_failedVersions_codec = FieldCodec.ForString(10u);

	private readonly RepeatedField<string> failedVersions_ = new RepeatedField<string>();

	public const int RegisteredBuildVersionsFieldNumber = 2;

	private static readonly FieldCodec<BuildVersionWithIdDto> _repeated_registeredBuildVersions_codec = FieldCodec.ForMessage(18u, BuildVersionWithIdDto.Parser);

	private readonly RepeatedField<BuildVersionWithIdDto> registeredBuildVersions_ = new RepeatedField<BuildVersionWithIdDto>();

	[DebuggerNonUserCode]
	public static MessageParser<RegisterBuildVersionsToolResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildsGroupContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<string> FailedVersions => failedVersions_;

	[DebuggerNonUserCode]
	public RepeatedField<BuildVersionWithIdDto> RegisteredBuildVersions => registeredBuildVersions_;

	[DebuggerNonUserCode]
	public RegisterBuildVersionsToolResponse()
	{
	}

	[DebuggerNonUserCode]
	public RegisterBuildVersionsToolResponse(RegisterBuildVersionsToolResponse other)
		: this()
	{
		failedVersions_ = other.failedVersions_.Clone();
		registeredBuildVersions_ = other.registeredBuildVersions_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RegisterBuildVersionsToolResponse Clone()
	{
		return new RegisterBuildVersionsToolResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RegisterBuildVersionsToolResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(RegisterBuildVersionsToolResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!failedVersions_.Equals(other.failedVersions_))
		{
			return false;
		}
		if (!registeredBuildVersions_.Equals(other.registeredBuildVersions_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= failedVersions_.GetHashCode();
		num ^= registeredBuildVersions_.GetHashCode();
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
		failedVersions_.WriteTo(ref output, _repeated_failedVersions_codec);
		registeredBuildVersions_.WriteTo(ref output, _repeated_registeredBuildVersions_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += failedVersions_.CalculateSize(_repeated_failedVersions_codec);
		num += registeredBuildVersions_.CalculateSize(_repeated_registeredBuildVersions_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RegisterBuildVersionsToolResponse other)
	{
		if (other != null)
		{
			failedVersions_.Add(other.failedVersions_);
			registeredBuildVersions_.Add(other.registeredBuildVersions_);
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
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				failedVersions_.AddEntriesFrom(ref input, _repeated_failedVersions_codec);
				break;
			case 18u:
				registeredBuildVersions_.AddEntriesFrom(ref input, _repeated_registeredBuildVersions_codec);
				break;
			}
		}
	}
}
