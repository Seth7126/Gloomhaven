using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class ScrollPositioned
{
	public Vector2 ScrollPosition { get; set; }
}
internal class ScrollPositioned<TViewModel> : ScrollPositioned
{
	public TViewModel ViewModel { get; private set; }

	public ScrollPositioned(TViewModel viewModel)
	{
		ViewModel = viewModel;
	}
}
