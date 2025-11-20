using System;
using System.Collections.Generic;

namespace Mono.Cecil;

internal sealed class TypeReferenceEqualityComparer : EqualityComparer<TypeReference>
{
	public override bool Equals(TypeReference x, TypeReference y)
	{
		return AreEqual(x, y);
	}

	public override int GetHashCode(TypeReference obj)
	{
		return GetHashCodeFor(obj);
	}

	public static bool AreEqual(TypeReference a, TypeReference b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		MetadataType metadataType = a.MetadataType;
		MetadataType metadataType2 = b.MetadataType;
		if (metadataType == MetadataType.GenericInstance || metadataType2 == MetadataType.GenericInstance)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual((GenericInstanceType)a, (GenericInstanceType)b);
		}
		if (metadataType == MetadataType.Array || metadataType2 == MetadataType.Array)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			ArrayType arrayType = (ArrayType)a;
			ArrayType arrayType2 = (ArrayType)b;
			if (arrayType.Rank != arrayType2.Rank)
			{
				return false;
			}
			return AreEqual(arrayType.ElementType, arrayType2.ElementType);
		}
		if (metadataType == MetadataType.Var || metadataType2 == MetadataType.Var)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual((GenericParameter)a, (GenericParameter)b);
		}
		if (metadataType == MetadataType.MVar || metadataType2 == MetadataType.MVar)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual((GenericParameter)a, (GenericParameter)b);
		}
		if (metadataType == MetadataType.ByReference || metadataType2 == MetadataType.ByReference)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual(((ByReferenceType)a).ElementType, ((ByReferenceType)b).ElementType);
		}
		if (metadataType == MetadataType.Pointer || metadataType2 == MetadataType.Pointer)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual(((PointerType)a).ElementType, ((PointerType)b).ElementType);
		}
		if (metadataType == MetadataType.RequiredModifier || metadataType2 == MetadataType.RequiredModifier)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			RequiredModifierType requiredModifierType = (RequiredModifierType)a;
			RequiredModifierType requiredModifierType2 = (RequiredModifierType)b;
			if (AreEqual(requiredModifierType.ModifierType, requiredModifierType2.ModifierType))
			{
				return AreEqual(requiredModifierType.ElementType, requiredModifierType2.ElementType);
			}
			return false;
		}
		if (metadataType == MetadataType.OptionalModifier || metadataType2 == MetadataType.OptionalModifier)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			OptionalModifierType optionalModifierType = (OptionalModifierType)a;
			OptionalModifierType optionalModifierType2 = (OptionalModifierType)b;
			if (AreEqual(optionalModifierType.ModifierType, optionalModifierType2.ModifierType))
			{
				return AreEqual(optionalModifierType.ElementType, optionalModifierType2.ElementType);
			}
			return false;
		}
		if (metadataType == MetadataType.Pinned || metadataType2 == MetadataType.Pinned)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual(((PinnedType)a).ElementType, ((PinnedType)b).ElementType);
		}
		if (metadataType == MetadataType.Sentinel || metadataType2 == MetadataType.Sentinel)
		{
			if (metadataType != metadataType2)
			{
				return false;
			}
			return AreEqual(((SentinelType)a).ElementType, ((SentinelType)b).ElementType);
		}
		if (!a.Name.Equals(b.Name) || !a.Namespace.Equals(b.Namespace))
		{
			return false;
		}
		TypeDefinition typeDefinition = a.Resolve();
		TypeDefinition typeDefinition2 = b.Resolve();
		return typeDefinition == typeDefinition2;
	}

	private static bool AreEqual(GenericParameter a, GenericParameter b)
	{
		if (a == b)
		{
			return true;
		}
		if (a.Position != b.Position)
		{
			return false;
		}
		if (a.Type != b.Type)
		{
			return false;
		}
		if (a.Owner is TypeReference a2 && AreEqual(a2, b.Owner as TypeReference))
		{
			return true;
		}
		if (a.Owner is MethodReference x && MethodReferenceComparer.AreEqual(x, b.Owner as MethodReference))
		{
			return true;
		}
		return false;
	}

	private static bool AreEqual(GenericInstanceType a, GenericInstanceType b)
	{
		if (a == b)
		{
			return true;
		}
		int count = a.GenericArguments.Count;
		if (count != b.GenericArguments.Count)
		{
			return false;
		}
		if (!AreEqual(a.ElementType, b.ElementType))
		{
			return false;
		}
		for (int i = 0; i < count; i++)
		{
			if (!AreEqual(a.GenericArguments[i], b.GenericArguments[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int GetHashCodeFor(TypeReference obj)
	{
		MetadataType metadataType = obj.MetadataType;
		switch (metadataType)
		{
		case MetadataType.GenericInstance:
		{
			GenericInstanceType genericInstanceType = (GenericInstanceType)obj;
			int num4 = GetHashCodeFor(genericInstanceType.ElementType) * 486187739 + 31;
			for (int i = 0; i < genericInstanceType.GenericArguments.Count; i++)
			{
				num4 = num4 * 486187739 + GetHashCodeFor(genericInstanceType.GenericArguments[i]);
			}
			return num4;
		}
		case MetadataType.Array:
		{
			ArrayType arrayType = (ArrayType)obj;
			return GetHashCodeFor(arrayType.ElementType) * 486187739 + arrayType.Rank.GetHashCode();
		}
		case MetadataType.Var:
		case MetadataType.MVar:
		{
			GenericParameter genericParameter = (GenericParameter)obj;
			int num = genericParameter.Position.GetHashCode() * 486187739;
			int num2 = (int)metadataType;
			int num3 = num + num2.GetHashCode();
			if (genericParameter.Owner is TypeReference obj2)
			{
				return num3 * 486187739 + GetHashCodeFor(obj2);
			}
			if (genericParameter.Owner is MethodReference obj3)
			{
				return num3 * 486187739 + MethodReferenceComparer.GetHashCodeFor(obj3);
			}
			throw new InvalidOperationException("Generic parameter encountered with invalid owner");
		}
		case MetadataType.ByReference:
			return GetHashCodeFor(((ByReferenceType)obj).ElementType) * 486187739 * 37;
		case MetadataType.Pointer:
			return GetHashCodeFor(((PointerType)obj).ElementType) * 486187739 * 41;
		case MetadataType.RequiredModifier:
		{
			RequiredModifierType requiredModifierType = (RequiredModifierType)obj;
			return GetHashCodeFor(requiredModifierType.ElementType) * 43 * 486187739 + GetHashCodeFor(requiredModifierType.ModifierType);
		}
		case MetadataType.OptionalModifier:
		{
			OptionalModifierType optionalModifierType = (OptionalModifierType)obj;
			return GetHashCodeFor(optionalModifierType.ElementType) * 47 * 486187739 + GetHashCodeFor(optionalModifierType.ModifierType);
		}
		case MetadataType.Pinned:
			return GetHashCodeFor(((PinnedType)obj).ElementType) * 486187739 * 53;
		case MetadataType.Sentinel:
			return GetHashCodeFor(((SentinelType)obj).ElementType) * 486187739 * 59;
		case MetadataType.FunctionPointer:
			throw new NotImplementedException("We currently don't handle function pointer types.");
		default:
			return obj.Namespace.GetHashCode() * 486187739 + obj.FullName.GetHashCode();
		}
	}
}
