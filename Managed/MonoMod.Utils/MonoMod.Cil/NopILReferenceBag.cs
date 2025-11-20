using System;
using System.Reflection;

namespace MonoMod.Cil;

public sealed class NopILReferenceBag : IILReferenceBag
{
	public static readonly NopILReferenceBag Instance = new NopILReferenceBag();

	private Exception NOP()
	{
		return new NotSupportedException("Inline references not supported in this context");
	}

	public T Get<T>(int id)
	{
		throw NOP();
	}

	public MethodInfo GetGetter<T>()
	{
		throw NOP();
	}

	public int Store<T>(T t)
	{
		throw NOP();
	}

	public void Clear<T>(int id)
	{
		throw NOP();
	}

	public MethodInfo GetDelegateInvoker<T>() where T : Delegate
	{
		throw NOP();
	}
}
