using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[Flags]
[NativeHeader("Modules/XR/Subsystems/Meshing/XRMeshBindings.h")]
[UsedByNativeCode]
public enum MeshGenerationOptions
{
	None = 0,
	ConsumeTransform = 1
}
