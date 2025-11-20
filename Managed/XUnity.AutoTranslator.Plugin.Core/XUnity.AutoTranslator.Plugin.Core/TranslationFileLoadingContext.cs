using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DynamicLinq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class TranslationFileLoadingContext
{
	public class SetLevelTranslationFileDirective : TranslationFileDirective
	{
		public int[] Levels { get; }

		public SetLevelTranslationFileDirective(int[] levels)
		{
			Levels = levels;
		}

		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Settings.EnableTranslationScoping)
			{
				int[] levels = Levels;
				foreach (int item in levels)
				{
					context._levels.Add(item);
				}
			}
		}

		public override string ToString()
		{
			return "#set level " + string.Join(",", Levels.Select((int x) => x.ToString(CultureInfo.InvariantCulture)).ToArray());
		}
	}

	public struct ResolutionCheckVariables
	{
		public int Width { get; }

		public int Height { get; }

		public ResolutionCheckVariables(int width, int height)
		{
			Width = width;
			Height = height;
		}
	}

	public class SetRequiredResolutionTranslationFileDirective : TranslationFileDirective
	{
		private readonly string _expression;

		private readonly Func<ResolutionCheckVariables, bool> _predicate;

		public SetRequiredResolutionTranslationFileDirective(string expression)
		{
			_expression = expression;
			_predicate = DynamicExpression.ParseLambda<ResolutionCheckVariables, bool>(expression, new object[0]).Compile();
		}

		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Settings.EnableTranslationScoping)
			{
				context._resolutionCheck = _predicate;
			}
		}

		public override string ToString()
		{
			return "#set required-resolution " + _expression;
		}
	}

	public class UnsetRequiredResolutionTranslationFileDirective : TranslationFileDirective
	{
		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Settings.EnableTranslationScoping)
			{
				context._resolutionCheck = DefaultResolutionCheck;
			}
		}

		public override string ToString()
		{
			return "#unset required-resolution";
		}
	}

	public class UnsetLevelTranslationFileDirective : TranslationFileDirective
	{
		public int[] Levels { get; }

		public UnsetLevelTranslationFileDirective(int[] levels)
		{
			Levels = levels;
		}

		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Settings.EnableTranslationScoping)
			{
				int[] levels = Levels;
				foreach (int item in levels)
				{
					context._levels.Remove(item);
				}
			}
		}

		public override string ToString()
		{
			return "#unset level " + string.Join(",", Levels.Select((int x) => x.ToString(CultureInfo.InvariantCulture)).ToArray());
		}
	}

	public class SetExeTranslationFileDirective : TranslationFileDirective
	{
		public string[] Executables { get; }

		public SetExeTranslationFileDirective(string[] executables)
		{
			Executables = executables;
		}

		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Settings.EnableTranslationScoping)
			{
				string[] executables = Executables;
				foreach (string item in executables)
				{
					context._executables.Add(item);
				}
			}
		}

		public override string ToString()
		{
			return "#set exe " + string.Join(",", Executables);
		}
	}

	public class UnsetExeTranslationFileDirective : TranslationFileDirective
	{
		public string[] Executables { get; }

		public UnsetExeTranslationFileDirective(string[] executables)
		{
			Executables = executables;
		}

		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Settings.EnableTranslationScoping)
			{
				string[] executables = Executables;
				foreach (string item in executables)
				{
					context._executables.Remove(item);
				}
			}
		}

		public override string ToString()
		{
			return "#unset exe " + string.Join(",", Executables);
		}
	}

	public class EnableTranslationFileDirective : TranslationFileDirective
	{
		public string Tag { get; }

		public EnableTranslationFileDirective(string tag)
		{
			Tag = tag;
		}

		public override void ModifyContext(TranslationFileLoadingContext context)
		{
			if (Tag != null)
			{
				context._enabledTags.Add(Tag);
			}
		}

		public override string ToString()
		{
			return "#enable " + Tag;
		}
	}

	private static readonly Func<ResolutionCheckVariables, bool> DefaultResolutionCheck = (ResolutionCheckVariables x) => true;

	private HashSet<string> _executables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private HashSet<int> _levels = new HashSet<int>();

	private HashSet<string> _enabledTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private Func<ResolutionCheckVariables, bool> _resolutionCheck = DefaultResolutionCheck;

	public bool IsApplicable()
	{
		if (IsValidExecutable())
		{
			return _resolutionCheck(new ResolutionCheckVariables(Screen.width, Screen.height));
		}
		return false;
	}

	public bool IsValidExecutable()
	{
		if (_executables.Count == 0)
		{
			return true;
		}
		return _executables.Contains(Settings.ApplicationName);
	}

	public HashSet<int> GetLevels()
	{
		return _levels;
	}

	public bool IsEnabled(string tag)
	{
		return _enabledTags.Contains(tag);
	}

	public void Apply(TranslationFileDirective directive)
	{
		directive.ModifyContext(this);
	}
}
