using System;
using System.Collections.Generic;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Patch;

public class JsonPatchAction : IJsonSerializable
{
	public JsonPatchOperation Operation { get; set; }

	public string Path { get; set; }

	public string? From { get; set; }

	public JsonValue? Value { get; set; }

	internal JsonPatchResult TryApply(JsonValue json)
	{
		return Operation switch
		{
			JsonPatchOperation.Add => _Add(json), 
			JsonPatchOperation.Remove => _Remove(json), 
			JsonPatchOperation.Replace => _Replace(json), 
			JsonPatchOperation.Move => _Move(json), 
			JsonPatchOperation.Copy => _Copy(json), 
			JsonPatchOperation.Test => _Test(json), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	void IJsonSerializable.FromJson(JsonValue json, JsonSerializer serializer)
	{
		JsonObject jsonObject = json.Object;
		Operation = serializer.Deserialize<JsonPatchOperation>(jsonObject["op"]);
		Path = jsonObject.TryGetString("path");
		From = jsonObject.TryGetString("from");
		jsonObject.TryGetValue("value", out JsonValue value);
		Value = value;
		_Validate();
	}

	JsonValue IJsonSerializable.ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject
		{
			["op"] = serializer.Serialize(Operation),
			["path"] = Path
		};
		if (From != null)
		{
			jsonObject["from"] = From;
		}
		if (Value != null)
		{
			jsonObject["value"] = Value;
		}
		return jsonObject;
	}

	private JsonPatchResult _Add(JsonValue json)
	{
		(JsonValue? result, bool success) tuple = JsonPointerFunctions.InsertValue(json, Path, Value, insertAfter: false);
		var (patched, _) = tuple;
		if (!tuple.success)
		{
			return new JsonPatchResult(json, "Could not add the value");
		}
		return new JsonPatchResult(patched);
	}

	private JsonPatchResult _Remove(JsonValue json)
	{
		return _RemoveAtPath(json, Path);
	}

	private JsonPatchResult _Replace(JsonValue json)
	{
		JsonPatchResult jsonPatchResult = _Remove(json);
		if (!jsonPatchResult.Success)
		{
			return jsonPatchResult;
		}
		return _Add(json);
	}

	private JsonPatchResult _Move(JsonValue json)
	{
		if (JsonPointer.Parse(From).Equals(JsonPointer.Parse(Path)))
		{
			return new JsonPatchResult(json);
		}
		JsonPatchResult jsonPatchResult = _Copy(json);
		if (jsonPatchResult.Success)
		{
			return _RemoveAtPath(json, From);
		}
		return jsonPatchResult;
	}

	private JsonPatchResult _Copy(JsonValue json)
	{
		PointerEvaluationResults pointerEvaluationResults = JsonPointer.Parse(From).Evaluate(json);
		if (pointerEvaluationResults.Error != null)
		{
			return new JsonPatchResult(json, pointerEvaluationResults.Error);
		}
		(JsonValue? result, bool success) tuple = JsonPointerFunctions.InsertValue(json, Path, pointerEvaluationResults.Result, insertAfter: true);
		var (patched, _) = tuple;
		if (!tuple.success)
		{
			return new JsonPatchResult(json, "Could not add the value");
		}
		return new JsonPatchResult(patched);
	}

	private JsonPatchResult _Test(JsonValue json)
	{
		PointerEvaluationResults pointerEvaluationResults = JsonPointer.Parse(Path).Evaluate(json);
		if (pointerEvaluationResults.Error != null)
		{
			return new JsonPatchResult(json, pointerEvaluationResults.Error);
		}
		if (pointerEvaluationResults.Result != Value)
		{
			return new JsonPatchResult(json, "The value at '" + Path + "' is not the expected value.");
		}
		return new JsonPatchResult(json);
	}

	private JsonPatchResult _RemoveAtPath(JsonValue json, string path)
	{
		if (string.IsNullOrEmpty(Path))
		{
			json.Object.Clear();
			return new JsonPatchResult(json);
		}
		(JsonValue? parent, string? key, int index, bool success) tuple = JsonPointerFunctions.ResolvePointer(json, path);
		var (jsonValue, key, index, _) = tuple;
		if (!tuple.success)
		{
			return new JsonPatchResult(json, "Path '" + path + "' not found.");
		}
		switch (jsonValue.Type)
		{
		case JsonValueType.Object:
			jsonValue.Object.Remove(key);
			break;
		case JsonValueType.Array:
			jsonValue.Array.RemoveAt(index);
			break;
		default:
			return new JsonPatchResult(json, $"Cannot remove a value from a '{jsonValue.Type}'");
		}
		return new JsonPatchResult(json);
	}

	private void _Validate()
	{
		List<string> errors = new List<string>();
		switch (Operation)
		{
		case JsonPatchOperation.Add:
		case JsonPatchOperation.Replace:
		case JsonPatchOperation.Test:
			_CheckProperty(Path, Operation, "path", errors);
			_CheckProperty(Value, Operation, "value", errors);
			break;
		case JsonPatchOperation.Remove:
			_CheckProperty(Path, Operation, "path", errors);
			break;
		case JsonPatchOperation.Move:
		case JsonPatchOperation.Copy:
			_CheckProperty(Path, Operation, "path", errors);
			_CheckProperty(From, Operation, "from", errors);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void _CheckProperty(object obj, JsonPatchOperation operation, string propertyName, List<string> errors)
	{
		if (obj == null)
		{
			errors.Add($"Operation '{operation}' requires a {propertyName}.");
		}
	}
}
