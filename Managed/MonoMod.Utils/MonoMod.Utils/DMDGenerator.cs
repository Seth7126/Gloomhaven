using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MonoMod.Utils;

public abstract class DMDGenerator<TSelf> : _IDMDGenerator where TSelf : DMDGenerator<TSelf>, new()
{
	private static TSelf _Instance;

	internal static readonly Dictionary<Type, FieldInfo> fmap_mono_assembly = new Dictionary<Type, FieldInfo>();

	protected abstract MethodInfo _Generate(DynamicMethodDefinition dmd, object context);

	MethodInfo _IDMDGenerator.Generate(DynamicMethodDefinition dmd, object context)
	{
		return _Postbuild(_Generate(dmd, context));
	}

	public static MethodInfo Generate(DynamicMethodDefinition dmd, object context = null)
	{
		return _Postbuild((_Instance ?? (_Instance = new TSelf()))._Generate(dmd, context));
	}

	private unsafe static MethodInfo _Postbuild(MethodInfo mi)
	{
		if (mi == null)
		{
			return null;
		}
		if (DynamicMethodDefinition._IsMono && !(mi is DynamicMethod) && mi.DeclaringType != null && !DynamicMethodDefinition._IsOldMonoSRE)
		{
			Module module = mi?.Module;
			if (module == null)
			{
				return mi;
			}
			Assembly assembly = module.Assembly;
			Type type = assembly?.GetType();
			if (type == null)
			{
				return mi;
			}
			FieldInfo value;
			lock (fmap_mono_assembly)
			{
				if (!fmap_mono_assembly.TryGetValue(type, out value))
				{
					value = type.GetField("_mono_assembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					fmap_mono_assembly[type] = value;
				}
			}
			if (value == null)
			{
				return mi;
			}
			IntPtr intPtr = (IntPtr)value.GetValue(assembly);
			int num = IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + 20 + 4 + 4 + 4 + ((!(typeof(object).Assembly.GetName().Name == "System.Private.CoreLib")) ? ((IntPtr.Size == 4) ? 12 : 16) : ((IntPtr.Size == 4) ? 20 : 24)) + IntPtr.Size + IntPtr.Size + 1 + 1 + 1;
			byte* ptr = (byte*)((long)intPtr + num);
			*ptr = 1;
		}
		return mi;
	}
}
