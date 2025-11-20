using System;
using System.Collections.Generic;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

namespace XUnity.Common.Utilities;

public static class CustomFastReflectionHelper
{
	private struct FastReflectionDelegateKey
	{
		public MethodBase Method { get; }

		public bool DirectBoxValueAccess { get; }

		public bool ForceNonVirtCall { get; }

		public FastReflectionDelegateKey(MethodBase method, bool directBoxValueAccess, bool forceNonVirtCall)
		{
			Method = method;
			DirectBoxValueAccess = directBoxValueAccess;
			ForceNonVirtCall = forceNonVirtCall;
		}

		public override bool Equals(object obj)
		{
			if (obj is FastReflectionDelegateKey fastReflectionDelegateKey && EqualityComparer<MethodBase>.Default.Equals(Method, fastReflectionDelegateKey.Method) && DirectBoxValueAccess == fastReflectionDelegateKey.DirectBoxValueAccess)
			{
				return ForceNonVirtCall == fastReflectionDelegateKey.ForceNonVirtCall;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((1017116076 * -1521134295 + EqualityComparer<MethodBase>.Default.GetHashCode(Method)) * -1521134295 + DirectBoxValueAccess.GetHashCode()) * -1521134295 + ForceNonVirtCall.GetHashCode();
		}
	}

	private static readonly Dictionary<FastReflectionDelegateKey, FastReflectionDelegate> MethodCache = new Dictionary<FastReflectionDelegateKey, FastReflectionDelegate>();

	public static FastReflectionDelegate CreateFastDelegate(this MethodBase method, bool directBoxValueAccess = true, bool forceNonVirtCall = false)
	{
		FastReflectionDelegateKey key = new FastReflectionDelegateKey(method, directBoxValueAccess, forceNonVirtCall);
		if (MethodCache.TryGetValue(key, out var value))
		{
			return value;
		}
		value = ((ClrTypes.DynamicMethodDefinition == null) ? GetFastDelegateForSRE(method, directBoxValueAccess, forceNonVirtCall) : GetFastDelegateForCecil(method, directBoxValueAccess, forceNonVirtCall));
		MethodCache.Add(key, value);
		return value;
	}

	public static Func<T, F> CreateFastFieldGetter<T, F>(FieldInfo fieldInfo)
	{
		if (ClrTypes.DynamicMethodDefinition != null)
		{
			return CreateFastFieldGetterForCecil<T, F>(fieldInfo);
		}
		return CreateFastFieldGetterForSRE<T, F>(fieldInfo);
	}

	public static Action<T, F> CreateFastFieldSetter<T, F>(FieldInfo fieldInfo)
	{
		if (ClrTypes.DynamicMethodDefinition != null)
		{
			return CreateFastFieldSetterForCecil<T, F>(fieldInfo);
		}
		return CreateFastFieldSetterForSRE<T, F>(fieldInfo);
	}

	private static FastReflectionDelegate GetFastDelegateForCecil(MethodBase method, bool directBoxValueAccess, bool forceNonVirtCall)
	{
		try
		{
			return CecilFastReflectionHelper.CreateFastDelegate(method, directBoxValueAccess, forceNonVirtCall);
		}
		catch (Exception e)
		{
			try
			{
				XuaLogger.Common.Warn(e, "Failed creating fast reflection delegate through with cecil. Retrying with reflection emit...");
				return ReflectionEmitFastReflectionHelper.CreateFastDelegate(method, directBoxValueAccess, forceNonVirtCall);
			}
			catch (Exception e2)
			{
				XuaLogger.Common.Warn(e2, "Failed creating fast reflection delegate through with reflection emit. Falling back to standard reflection...");
				return (object target, object[] args) => method.Invoke(target, args);
			}
		}
	}

	private static Func<T, F> CreateFastFieldGetterForCecil<T, F>(FieldInfo fieldInfo)
	{
		try
		{
			return CecilFastReflectionHelper.CreateFastFieldGetter<T, F>(fieldInfo);
		}
		catch (Exception e)
		{
			try
			{
				XuaLogger.Common.Warn(e, "Failed creating fast reflection delegate through with cecil. Retrying with reflection emit...");
				return ReflectionEmitFastReflectionHelper.CreateFastFieldGetter<T, F>(fieldInfo);
			}
			catch (Exception e2)
			{
				XuaLogger.Common.Warn(e2, "Failed creating fast reflection delegate through with reflection emit. Falling back to standard reflection...");
				return (T target) => (F)fieldInfo.GetValue(target);
			}
		}
	}

	private static Action<T, F> CreateFastFieldSetterForCecil<T, F>(FieldInfo fieldInfo)
	{
		try
		{
			return CecilFastReflectionHelper.CreateFastFieldSetter<T, F>(fieldInfo);
		}
		catch (Exception e)
		{
			try
			{
				XuaLogger.Common.Warn(e, "Failed creating fast reflection delegate through with cecil. Retrying with reflection emit...");
				return ReflectionEmitFastReflectionHelper.CreateFastFieldSetter<T, F>(fieldInfo);
			}
			catch (Exception e2)
			{
				XuaLogger.Common.Warn(e2, "Failed creating fast reflection delegate through with reflection emit. Falling back to standard reflection...");
				return delegate(T target, F value)
				{
					fieldInfo.SetValue(target, value);
				};
			}
		}
	}

	private static FastReflectionDelegate GetFastDelegateForSRE(MethodBase method, bool directBoxValueAccess, bool forceNonVirtCall)
	{
		try
		{
			return ReflectionEmitFastReflectionHelper.CreateFastDelegate(method, directBoxValueAccess, forceNonVirtCall);
		}
		catch (Exception e)
		{
			XuaLogger.Common.Warn(e, "Failed creating fast reflection delegate through with reflection emit. Falling back to standard reflection...");
			return (object target, object[] args) => method.Invoke(target, args);
		}
	}

	private static Func<T, F> CreateFastFieldGetterForSRE<T, F>(FieldInfo fieldInfo)
	{
		try
		{
			return ReflectionEmitFastReflectionHelper.CreateFastFieldGetter<T, F>(fieldInfo);
		}
		catch (Exception e)
		{
			XuaLogger.Common.Warn(e, "Failed creating fast reflection delegate through with reflection emit. Falling back to standard reflection...");
			return (T target) => (F)fieldInfo.GetValue(target);
		}
	}

	private static Action<T, F> CreateFastFieldSetterForSRE<T, F>(FieldInfo fieldInfo)
	{
		try
		{
			return ReflectionEmitFastReflectionHelper.CreateFastFieldSetter<T, F>(fieldInfo);
		}
		catch (Exception e)
		{
			XuaLogger.Common.Warn(e, "Failed creating fast reflection delegate through with reflection emit. Falling back to standard reflection...");
			return delegate(T target, F value)
			{
				fieldInfo.SetValue(target, value);
			};
		}
	}
}
