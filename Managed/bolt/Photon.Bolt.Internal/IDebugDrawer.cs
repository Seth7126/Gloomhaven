using UnityEngine;

namespace Photon.Bolt.Internal;

public interface IDebugDrawer
{
	void IsEditor(bool isEditor);

	void Indent(int adjust);

	void Label(string text);

	void LabelBold(string text);

	void LabelField(string text, object value);

	void DrawObjectArray(IDebugDrawerObjectArray array);

	void Separator();

	void SelectGameObject(GameObject gameObject);
}
