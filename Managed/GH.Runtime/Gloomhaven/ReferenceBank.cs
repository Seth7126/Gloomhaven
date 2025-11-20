using UnityEngine;

namespace Gloomhaven;

public class ReferenceBank : Singleton<ReferenceBank>
{
	[SerializeField]
	private GameObject scrollBarPrefab;

	[SerializeField]
	private Sprite maskSprite;

	public GameObject ScrollBarPrefab => scrollBarPrefab;

	public Sprite MaskSprite => maskSprite;
}
