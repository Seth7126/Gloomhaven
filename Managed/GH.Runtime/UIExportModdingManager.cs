using GLOOM;
using UnityEngine;

public class UIExportModdingManager : MonoBehaviour
{
	[SerializeField]
	private UIBackgroundTaskMask loadingScreen;

	[SerializeField]
	private TextLocalizedListener hintText;

	private bool _exporting;

	public bool Exporting => _exporting;

	public void Export()
	{
		loadingScreen.Show();
		hintText.SetArguments(RootSaveData.ModdingExportFolder);
		_exporting = true;
		StartCoroutine(GHModding.ExportModdableContent(OnExportSuccessed, OnExportFailed));
	}

	private void OnExportFailed()
	{
		FinishExport();
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_EXPORT_FAILED_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_EXPORT_FAILED_CONFIRMATION"), "GUI_CLOSE");
	}

	private void OnExportSuccessed()
	{
		FinishExport();
		UIModdingNotifications.ShowExportSuccessNotification(RootSaveData.ModdingExportFolder);
	}

	private void FinishExport()
	{
		_exporting = false;
		loadingScreen.Hide();
	}
}
