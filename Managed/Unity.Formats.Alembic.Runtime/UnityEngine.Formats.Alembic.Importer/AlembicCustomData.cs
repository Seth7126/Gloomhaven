using System.Collections.Generic;

namespace UnityEngine.Formats.Alembic.Importer;

public class AlembicCustomData : MonoBehaviour
{
	[SerializeField]
	private List<string> faceSetNames;

	public List<string> FaceSetNames => faceSetNames;

	internal void SetFacesetNames(List<string> names)
	{
		faceSetNames = names;
		for (int i = 0; i < faceSetNames.Count; i++)
		{
			faceSetNames[i] = faceSetNames[i].TrimEnd('\0');
		}
	}
}
