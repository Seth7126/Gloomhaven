using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Compendium/Section")]
public class CompendiumSection : ScriptableObject
{
	public string Title;

	[HideInInspector]
	public Guid Id = Guid.NewGuid();

	public List<CompendiumSubsection> Subsections;

	public bool HasInformation()
	{
		if (Subsections.Count > 0)
		{
			return Subsections.Any((CompendiumSubsection it) => !it.IsEmpty());
		}
		return false;
	}
}
