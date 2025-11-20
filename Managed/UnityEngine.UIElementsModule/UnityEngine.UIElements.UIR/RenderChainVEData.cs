using System.Collections.Generic;
using UnityEngine.UIElements.UIR.Implementation;

namespace UnityEngine.UIElements.UIR;

internal struct RenderChainVEData
{
	internal VisualElement prev;

	internal VisualElement next;

	internal VisualElement groupTransformAncestor;

	internal VisualElement boneTransformAncestor;

	internal VisualElement prevDirty;

	internal VisualElement nextDirty;

	internal int hierarchyDepth;

	internal RenderDataDirtyTypes dirtiedValues;

	internal uint dirtyID;

	internal RenderChainCommand firstCommand;

	internal RenderChainCommand lastCommand;

	internal RenderChainCommand firstClosingCommand;

	internal RenderChainCommand lastClosingCommand;

	internal bool isInChain;

	internal bool isHierarchyHidden;

	internal bool localFlipsWinding;

	internal bool worldFlipsWinding;

	internal ClipMethod clipMethod;

	internal int childrenStencilRef;

	internal int childrenMaskDepth;

	internal bool disableNudging;

	internal bool usesLegacyText;

	internal MeshHandle data;

	internal MeshHandle closingData;

	internal Matrix4x4 verticesSpace;

	internal int displacementUVStart;

	internal int displacementUVEnd;

	internal BMPAlloc transformID;

	internal BMPAlloc clipRectID;

	internal BMPAlloc opacityID;

	internal BMPAlloc textCoreSettingsID;

	internal BMPAlloc backgroundColorID;

	internal BMPAlloc borderLeftColorID;

	internal BMPAlloc borderTopColorID;

	internal BMPAlloc borderRightColorID;

	internal BMPAlloc borderBottomColorID;

	internal BMPAlloc tintColorID;

	internal float compositeOpacity;

	internal Color backgroundColor;

	internal VisualElement prevText;

	internal VisualElement nextText;

	internal List<RenderChainTextEntry> textEntries;

	internal BasicNode<TextureEntry> textures;

	internal RenderChainCommand lastClosingOrLastCommand => lastClosingCommand ?? lastCommand;

	internal static bool AllocatesID(BMPAlloc alloc)
	{
		return alloc.ownedState == OwnedState.Owned && alloc.IsValid();
	}

	internal static bool InheritsID(BMPAlloc alloc)
	{
		return alloc.ownedState == OwnedState.Inherited && alloc.IsValid();
	}
}
