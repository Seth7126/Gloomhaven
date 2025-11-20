namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class IndividualTranslatorTranslationViewModel
{
	public TranslatorViewModel Translator { get; private set; }

	public IndividualTranslationViewModel Translation { get; private set; }

	public IndividualTranslatorTranslationViewModel(TranslatorViewModel translator, IndividualTranslationViewModel translation)
	{
		Translator = translator;
		Translation = translation;
	}

	public void CopyToClipboard()
	{
		Translation.CopyToClipboard();
	}

	public bool CanCopyToClipboard()
	{
		return Translation.CanCopyToClipboard();
	}
}
