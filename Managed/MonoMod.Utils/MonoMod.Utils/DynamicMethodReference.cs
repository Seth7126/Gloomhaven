using System.Reflection.Emit;
using Mono.Cecil;

namespace MonoMod.Utils;

public class DynamicMethodReference : MethodReference
{
	public DynamicMethod DynamicMethod;

	public DynamicMethodReference(ModuleDefinition module, DynamicMethod dm)
		: base("", module.TypeSystem.Void)
	{
		DynamicMethod = dm;
	}
}
