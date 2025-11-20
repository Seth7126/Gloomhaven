using System;

namespace SpriteMemoryManagement;

public struct ReferenceToSpriteState : IEquatable<ReferenceToSpriteState>
{
	public ReferenceToSprite HighlightedSprite { get; set; }

	public ReferenceToSprite PressedSprite { get; set; }

	public ReferenceToSprite SelectedSprite { get; set; }

	public ReferenceToSprite DisabledSprite { get; set; }

	public bool Equals(ReferenceToSpriteState other)
	{
		if (other.HighlightedSprite == HighlightedSprite && other.PressedSprite == PressedSprite && other.SelectedSprite == SelectedSprite)
		{
			return other.DisabledSprite == DisabledSprite;
		}
		return false;
	}
}
