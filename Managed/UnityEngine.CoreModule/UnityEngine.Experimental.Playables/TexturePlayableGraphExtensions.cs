using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Playables;

namespace UnityEngine.Experimental.Playables;

[NativeHeader("Runtime/Director/Core/HPlayableOutput.h")]
[StaticAccessor("TexturePlayableGraphExtensionsBindings", StaticAccessorType.DoubleColon)]
[NativeHeader("Runtime/Export/Director/TexturePlayableGraphExtensions.bindings.h")]
internal static class TexturePlayableGraphExtensions
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool InternalCreateTextureOutput(ref PlayableGraph graph, string name, out PlayableOutputHandle handle);
}
