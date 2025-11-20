namespace XUnity.AutoTranslator.Plugin.Core.UIResize;

internal interface IFontAutoResizeCommand
{
	bool ShouldAutoResize();

	double? GetMinSize();

	double? GetMaxSize();
}
