using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class GetTournamentsSchemaAndSampleJsonResponse : IMessage<GetTournamentsSchemaAndSampleJsonResponse>, IMessage, IEquatable<GetTournamentsSchemaAndSampleJsonResponse>, IDeepCloneable<GetTournamentsSchemaAndSampleJsonResponse>, IBufferMessage
{
	private static readonly MessageParser<GetTournamentsSchemaAndSampleJsonResponse> _parser = new MessageParser<GetTournamentsSchemaAndSampleJsonResponse>(() => new GetTournamentsSchemaAndSampleJsonResponse());

	private UnknownFieldSet _unknownFields;

	public const int JsonSchemaFieldNumber = 1;

	private string jsonSchema_ = "";

	public const int SampleJsonFieldNumber = 2;

	private string sampleJson_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetTournamentsSchemaAndSampleJsonResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[44];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string JsonSchema
	{
		get
		{
			return jsonSchema_;
		}
		set
		{
			jsonSchema_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string SampleJson
	{
		get
		{
			return sampleJson_;
		}
		set
		{
			sampleJson_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetTournamentsSchemaAndSampleJsonResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetTournamentsSchemaAndSampleJsonResponse(GetTournamentsSchemaAndSampleJsonResponse other)
		: this()
	{
		jsonSchema_ = other.jsonSchema_;
		sampleJson_ = other.sampleJson_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetTournamentsSchemaAndSampleJsonResponse Clone()
	{
		return new GetTournamentsSchemaAndSampleJsonResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetTournamentsSchemaAndSampleJsonResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetTournamentsSchemaAndSampleJsonResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (JsonSchema != other.JsonSchema)
		{
			return false;
		}
		if (SampleJson != other.SampleJson)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (JsonSchema.Length != 0)
		{
			num ^= JsonSchema.GetHashCode();
		}
		if (SampleJson.Length != 0)
		{
			num ^= SampleJson.GetHashCode();
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
		if (JsonSchema.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(JsonSchema);
		}
		if (SampleJson.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(SampleJson);
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
		if (JsonSchema.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(JsonSchema);
		}
		if (SampleJson.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SampleJson);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetTournamentsSchemaAndSampleJsonResponse other)
	{
		if (other != null)
		{
			if (other.JsonSchema.Length != 0)
			{
				JsonSchema = other.JsonSchema;
			}
			if (other.SampleJson.Length != 0)
			{
				SampleJson = other.SampleJson;
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
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				JsonSchema = input.ReadString();
				break;
			case 18u:
				SampleJson = input.ReadString();
				break;
			}
		}
	}
}
