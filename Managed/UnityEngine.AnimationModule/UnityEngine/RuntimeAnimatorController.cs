using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
[NativeHeader("Modules/Animation/RuntimeAnimatorController.h")]
[ExcludeFromObjectFactory]
public class RuntimeAnimatorController : Object
{
	public extern AnimationClip[] animationClips
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	protected RuntimeAnimatorController()
	{
	}
}
