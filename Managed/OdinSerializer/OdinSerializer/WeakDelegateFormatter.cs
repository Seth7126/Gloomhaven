using System;

namespace OdinSerializer;

public class WeakDelegateFormatter : DelegateFormatter<Delegate>
{
	public WeakDelegateFormatter(Type delegateType)
		: base(delegateType)
	{
	}
}
