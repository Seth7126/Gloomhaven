using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public interface IUiNavigationManager
{
	IUiNavigationSelectable CurrentlySelectedElement { get; }

	event Action<IUiNavigationSelectable> OnElementSelectedEvent;

	IUiNavigationRoot RootByName(string navigationName);

	IUiNavigationElement ElementByName(string navigationName);

	List<IUiNavigationNode> PathToSelectable(IUiNavigationSelectable uiNavigationSelectable);

	IUiNavigationSelectable GetPreviouslySelectedIn(IUiNavigationNode navigationNode);

	IUiNavigationElement GetPreviouslyPickedIn(IUiNavigationNode navigationNode);

	bool SetCurrentRoot(string navigationName, bool selectFirst, IUiNavigationSelectable selectConcrete = null);

	bool SetCurrentRoot(IUiNavigationRoot uiNavigationRoot, bool selectFirst, IUiNavigationSelectable selectConcrete = null);

	bool TrySelect(IUiNavigationSelectable uiNavigationSelectable);

	bool TrySelectFirstIn(IUiNavigationNode navigationNode);

	bool TrySelectIn(IUiNavigationNode navigationNode, IUiNavigationSelectable uiNavigationSelectable);

	bool TrySelectPreviousIn(IUiNavigationNode navigationNode);

	void DeselectCurrentSelectable();

	void DeselectAll();

	void BlockNavigation(UiNavigationBlocker blocker);

	void UnblockNavigation(UiNavigationBlocker blocker);

	void GetTaggedElements(string navigationTag, List<IUiNavigationElement> result, Func<IUiNavigationElement, bool> additionalFilter = null);
}
