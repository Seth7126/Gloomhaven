using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[NativeHeader("Runtime/Graphics/Mesh/MeshScriptBindings.h")]
[NativeHeader("Runtime/Graphics/Mesh/MeshCombiner.h")]
internal struct StaticBatchingHelper
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("MeshScripting::CombineMeshVerticesForStaticBatching")]
	internal static extern Mesh InternalCombineVertices(MeshSubsetCombineUtility.MeshInstance[] meshes, string meshName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("MeshScripting::CombineMeshIndicesForStaticBatching")]
	internal static extern void InternalCombineIndices(MeshSubsetCombineUtility.SubMeshInstance[] submeshes, Mesh combinedMesh);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("IsMeshBatchable")]
	internal static extern bool IsMeshBatchable(Mesh mesh);
}
