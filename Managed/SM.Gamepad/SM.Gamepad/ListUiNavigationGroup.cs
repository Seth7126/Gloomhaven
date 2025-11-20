using UnityEngine;

namespace SM.Gamepad;

public class ListUiNavigationGroup : UiNavigationGroup
{
	[Header("List Properties")]
	public ListUiNavigationHandler.ListNavigationMode NavigationMode;

	[Tooltip("Should list be looped around")]
	public ListUiNavigationHandler.ListLoopingTypes LoopingTypes;

	protected override void InitializeOldFashionNavigationHandlers()
	{
		OuterUINavigation = new OuterUiNavigationHandler(base.Root as UiNavigationRoot, this);
		InnerUINavigationHandler = new ListUiNavigationHandler(base.Root as UiNavigationRoot, this);
	}
}
