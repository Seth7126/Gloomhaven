using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class SpamChecker
{
	private int[] _currentTranslationsQueuedPerSecondRollingWindow = new int[Settings.TranslationQueueWatchWindow];

	private float? _timeExceededThreshold;

	private float _translationsQueuedPerSecond;

	private string[] _previouslyQueuedText = new string[Settings.PreviousTextStaggerCount];

	private int _staggerTextCursor;

	private int _concurrentStaggers;

	private int _lastStaggerCheckFrame = -1;

	private int _frameForLastQueuedTranslation = -1;

	private int _consecutiveFramesTranslated;

	private int _secondForQueuedTranslation = -1;

	private int _consecutiveSecondsTranslated;

	private TranslationManager _translationManager;

	public SpamChecker(TranslationManager translationManager)
	{
		_translationManager = translationManager;
	}

	public void PerformChecks(string untranslatedText, TranslationEndpointManager endpoint)
	{
		CheckStaggerText(untranslatedText);
		CheckConsecutiveFrames();
		CheckConsecutiveSeconds(endpoint);
		CheckThresholds(endpoint);
	}

	public void Update()
	{
		PeriodicResetFrameCheck();
		ResetThresholdTimerIfRequired();
	}

	private void CheckConsecutiveSeconds(TranslationEndpointManager endpoint)
	{
		int num = (int)Time.time;
		if (num - 1 == _secondForQueuedTranslation)
		{
			_consecutiveSecondsTranslated++;
			if (_consecutiveSecondsTranslated > Settings.MaximumConsecutiveSecondsTranslated && endpoint.EnableSpamChecks)
			{
				_translationManager.ClearAllJobs();
				Settings.IsShutdown = true;
				XuaLogger.AutoTranslator.Error($"SPAM DETECTED: Translations were queued every second for more than {Settings.MaximumConsecutiveSecondsTranslated} consecutive seconds. Shutting down plugin.");
			}
		}
		else if (num != _secondForQueuedTranslation)
		{
			_consecutiveSecondsTranslated = 0;
		}
		_secondForQueuedTranslation = num;
	}

	private void CheckConsecutiveFrames()
	{
		int frameCount = Time.frameCount;
		if (frameCount - 1 == _frameForLastQueuedTranslation)
		{
			_consecutiveFramesTranslated++;
			if (_consecutiveFramesTranslated > Settings.MaximumConsecutiveFramesTranslated)
			{
				_translationManager.ClearAllJobs();
				Settings.IsShutdown = true;
				XuaLogger.AutoTranslator.Error($"SPAM DETECTED: Translations were queued every frame for more than {Settings.MaximumConsecutiveFramesTranslated} consecutive frames. Shutting down plugin.");
			}
		}
		else if (frameCount != _frameForLastQueuedTranslation && _consecutiveFramesTranslated > 0)
		{
			_consecutiveFramesTranslated--;
		}
		_frameForLastQueuedTranslation = frameCount;
	}

	private void PeriodicResetFrameCheck()
	{
		if ((int)Time.time % 100 == 0)
		{
			_consecutiveFramesTranslated = 0;
		}
	}

	private void CheckStaggerText(string untranslatedText)
	{
		int frameCount = Time.frameCount;
		if (frameCount == _lastStaggerCheckFrame)
		{
			return;
		}
		_lastStaggerCheckFrame = frameCount;
		bool flag = false;
		for (int i = 0; i < _previouslyQueuedText.Length; i++)
		{
			string text = _previouslyQueuedText[i];
			if (text != null && untranslatedText.RemindsOf(text))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			_concurrentStaggers++;
			if (_concurrentStaggers > Settings.MaximumStaggers)
			{
				_translationManager.ClearAllJobs();
				Settings.IsShutdown = true;
				XuaLogger.AutoTranslator.Error("SPAM DETECTED: Text that is 'scrolling in' is being translated. Disable that feature. Shutting down plugin.");
			}
		}
		else
		{
			_concurrentStaggers = 0;
		}
		_previouslyQueuedText[_staggerTextCursor % _previouslyQueuedText.Length] = untranslatedText;
		_staggerTextCursor++;
	}

	private void CheckThresholds(TranslationEndpointManager endpoint)
	{
		if (_translationManager.UnstartedTranslations > Settings.MaxUnstartedJobs)
		{
			_translationManager.ClearAllJobs();
			Settings.IsShutdown = true;
			XuaLogger.AutoTranslator.Error($"SPAM DETECTED: More than {Settings.MaxUnstartedJobs} queued for translations due to unknown reasons. Shutting down plugin.");
		}
		float time = Time.time;
		int num = (int)(time - Time.deltaTime) % Settings.TranslationQueueWatchWindow;
		int num2 = (int)time % Settings.TranslationQueueWatchWindow;
		if (num != num2)
		{
			_currentTranslationsQueuedPerSecondRollingWindow[num2] = 0;
		}
		_currentTranslationsQueuedPerSecondRollingWindow[num2]++;
		int num3 = _currentTranslationsQueuedPerSecondRollingWindow.Sum();
		_translationsQueuedPerSecond = (float)num3 / (float)Settings.TranslationQueueWatchWindow;
		if (_translationsQueuedPerSecond > Settings.MaxTranslationsQueuedPerSecond)
		{
			if (!_timeExceededThreshold.HasValue)
			{
				_timeExceededThreshold = time;
			}
			if (time - _timeExceededThreshold.Value > (float)Settings.MaxSecondsAboveTranslationThreshold && endpoint.EnableSpamChecks)
			{
				_translationManager.ClearAllJobs();
				Settings.IsShutdown = true;
				XuaLogger.AutoTranslator.Error($"SPAM DETECTED: More than {Settings.MaxTranslationsQueuedPerSecond} translations per seconds queued for a {Settings.MaxSecondsAboveTranslationThreshold} second period. Shutting down plugin.");
			}
		}
		else
		{
			_timeExceededThreshold = null;
		}
	}

	private void ResetThresholdTimerIfRequired()
	{
		float time = Time.time;
		int num = (int)(time - Time.deltaTime) % Settings.TranslationQueueWatchWindow;
		int num2 = (int)time % Settings.TranslationQueueWatchWindow;
		if (num != num2)
		{
			_currentTranslationsQueuedPerSecondRollingWindow[num2] = 0;
		}
		int num3 = _currentTranslationsQueuedPerSecondRollingWindow.Sum();
		_translationsQueuedPerSecond = (float)num3 / (float)Settings.TranslationQueueWatchWindow;
		if (_translationsQueuedPerSecond <= Settings.MaxTranslationsQueuedPerSecond)
		{
			_timeExceededThreshold = null;
		}
	}
}
