using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class TranslationAggregatorViewModel
{
	private DebounceFunction _saveHeightAndWidth;

	private LinkedList<AggregatedTranslationViewModel> _translations;

	private LinkedListNode<AggregatedTranslationViewModel> _current;

	private List<Translation> _translationsToAggregate = new List<Translation>();

	private HashSet<string> _textsToAggregate = new HashSet<string>();

	private float _lastUpdate;

	private float _height;

	private float _width;

	public bool IsShown { get; set; }

	public bool IsShowingOptions { get; set; }

	public float Height
	{
		get
		{
			return _height;
		}
		set
		{
			if (_height != value)
			{
				_height = value;
				_saveHeightAndWidth.Execute();
			}
		}
	}

	public float Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (_width != value)
			{
				_width = value;
				_saveHeightAndWidth.Execute();
			}
		}
	}

	public List<TranslatorViewModel> AvailableTranslators { get; }

	public List<TranslatorViewModel> AllTranslators { get; }

	public TranslationManager Manager { get; set; }

	public AggregatedTranslationViewModel Current => _current?.Value;

	public TranslationAggregatorViewModel(TranslationManager translationManager)
	{
		_translations = new LinkedList<AggregatedTranslationViewModel>();
		_saveHeightAndWidth = new DebounceFunction(1f, SaveHeightAndWidth);
		Manager = translationManager;
		Height = Settings.Height;
		Width = Settings.Width;
		AllTranslators = translationManager.AllEndpoints.Select((TranslationEndpointManager x) => new TranslatorViewModel(x)).ToList();
		AvailableTranslators = AllTranslators.Where((TranslatorViewModel x) => x.Endpoint.Error == null).ToList();
	}

	private void SaveHeightAndWidth()
	{
		Settings.SetTranslationAggregatorBounds(Width, Height);
	}

	public void OnNewTranslationAdded(string originalText, string defaultTranslation)
	{
		if (!_textsToAggregate.Contains(originalText))
		{
			Translation item = new Translation(originalText, defaultTranslation);
			_textsToAggregate.Add(originalText);
			_translationsToAggregate.Add(item);
			_lastUpdate = Time.realtimeSinceStartup;
			if (_translationsToAggregate.Count >= 10)
			{
				CreateNewAggregatedTranslation();
			}
		}
	}

	private void CreateNewAggregatedTranslation()
	{
		try
		{
			List<Translation> translations = _translationsToAggregate.ToList();
			AggregatedTranslationViewModel value = new AggregatedTranslationViewModel(this, translations);
			LinkedListNode<AggregatedTranslationViewModel> last = _translations.Last;
			_translations.AddLast(value);
			if (_current == null)
			{
				_current = _translations.Last;
			}
			else if (_current == last)
			{
				_current = _translations.Last;
			}
			if (_translations.Count >= 100)
			{
				LinkedListNode<AggregatedTranslationViewModel> first = _translations.First;
				_translations.RemoveFirst();
				if (_current == first)
				{
					_current = _translations.First;
				}
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error while copying text to clipboard.");
		}
		finally
		{
			_textsToAggregate.Clear();
			_translationsToAggregate.Clear();
		}
	}

	public void Update()
	{
		if (_translationsToAggregate.Count > 0 && Time.realtimeSinceStartup - _lastUpdate > Settings.ClipboardDebounceTime)
		{
			CreateNewAggregatedTranslation();
		}
		if (_current != null)
		{
			_current.Value.Update();
		}
	}

	public bool HasPrevious()
	{
		return _current?.Previous != null;
	}

	public void MovePrevious()
	{
		_current = _current.Previous;
	}

	public bool HasNext()
	{
		return _current?.Next != null;
	}

	public void MoveNext()
	{
		_current = _current.Next;
	}

	public void MoveLatest()
	{
		_current = _translations.Last;
	}
}
