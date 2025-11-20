using GLOO.Introduction;

public class UIGuildmasterButtonFTUEHighlight : UIIntroduceElementHighlight
{
	protected override void SetHighlighted(bool highlighted)
	{
		base.SetHighlighted(highlighted);
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
	}
}
