using System;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.PartyDisplay;

[Serializable]
public class DimmingColorModel : IEquatable<DimmingColorModel>
{
	public Color BeforeDimming { get; }

	public Color AfterDimming { get; set; }

	public Graphic Graphic { get; }

	public DimmingColorModel(Graphic graphic, Color beforeDimming, Color afterDimming)
	{
		Graphic = graphic;
		BeforeDimming = beforeDimming;
		AfterDimming = afterDimming;
	}

	public bool Equals(DimmingColorModel other)
	{
		if (other != null)
		{
			return object.Equals(Graphic, other.Graphic);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((DimmingColorModel)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(BeforeDimming, Graphic);
	}
}
