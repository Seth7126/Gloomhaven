using UnityEngine;

namespace SM.Gamepad;

public class GridUINavigationGroup : UiNavigationGroup
{
	[Header("Grid Properties")]
	[Tooltip("Should grid be looped around")]
	public GridUINavigationHandler.LoopingTypes LoopingTypes;

	[Min(1f)]
	public int Rows;

	[Min(1f)]
	public int Columns;

	protected override void InitializeOldFashionNavigationHandlers()
	{
		OuterUINavigation = new OuterUiNavigationHandler(base.Root as UiNavigationRoot, this);
		InnerUINavigationHandler = new GridUINavigationHandler(base.Root as UiNavigationRoot, this);
	}
}
