using System.Collections.Generic;

namespace Mono.Cecil;

internal sealed class MethodReferenceComparer : EqualityComparer<MethodReference>
{
	public override bool Equals(MethodReference x, MethodReference y)
	{
		return AreEqual(x, y);
	}

	public override int GetHashCode(MethodReference obj)
	{
		return GetHashCodeFor(obj);
	}

	public static bool AreEqual(MethodReference x, MethodReference y)
	{
		if (x == y)
		{
			return true;
		}
		if (x.HasThis != y.HasThis)
		{
			return false;
		}
		if (x.HasParameters != y.HasParameters)
		{
			return false;
		}
		if (x.HasGenericParameters != y.HasGenericParameters)
		{
			return false;
		}
		if (x.Parameters.Count != y.Parameters.Count)
		{
			return false;
		}
		if (x.Name != y.Name)
		{
			return false;
		}
		if (!TypeReferenceEqualityComparer.AreEqual(x.DeclaringType, y.DeclaringType))
		{
			return false;
		}
		GenericInstanceMethod genericInstanceMethod = x as GenericInstanceMethod;
		GenericInstanceMethod genericInstanceMethod2 = y as GenericInstanceMethod;
		if (genericInstanceMethod != null || genericInstanceMethod2 != null)
		{
			if (genericInstanceMethod == null || genericInstanceMethod2 == null)
			{
				return false;
			}
			if (genericInstanceMethod.GenericArguments.Count != genericInstanceMethod2.GenericArguments.Count)
			{
				return false;
			}
			for (int i = 0; i < genericInstanceMethod.GenericArguments.Count; i++)
			{
				if (!TypeReferenceEqualityComparer.AreEqual(genericInstanceMethod.GenericArguments[i], genericInstanceMethod2.GenericArguments[i]))
				{
					return false;
				}
			}
		}
		if (x.Resolve() != y.Resolve())
		{
			return false;
		}
		return true;
	}

	public static bool AreSignaturesEqual(MethodReference x, MethodReference y)
	{
		if (x.HasThis != y.HasThis)
		{
			return false;
		}
		if (x.Parameters.Count != y.Parameters.Count)
		{
			return false;
		}
		if (x.GenericParameters.Count != y.GenericParameters.Count)
		{
			return false;
		}
		for (int i = 0; i < x.Parameters.Count; i++)
		{
			if (!TypeReferenceEqualityComparer.AreEqual(x.Parameters[i].ParameterType, y.Parameters[i].ParameterType))
			{
				return false;
			}
		}
		if (!TypeReferenceEqualityComparer.AreEqual(x.ReturnType, y.ReturnType))
		{
			return false;
		}
		return true;
	}

	public static int GetHashCodeFor(MethodReference obj)
	{
		if (obj is GenericInstanceMethod genericInstanceMethod)
		{
			int num = GetHashCodeFor(genericInstanceMethod.ElementMethod);
			for (int i = 0; i < genericInstanceMethod.GenericArguments.Count; i++)
			{
				num = num * 486187739 + TypeReferenceEqualityComparer.GetHashCodeFor(genericInstanceMethod.GenericArguments[i]);
			}
			return num;
		}
		return TypeReferenceEqualityComparer.GetHashCodeFor(obj.DeclaringType) * 486187739 + obj.Name.GetHashCode();
	}
}
