using System;

namespace UnityEngine.UIElements;

[Flags]
internal enum VisualElementFlags
{
	WorldTransformDirty = 1,
	WorldTransformInverseDirty = 2,
	WorldClipDirty = 4,
	BoundingBoxDirty = 8,
	WorldBoundingBoxDirty = 0x10,
	LayoutManual = 0x20,
	CompositeRoot = 0x40,
	RequireMeasureFunction = 0x80,
	EnableViewDataPersistence = 0x100,
	DisableClipping = 0x200,
	NeedsAttachToPanelEvent = 0x400,
	HierarchyDisplayed = 0x800,
	StyleInitialized = 0x1000,
	Init = 0x81F
}
