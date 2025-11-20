using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Util;

internal abstract class ComponentCapturer
{
	public AlembicRecorder recorder;

	public ComponentCapturer parent;

	public aeObject abcObject;

	public int timeSamplingIndex;

	public abstract void Setup(Component c);

	public abstract void Capture();

	public void MarkForceInvisible()
	{
		abcObject.MarkForceInvisible();
	}
}
