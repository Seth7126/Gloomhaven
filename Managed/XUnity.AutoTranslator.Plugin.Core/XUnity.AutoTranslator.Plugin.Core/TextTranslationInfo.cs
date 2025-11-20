using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Fonts;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.AutoTranslator.Plugin.Core.UIResize;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal sealed class TextTranslationInfo
{
	private Action<object> _unresize;

	private Action<object> _unfont;

	private bool _hasCheckedTypeWriter;

	private MonoBehaviour _typewriter;

	private float? _alteredLineSpacing;

	private int? _alteredFontSize;

	private bool _initialized;

	private HashSet<string> _redirectedTranslations;

	private static readonly WeakDictionary<Material, Material> _FontMaterialCopies = new WeakDictionary<Material, Material>();

	public ITextComponentManipulator TextManipulator { get; set; }

	public string OriginalText { get; set; }

	public string TranslatedText { get; set; }

	public bool IsTranslated { get; set; }

	public bool IsCurrentlySettingText { get; set; }

	public bool IsStabilizingText { get; set; }

	public bool IsKnownTextComponent { get; set; }

	public bool SupportsStabilization { get; set; }

	public bool ShouldIgnore { get; set; }

	public HashSet<string> RedirectedTranslations => _redirectedTranslations ?? (_redirectedTranslations = new HashSet<string>());

	public IReadOnlyTextTranslationCache TextCache { get; set; }

	public void Initialize(object ui)
	{
		if (!_initialized)
		{
			_initialized = true;
			IsKnownTextComponent = ui.IsKnownTextType();
			SupportsStabilization = ui.SupportsStabilization();
			ShouldIgnore = ui.ShouldIgnoreTextComponent();
			TextManipulator = ui.GetTextManipulator();
		}
	}

	public void Reset(string newText)
	{
		IsTranslated = false;
		TranslatedText = null;
		OriginalText = newText;
	}

	public void SetTranslatedText(string translatedText)
	{
		IsTranslated = true;
		TranslatedText = translatedText;
	}

	public void ResetScrollIn(object ui)
	{
		if (!_hasCheckedTypeWriter)
		{
			_hasCheckedTypeWriter = true;
			if (UnityTypes.Typewriter != null)
			{
				Component val = (Component)((ui is Component) ? ui : null);
				if (ui != null && val.GetComponent(UnityTypes.Typewriter.UnityType).TryCastTo<MonoBehaviour>(out var castedObject))
				{
					_typewriter = castedObject;
				}
			}
		}
		if ((Object)(object)_typewriter != (Object)null)
		{
			AccessToolsShim.Method(UnityTypes.Typewriter.ClrType, "OnEnable")?.Invoke(_typewriter, null);
		}
	}

	public void ChangeFont(object ui)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		if (ui == null)
		{
			return;
		}
		Type unityType = ui.GetUnityType();
		if (UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom(unityType))
		{
			if (string.IsNullOrEmpty(Settings.OverrideFont))
			{
				return;
			}
			CachedProperty Text_fontProperty = UnityTypes.Text_Properties.Font;
			Font previousFont = (Font)Text_fontProperty.Get(ui);
			CachedProperty fontSize = UnityTypes.Font_Properties.FontSize;
			if ((Object)(object)previousFont == (Object)null)
			{
				return;
			}
			Font orCreate = FontCache.GetOrCreate((int)fontSize?.Get(previousFont));
			if (!((Object)(object)orCreate == (Object)null) && !UnityObjectReferenceComparer.Default.Equals(orCreate, previousFont))
			{
				Text_fontProperty.Set(ui, orCreate);
				_unfont = delegate(object instance2)
				{
					Text_fontProperty.Set(instance2, previousFont);
				};
			}
		}
		else
		{
			if ((UnityTypes.TextMeshPro == null || !UnityTypes.TextMeshPro.IsAssignableFrom(unityType)) && (UnityTypes.TextMeshProUGUI == null || !UnityTypes.TextMeshProUGUI.IsAssignableFrom(unityType)))
			{
				return;
			}
			if (string.IsNullOrEmpty(Settings.OverrideFontTextMeshPro))
			{
				return;
			}
			Type type = ui.GetType();
			CachedProperty fontProperty = type.CachedProperty("font");
			object previousFont2 = fontProperty.Get(ui);
			if (previousFont2 == null)
			{
				return;
			}
			object orCreateOverrideFontTextMeshPro = FontCache.GetOrCreateOverrideFontTextMeshPro();
			if (orCreateOverrideFontTextMeshPro == null || UnityObjectReferenceComparer.Default.Equals(orCreateOverrideFontTextMeshPro, previousFont2))
			{
				return;
			}
			CachedProperty fontMaterialProperty = type.CachedProperty("fontSharedMaterial");
			Material oldMaterial = default(Material);
			ref Material reference = ref oldMaterial;
			object obj = fontMaterialProperty.Get(ui);
			reference = (Material)((obj is Material) ? obj : null);
			fontProperty.Set(ui, orCreateOverrideFontTextMeshPro);
			object obj2 = fontMaterialProperty.Get(ui);
			Material val = (Material)((obj2 is Material) ? obj2 : null);
			if ((Object)(object)oldMaterial != (Object)null && (Object)(object)val != (Object)null)
			{
				if (!_FontMaterialCopies.TryGetValue(oldMaterial, out var value))
				{
					Material val2 = (_FontMaterialCopies[oldMaterial] = Object.Instantiate<Material>(oldMaterial));
					value = val2;
					Object instance = Object.Instantiate((Object)((ui is Object) ? ui : null));
					fontProperty.Set(instance, previousFont2);
					fontMaterialProperty.Set(instance, oldMaterial);
					value.SetTexture("_MainTex", val.GetTexture("_MainTex"));
					value.SetFloat("_TextureHeight", val.GetFloat("_TextureHeight"));
					value.SetFloat("_TextureWidth", val.GetFloat("_TextureWidth"));
					value.SetFloat("_GradientScale", val.GetFloat("_GradientScale"));
				}
				fontMaterialProperty.Set(ui, value);
			}
			_unfont = delegate(object instance2)
			{
				fontProperty.Set(instance2, previousFont2);
				fontMaterialProperty.Set(instance2, oldMaterial);
			};
		}
	}

	public void UnchangeFont(object ui)
	{
		if (ui != null)
		{
			_unfont?.Invoke(ui);
			_unfont = null;
		}
	}

	private static float GetComponentWidth(Component component)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (component.transform.TryCastTo<RectTransform>(out var castedObject))
		{
			Rect rect = castedObject.rect;
			return ((Rect)(ref rect)).width;
		}
		return 0f;
	}

	public void ResizeUI(object ui, UIResizeCache cache)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0912: Unknown result type (might be due to invalid IL or missing references)
		//IL_0919: Expected O, but got Unknown
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_0779: Expected O, but got Unknown
		if (ui == null)
		{
			return;
		}
		Type unityType = ui.GetUnityType();
		if (UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom(unityType))
		{
			Component val = (Component)ui;
			if (!Object.op_Implicit((Object)(object)val) || !Object.op_Implicit((Object)(object)val.gameObject))
			{
				return;
			}
			float componentWidth = GetComponentWidth(val);
			int num = Screen.width / 4;
			bool num2 = componentWidth > (float)num;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = _unresize == null;
			if (cache.HasAnyResizeCommands)
			{
				string[] pathSegments = val.gameObject.GetPathSegments();
				int scope = TranslationScopeHelper.GetScope(ui);
				if (cache.TryGetUIResize(pathSegments, scope, out var result))
				{
					if (result.AutoResizeCommand != null)
					{
						object resizeTextForBestFitValue = UnityTypes.Text_Properties.ResizeTextForBestFit.Get(ui);
						if (resizeTextForBestFitValue != null)
						{
							object resizeTextMinSizeValue = UnityTypes.Text_Properties.ResizeTextMinSize?.Get(ui);
							object resizeTextMaxSizeValue = UnityTypes.Text_Properties.ResizeTextMaxSize?.Get(ui);
							double num3 = result.AutoResizeCommand.GetMinSize() ?? 1.0;
							if (UnityTypes.Text_Properties.ResizeTextMinSize != null)
							{
								int num4 = (double.IsNaN(num3) ? 1 : ((int)num3));
								UnityTypes.Text_Properties.ResizeTextMinSize.Set(ui, num4);
							}
							double? maxSize = result.AutoResizeCommand.GetMaxSize();
							if (maxSize.HasValue && UnityTypes.Text_Properties.ResizeTextMaxSize != null)
							{
								int num5 = (double.IsNaN(maxSize.Value) ? 1 : ((int)maxSize.Value));
								UnityTypes.Text_Properties.ResizeTextMaxSize?.Set(ui, num5);
							}
							bool flag5 = result.AutoResizeCommand.ShouldAutoResize();
							UnityTypes.Text_Properties.ResizeTextForBestFit.Set(ui, flag5);
							if (flag4)
							{
								_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
								{
									UnityTypes.Text_Properties.ResizeTextForBestFit.Set(g, resizeTextForBestFitValue);
								});
								if (UnityTypes.Text_Properties.ResizeTextMinSize != null)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										UnityTypes.Text_Properties.ResizeTextMinSize.Set(g, resizeTextMinSizeValue);
									});
								}
								if (maxSize.HasValue && UnityTypes.Text_Properties.ResizeTextMaxSize != null)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										UnityTypes.Text_Properties.ResizeTextMaxSize.Set(g, resizeTextMaxSizeValue);
									});
								}
							}
						}
					}
					if (result.ResizeCommand != null)
					{
						int? currentFontSize = (int?)UnityTypes.Text_Properties.FontSize.Get(ui);
						if (currentFontSize.HasValue && !object.Equals(_alteredFontSize, currentFontSize))
						{
							int? size = result.ResizeCommand.GetSize(currentFontSize.Value);
							if (size.HasValue)
							{
								UnityTypes.Text_Properties.FontSize.Set(ui, size.Value);
								_alteredFontSize = size.Value;
								if (flag4)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										UnityTypes.Text_Properties.FontSize.Set(g, currentFontSize);
									});
								}
							}
						}
					}
					if (result.LineSpacingCommand != null)
					{
						float? lineSpacingValue = (float?)UnityTypes.Text_Properties.LineSpacing.Get(ui);
						if (lineSpacingValue.HasValue && !object.Equals(_alteredLineSpacing, lineSpacingValue))
						{
							float? lineSpacing = result.LineSpacingCommand.GetLineSpacing(lineSpacingValue.Value);
							if (lineSpacing.HasValue)
							{
								flag = true;
								UnityTypes.Text_Properties.LineSpacing.Set(ui, lineSpacing.Value);
								_alteredLineSpacing = lineSpacing;
								if (flag4)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										UnityTypes.Text_Properties.LineSpacing.Set(g, lineSpacingValue);
									});
								}
							}
						}
					}
					if (result.HorizontalOverflowCommand != null)
					{
						object horizontalOverflowValue = UnityTypes.Text_Properties.HorizontalOverflow.Get(ui);
						if (horizontalOverflowValue != null)
						{
							int? mode = result.HorizontalOverflowCommand.GetMode();
							if (mode.HasValue)
							{
								flag2 = true;
								UnityTypes.Text_Properties.HorizontalOverflow.Set(ui, mode.Value);
								if (flag4)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										UnityTypes.Text_Properties.HorizontalOverflow.Set(g, horizontalOverflowValue);
									});
								}
							}
						}
					}
					if (result.VerticalOverflowCommand != null)
					{
						object verticalOverflowValue = UnityTypes.Text_Properties.VerticalOverflow.Get(ui);
						if (verticalOverflowValue != null)
						{
							int? mode2 = result.VerticalOverflowCommand.GetMode();
							if (mode2.HasValue)
							{
								flag3 = true;
								UnityTypes.Text_Properties.VerticalOverflow.Set(ui, mode2.Value);
								if (flag4)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										UnityTypes.Text_Properties.VerticalOverflow.Set(g, verticalOverflowValue);
									});
								}
							}
						}
					}
				}
			}
			if (!num2 || (UnityTypes.Text_Properties.ResizeTextForBestFit != null && (bool)UnityTypes.Text_Properties.ResizeTextForBestFit.Get(val)))
			{
				return;
			}
			if (!flag && Settings.ResizeUILineSpacingScale.HasValue && UnityTypes.Text_Properties.LineSpacing != null)
			{
				object originalLineSpacing = UnityTypes.Text_Properties.LineSpacing.Get(val);
				if (!object.Equals(_alteredLineSpacing, originalLineSpacing))
				{
					float num6 = (float)originalLineSpacing * Settings.ResizeUILineSpacingScale.Value;
					UnityTypes.Text_Properties.LineSpacing.Set(val, num6);
					_alteredLineSpacing = num6;
					if (flag4)
					{
						_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
						{
							UnityTypes.Text_Properties.LineSpacing.Set(g, originalLineSpacing);
						});
					}
				}
			}
			if (!flag3 && UnityTypes.Text_Properties.VerticalOverflow != null)
			{
				object originalVerticalOverflow = UnityTypes.Text_Properties.VerticalOverflow.Get(val);
				UnityTypes.Text_Properties.VerticalOverflow.Set(val, 1);
				if (flag4)
				{
					_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
					{
						UnityTypes.Text_Properties.VerticalOverflow.Set(g, originalVerticalOverflow);
					});
				}
			}
			if (flag2 || UnityTypes.Text_Properties.HorizontalOverflow == null)
			{
				return;
			}
			object originalHorizontalOverflow = UnityTypes.Text_Properties.HorizontalOverflow.Get(val);
			UnityTypes.Text_Properties.HorizontalOverflow.Set(val, 0);
			if (flag4)
			{
				_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
				{
					UnityTypes.Text_Properties.HorizontalOverflow.Set(g, originalHorizontalOverflow);
				});
			}
		}
		else if (unityType == UnityTypes.UILabel?.UnityType)
		{
			object useFloatSpacingPropertyValue = UnityTypes.UILabel_Properties.UseFloatSpacing?.Get(ui);
			object spacingXPropertyValue = UnityTypes.UILabel_Properties.SpacingX?.Get(ui);
			object multiLinePropertyValue = UnityTypes.UILabel_Properties.MultiLine?.Get(ui);
			object overflowMethodPropertyValue = UnityTypes.UILabel_Properties.OverflowMethod?.Get(ui);
			UnityTypes.UILabel_Properties.UseFloatSpacing?.Set(ui, false);
			UnityTypes.UILabel_Properties.SpacingX?.Set(ui, -1);
			UnityTypes.UILabel_Properties.MultiLine?.Set(ui, true);
			UnityTypes.UILabel_Properties.OverflowMethod?.Set(ui, 0);
			if (_unresize == null)
			{
				_unresize = delegate(object g)
				{
					UnityTypes.UILabel_Properties.UseFloatSpacing?.Set(g, useFloatSpacingPropertyValue);
					UnityTypes.UILabel_Properties.SpacingX?.Set(g, spacingXPropertyValue);
					UnityTypes.UILabel_Properties.MultiLine?.Set(g, multiLinePropertyValue);
					UnityTypes.UILabel_Properties.OverflowMethod?.Set(g, overflowMethodPropertyValue);
				};
			}
			if (!cache.HasAnyResizeCommands)
			{
				return;
			}
			Component val2 = (Component)ui;
			if (!Object.op_Implicit((Object)(object)val2) || !Object.op_Implicit((Object)(object)val2.gameObject))
			{
				return;
			}
			Type type = ui.GetType();
			string[] pathSegments2 = val2.gameObject.GetPathSegments();
			int scope2 = TranslationScopeHelper.GetScope(ui);
			if (!cache.TryGetUIResize(pathSegments2, scope2, out var result2) || result2.ResizeCommand == null)
			{
				return;
			}
			CachedProperty fontSizeProperty = type.CachedProperty("fontSize");
			int currentFontSize2 = (int)fontSizeProperty.Get(ui);
			if (object.Equals(_alteredFontSize, currentFontSize2))
			{
				return;
			}
			int? size2 = result2.ResizeCommand.GetSize(currentFontSize2);
			if (size2.HasValue)
			{
				fontSizeProperty.Set(ui, size2.Value);
				_alteredFontSize = size2.Value;
				_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
				{
					fontSizeProperty.Set(g, currentFontSize2);
				});
			}
		}
		else
		{
			if (unityType != UnityTypes.TextMeshPro?.UnityType && unityType != UnityTypes.TextMeshProUGUI?.UnityType)
			{
				return;
			}
			Type type2 = ui.GetType();
			CachedProperty overflowModeProperty = type2.CachedProperty("overflowMode");
			object originalOverflowMode = overflowModeProperty?.Get(ui);
			bool flag6 = false;
			bool flag7 = _unresize == null;
			if (cache.HasAnyResizeCommands)
			{
				Component val3 = (Component)ui;
				if (!Object.op_Implicit((Object)(object)val3) || !Object.op_Implicit((Object)(object)val3.gameObject))
				{
					return;
				}
				string[] pathSegments3 = val3.gameObject.GetPathSegments();
				int scope3 = TranslationScopeHelper.GetScope(ui);
				if (cache.TryGetUIResize(pathSegments3, scope3, out var result3))
				{
					if (result3.OverflowCommand != null)
					{
						flag6 = true;
						if (overflowModeProperty != null)
						{
							int? mode3 = result3.OverflowCommand.GetMode();
							if (mode3.HasValue)
							{
								overflowModeProperty.Set(ui, mode3);
								if (flag7)
								{
									_unresize = delegate(object g)
									{
										overflowModeProperty.Set(g, originalOverflowMode);
									};
								}
							}
						}
					}
					if (result3.AlignmentCommand != null)
					{
						CachedProperty alignmentProperty = type2.CachedProperty("alignment");
						if (alignmentProperty != null)
						{
							object alignmentValue = alignmentProperty.Get(ui);
							int? mode4 = result3.AlignmentCommand.GetMode();
							if (mode4.HasValue)
							{
								alignmentProperty.Set(ui, mode4.Value);
								if (flag7)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										alignmentProperty.Set(g, alignmentValue);
									});
								}
							}
						}
					}
					if (result3.AutoResizeCommand != null)
					{
						CachedProperty enableAutoSizingProperty = type2.CachedProperty("enableAutoSizing");
						CachedProperty fontSizeMinProperty = type2.CachedProperty("fontSizeMin");
						CachedProperty fontSizeMaxProperty = type2.CachedProperty("fontSizeMax");
						CachedProperty fontSizeProperty2 = type2.CachedProperty("fontSize");
						float? currentFontSize3 = (float?)fontSizeProperty2.Get(ui);
						if (enableAutoSizingProperty != null)
						{
							object enableAutoSizingValue = enableAutoSizingProperty.Get(ui);
							object fontSizeMinValue = fontSizeMinProperty?.Get(ui);
							object fontSizeMaxValue = fontSizeMaxProperty?.Get(ui);
							double? minSize = result3.AutoResizeCommand.GetMinSize();
							if (minSize.HasValue && fontSizeMinProperty != null)
							{
								float num7 = (double.IsNaN(minSize.Value) ? 0f : ((float)minSize.Value));
								fontSizeMinProperty?.Set(ui, num7);
							}
							double? maxSize2 = result3.AutoResizeCommand.GetMaxSize();
							if (maxSize2.HasValue && fontSizeMaxProperty != null)
							{
								float num8 = (double.IsNaN(maxSize2.Value) ? float.MaxValue : ((float)maxSize2.Value));
								fontSizeMaxProperty?.Set(ui, num8);
							}
							bool flag8 = result3.AutoResizeCommand.ShouldAutoResize();
							enableAutoSizingProperty.Set(ui, flag8);
							if (flag7)
							{
								_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
								{
									enableAutoSizingProperty.Set(g, enableAutoSizingValue);
								});
								if (minSize.HasValue && fontSizeMinProperty != null)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										fontSizeMinProperty.Set(g, fontSizeMinValue);
									});
								}
								if (maxSize2.HasValue && fontSizeMaxProperty != null)
								{
									_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
									{
										fontSizeMaxProperty.Set(g, fontSizeMaxValue);
									});
								}
								_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
								{
									fontSizeProperty2.Set(g, currentFontSize3);
								});
							}
						}
					}
					if (result3.ResizeCommand != null)
					{
						CachedProperty fontSizeProperty3 = type2.CachedProperty("fontSize");
						float? currentFontSize4 = (float?)fontSizeProperty3.Get(ui);
						if (currentFontSize4.HasValue)
						{
							int num9 = (int)currentFontSize4.Value;
							if (!object.Equals(_alteredFontSize, num9))
							{
								int? size3 = result3.ResizeCommand.GetSize((int)currentFontSize4.Value);
								if (size3.HasValue)
								{
									fontSizeProperty3.Set(ui, (float)size3.Value);
									_alteredFontSize = size3.Value;
									if (flag7)
									{
										_unresize = (Action<object>)Delegate.Combine(_unresize, (Action<object>)delegate(object g)
										{
											fontSizeProperty3.Set(g, currentFontSize4);
										});
									}
								}
							}
						}
					}
				}
			}
			if (flag6 || originalOverflowMode == null || (int)originalOverflowMode != 2)
			{
				return;
			}
			overflowModeProperty.Set(ui, 3);
			if (flag7)
			{
				_unresize = delegate(object g)
				{
					overflowModeProperty.Set(g, 2);
				};
			}
		}
	}

	public void UnresizeUI(object ui)
	{
		if (ui != null)
		{
			_unresize?.Invoke(ui);
			_unresize = null;
			_alteredFontSize = null;
		}
	}
}
