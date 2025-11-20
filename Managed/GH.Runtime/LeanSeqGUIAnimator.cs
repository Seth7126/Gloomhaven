public abstract class LeanSeqGUIAnimator : GUIAnimator
{
	private LTSeq seq;

	protected override void Animate()
	{
		seq = BuidSeq();
		seq.append(delegate
		{
			seq = null;
			FinishAnimation();
		});
	}

	protected abstract LTSeq BuidSeq();

	protected override void Clear()
	{
		base.Clear();
		if (seq != null)
		{
			LeanTween.cancel(seq.id, "LeanSeqGUIAnimator " + base.gameObject.name);
		}
		seq = null;
	}

	private void OnDisable()
	{
		Stop();
	}
}
