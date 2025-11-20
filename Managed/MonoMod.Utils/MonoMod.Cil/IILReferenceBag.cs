using System;
using System.Reflection;

namespace MonoMod.Cil;

public interface IILReferenceBag
{
	T Get<T>(int id);

	MethodInfo GetGetter<T>();

	int Store<T>(T t);

	void Clear<T>(int id);

	MethodInfo GetDelegateInvoker<T>() where T : Delegate;
}
