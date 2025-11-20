using System;
using System.Collections.Generic;
using Platforms.Profanity;

namespace Platforms.Utils;

public class ProfanityFilterCache : IPlatformProfanity
{
	public class Request
	{
		private bool _isExecutionStarted;

		private readonly List<Action> _completeActions;

		public string Text { get; }

		public string FilteredText { get; private set; }

		public OperationResult? Result { get; private set; }

		public bool IsMasked => Text != FilteredText;

		public bool IsExecuting
		{
			get
			{
				if (_isExecutionStarted)
				{
					return !Result.HasValue;
				}
				return false;
			}
		}

		public bool IsSuccess
		{
			get
			{
				OperationResult? result = Result;
				if (result.HasValue)
				{
					return result.GetValueOrDefault() == OperationResult.Success;
				}
				return false;
			}
		}

		public Request(string text)
		{
			Text = text;
			_completeActions = new List<Action>();
		}

		public void OnExecute()
		{
			FilteredText = string.Empty;
			Result = null;
			_isExecutionStarted = true;
		}

		public void SetResult(OperationResult result, string filteredText)
		{
			Result = result;
			FilteredText = filteredText;
			foreach (Action completeAction in _completeActions)
			{
				completeAction?.Invoke();
			}
			_completeActions.Clear();
		}

		public void AddOnComplete(Action action)
		{
			_completeActions.Add(action);
		}
	}

	private readonly IPlatformProfanity _filter;

	private readonly Dictionary<string, Request> _requestsCache = new Dictionary<string, Request>();

	public ProfanityFilterCache(IPlatformProfanity filter)
	{
		_filter = filter;
	}

	public void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback)
	{
		if (string.IsNullOrEmpty(text))
		{
			resultCallback?.Invoke(OperationResult.Success, arg2: false);
			return;
		}
		Request request = GetRequest(text);
		if (request.IsSuccess)
		{
			InvokeCheckCallback(resultCallback, request);
			return;
		}
		request.AddOnComplete(delegate
		{
			InvokeCheckCallback(resultCallback, request);
		});
		TryExecuteRequest(request);
	}

	public void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback)
	{
		if (string.IsNullOrEmpty(text))
		{
			resultCallback?.Invoke(OperationResult.Success, text);
			return;
		}
		Request request = GetRequest(text);
		if (request.IsSuccess)
		{
			InvokeMaskCallback(resultCallback, request);
			return;
		}
		request.AddOnComplete(delegate
		{
			InvokeMaskCallback(resultCallback, request);
		});
		TryExecuteRequest(request);
	}

	private static void InvokeCheckCallback(Action<OperationResult, bool> resultCallback, Request request)
	{
		resultCallback?.Invoke(request.Result.Value, request.IsMasked);
	}

	private static void InvokeMaskCallback(Action<OperationResult, string> resultCallback, Request request)
	{
		resultCallback?.Invoke(request.Result.Value, request.FilteredText);
	}

	private void TryExecuteRequest(Request request)
	{
		if (!request.IsExecuting)
		{
			request.OnExecute();
			_filter.MaskBadWordsAsync(request.Text, delegate(OperationResult opRes, string filteredText)
			{
				request.SetResult(opRes, filteredText);
			});
		}
	}

	private Request GetRequest(string text)
	{
		if (!_requestsCache.TryGetValue(text, out var value))
		{
			value = new Request(text);
			_requestsCache.Add(text, value);
		}
		return value;
	}
}
