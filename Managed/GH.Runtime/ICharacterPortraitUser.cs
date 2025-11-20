using UnityEngine;

public interface ICharacterPortraitUser
{
	void UpdateTexture(Texture texture, Rect coords);

	void UnloadedTexture();
}
