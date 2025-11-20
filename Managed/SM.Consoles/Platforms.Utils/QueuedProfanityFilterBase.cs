using System;
using System.Collections.Generic;
using Platforms.Profanity;

namespace Platforms.Utils;

public abstract class QueuedProfanityFilterBase : IPlatformProfanity
{
	private struct QueueItem
	{
		public string Text;

		public Action<OperationResult, bool> CheckCallback;

		public Action<OperationResult, string> MaskCallback;
	}

	private readonly Queue<QueueItem> _queue = new Queue<QueueItem>();

	private QueueItem? _current;

	protected abstract void RunMaskCheckAsync(string text, Action<OperationResult, string> resultCallback);

	public void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback)
	{
		if (string.IsNullOrEmpty(text))
		{
			resultCallback(OperationResult.Success, arg2: false);
		}
		else
		{
			EnqueueCheck(text, resultCallback);
		}
	}

	public void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback)
	{
		if (string.IsNullOrEmpty(text))
		{
			resultCallback(OperationResult.Success, text);
		}
		else
		{
			EnqueueMask(text, resultCallback);
		}
	}

	private void EnqueueCheck(string text, Action<OperationResult, bool> check)
	{
		_queue.Enqueue(new QueueItem
		{
			Text = text,
			CheckCallback = check
		});
		RunNextItem();
	}

	private void EnqueueMask(string text, Action<OperationResult, string> mask)
	{
		_queue.Enqueue(new QueueItem
		{
			Text = text,
			MaskCallback = mask
		});
		RunNextItem();
	}

	private void RunNextItem()
	{
		if (!_current.HasValue && _queue.Count > 0)
		{
			_current = _queue.Dequeue();
			RunMaskCheckAsync(_current.Value.Text, RunCheckCompleted);
		}
		void RunCheckCompleted(OperationResult result, string filteredText)
		{
			QueueItem value = _current.Value;
			RunCallbacks(value, result, filteredText);
		}
	}

	private void RunCallbacks(QueueItem item, OperationResult result, string filteredText)
	{
		_current = null;
		if (item.CheckCallback != null)
		{
			item.CheckCallback(result, item.Text != filteredText);
		}
		else
		{
			item.MaskCallback(result, filteredText);
		}
		RunNextItem();
	}
}
