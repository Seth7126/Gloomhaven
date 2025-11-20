using System.Collections.Generic;
using UnityEngine;

namespace Script.Optimization;

public class MaterialLoaderTest : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _particlesToTest;

	[SerializeField]
	private GameObject _selectedParticlePrefab;

	[SerializeField]
	private string _rootPath = "Assets/Content/VFX";

	private GameObject _currentObject;

	private int _currentIndex;

	public void StartTest()
	{
		InstantiateParticle();
	}

	public void RestartParticle()
	{
		if (!(_currentObject == null))
		{
			_currentObject.GetComponent<ParticleSystem>().Stop();
			_currentObject.GetComponent<ParticleSystem>().Play();
		}
	}

	public void RecreateParticle()
	{
		if (!(_currentObject == null))
		{
			InstantiateParticle();
		}
	}

	private void InstantiateParticle()
	{
		if (_currentObject != null)
		{
			Object.DestroyImmediate(_currentObject);
		}
		_currentObject = Object.Instantiate((_selectedParticlePrefab == null) ? _particlesToTest[_currentIndex] : _selectedParticlePrefab, Vector3.zero, Quaternion.identity);
	}

	public void Next()
	{
		if (!(_selectedParticlePrefab != null))
		{
			_currentIndex++;
			if (_currentIndex >= _particlesToTest.Count)
			{
				_currentIndex = _particlesToTest.Count - 1;
			}
			InstantiateParticle();
		}
	}

	public void Previous()
	{
		if (!(_selectedParticlePrefab != null))
		{
			_currentIndex--;
			if (_currentIndex < 0)
			{
				_currentIndex = 0;
			}
			InstantiateParticle();
		}
	}
}
