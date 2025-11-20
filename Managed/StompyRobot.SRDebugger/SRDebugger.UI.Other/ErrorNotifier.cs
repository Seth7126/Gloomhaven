using UnityEngine;

namespace SRDebugger.UI.Other;

public class ErrorNotifier : MonoBehaviour
{
	private const float DisplayTime = 6f;

	[SerializeField]
	private Animator _animator;

	private int _triggerHash;

	private float _hideTime;

	private bool _isShowing;

	private bool _queueWarning;

	public bool IsVisible => _isShowing;

	private void Awake()
	{
		_triggerHash = Animator.StringToHash("Display");
	}

	public void ShowErrorWarning()
	{
		_queueWarning = true;
	}

	private void Update()
	{
		if (_queueWarning)
		{
			_hideTime = Time.realtimeSinceStartup + 6f;
			if (!_isShowing)
			{
				_isShowing = true;
				_animator.SetBool(_triggerHash, value: true);
			}
			_queueWarning = false;
		}
		if (_isShowing && Time.realtimeSinceStartup > _hideTime)
		{
			_animator.SetBool(_triggerHash, value: false);
			_isShowing = false;
		}
	}
}
