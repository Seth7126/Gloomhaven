using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Mouse Cursor", 302)]
public class MouseCursor : MonoBehaviour
{
	[SerializeField]
	private Texture2D _texture;

	[SerializeField]
	private Vector2 _hotspotOffset = Vector2.zero;

	[SerializeField]
	[Range(1f, 16f)]
	private int _sizeScale = 1;

	[SerializeField]
	private int _depth = -9999;

	private GUIContent _content;

	private void Start()
	{
		SetTexture(_texture);
	}

	public void SetTexture(Texture2D texture)
	{
		if (texture != null)
		{
			_content = new GUIContent(texture);
			_texture = texture;
		}
	}

	private void OnGUI()
	{
		if (_content != null)
		{
			GUI.depth = _depth;
			Vector2 mousePosition = Event.current.mousePosition;
			GUI.Label(new Rect(mousePosition.x - _hotspotOffset.x, mousePosition.y - _hotspotOffset.y, _texture.width * _sizeScale, _texture.height * _sizeScale), _content);
		}
	}
}
