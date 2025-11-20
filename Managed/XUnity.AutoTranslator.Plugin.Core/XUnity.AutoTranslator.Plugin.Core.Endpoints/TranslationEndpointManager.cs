using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

internal class TranslationEndpointManager
{
	private class PriorTranslation
	{
		public string UntranslatedText { get; set; }

		public float Time { get; set; }
	}

	private List<PriorTranslation> _priorTranslations;

	private Dictionary<string, byte> _failedTranslations;

	private Dictionary<UntranslatedText, TranslationJob> _unstartedJobs;

	private Dictionary<UntranslatedText, TranslationJob> _ongoingJobs;

	private int _ongoingTranslations;

	private Dictionary<string, string> _translations;

	private Dictionary<string, string> _reverseTranslations;

	public TranslationManager Manager { get; set; }

	public ITranslateEndpoint Endpoint { get; }

	public Exception Error { get; }

	public bool IsBusy => _ongoingTranslations >= Endpoint.MaxConcurrency;

	public bool HasBatchLogicFailed { get; set; }

	public int AvailableBatchOperations { get; set; }

	public int ConsecutiveErrors { get; set; }

	public bool CanBatch
	{
		get
		{
			if (Endpoint.MaxTranslationsPerRequest > 1 && _unstartedJobs.Count > 1 && !HasBatchLogicFailed)
			{
				return AvailableBatchOperations > 0;
			}
			return false;
		}
	}

	public bool HasUnstartedBatch
	{
		get
		{
			if (_unstartedJobs.Count > 0)
			{
				return AvailableBatchOperations > 0;
			}
			return false;
		}
	}

	public bool HasUnstartedJob => _unstartedJobs.Count > 0;

	public bool HasFailedDueToConsecutiveErrors => ConsecutiveErrors >= Settings.MaxErrors;

	public bool EnableSpamChecks { get; set; } = true;

	public float TranslationDelay { get; set; } = Settings.DefaultTranslationDelay;

	public int MaxRetries { get; set; } = Settings.DefaultMaxRetries;

	public TranslationEndpointManager(ITranslateEndpoint endpoint, Exception error, InitializationContext context)
	{
		Endpoint = endpoint;
		Error = error;
		_ongoingTranslations = 0;
		_failedTranslations = new Dictionary<string, byte>();
		_unstartedJobs = new Dictionary<UntranslatedText, TranslationJob>();
		_ongoingJobs = new Dictionary<UntranslatedText, TranslationJob>();
		_priorTranslations = new List<PriorTranslation>();
		_translations = new Dictionary<string, string>();
		_reverseTranslations = new Dictionary<string, string>();
		HasBatchLogicFailed = false;
		AvailableBatchOperations = Settings.MaxAvailableBatchOperations;
		EnableSpamChecks = context.SpamChecksEnabled;
		TranslationDelay = context.TranslationDelay;
		MaxRetries = (int)(60f / context.TranslationDelay);
		if (MaxRetries < 3)
		{
			MaxRetries = 3;
		}
	}

	public bool TryGetTranslation(UntranslatedText key, out string value)
	{
		bool flag;
		if (key.IsTemplated && !key.IsFromSpammingComponent)
		{
			string key2 = key.Untemplate(key.TemplatedOriginal_Text);
			flag = _translations.TryGetValue(key2, out value);
			if (flag)
			{
				return flag;
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
			{
				key2 = key.Untemplate(key.TemplatedOriginal_Text_ExternallyTrimmed);
				flag = _translations.TryGetValue(key2, out value);
				if (flag)
				{
					string text = key.LeadingWhitespace + value + key.TrailingWhitespace;
					string key3 = key.Untemplate(key.TemplatedOriginal_Text);
					AddTranslationToCache(key3, text);
					value = text;
					return flag;
				}
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
			{
				key2 = key.Untemplate(key.TemplatedOriginal_Text_InternallyTrimmed);
				flag = _translations.TryGetValue(key2, out value);
				if (flag)
				{
					string text = value;
					string key3 = key.Untemplate(key.TemplatedOriginal_Text);
					AddTranslationToCache(key3, text);
					value = text;
					return flag;
				}
			}
			if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
			{
				key2 = key.Untemplate(key.TemplatedOriginal_Text_FullyTrimmed);
				flag = _translations.TryGetValue(key2, out value);
				if (flag)
				{
					string text = key.LeadingWhitespace + value + key.TrailingWhitespace;
					string key3 = key.Untemplate(key.TemplatedOriginal_Text);
					AddTranslationToCache(key3, text);
					value = text;
					return flag;
				}
			}
		}
		flag = _translations.TryGetValue(key.TemplatedOriginal_Text, out value);
		if (flag)
		{
			return flag;
		}
		if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
		{
			flag = _translations.TryGetValue(key.TemplatedOriginal_Text_ExternallyTrimmed, out value);
			if (flag)
			{
				string text = key.LeadingWhitespace + value + key.TrailingWhitespace;
				AddTranslationToCache(key.TemplatedOriginal_Text, text);
				value = text;
				return flag;
			}
		}
		if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
		{
			flag = _translations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value);
			if (flag)
			{
				AddTranslationToCache(key.TemplatedOriginal_Text, value);
				return flag;
			}
		}
		if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
		{
			flag = _translations.TryGetValue(key.TemplatedOriginal_Text_FullyTrimmed, out value);
			if (flag)
			{
				string text = key.LeadingWhitespace + value + key.TrailingWhitespace;
				AddTranslationToCache(key.TemplatedOriginal_Text, text);
				value = text;
				return flag;
			}
		}
		return flag;
	}

	private void AddTranslation(string key, string value)
	{
		if (key != null && value != null)
		{
			_translations[key] = value;
			_reverseTranslations[value] = key;
		}
	}

	private void QueueNewTranslationForDisk(string key, string value)
	{
	}

	public void AddTranslationToCache(string key, string value)
	{
		if (!HasTranslated(key))
		{
			AddTranslation(key, value);
			UntranslatedText untranslatedText = new UntranslatedText(key, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
			UntranslatedText untranslatedText2 = new UntranslatedText(value, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.ToLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
			if (untranslatedText.Original_Text_ExternallyTrimmed != key && !HasTranslated(untranslatedText.Original_Text_ExternallyTrimmed))
			{
				AddTranslation(untranslatedText.Original_Text_ExternallyTrimmed, untranslatedText2.Original_Text_ExternallyTrimmed);
			}
			if (untranslatedText.Original_Text_ExternallyTrimmed != untranslatedText.Original_Text_FullyTrimmed && !HasTranslated(untranslatedText.Original_Text_FullyTrimmed))
			{
				AddTranslation(untranslatedText.Original_Text_FullyTrimmed, untranslatedText2.Original_Text_FullyTrimmed);
			}
			QueueNewTranslationForDisk(key, value);
		}
	}

	public bool IsTranslatable(string text)
	{
		return !IsTranslation(text);
	}

	private bool IsTranslation(string translation)
	{
		if (!HasTranslated(translation))
		{
			return _reverseTranslations.ContainsKey(translation);
		}
		return false;
	}

	private bool HasTranslated(string key)
	{
		return _translations.ContainsKey(key);
	}

	private string GetTextToTranslate(TranslationJob job)
	{
		string text = ((!Settings.IgnoreWhitespaceInDialogue || job.Key.Original_Text.Length <= Settings.MinDialogueChars) ? job.Key.TemplatedOriginal_Text_ExternallyTrimmed : job.Key.TemplatedOriginal_Text_FullyTrimmed);
		return PreProcessUntranslatedText(text);
	}

	public UntranslatedTextInfo PrepareUntranslatedText(string unpreparedUntranslatedText, TranslationJob job)
	{
		string untranslatedText = job.Key.PrepareUntranslatedText(unpreparedUntranslatedText);
		UntranslatedTextInfo untranslatedTextInfo = job.UntranslatedTextInfo;
		if (untranslatedTextInfo != null)
		{
			return new UntranslatedTextInfo(untranslatedText, untranslatedTextInfo.ContextBefore, untranslatedTextInfo.ContextAfter);
		}
		return new UntranslatedTextInfo(untranslatedText);
	}

	public void HandleNextBatch()
	{
		try
		{
			List<KeyValuePair<UntranslatedText, TranslationJob>> list = _unstartedJobs.Take(Endpoint.MaxTranslationsPerRequest).ToList();
			List<UntranslatedTextInfo> list2 = new List<UntranslatedTextInfo>();
			List<TranslationJob> list3 = new List<TranslationJob>();
			foreach (KeyValuePair<UntranslatedText, TranslationJob> item2 in list)
			{
				UntranslatedText key = item2.Key;
				TranslationJob value = item2.Value;
				_unstartedJobs.Remove(key);
				Manager.UnstartedTranslations--;
				if (value.IsTranslatable)
				{
					string textToTranslate = GetTextToTranslate(value);
					UntranslatedTextInfo item = PrepareUntranslatedText(textToTranslate, value);
					if (CanTranslate(textToTranslate))
					{
						list3.Add(value);
						list2.Add(item);
						_ongoingJobs[key] = value;
						Manager.OngoingTranslations++;
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Debug("Started: '" + textToTranslate + "'");
						}
					}
					else
					{
						XuaLogger.AutoTranslator.Warn("Dequeued: '" + textToTranslate + "' because the current endpoint has already failed this translation 3 times.");
						value.State = TranslationJobState.Failed;
						value.ErrorMessage = "The endpoint failed to perform this translation 3 or more times.";
						InvokeJobFailedWithFallbaack(value);
					}
				}
				else
				{
					_ongoingJobs[key] = value;
					Manager.OngoingTranslations++;
					OnSingleTranslationCompleted(value, new string[1] { key.TemplatedOriginal_Text_ExternallyTrimmed }, useTranslatorFriendlyArgs: false);
				}
			}
			if (list3.Count > 0)
			{
				AvailableBatchOperations--;
				TranslationJob[] jobsArray = list3.ToArray();
				CoroutineHelper.Start(Translate(list2.ToArray(), Settings.FromLanguage, Settings.Language, delegate(string[] translatedText)
				{
					OnBatchTranslationCompleted(jobsArray, translatedText);
				}, delegate(string msg, Exception e)
				{
					OnTranslationFailed(jobsArray, msg, e);
				}));
			}
		}
		finally
		{
			if (_unstartedJobs.Count == 0)
			{
				Manager.UnscheduleUnstartedJobs(this);
			}
		}
	}

	public void HandleNextJob()
	{
		try
		{
			KeyValuePair<UntranslatedText, TranslationJob> keyValuePair = _unstartedJobs.FirstOrDefault();
			UntranslatedText key = keyValuePair.Key;
			TranslationJob job = keyValuePair.Value;
			_unstartedJobs.Remove(key);
			Manager.UnstartedTranslations--;
			if (job.IsTranslatable)
			{
				string textToTranslate = GetTextToTranslate(job);
				UntranslatedTextInfo untranslatedTextInfo = PrepareUntranslatedText(textToTranslate, job);
				if (CanTranslate(textToTranslate))
				{
					_ongoingJobs[key] = job;
					Manager.OngoingTranslations++;
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Debug("Started: '" + textToTranslate + "'");
					}
					CoroutineHelper.Start(Translate(new UntranslatedTextInfo[1] { untranslatedTextInfo }, Settings.FromLanguage, Settings.Language, delegate(string[] translatedText)
					{
						OnSingleTranslationCompleted(job, translatedText, useTranslatorFriendlyArgs: true);
					}, delegate(string msg, Exception e)
					{
						OnTranslationFailed(new TranslationJob[1] { job }, msg, e);
					}));
				}
				else
				{
					XuaLogger.AutoTranslator.Warn("Dequeued: '" + textToTranslate + "' because the current endpoint has already failed this translation 3 times.");
					job.State = TranslationJobState.Failed;
					job.ErrorMessage = "The endpoint failed to perform this translation 3 or more times.";
					InvokeJobFailedWithFallbaack(job);
				}
			}
			else
			{
				_ongoingJobs[key] = job;
				Manager.OngoingTranslations++;
				OnSingleTranslationCompleted(job, new string[1] { key.TemplatedOriginal_Text_ExternallyTrimmed }, useTranslatorFriendlyArgs: false);
			}
		}
		finally
		{
			if (_unstartedJobs.Count == 0)
			{
				Manager.UnscheduleUnstartedJobs(this);
			}
		}
	}

	private void OnBatchTranslationCompleted(TranslationJob[] jobs, string[] translatedTexts)
	{
		ConsecutiveErrors = 0;
		if (jobs.Length == translatedTexts.Length)
		{
			for (int i = 0; i < jobs.Length; i++)
			{
				TranslationJob translationJob = jobs[i];
				string translatedText = translatedTexts[i];
				translationJob.TranslatedText = PostProcessTranslation(translationJob.Key, translatedText, useTranslatorFriendlyArgs: true);
				translationJob.State = TranslationJobState.Succeeded;
				RemoveOngoingTranslation(translationJob.Key);
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Info("Completed: '" + translationJob.Key.TemplatedOriginal_Text + "' => '" + translationJob.TranslatedText + "'");
				}
				Manager.InvokeJobCompleted(translationJob);
			}
		}
		else
		{
			if (!HasBatchLogicFailed)
			{
				CoroutineHelper.Start(EnableBatchingAfterDelay());
			}
			HasBatchLogicFailed = true;
			foreach (TranslationJob translationJob2 in jobs)
			{
				UntranslatedText key = translationJob2.Key;
				AddUnstartedJob(key, translationJob2);
				RemoveOngoingTranslation(key);
			}
			XuaLogger.AutoTranslator.Error("A batch operation failed. Disabling batching and restarting failed jobs.");
		}
	}

	private void OnSingleTranslationCompleted(TranslationJob job, string[] translatedTexts, bool useTranslatorFriendlyArgs)
	{
		string translatedText = translatedTexts[0];
		ConsecutiveErrors = 0;
		job.TranslatedText = PostProcessTranslation(job.Key, translatedText, useTranslatorFriendlyArgs);
		job.State = TranslationJobState.Succeeded;
		RemoveOngoingTranslation(job.Key);
		if (!Settings.EnableSilentMode)
		{
			XuaLogger.AutoTranslator.Info("Completed: '" + job.Key.TemplatedOriginal_Text + "' => '" + job.TranslatedText + "'");
		}
		Manager.InvokeJobCompleted(job);
	}

	private string PostProcessTranslation(UntranslatedText key, string translatedText, bool useTranslatorFriendlyArgs)
	{
		if (!string.IsNullOrEmpty(translatedText))
		{
			translatedText = key.FixTranslatedText(translatedText, useTranslatorFriendlyArgs);
			translatedText = key.LeadingWhitespace + translatedText + key.TrailingWhitespace;
			if (Settings.Language == Settings.Romaji && Settings.RomajiPostProcessing != TextPostProcessing.None)
			{
				translatedText = RomanizationHelper.PostProcess(translatedText, Settings.RomajiPostProcessing);
			}
			else if (Settings.TranslationPostProcessing != TextPostProcessing.None)
			{
				translatedText = RomanizationHelper.PostProcess(translatedText, Settings.TranslationPostProcessing);
			}
			foreach (KeyValuePair<string, string> postprocessor in Settings.Postprocessors)
			{
				translatedText = translatedText.Replace(postprocessor.Key, postprocessor.Value);
			}
			if (Settings.ForceSplitTextAfterCharacters > 0)
			{
				translatedText = translatedText.SplitToLines(Settings.ForceSplitTextAfterCharacters, '\n', ' ', '\u3000');
			}
		}
		return translatedText;
	}

	private string PreProcessUntranslatedText(string text)
	{
		if (Settings.HtmlEntityPreprocessing)
		{
			text = WebUtility.HtmlDecode(text);
		}
		if (Settings.Preprocessors.Count == 0)
		{
			return text;
		}
		foreach (KeyValuePair<string, string> preprocessor in Settings.Preprocessors)
		{
			text = text.Replace(preprocessor.Key, preprocessor.Value);
		}
		return text;
	}

	private void OnTranslationFailed(TranslationJob[] jobs, string error, Exception e)
	{
		if (e == null)
		{
			XuaLogger.AutoTranslator.Error(error);
		}
		else
		{
			XuaLogger.AutoTranslator.Error(e, error);
		}
		if (jobs.Length == 1)
		{
			foreach (TranslationJob translationJob in jobs)
			{
				UntranslatedText key = translationJob.Key;
				translationJob.State = TranslationJobState.Failed;
				translationJob.ErrorMessage = error;
				RemoveOngoingTranslation(key);
				RegisterTranslationFailureFor(key.TemplatedOriginal_Text);
				XuaLogger.AutoTranslator.Error("Failed: '" + translationJob.Key.TemplatedOriginal_Text + "'");
				InvokeJobFailedWithFallbaack(translationJob);
			}
		}
		else
		{
			if (!HasBatchLogicFailed)
			{
				CoroutineHelper.Start(EnableBatchingAfterDelay());
			}
			HasBatchLogicFailed = true;
			foreach (TranslationJob translationJob2 in jobs)
			{
				UntranslatedText key2 = translationJob2.Key;
				AddUnstartedJob(key2, translationJob2);
				RemoveOngoingTranslation(key2);
				XuaLogger.AutoTranslator.Error("Failed: '" + translationJob2.Key.TemplatedOriginal_Text + "'");
			}
			XuaLogger.AutoTranslator.Error("A batch operation failed. Disabling batching and restarting failed jobs.");
		}
		if (!HasFailedDueToConsecutiveErrors)
		{
			ConsecutiveErrors++;
			if (HasFailedDueToConsecutiveErrors)
			{
				XuaLogger.AutoTranslator.Error($"{Settings.MaxErrors} or more consecutive errors occurred. Shutting down translator endpoint.");
				ClearAllJobs();
			}
		}
	}

	private void InvokeJobFailedWithFallbaack(TranslationJob job)
	{
		if (job.AllowFallback && Manager.IsFallbackAvailableFor(this) && !Manager.FallbackEndpoint.HasFailedDueToConsecutiveErrors)
		{
			XuaLogger.AutoTranslator.Warn("Retrying translation for '" + job.Key.TemplatedOriginal_Text + "' against fallback endpoint instead.");
			job.Endpoint = Manager.FallbackEndpoint;
			Manager.FallbackEndpoint.AddUnstartedJob(job.Key, job);
		}
		else
		{
			Manager.InvokeJobFailed(job);
		}
	}

	private IEnumerator EnableBatchingAfterDelay()
	{
		yield return CoroutineHelper.CreateWaitForSeconds(60f);
		HasBatchLogicFailed = false;
		XuaLogger.AutoTranslator.Info("Re-enabled batching.");
	}

	public TranslationJob EnqueueTranslation(object ui, UntranslatedText key, InternalTranslationResult translationResult, ParserTranslationContext context, UntranslatedTextInfo untranslatedTextContext, bool checkOtherEndpoints, bool saveResultGlobally, bool isTranslatable, bool allowFallback)
	{
		if (AssociateWithExistingJobIfPossible(ui, key, translationResult, context, untranslatedTextContext, saveResultGlobally, allowFallback))
		{
			return null;
		}
		if (checkOtherEndpoints)
		{
			List<TranslationEndpointManager> configuredEndpoints = Manager.ConfiguredEndpoints;
			int count = configuredEndpoints.Count;
			for (int i = 0; i < count; i++)
			{
				TranslationEndpointManager translationEndpointManager = configuredEndpoints[i];
				if (translationEndpointManager != this && translationEndpointManager.AssociateWithExistingJobIfPossible(ui, key, translationResult, context, untranslatedTextContext, saveResultGlobally, allowFallback))
				{
					return null;
				}
			}
		}
		if (!Settings.EnableSilentMode)
		{
			XuaLogger.AutoTranslator.Debug("Queued: '" + key.TemplatedOriginal_Text + "'");
		}
		TranslationJob translationJob = new TranslationJob(this, key, saveResultGlobally, isTranslatable);
		translationJob.Associate(key, ui, translationResult, context, untranslatedTextContext, saveResultGlobally, allowFallback);
		return AddUnstartedJob(key, translationJob);
	}

	private bool AssociateWithExistingJobIfPossible(object ui, UntranslatedText key, InternalTranslationResult translationResult, ParserTranslationContext context, UntranslatedTextInfo untranslatedTextContext, bool saveResultGlobally, bool allowFallback)
	{
		if (_unstartedJobs.TryGetValue(key, out var value))
		{
			value.Associate(key, ui, translationResult, context, untranslatedTextContext, saveResultGlobally, allowFallback);
			return true;
		}
		if (_ongoingJobs.TryGetValue(key, out var value2))
		{
			value2.Associate(key, ui, translationResult, context, untranslatedTextContext, saveResultGlobally, allowFallback);
			return true;
		}
		return false;
	}

	private TranslationJob AddUnstartedJob(UntranslatedText key, TranslationJob job)
	{
		if (!_unstartedJobs.ContainsKey(key))
		{
			int count = _unstartedJobs.Count;
			_unstartedJobs.Add(key, job);
			Manager.UnstartedTranslations++;
			if (count == 0)
			{
				Manager.ScheduleUnstartedJobs(this);
			}
			return job;
		}
		return null;
	}

	private void RemoveOngoingTranslation(UntranslatedText key)
	{
		if (_ongoingJobs.Remove(key))
		{
			Manager.OngoingTranslations--;
		}
	}

	public void ClearAllJobs()
	{
		int count = _ongoingJobs.Count;
		int count2 = _unstartedJobs.Count;
		List<KeyValuePair<UntranslatedText, TranslationJob>> list = _unstartedJobs.ToList();
		_ongoingJobs.Clear();
		_unstartedJobs.Clear();
		foreach (KeyValuePair<UntranslatedText, TranslationJob> item in list)
		{
			XuaLogger.AutoTranslator.Warn("Dequeued: '" + item.Key.TemplatedOriginal_Text + "'");
			item.Value.State = TranslationJobState.Failed;
			item.Value.ErrorMessage = "Translation failed because all jobs on endpoint was cleared.";
			Manager.InvokeJobFailed(item.Value);
		}
		Manager.OngoingTranslations -= count;
		Manager.UnstartedTranslations -= count2;
		Manager.UnscheduleUnstartedJobs(this);
	}

	private bool CanTranslate(string untranslatedText)
	{
		if (_failedTranslations.TryGetValue(untranslatedText, out var value))
		{
			return value < Settings.MaxFailuresForSameTextPerEndpoint;
		}
		return true;
	}

	private void RegisterTranslationFailureFor(string untranslatedText)
	{
		byte value = (byte)((!_failedTranslations.TryGetValue(untranslatedText, out value)) ? 1 : ((byte)(value + 1)));
		_failedTranslations[untranslatedText] = value;
	}

	public IEnumerator Translate(UntranslatedTextInfo[] untranslatedTextInfos, string from, string to, Action<string[]> success, Action<string, Exception> failure)
	{
		float startTime = Time.realtimeSinceStartup;
		TranslationContext context = new TranslationContext(untranslatedTextInfos, from, to, success, failure);
		_ongoingTranslations++;
		try
		{
			if (Settings.SimulateDelayedError)
			{
				yield return CoroutineHelper.CreateWaitForSeconds(1f);
				context.FailWithoutThrowing("Simulating delayed error. Press CTRL+ALT+NP8 to disable!", null);
				yield break;
			}
			if (Settings.SimulateError)
			{
				context.FailWithoutThrowing("Simulating error. Press CTRL+ALT+NP9 to disable!", null);
				yield break;
			}
			IEnumerator iterator = Endpoint.Translate(context);
			if (iterator == null)
			{
				yield break;
			}
			while (true)
			{
				bool flag;
				try
				{
					flag = iterator.MoveNext();
					if (Time.realtimeSinceStartup - startTime > Settings.Timeout)
					{
						flag = false;
						context.FailWithoutThrowing($"Timeout occurred during translation (took more than {Settings.Timeout} seconds)", null);
					}
				}
				catch (TranslationContextException)
				{
					flag = false;
				}
				catch (Exception exception)
				{
					flag = false;
					context.FailWithoutThrowing("Error occurred during translation.", exception);
				}
				if (flag)
				{
					yield return iterator.Current;
					continue;
				}
				break;
			}
		}
		finally
		{
			_ongoingTranslations--;
			context.FailIfNotCompleted();
		}
	}
}
