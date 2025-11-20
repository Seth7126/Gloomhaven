using System.Collections.Generic;
using UnityEngine;

public abstract class MonoLabelProvider : MonoBehaviour, ILabelProvider
{
	public abstract IEnumerable<string> GetLabels();
}
