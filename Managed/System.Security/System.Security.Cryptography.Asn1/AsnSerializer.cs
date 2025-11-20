using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace System.Security.Cryptography.Asn1;

internal static class AsnSerializer
{
	private delegate void Serializer(object value, AsnWriter writer);

	private delegate object Deserializer(AsnReader reader);

	private delegate bool TryDeserializer<T>(AsnReader reader, out T value);

	private struct SerializerFieldData
	{
		internal bool WasCustomized;

		internal UniversalTagNumber? TagType;

		internal bool? PopulateOidFriendlyName;

		internal bool IsAny;

		internal bool IsCollection;

		internal byte[] DefaultContents;

		internal bool HasExplicitTag;

		internal bool SpecifiedTag;

		internal bool IsOptional;

		internal int? TwoDigitYearMax;

		internal Asn1Tag ExpectedTag;

		internal bool? DisallowGeneralizedTimeFractions;
	}

	private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly ConcurrentDictionary<Type, FieldInfo[]> s_orderedFields = new ConcurrentDictionary<Type, FieldInfo[]>();

	private static Deserializer TryOrFail<T>(TryDeserializer<T> tryDeserializer)
	{
		return delegate(AsnReader reader)
		{
			if (tryDeserializer(reader, out var value))
			{
				return value;
			}
			throw new CryptographicException("ASN1 corrupted data.");
		};
	}

	private static FieldInfo[] GetOrderedFields(Type typeT)
	{
		return s_orderedFields.GetOrAdd(typeT, delegate(Type t)
		{
			FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fields.Length == 0)
			{
				return Array.Empty<FieldInfo>();
			}
			try
			{
				_ = fields[0].MetadataToken;
			}
			catch (InvalidOperationException)
			{
				return fields;
			}
			Array.Sort(fields, (FieldInfo x, FieldInfo y) => x.MetadataToken.CompareTo(y.MetadataToken));
			return fields;
		});
	}

	private static ChoiceAttribute GetChoiceAttribute(Type typeT)
	{
		ChoiceAttribute customAttribute = typeT.GetCustomAttribute<ChoiceAttribute>(inherit: false);
		if (customAttribute == null)
		{
			return null;
		}
		if (customAttribute.AllowNull && !CanBeNull(typeT))
		{
			throw new AsnSerializationConstraintException(global::SR.Format("[Choice].AllowNull=true is not valid because type '{0}' cannot have a null value.", typeT.FullName));
		}
		return customAttribute;
	}

	private static bool CanBeNull(Type t)
	{
		if (t.IsValueType)
		{
			if (t.IsGenericType)
			{
				return t.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return false;
		}
		return true;
	}

	private static void PopulateChoiceLookup(Dictionary<(TagClass, int), LinkedList<FieldInfo>> lookup, Type typeT, LinkedList<FieldInfo> currentSet)
	{
		FieldInfo[] orderedFields = GetOrderedFields(typeT);
		foreach (FieldInfo fieldInfo in orderedFields)
		{
			Type fieldType = fieldInfo.FieldType;
			if (!CanBeNull(fieldType))
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on [Choice] type '{1}' can not be assigned a null value.", fieldInfo.Name, fieldInfo.DeclaringType.FullName));
			}
			fieldType = UnpackIfNullable(fieldType);
			if (currentSet.Contains(fieldInfo))
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on [Choice] type '{1}' has introduced a type chain cycle.", fieldInfo.Name, fieldInfo.DeclaringType.FullName));
			}
			LinkedListNode<FieldInfo> node = new LinkedListNode<FieldInfo>(fieldInfo);
			currentSet.AddLast(node);
			if (GetChoiceAttribute(fieldType) != null)
			{
				PopulateChoiceLookup(lookup, fieldType, currentSet);
			}
			else
			{
				GetFieldInfo(fieldType, fieldInfo, out var serializerFieldData);
				if (serializerFieldData.DefaultContents != null)
				{
					throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on [Choice] type '{1}' has a default value, which is not permitted.", fieldInfo.Name, fieldInfo.DeclaringType.FullName));
				}
				(TagClass, int) key = (serializerFieldData.ExpectedTag.TagClass, serializerFieldData.ExpectedTag.TagValue);
				if (lookup.TryGetValue(key, out var value))
				{
					FieldInfo value2 = value.Last.Value;
					throw new AsnSerializationConstraintException(global::SR.Format("The tag ({0} {1}) for field '{2}' on type '{3}' already is associated in this context with field '{4}' on type '{5}'.", serializerFieldData.ExpectedTag.TagClass, serializerFieldData.ExpectedTag.TagValue, fieldInfo.Name, fieldInfo.DeclaringType.FullName, value2.Name, value2.DeclaringType.FullName));
				}
				lookup.Add(key, new LinkedList<FieldInfo>(currentSet));
			}
			currentSet.RemoveLast();
		}
	}

	private static void SerializeChoice(Type typeT, object value, AsnWriter writer)
	{
		Dictionary<(TagClass, int), LinkedList<FieldInfo>> lookup = new Dictionary<(TagClass, int), LinkedList<FieldInfo>>();
		LinkedList<FieldInfo> currentSet = new LinkedList<FieldInfo>();
		PopulateChoiceLookup(lookup, typeT, currentSet);
		FieldInfo fieldInfo = null;
		object value2 = null;
		if (value == null)
		{
			if (GetChoiceAttribute(typeT).AllowNull)
			{
				writer.WriteNull();
				return;
			}
		}
		else
		{
			FieldInfo[] orderedFields = GetOrderedFields(typeT);
			foreach (FieldInfo fieldInfo2 in orderedFields)
			{
				object value3 = fieldInfo2.GetValue(value);
				if (value3 != null)
				{
					if (fieldInfo != null)
					{
						throw new AsnSerializationConstraintException(global::SR.Format("Fields '{0}' and '{1}' on type '{2}' are both non-null when only one value is permitted.", fieldInfo2.Name, fieldInfo.Name, typeT.FullName));
					}
					fieldInfo = fieldInfo2;
					value2 = value3;
				}
			}
		}
		if (fieldInfo == null)
		{
			throw new AsnSerializationConstraintException(global::SR.Format("An instance of [Choice] type '{0}' has no non-null fields.", typeT.FullName));
		}
		GetSerializer(fieldInfo.FieldType, fieldInfo)(value2, writer);
	}

	private static object DeserializeChoice(AsnReader reader, Type typeT)
	{
		Dictionary<(TagClass, int), LinkedList<FieldInfo>> dictionary = new Dictionary<(TagClass, int), LinkedList<FieldInfo>>();
		LinkedList<FieldInfo> currentSet = new LinkedList<FieldInfo>();
		PopulateChoiceLookup(dictionary, typeT, currentSet);
		Asn1Tag asn1Tag = reader.PeekTag();
		if (asn1Tag == Asn1Tag.Null)
		{
			if (GetChoiceAttribute(typeT).AllowNull)
			{
				reader.ReadNull();
				return null;
			}
			throw new CryptographicException("ASN1 corrupted data.");
		}
		(TagClass, int) key = (asn1Tag.TagClass, asn1Tag.TagValue);
		if (dictionary.TryGetValue(key, out var value))
		{
			LinkedListNode<FieldInfo> linkedListNode = value.Last;
			FieldInfo value2 = linkedListNode.Value;
			object obj = Activator.CreateInstance(value2.DeclaringType);
			object value3 = GetDeserializer(value2.FieldType, value2)(reader);
			value2.SetValue(obj, value3);
			while (linkedListNode.Previous != null)
			{
				linkedListNode = linkedListNode.Previous;
				value2 = linkedListNode.Value;
				object obj2 = Activator.CreateInstance(value2.DeclaringType);
				value2.SetValue(obj2, obj);
				obj = obj2;
			}
			return obj;
		}
		throw new CryptographicException("ASN1 corrupted data.");
	}

	private static void SerializeCustomType(Type typeT, object value, AsnWriter writer, Asn1Tag tag)
	{
		writer.PushSequence(tag);
		FieldInfo[] fields = typeT.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			GetSerializer(fieldInfo.FieldType, fieldInfo)(fieldInfo.GetValue(value), writer);
		}
		writer.PopSequence(tag);
	}

	private static object DeserializeCustomType(AsnReader reader, Type typeT, Asn1Tag expectedTag)
	{
		object obj = Activator.CreateInstance(typeT);
		AsnReader asnReader = reader.ReadSequence(expectedTag);
		FieldInfo[] fields = typeT.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			Deserializer deserializer = GetDeserializer(fieldInfo.FieldType, fieldInfo);
			try
			{
				fieldInfo.SetValue(obj, deserializer(asnReader));
			}
			catch (Exception inner)
			{
				throw new CryptographicException(global::SR.Format("Unable to set field {0} on type {1}.", fieldInfo.Name, fieldInfo.DeclaringType.FullName), inner);
			}
		}
		asnReader.ThrowIfNotEmpty();
		return obj;
	}

	private static Deserializer ExplicitValueDeserializer(Deserializer valueDeserializer, Asn1Tag expectedTag)
	{
		return (AsnReader reader) => ExplicitValueDeserializer(reader, valueDeserializer, expectedTag);
	}

	private static object ExplicitValueDeserializer(AsnReader reader, Deserializer valueDeserializer, Asn1Tag expectedTag)
	{
		AsnReader asnReader = reader.ReadSequence(expectedTag);
		object result = valueDeserializer(asnReader);
		asnReader.ThrowIfNotEmpty();
		return result;
	}

	private static Deserializer DefaultValueDeserializer(Deserializer valueDeserializer, bool isOptional, byte[] defaultContents, Asn1Tag? expectedTag)
	{
		return (AsnReader reader) => DefaultValueDeserializer(reader, expectedTag, valueDeserializer, defaultContents, isOptional);
	}

	private static object DefaultValueDeserializer(AsnReader reader, Asn1Tag? expectedTag, Deserializer valueDeserializer, byte[] defaultContents, bool isOptional)
	{
		if (reader.HasData)
		{
			Asn1Tag asn1Tag = reader.PeekTag();
			if (!expectedTag.HasValue || asn1Tag.AsPrimitive() == expectedTag.Value.AsPrimitive())
			{
				return valueDeserializer(reader);
			}
		}
		if (isOptional)
		{
			return null;
		}
		if (defaultContents != null)
		{
			return DefaultValue(defaultContents, valueDeserializer);
		}
		throw new CryptographicException("ASN1 corrupted data.");
	}

	private static Serializer GetSerializer(Type typeT, FieldInfo fieldInfo)
	{
		byte[] defaultContents;
		bool isOptional;
		Asn1Tag? explicitTag;
		Serializer literalValueSerializer = GetSimpleSerializer(typeT, fieldInfo, out defaultContents, out isOptional, out explicitTag);
		Serializer serializer = literalValueSerializer;
		if (isOptional)
		{
			serializer = delegate(object obj, AsnWriter writer)
			{
				if (obj != null)
				{
					literalValueSerializer(obj, writer);
				}
			};
		}
		else if (defaultContents != null)
		{
			serializer = delegate(object obj, AsnWriter writer)
			{
				AsnReader asnReader;
				using (AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER))
				{
					literalValueSerializer(obj, asnWriter);
					asnReader = new AsnReader(asnWriter.Encode(), AsnEncodingRules.DER);
				}
				ReadOnlySpan<byte> span = asnReader.GetEncodedValue().Span;
				bool flag = false;
				if (span.Length == defaultContents.Length)
				{
					flag = true;
					for (int i = 0; i < span.Length; i++)
					{
						if (span[i] != defaultContents[i])
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					literalValueSerializer(obj, writer);
				}
			};
		}
		if (explicitTag.HasValue)
		{
			return delegate(object obj, AsnWriter writer)
			{
				using AsnWriter asnWriter = new AsnWriter(writer.RuleSet);
				serializer(obj, asnWriter);
				if (asnWriter.Encode().Length != 0)
				{
					writer.PushSequence(explicitTag.Value);
					serializer(obj, writer);
					writer.PopSequence(explicitTag.Value);
				}
			};
		}
		return serializer;
	}

	private static Serializer GetSimpleSerializer(Type typeT, FieldInfo fieldInfo, out byte[] defaultContents, out bool isOptional, out Asn1Tag? explicitTag)
	{
		if (!typeT.IsSealed || typeT.ContainsGenericParameters)
		{
			throw new AsnSerializationConstraintException(global::SR.Format("Type '{0}' cannot be serialized or deserialized because it is not sealed or has unbound generic parameters.", typeT.FullName));
		}
		GetFieldInfo(typeT, fieldInfo, out var fieldData);
		defaultContents = fieldData.DefaultContents;
		isOptional = fieldData.IsOptional;
		typeT = UnpackIfNullable(typeT);
		bool flag = GetChoiceAttribute(typeT) != null;
		Asn1Tag tag;
		if (fieldData.HasExplicitTag)
		{
			explicitTag = fieldData.ExpectedTag;
			tag = new Asn1Tag(fieldData.TagType.GetValueOrDefault());
		}
		else
		{
			explicitTag = null;
			tag = fieldData.ExpectedTag;
		}
		if (typeT.IsPrimitive)
		{
			return GetPrimitiveSerializer(typeT, tag);
		}
		if (typeT.IsEnum)
		{
			if (typeT.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length != 0)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteNamedBitList(tag, value);
				};
			}
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteEnumeratedValue(tag, value);
			};
		}
		if (typeT == typeof(string))
		{
			if (fieldData.TagType == UniversalTagNumber.ObjectIdentifier)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteObjectIdentifier(tag, (string)value);
				};
			}
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteCharacterString(tag, fieldData.TagType.Value, (string)value);
			};
		}
		if (typeT == typeof(ReadOnlyMemory<byte>) && !fieldData.IsCollection)
		{
			if (fieldData.IsAny)
			{
				if (fieldData.SpecifiedTag && !fieldData.HasExplicitTag)
				{
					return delegate(object value, AsnWriter writer)
					{
						ReadOnlyMemory<byte> preEncodedValue = (ReadOnlyMemory<byte>)value;
						if (!Asn1Tag.TryParse(preEncodedValue.Span, out var tag2, out var _) || tag2.AsPrimitive() != fieldData.ExpectedTag.AsPrimitive())
						{
							throw new CryptographicException("ASN1 corrupted data.");
						}
						writer.WriteEncodedValue(preEncodedValue);
					};
				}
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteEncodedValue((ReadOnlyMemory<byte>)value);
				};
			}
			if (fieldData.TagType == UniversalTagNumber.BitString)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteBitString(tag, ((ReadOnlyMemory<byte>)value).Span);
				};
			}
			if (fieldData.TagType == UniversalTagNumber.OctetString)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteOctetString(tag, ((ReadOnlyMemory<byte>)value).Span);
				};
			}
			if (fieldData.TagType == UniversalTagNumber.Integer)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteInteger(tag, ((ReadOnlyMemory<byte>)value).Span);
				};
			}
			throw new CryptographicException();
		}
		if (typeT == typeof(Oid))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteObjectIdentifier(fieldData.ExpectedTag, (Oid)value);
			};
		}
		if (typeT.IsArray)
		{
			if (typeT.GetArrayRank() != 1)
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Type '{0}' cannot be serialized or deserialized because it is a multi-dimensional array.", typeT.FullName));
			}
			Type elementType = typeT.GetElementType();
			if (elementType.IsArray)
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Type '{0}' cannot be serialized or deserialized because it is an array of arrays.", typeT.FullName));
			}
			Serializer serializer = GetSerializer(elementType, null);
			if (fieldData.TagType == UniversalTagNumber.Set)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.PushSetOf(tag);
					foreach (object item in (Array)value)
					{
						serializer(item, writer);
					}
					writer.PopSetOf(tag);
				};
			}
			return delegate(object value, AsnWriter writer)
			{
				writer.PushSequence(tag);
				foreach (object item2 in (Array)value)
				{
					serializer(item2, writer);
				}
				writer.PopSequence(tag);
			};
		}
		if (typeT == typeof(DateTimeOffset))
		{
			if (fieldData.TagType == UniversalTagNumber.UtcTime)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteUtcTime(tag, (DateTimeOffset)value);
				};
			}
			if (fieldData.TagType == UniversalTagNumber.GeneralizedTime)
			{
				return delegate(object value, AsnWriter writer)
				{
					writer.WriteGeneralizedTime(tag, (DateTimeOffset)value, fieldData.DisallowGeneralizedTimeFractions.Value);
				};
			}
			throw new CryptographicException();
		}
		if (typeT == typeof(BigInteger))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(tag, (BigInteger)value);
			};
		}
		if (typeT.IsLayoutSequential)
		{
			if (flag)
			{
				return delegate(object value, AsnWriter writer)
				{
					SerializeChoice(typeT, value, writer);
				};
			}
			if (fieldData.TagType == UniversalTagNumber.Sequence)
			{
				return delegate(object value, AsnWriter writer)
				{
					SerializeCustomType(typeT, value, writer, tag);
				};
			}
		}
		throw new AsnSerializationConstraintException(global::SR.Format("Could not determine how to serialize or deserialize type '{0}'.", typeT.FullName));
	}

	private static Deserializer GetDeserializer(Type typeT, FieldInfo fieldInfo)
	{
		SerializerFieldData fieldData;
		Deserializer deserializer = GetSimpleDeserializer(typeT, fieldInfo, out fieldData);
		if (fieldData.HasExplicitTag)
		{
			deserializer = ExplicitValueDeserializer(deserializer, fieldData.ExpectedTag);
		}
		if (fieldData.IsOptional || fieldData.DefaultContents != null)
		{
			Asn1Tag? expectedTag = null;
			if (fieldData.SpecifiedTag || fieldData.TagType.HasValue)
			{
				expectedTag = fieldData.ExpectedTag;
			}
			deserializer = DefaultValueDeserializer(deserializer, fieldData.IsOptional, fieldData.DefaultContents, expectedTag);
		}
		return deserializer;
	}

	private static Deserializer GetSimpleDeserializer(Type typeT, FieldInfo fieldInfo, out SerializerFieldData fieldData)
	{
		if (!typeT.IsSealed || typeT.ContainsGenericParameters)
		{
			throw new AsnSerializationConstraintException(global::SR.Format("Type '{0}' cannot be serialized or deserialized because it is not sealed or has unbound generic parameters.", typeT.FullName));
		}
		GetFieldInfo(typeT, fieldInfo, out fieldData);
		SerializerFieldData localFieldData = fieldData;
		typeT = UnpackIfNullable(typeT);
		if (fieldData.IsAny)
		{
			if (typeT == typeof(ReadOnlyMemory<byte>))
			{
				Asn1Tag matchTag = fieldData.ExpectedTag;
				if (fieldData.HasExplicitTag || !fieldData.SpecifiedTag)
				{
					return (AsnReader reader) => reader.GetEncodedValue();
				}
				return delegate(AsnReader reader)
				{
					Asn1Tag asn1Tag = reader.PeekTag();
					if (matchTag.TagClass != asn1Tag.TagClass || matchTag.TagValue != asn1Tag.TagValue)
					{
						throw new CryptographicException("ASN1 corrupted data.");
					}
					return reader.GetEncodedValue();
				};
			}
			throw new AsnSerializationConstraintException(global::SR.Format("Could not determine how to serialize or deserialize type '{0}'.", typeT.FullName));
		}
		if (GetChoiceAttribute(typeT) != null)
		{
			return (AsnReader reader) => DeserializeChoice(reader, typeT);
		}
		Asn1Tag expectedTag = (fieldData.HasExplicitTag ? new Asn1Tag(fieldData.TagType.Value) : fieldData.ExpectedTag);
		if (typeT.IsPrimitive)
		{
			return GetPrimitiveDeserializer(typeT, expectedTag);
		}
		if (typeT.IsEnum)
		{
			if (typeT.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length != 0)
			{
				return (AsnReader reader) => reader.GetNamedBitListValue(expectedTag, typeT);
			}
			return (AsnReader reader) => reader.GetEnumeratedValue(expectedTag, typeT);
		}
		if (typeT == typeof(string))
		{
			if (fieldData.TagType == UniversalTagNumber.ObjectIdentifier)
			{
				return (AsnReader reader) => reader.ReadObjectIdentifierAsString(expectedTag);
			}
			return (AsnReader reader) => reader.GetCharacterString(expectedTag, localFieldData.TagType.Value);
		}
		if (typeT == typeof(ReadOnlyMemory<byte>) && !fieldData.IsCollection)
		{
			if (fieldData.TagType == UniversalTagNumber.BitString)
			{
				return delegate(AsnReader reader)
				{
					if (reader.TryGetPrimitiveBitStringValue(expectedTag, out var unusedBitCount, out var value))
					{
						return value;
					}
					int length = reader.PeekEncodedValue().Length;
					byte[] array = ArrayPool<byte>.Shared.Rent(length);
					try
					{
						if (!reader.TryCopyBitStringBytes(expectedTag, array, out unusedBitCount, out var bytesWritten))
						{
							throw new CryptographicException();
						}
						return new ReadOnlyMemory<byte>(array.AsSpan(0, bytesWritten).ToArray());
					}
					finally
					{
						Array.Clear(array, 0, length);
						ArrayPool<byte>.Shared.Return(array);
					}
				};
			}
			if (fieldData.TagType == UniversalTagNumber.OctetString)
			{
				return delegate(AsnReader reader)
				{
					if (reader.TryGetPrimitiveOctetStringBytes(expectedTag, out var contents))
					{
						return contents;
					}
					int length = reader.PeekEncodedValue().Length;
					byte[] array = ArrayPool<byte>.Shared.Rent(length);
					try
					{
						if (!reader.TryCopyOctetStringBytes(expectedTag, array, out var bytesWritten))
						{
							throw new CryptographicException();
						}
						return new ReadOnlyMemory<byte>(array.AsSpan(0, bytesWritten).ToArray());
					}
					finally
					{
						Array.Clear(array, 0, length);
						ArrayPool<byte>.Shared.Return(array);
					}
				};
			}
			if (fieldData.TagType == UniversalTagNumber.Integer)
			{
				return (AsnReader reader) => reader.GetIntegerBytes(expectedTag);
			}
			throw new CryptographicException();
		}
		if (typeT == typeof(Oid))
		{
			bool skipFriendlyName = fieldData.PopulateOidFriendlyName != true;
			return (AsnReader reader) => reader.ReadObjectIdentifier(expectedTag, skipFriendlyName);
		}
		if (typeT.IsArray)
		{
			if (typeT.GetArrayRank() != 1)
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Type '{0}' cannot be serialized or deserialized because it is a multi-dimensional array.", typeT.FullName));
			}
			Type baseType = typeT.GetElementType();
			if (baseType.IsArray)
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Type '{0}' cannot be serialized or deserialized because it is an array of arrays.", typeT.FullName));
			}
			return delegate(AsnReader reader)
			{
				LinkedList<object> linkedList = new LinkedList<object>();
				AsnReader asnReader = ((localFieldData.TagType != UniversalTagNumber.Set) ? reader.ReadSequence(expectedTag) : reader.ReadSetOf(expectedTag));
				Deserializer deserializer = GetDeserializer(baseType, null);
				while (asnReader.HasData)
				{
					LinkedListNode<object> node = new LinkedListNode<object>(deserializer(asnReader));
					linkedList.AddLast(node);
				}
				object[] array = linkedList.ToArray();
				Array array2 = Array.CreateInstance(baseType, array.Length);
				Array.Copy(array, array2, array.Length);
				return array2;
			};
		}
		if (typeT == typeof(DateTimeOffset))
		{
			if (fieldData.TagType == UniversalTagNumber.UtcTime)
			{
				if (fieldData.TwoDigitYearMax.HasValue)
				{
					return (AsnReader reader) => reader.GetUtcTime(expectedTag, localFieldData.TwoDigitYearMax.Value);
				}
				return (AsnReader reader) => reader.GetUtcTime(expectedTag);
			}
			if (fieldData.TagType == UniversalTagNumber.GeneralizedTime)
			{
				bool disallowFractions = fieldData.DisallowGeneralizedTimeFractions.Value;
				return (AsnReader reader) => reader.GetGeneralizedTime(expectedTag, disallowFractions);
			}
			throw new CryptographicException();
		}
		if (typeT == typeof(BigInteger))
		{
			return (AsnReader reader) => reader.GetInteger(expectedTag);
		}
		if (typeT.IsLayoutSequential && fieldData.TagType == UniversalTagNumber.Sequence)
		{
			return (AsnReader reader) => DeserializeCustomType(reader, typeT, expectedTag);
		}
		throw new AsnSerializationConstraintException(global::SR.Format("Could not determine how to serialize or deserialize type '{0}'.", typeT.FullName));
	}

	private static object DefaultValue(byte[] defaultContents, Deserializer valueDeserializer)
	{
		try
		{
			AsnReader asnReader = new AsnReader(defaultContents, AsnEncodingRules.DER);
			object result = valueDeserializer(asnReader);
			if (asnReader.HasData)
			{
				throw new AsnSerializerInvalidDefaultException();
			}
			return result;
		}
		catch (AsnSerializerInvalidDefaultException)
		{
			throw;
		}
		catch (CryptographicException innerException)
		{
			throw new AsnSerializerInvalidDefaultException(innerException);
		}
	}

	private static void GetFieldInfo(Type typeT, FieldInfo fieldInfo, out SerializerFieldData serializerFieldData)
	{
		serializerFieldData = default(SerializerFieldData);
		object[] array = fieldInfo?.GetCustomAttributes(typeof(AsnTypeAttribute), inherit: false) ?? Array.Empty<object>();
		if (array.Length > 1)
		{
			throw new AsnSerializationConstraintException(global::SR.Format(fieldInfo.Name, fieldInfo.DeclaringType.FullName, typeof(AsnTypeAttribute).FullName));
		}
		Type type = UnpackIfNullable(typeT);
		if (array.Length == 1)
		{
			object obj = array[0];
			serializerFieldData.WasCustomized = true;
			Type[] array2;
			if (obj is AnyValueAttribute)
			{
				serializerFieldData.IsAny = true;
				array2 = new Type[1] { typeof(ReadOnlyMemory<byte>) };
			}
			else if (obj is IntegerAttribute)
			{
				array2 = new Type[1] { typeof(ReadOnlyMemory<byte>) };
				serializerFieldData.TagType = UniversalTagNumber.Integer;
			}
			else if (obj is BitStringAttribute)
			{
				array2 = new Type[1] { typeof(ReadOnlyMemory<byte>) };
				serializerFieldData.TagType = UniversalTagNumber.BitString;
			}
			else if (obj is OctetStringAttribute)
			{
				array2 = new Type[1] { typeof(ReadOnlyMemory<byte>) };
				serializerFieldData.TagType = UniversalTagNumber.OctetString;
			}
			else if (obj is ObjectIdentifierAttribute objectIdentifierAttribute)
			{
				serializerFieldData.PopulateOidFriendlyName = objectIdentifierAttribute.PopulateFriendlyName;
				array2 = new Type[2]
				{
					typeof(Oid),
					typeof(string)
				};
				serializerFieldData.TagType = UniversalTagNumber.ObjectIdentifier;
				if (objectIdentifierAttribute.PopulateFriendlyName && type == typeof(string))
				{
					throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on type '{1}' has [ObjectIdentifier].PopulateFriendlyName set to true, which is not applicable to a string.  Change the field to '{2}' or set PopulateFriendlyName to false.", fieldInfo.Name, fieldInfo.DeclaringType.FullName, typeof(Oid).FullName));
				}
			}
			else if (obj is BMPStringAttribute)
			{
				array2 = new Type[1] { typeof(string) };
				serializerFieldData.TagType = UniversalTagNumber.BMPString;
			}
			else if (obj is IA5StringAttribute)
			{
				array2 = new Type[1] { typeof(string) };
				serializerFieldData.TagType = UniversalTagNumber.IA5String;
			}
			else if (obj is UTF8StringAttribute)
			{
				array2 = new Type[1] { typeof(string) };
				serializerFieldData.TagType = UniversalTagNumber.UTF8String;
			}
			else if (obj is PrintableStringAttribute)
			{
				array2 = new Type[1] { typeof(string) };
				serializerFieldData.TagType = UniversalTagNumber.PrintableString;
			}
			else if (obj is VisibleStringAttribute)
			{
				array2 = new Type[1] { typeof(string) };
				serializerFieldData.TagType = UniversalTagNumber.VisibleString;
			}
			else if (obj is SequenceOfAttribute)
			{
				serializerFieldData.IsCollection = true;
				array2 = null;
				serializerFieldData.TagType = UniversalTagNumber.Sequence;
			}
			else if (obj is SetOfAttribute)
			{
				serializerFieldData.IsCollection = true;
				array2 = null;
				serializerFieldData.TagType = UniversalTagNumber.Set;
			}
			else if (obj is UtcTimeAttribute utcTimeAttribute)
			{
				array2 = new Type[1] { typeof(DateTimeOffset) };
				serializerFieldData.TagType = UniversalTagNumber.UtcTime;
				if (utcTimeAttribute.TwoDigitYearMax != 0)
				{
					serializerFieldData.TwoDigitYearMax = utcTimeAttribute.TwoDigitYearMax;
					if (serializerFieldData.TwoDigitYearMax < 99)
					{
						throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on type '{1}' has a [UtcTime] TwoDigitYearMax value ({2}) smaller than the minimum (99).", fieldInfo.Name, fieldInfo.DeclaringType.FullName, serializerFieldData.TwoDigitYearMax));
					}
				}
			}
			else
			{
				if (!(obj is GeneralizedTimeAttribute generalizedTimeAttribute))
				{
					throw new CryptographicException();
				}
				array2 = new Type[1] { typeof(DateTimeOffset) };
				serializerFieldData.TagType = UniversalTagNumber.GeneralizedTime;
				serializerFieldData.DisallowGeneralizedTimeFractions = generalizedTimeAttribute.DisallowFractions;
			}
			if (!serializerFieldData.IsCollection && Array.IndexOf(array2, type) < 0)
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' of type '{1}' has an effective type of '{2}' when one of ({3}) was expected.", fieldInfo.Name, fieldInfo.DeclaringType.Namespace, type.FullName, string.Join(", ", array2.Select((Type t) => t.FullName))));
			}
		}
		serializerFieldData.DefaultContents = (fieldInfo?.GetCustomAttribute<DefaultValueAttribute>(inherit: false))?.EncodedBytes;
		if (!serializerFieldData.TagType.HasValue && !serializerFieldData.IsAny)
		{
			if (type == typeof(bool))
			{
				serializerFieldData.TagType = UniversalTagNumber.Boolean;
			}
			else if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) || type == typeof(BigInteger))
			{
				serializerFieldData.TagType = UniversalTagNumber.Integer;
			}
			else if (type.IsLayoutSequential)
			{
				serializerFieldData.TagType = UniversalTagNumber.Sequence;
			}
			else
			{
				if (type == typeof(ReadOnlyMemory<byte>) || type == typeof(string) || type == typeof(DateTimeOffset))
				{
					throw new AsnAmbiguousFieldTypeException(fieldInfo, type);
				}
				if (type == typeof(Oid))
				{
					serializerFieldData.TagType = UniversalTagNumber.ObjectIdentifier;
				}
				else if (type.IsArray)
				{
					serializerFieldData.TagType = UniversalTagNumber.Sequence;
				}
				else if (type.IsEnum)
				{
					if (typeT.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length != 0)
					{
						serializerFieldData.TagType = UniversalTagNumber.BitString;
					}
					else
					{
						serializerFieldData.TagType = UniversalTagNumber.Enumerated;
					}
				}
				else if (fieldInfo != null)
				{
					throw new AsnSerializationConstraintException();
				}
			}
		}
		serializerFieldData.IsOptional = fieldInfo?.GetCustomAttribute<OptionalValueAttribute>(inherit: false) != null;
		if (serializerFieldData.IsOptional && !CanBeNull(typeT))
		{
			throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on type '{1}' is declared [OptionalValue], but it can not be assigned a null value.", fieldInfo.Name, fieldInfo.DeclaringType.FullName));
		}
		bool flag = GetChoiceAttribute(typeT) != null;
		ExpectedTagAttribute expectedTagAttribute = fieldInfo?.GetCustomAttribute<ExpectedTagAttribute>(inherit: false);
		if (expectedTagAttribute != null)
		{
			if (flag && !expectedTagAttribute.ExplicitTag)
			{
				throw new AsnSerializationConstraintException(global::SR.Format("Field '{0}' on type '{1}' has specified an implicit tag value via [ExpectedTag] for [Choice] type '{2}'. ExplicitTag must be true, or the [ExpectedTag] attribute removed.", fieldInfo.Name, fieldInfo.DeclaringType.FullName, typeT.FullName));
			}
			serializerFieldData.ExpectedTag = new Asn1Tag(expectedTagAttribute.TagClass, expectedTagAttribute.TagValue);
			serializerFieldData.HasExplicitTag = expectedTagAttribute.ExplicitTag;
			serializerFieldData.SpecifiedTag = true;
		}
		else
		{
			if (flag)
			{
				serializerFieldData.TagType = null;
			}
			serializerFieldData.SpecifiedTag = false;
			serializerFieldData.HasExplicitTag = false;
			serializerFieldData.ExpectedTag = new Asn1Tag(serializerFieldData.TagType.GetValueOrDefault());
		}
	}

	private static Type UnpackIfNullable(Type typeT)
	{
		return Nullable.GetUnderlyingType(typeT) ?? typeT;
	}

	private static Deserializer GetPrimitiveDeserializer(Type typeT, Asn1Tag tag)
	{
		if (typeT == typeof(bool))
		{
			return (AsnReader reader) => reader.ReadBoolean(tag);
		}
		if (typeT == typeof(int))
		{
			return TryOrFail(delegate(AsnReader reader, out int value)
			{
				return reader.TryReadInt32(tag, out value);
			});
		}
		if (typeT == typeof(uint))
		{
			return TryOrFail(delegate(AsnReader reader, out uint value)
			{
				return reader.TryReadUInt32(tag, out value);
			});
		}
		if (typeT == typeof(short))
		{
			return TryOrFail(delegate(AsnReader reader, out short value)
			{
				return reader.TryReadInt16(tag, out value);
			});
		}
		if (typeT == typeof(ushort))
		{
			return TryOrFail(delegate(AsnReader reader, out ushort value)
			{
				return reader.TryReadUInt16(tag, out value);
			});
		}
		if (typeT == typeof(byte))
		{
			return TryOrFail(delegate(AsnReader reader, out byte value)
			{
				return reader.TryReadUInt8(tag, out value);
			});
		}
		if (typeT == typeof(sbyte))
		{
			return TryOrFail(delegate(AsnReader reader, out sbyte value)
			{
				return reader.TryReadInt8(tag, out value);
			});
		}
		if (typeT == typeof(long))
		{
			return TryOrFail(delegate(AsnReader reader, out long value)
			{
				return reader.TryReadInt64(tag, out value);
			});
		}
		if (typeT == typeof(ulong))
		{
			return TryOrFail(delegate(AsnReader reader, out ulong value)
			{
				return reader.TryReadUInt64(tag, out value);
			});
		}
		throw new AsnSerializationConstraintException(global::SR.Format("Could not determine how to serialize or deserialize type '{0}'.", typeT.FullName));
	}

	private static Serializer GetPrimitiveSerializer(Type typeT, Asn1Tag primitiveTag)
	{
		if (typeT == typeof(bool))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteBoolean(primitiveTag, (bool)value);
			};
		}
		if (typeT == typeof(int))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (int)value);
			};
		}
		if (typeT == typeof(uint))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (uint)value);
			};
		}
		if (typeT == typeof(short))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (short)value);
			};
		}
		if (typeT == typeof(ushort))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (ushort)value);
			};
		}
		if (typeT == typeof(byte))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (byte)value);
			};
		}
		if (typeT == typeof(sbyte))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (sbyte)value);
			};
		}
		if (typeT == typeof(long))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (long)value);
			};
		}
		if (typeT == typeof(ulong))
		{
			return delegate(object value, AsnWriter writer)
			{
				writer.WriteInteger(primitiveTag, (ulong)value);
			};
		}
		throw new AsnSerializationConstraintException(global::SR.Format("Could not determine how to serialize or deserialize type '{0}'.", typeT.FullName));
	}

	public static T Deserialize<T>(ReadOnlyMemory<byte> source, AsnEncodingRules ruleSet)
	{
		Deserializer deserializer = GetDeserializer(typeof(T), null);
		AsnReader asnReader = new AsnReader(source, ruleSet);
		T result = (T)deserializer(asnReader);
		asnReader.ThrowIfNotEmpty();
		return result;
	}

	public static T Deserialize<T>(ReadOnlyMemory<byte> source, AsnEncodingRules ruleSet, out int bytesRead)
	{
		Deserializer deserializer = GetDeserializer(typeof(T), null);
		AsnReader asnReader = new AsnReader(source, ruleSet);
		ReadOnlyMemory<byte> readOnlyMemory = asnReader.PeekEncodedValue();
		T result = (T)deserializer(asnReader);
		bytesRead = readOnlyMemory.Length;
		return result;
	}

	public static AsnWriter Serialize<T>(T value, AsnEncodingRules ruleSet)
	{
		AsnWriter asnWriter = new AsnWriter(ruleSet);
		try
		{
			Serialize(value, asnWriter);
			return asnWriter;
		}
		catch
		{
			asnWriter.Dispose();
			throw;
		}
	}

	public static void Serialize<T>(T value, AsnWriter existingWriter)
	{
		if (existingWriter == null)
		{
			throw new ArgumentNullException("existingWriter");
		}
		GetSerializer(typeof(T), null)(value, existingWriter);
	}
}
