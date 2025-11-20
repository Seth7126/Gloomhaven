using Platforms.Activities;
using UnityEngine;

namespace Script.PlatformLayer;

public abstract class GhActivityBase : ScriptableObject, IActivityBase
{
	[SerializeField]
	private string _id;

	[SerializeField]
	private bool _saveIndependent;

	[SerializeField]
	private string[] _filterTags;

	public string ID
	{
		get
		{
			if (!_id.IsNullOrEmpty())
			{
				return _id;
			}
			return base.name;
		}
	}

	public bool SaveIndependent => _saveIndependent;

	public string[] FilterTags => _filterTags;
}
