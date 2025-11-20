using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem;

public static class InputActionRebindingExtensions
{
	public sealed class RebindingOperation : IDisposable
	{
		[Flags]
		private enum Flags
		{
			Started = 1,
			Completed = 2,
			Canceled = 4,
			OnEventHooked = 8,
			OnAfterUpdateHooked = 0x10,
			DontIgnoreNoisyControls = 0x40,
			DontGeneralizePathOfSelectedControl = 0x80,
			AddNewBinding = 0x100,
			SuppressMatchingEvents = 0x200
		}

		public const float kDefaultMagnitudeThreshold = 0.2f;

		private InputAction m_ActionToRebind;

		private InputBinding? m_BindingMask;

		private Type m_ControlType;

		private InternedString m_ExpectedLayout;

		private int m_IncludePathCount;

		private string[] m_IncludePaths;

		private int m_ExcludePathCount;

		private string[] m_ExcludePaths;

		private int m_TargetBindingIndex = -1;

		private string m_BindingGroupForNewBinding;

		private string m_CancelBinding;

		private float m_MagnitudeThreshold = 0.2f;

		private float[] m_Scores;

		private float[] m_Magnitudes;

		private double m_LastMatchTime;

		private double m_StartTime;

		private float m_Timeout;

		private float m_WaitSecondsAfterMatch;

		private InputControlList<InputControl> m_Candidates;

		private Action<RebindingOperation> m_OnComplete;

		private Action<RebindingOperation> m_OnCancel;

		private Action<RebindingOperation> m_OnPotentialMatch;

		private Func<InputControl, string> m_OnGeneratePath;

		private Func<InputControl, InputEventPtr, float> m_OnComputeScore;

		private Action<RebindingOperation, string> m_OnApplyBinding;

		private Action<InputEventPtr, InputDevice> m_OnEventDelegate;

		private Action m_OnAfterUpdateDelegate;

		private InputControlLayout.Cache m_LayoutCache;

		private StringBuilder m_PathBuilder;

		private Flags m_Flags;

		private Dictionary<InputControl, float> m_StartingActuations = new Dictionary<InputControl, float>();

		public InputAction action => m_ActionToRebind;

		public InputBinding? bindingMask => m_BindingMask;

		public InputControlList<InputControl> candidates => m_Candidates;

		public ReadOnlyArray<float> scores => new ReadOnlyArray<float>(m_Scores, 0, m_Candidates.Count);

		public ReadOnlyArray<float> magnitudes => new ReadOnlyArray<float>(m_Magnitudes, 0, m_Candidates.Count);

		public InputControl selectedControl
		{
			get
			{
				if (m_Candidates.Count == 0)
				{
					return null;
				}
				return m_Candidates[0];
			}
		}

		public bool started => (m_Flags & Flags.Started) != 0;

		public bool completed => (m_Flags & Flags.Completed) != 0;

		public bool canceled => (m_Flags & Flags.Canceled) != 0;

		public double startTime => m_StartTime;

		public float timeout => m_Timeout;

		public string expectedControlType => m_ExpectedLayout;

		public RebindingOperation WithAction(InputAction action)
		{
			ThrowIfRebindInProgress();
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (action.enabled)
			{
				throw new InvalidOperationException($"Cannot rebind action '{action}' while it is enabled");
			}
			m_ActionToRebind = action;
			if (!string.IsNullOrEmpty(action.expectedControlType))
			{
				WithExpectedControlType(action.expectedControlType);
			}
			else if (action.type == InputActionType.Button)
			{
				WithExpectedControlType("Button");
			}
			return this;
		}

		public RebindingOperation WithMatchingEventsBeingSuppressed(bool value = true)
		{
			ThrowIfRebindInProgress();
			if (value)
			{
				m_Flags |= Flags.SuppressMatchingEvents;
			}
			else
			{
				m_Flags &= ~Flags.SuppressMatchingEvents;
			}
			return this;
		}

		public RebindingOperation WithCancelingThrough(string binding)
		{
			ThrowIfRebindInProgress();
			m_CancelBinding = binding;
			return this;
		}

		public RebindingOperation WithCancelingThrough(InputControl control)
		{
			ThrowIfRebindInProgress();
			if (control == null)
			{
				throw new ArgumentNullException("control");
			}
			return WithCancelingThrough(control.path);
		}

		public RebindingOperation WithExpectedControlType(string layoutName)
		{
			ThrowIfRebindInProgress();
			m_ExpectedLayout = new InternedString(layoutName);
			return this;
		}

		public RebindingOperation WithExpectedControlType(Type type)
		{
			ThrowIfRebindInProgress();
			if (type != null && !typeof(InputControl).IsAssignableFrom(type))
			{
				throw new ArgumentException("Type '" + type.Name + "' is not an InputControl", "type");
			}
			m_ControlType = type;
			return this;
		}

		public RebindingOperation WithExpectedControlType<TControl>() where TControl : InputControl
		{
			ThrowIfRebindInProgress();
			return WithExpectedControlType(typeof(TControl));
		}

		public RebindingOperation WithTargetBinding(int bindingIndex)
		{
			if (bindingIndex < 0)
			{
				throw new ArgumentOutOfRangeException("bindingIndex");
			}
			m_TargetBindingIndex = bindingIndex;
			if (m_ActionToRebind != null && bindingIndex < m_ActionToRebind.bindings.Count)
			{
				InputBinding inputBinding = m_ActionToRebind.bindings[bindingIndex];
				if (inputBinding.isPartOfComposite)
				{
					string nameOfComposite = m_ActionToRebind.ChangeBinding(bindingIndex).PreviousCompositeBinding().binding.GetNameOfComposite();
					string name = inputBinding.name;
					string expectedControlLayoutName = InputBindingComposite.GetExpectedControlLayoutName(nameOfComposite, name);
					if (!string.IsNullOrEmpty(expectedControlLayoutName))
					{
						WithExpectedControlType(expectedControlLayoutName);
					}
				}
				InputActionAsset inputActionAsset = action.actionMap?.asset;
				if (inputActionAsset != null && !string.IsNullOrEmpty(inputBinding.groups))
				{
					string[] array = inputBinding.groups.Split(';');
					foreach (string group in array)
					{
						int num = inputActionAsset.controlSchemes.IndexOf((InputControlScheme x) => group.Equals(x.bindingGroup, StringComparison.InvariantCultureIgnoreCase));
						if (num == -1)
						{
							continue;
						}
						foreach (InputControlScheme.DeviceRequirement deviceRequirement in inputActionAsset.controlSchemes[num].deviceRequirements)
						{
							WithControlsHavingToMatchPath(deviceRequirement.controlPath);
						}
					}
				}
			}
			return this;
		}

		public RebindingOperation WithBindingMask(InputBinding? bindingMask)
		{
			m_BindingMask = bindingMask;
			return this;
		}

		public RebindingOperation WithBindingGroup(string group)
		{
			return WithBindingMask(new InputBinding
			{
				groups = group
			});
		}

		public RebindingOperation WithoutGeneralizingPathOfSelectedControl()
		{
			m_Flags |= Flags.DontGeneralizePathOfSelectedControl;
			return this;
		}

		public RebindingOperation WithRebindAddingNewBinding(string group = null)
		{
			m_Flags |= Flags.AddNewBinding;
			m_BindingGroupForNewBinding = group;
			return this;
		}

		public RebindingOperation WithMagnitudeHavingToBeGreaterThan(float magnitude)
		{
			ThrowIfRebindInProgress();
			if (magnitude < 0f)
			{
				throw new ArgumentException($"Magnitude has to be positive but was {magnitude}", "magnitude");
			}
			m_MagnitudeThreshold = magnitude;
			return this;
		}

		public RebindingOperation WithoutIgnoringNoisyControls()
		{
			ThrowIfRebindInProgress();
			m_Flags |= Flags.DontIgnoreNoisyControls;
			return this;
		}

		public RebindingOperation WithControlsHavingToMatchPath(string path)
		{
			ThrowIfRebindInProgress();
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			for (int i = 0; i < m_IncludePathCount; i++)
			{
				if (string.Compare(m_IncludePaths[i], path, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return this;
				}
			}
			ArrayHelpers.AppendWithCapacity(ref m_IncludePaths, ref m_IncludePathCount, path);
			return this;
		}

		public RebindingOperation WithControlsExcluding(string path)
		{
			ThrowIfRebindInProgress();
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			for (int i = 0; i < m_ExcludePathCount; i++)
			{
				if (string.Compare(m_ExcludePaths[i], path, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return this;
				}
			}
			ArrayHelpers.AppendWithCapacity(ref m_ExcludePaths, ref m_ExcludePathCount, path);
			return this;
		}

		public RebindingOperation WithTimeout(float timeInSeconds)
		{
			m_Timeout = timeInSeconds;
			return this;
		}

		public RebindingOperation OnComplete(Action<RebindingOperation> callback)
		{
			m_OnComplete = callback;
			return this;
		}

		public RebindingOperation OnCancel(Action<RebindingOperation> callback)
		{
			m_OnCancel = callback;
			return this;
		}

		public RebindingOperation OnPotentialMatch(Action<RebindingOperation> callback)
		{
			m_OnPotentialMatch = callback;
			return this;
		}

		public RebindingOperation OnGeneratePath(Func<InputControl, string> callback)
		{
			m_OnGeneratePath = callback;
			return this;
		}

		public RebindingOperation OnComputeScore(Func<InputControl, InputEventPtr, float> callback)
		{
			m_OnComputeScore = callback;
			return this;
		}

		public RebindingOperation OnApplyBinding(Action<RebindingOperation, string> callback)
		{
			m_OnApplyBinding = callback;
			return this;
		}

		public RebindingOperation OnMatchWaitForAnother(float seconds)
		{
			m_WaitSecondsAfterMatch = seconds;
			return this;
		}

		public RebindingOperation Start()
		{
			if (started)
			{
				return this;
			}
			if (m_ActionToRebind != null && m_ActionToRebind.bindings.Count == 0 && (m_Flags & Flags.AddNewBinding) == 0)
			{
				throw new InvalidOperationException($"Action '{action}' must have at least one existing binding or must be used with WithRebindingAddNewBinding()");
			}
			if (m_ActionToRebind == null && m_OnApplyBinding == null)
			{
				throw new InvalidOperationException("Must either have an action (call WithAction()) to apply binding to or have a custom callback to apply the binding (call OnApplyBinding())");
			}
			m_StartTime = InputRuntime.s_Instance.currentTime;
			if (m_WaitSecondsAfterMatch > 0f || m_Timeout > 0f)
			{
				HookOnAfterUpdate();
				m_LastMatchTime = -1.0;
			}
			HookOnEvent();
			m_Flags |= Flags.Started;
			m_Flags &= ~Flags.Canceled;
			m_Flags &= ~Flags.Completed;
			return this;
		}

		public void Cancel()
		{
			if (started)
			{
				OnCancel();
			}
		}

		public void Complete()
		{
			if (started)
			{
				OnComplete();
			}
		}

		public void AddCandidate(InputControl control, float score, float magnitude = -1f)
		{
			if (control == null)
			{
				throw new ArgumentNullException("control");
			}
			int num = m_Candidates.IndexOf(control);
			if (num != -1)
			{
				m_Scores[num] = score;
			}
			else
			{
				int count = m_Candidates.Count;
				int count2 = m_Candidates.Count;
				m_Candidates.Add(control);
				ArrayHelpers.AppendWithCapacity(ref m_Scores, ref count, score);
				ArrayHelpers.AppendWithCapacity(ref m_Magnitudes, ref count2, magnitude);
			}
			SortCandidatesByScore();
		}

		public void RemoveCandidate(InputControl control)
		{
			if (control == null)
			{
				throw new ArgumentNullException("control");
			}
			int num = m_Candidates.IndexOf(control);
			if (num != -1)
			{
				int count = m_Candidates.Count;
				m_Candidates.RemoveAt(num);
				m_Scores.EraseAtWithCapacity(ref count, num);
			}
		}

		public void Dispose()
		{
			UnhookOnEvent();
			UnhookOnAfterUpdate();
			m_Candidates.Dispose();
			m_LayoutCache.Clear();
		}

		~RebindingOperation()
		{
			Dispose();
		}

		public RebindingOperation Reset()
		{
			Cancel();
			m_ActionToRebind = null;
			m_BindingMask = null;
			m_ControlType = null;
			m_ExpectedLayout = default(InternedString);
			m_IncludePathCount = 0;
			m_ExcludePathCount = 0;
			m_TargetBindingIndex = -1;
			m_BindingGroupForNewBinding = null;
			m_CancelBinding = null;
			m_MagnitudeThreshold = 0.2f;
			m_Timeout = 0f;
			m_WaitSecondsAfterMatch = 0f;
			m_Flags = (Flags)0;
			m_StartingActuations?.Clear();
			return this;
		}

		private void HookOnEvent()
		{
			if ((m_Flags & Flags.OnEventHooked) == 0)
			{
				if (m_OnEventDelegate == null)
				{
					m_OnEventDelegate = OnEvent;
				}
				InputSystem.onEvent += m_OnEventDelegate;
				m_Flags |= Flags.OnEventHooked;
			}
		}

		private void UnhookOnEvent()
		{
			if ((m_Flags & Flags.OnEventHooked) != 0)
			{
				InputSystem.onEvent -= m_OnEventDelegate;
				m_Flags &= ~Flags.OnEventHooked;
			}
		}

		private unsafe void OnEvent(InputEventPtr eventPtr, InputDevice device)
		{
			FourCC type = eventPtr.type;
			if (type != 1398030676 && type != 1145852993)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			InputControlExtensions.Enumerate enumerate = InputControlExtensions.Enumerate.IncludeSyntheticControls | InputControlExtensions.Enumerate.IncludeNonLeafControls;
			if ((m_Flags & Flags.DontIgnoreNoisyControls) != 0)
			{
				enumerate |= InputControlExtensions.Enumerate.IncludeNoisyControls;
			}
			foreach (InputControl item in eventPtr.EnumerateControls(enumerate, device))
			{
				void* statePtrFromStateEventUnchecked = item.GetStatePtrFromStateEventUnchecked(eventPtr, type);
				if (!string.IsNullOrEmpty(m_CancelBinding) && InputControlPath.Matches(m_CancelBinding, item) && item.HasValueChangeInState(statePtrFromStateEventUnchecked))
				{
					OnCancel();
					break;
				}
				if (m_ExcludePathCount > 0 && HavePathMatch(item, m_ExcludePaths, m_ExcludePathCount))
				{
					continue;
				}
				flag2 = true;
				if ((m_IncludePathCount > 0 && !HavePathMatch(item, m_IncludePaths, m_IncludePathCount)) || (m_ControlType != null && !m_ControlType.IsInstanceOfType(item)) || (!m_ExpectedLayout.IsEmpty() && m_ExpectedLayout != item.m_Layout && !InputControlLayout.s_Layouts.IsBasedOn(m_ExpectedLayout, item.m_Layout)))
				{
					continue;
				}
				if (item.CheckStateIsAtDefault(statePtrFromStateEventUnchecked, null))
				{
					if (!m_StartingActuations.ContainsKey(item))
					{
						m_StartingActuations.Add(item, 0f);
					}
					m_StartingActuations[item] = 0f;
					continue;
				}
				float num = item.EvaluateMagnitude(statePtrFromStateEventUnchecked);
				if (num >= 0f)
				{
					if (!m_StartingActuations.TryGetValue(item, out var value))
					{
						value = item.EvaluateMagnitude();
						m_StartingActuations.Add(item, value);
					}
					if (Mathf.Abs(value - num) < m_MagnitudeThreshold)
					{
						continue;
					}
				}
				float num2;
				if (m_OnComputeScore != null)
				{
					num2 = m_OnComputeScore(item, eventPtr);
				}
				else
				{
					num2 = num;
					if (!item.synthetic)
					{
						num2 += 1f;
					}
				}
				int num3 = m_Candidates.IndexOf(item);
				if (num3 != -1)
				{
					if (m_Scores[num3] < num2)
					{
						flag = true;
						m_Scores[num3] = num2;
						if (m_WaitSecondsAfterMatch > 0f)
						{
							m_LastMatchTime = InputRuntime.s_Instance.currentTime;
						}
					}
					continue;
				}
				int count = m_Candidates.Count;
				int count2 = m_Candidates.Count;
				m_Candidates.Add(item);
				ArrayHelpers.AppendWithCapacity(ref m_Scores, ref count, num2);
				ArrayHelpers.AppendWithCapacity(ref m_Magnitudes, ref count2, num);
				flag = true;
				if (m_WaitSecondsAfterMatch > 0f)
				{
					m_LastMatchTime = InputRuntime.s_Instance.currentTime;
				}
			}
			if (flag2 && (m_Flags & Flags.SuppressMatchingEvents) != 0)
			{
				eventPtr.handled = true;
			}
			if (flag && !canceled)
			{
				if (m_OnPotentialMatch != null)
				{
					SortCandidatesByScore();
					m_OnPotentialMatch(this);
				}
				else if (m_WaitSecondsAfterMatch <= 0f)
				{
					OnComplete();
				}
				else
				{
					SortCandidatesByScore();
				}
			}
		}

		private void SortCandidatesByScore()
		{
			int count = m_Candidates.Count;
			if (count <= 1)
			{
				return;
			}
			for (int i = 1; i < count; i++)
			{
				int num = i;
				while (num > 0 && m_Scores[num - 1] < m_Scores[num])
				{
					int index = num - 1;
					m_Scores.SwapElements(num, index);
					m_Candidates.SwapElements(num, index);
					m_Magnitudes.SwapElements(num, index);
					num--;
				}
			}
		}

		private static bool HavePathMatch(InputControl control, string[] paths, int pathCount)
		{
			for (int i = 0; i < pathCount; i++)
			{
				if (InputControlPath.MatchesPrefix(paths[i], control))
				{
					return true;
				}
			}
			return false;
		}

		private void HookOnAfterUpdate()
		{
			if ((m_Flags & Flags.OnAfterUpdateHooked) == 0)
			{
				if (m_OnAfterUpdateDelegate == null)
				{
					m_OnAfterUpdateDelegate = OnAfterUpdate;
				}
				InputSystem.onAfterUpdate += m_OnAfterUpdateDelegate;
				m_Flags |= Flags.OnAfterUpdateHooked;
			}
		}

		private void UnhookOnAfterUpdate()
		{
			if ((m_Flags & Flags.OnAfterUpdateHooked) != 0)
			{
				InputSystem.onAfterUpdate -= m_OnAfterUpdateDelegate;
				m_Flags &= ~Flags.OnAfterUpdateHooked;
			}
		}

		private void OnAfterUpdate()
		{
			if (m_LastMatchTime < 0.0 && m_Timeout > 0f && InputRuntime.s_Instance.currentTime - m_StartTime > (double)m_Timeout)
			{
				Cancel();
			}
			else if (!(m_WaitSecondsAfterMatch <= 0f) && !(m_LastMatchTime < 0.0) && InputRuntime.s_Instance.currentTime >= m_LastMatchTime + (double)m_WaitSecondsAfterMatch)
			{
				Complete();
			}
		}

		private void OnComplete()
		{
			SortCandidatesByScore();
			if (m_Candidates.Count > 0)
			{
				InputControl inputControl = m_Candidates[0];
				string text = inputControl.path;
				if (m_OnGeneratePath != null)
				{
					string text2 = m_OnGeneratePath(inputControl);
					if (!string.IsNullOrEmpty(text2))
					{
						text = text2;
					}
					else if ((m_Flags & Flags.DontGeneralizePathOfSelectedControl) == 0)
					{
						text = GeneratePathForControl(inputControl);
					}
				}
				else if ((m_Flags & Flags.DontGeneralizePathOfSelectedControl) == 0)
				{
					text = GeneratePathForControl(inputControl);
				}
				if (m_OnApplyBinding != null)
				{
					m_OnApplyBinding(this, text);
				}
				else if ((m_Flags & Flags.AddNewBinding) != 0)
				{
					m_ActionToRebind.AddBinding(text, null, null, m_BindingGroupForNewBinding);
				}
				else if (m_TargetBindingIndex >= 0)
				{
					if (m_TargetBindingIndex >= m_ActionToRebind.bindings.Count)
					{
						throw new InvalidOperationException($"Target binding index {m_TargetBindingIndex} out of range for action '{m_ActionToRebind}' with {m_ActionToRebind.bindings.Count} bindings");
					}
					m_ActionToRebind.ApplyBindingOverride(m_TargetBindingIndex, text);
				}
				else if (m_BindingMask.HasValue)
				{
					InputBinding value = m_BindingMask.Value;
					value.overridePath = text;
					m_ActionToRebind.ApplyBindingOverride(value);
				}
				else
				{
					m_ActionToRebind.ApplyBindingOverride(text);
				}
			}
			m_Flags |= Flags.Completed;
			m_OnComplete?.Invoke(this);
			ResetAfterMatchCompleted();
		}

		private void OnCancel()
		{
			m_Flags |= Flags.Canceled;
			m_OnCancel?.Invoke(this);
			ResetAfterMatchCompleted();
		}

		private void ResetAfterMatchCompleted()
		{
			m_Flags &= ~Flags.Started;
			m_Candidates.Clear();
			m_Candidates.Capacity = 0;
			m_StartTime = -1.0;
			m_StartingActuations.Clear();
			UnhookOnEvent();
			UnhookOnAfterUpdate();
		}

		private void ThrowIfRebindInProgress()
		{
			if (started)
			{
				throw new InvalidOperationException("Cannot reconfigure rebinding while operation is in progress");
			}
		}

		private string GeneratePathForControl(InputControl control)
		{
			_ = control.device;
			InternedString internedString = InputControlLayout.s_Layouts.FindLayoutThatIntroducesControl(control, m_LayoutCache);
			if (m_PathBuilder == null)
			{
				m_PathBuilder = new StringBuilder();
			}
			else
			{
				m_PathBuilder.Length = 0;
			}
			control.BuildPath(internedString, m_PathBuilder);
			return m_PathBuilder.ToString();
		}
	}

	internal class DeferBindingResolutionWrapper : IDisposable
	{
		public void Acquire()
		{
			InputActionMap.s_DeferBindingResolution++;
		}

		public void Dispose()
		{
			if (InputActionMap.s_DeferBindingResolution > 0)
			{
				InputActionMap.s_DeferBindingResolution--;
			}
			if (InputActionMap.s_DeferBindingResolution == 0)
			{
				InputActionState.DeferredResolutionOfBindings();
			}
		}
	}

	private static DeferBindingResolutionWrapper s_DeferBindingResolutionWrapper;

	public static int GetBindingIndex(this InputAction action, InputBinding bindingMask)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		ReadOnlyArray<InputBinding> bindings = action.bindings;
		for (int i = 0; i < bindings.Count; i++)
		{
			if (bindingMask.Matches(bindings[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int GetBindingIndex(this InputActionMap actionMap, InputBinding bindingMask)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		ReadOnlyArray<InputBinding> bindings = actionMap.bindings;
		for (int i = 0; i < bindings.Count; i++)
		{
			if (bindingMask.Matches(bindings[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int GetBindingIndex(this InputAction action, string group = null, string path = null)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return action.GetBindingIndex(new InputBinding(path, null, group));
	}

	public static InputBinding? GetBindingForControl(this InputAction action, InputControl control)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		int bindingIndexForControl = action.GetBindingIndexForControl(control);
		if (bindingIndexForControl == -1)
		{
			return null;
		}
		return action.bindings[bindingIndexForControl];
	}

	public unsafe static int GetBindingIndexForControl(this InputAction action, InputControl control)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		InputActionMap orCreateActionMap = action.GetOrCreateActionMap();
		orCreateActionMap.ResolveBindingsIfNecessary();
		InputActionState state = orCreateActionMap.m_State;
		InputControl[] controls = state.controls;
		int totalControlCount = state.totalControlCount;
		InputActionState.BindingState* bindingStates = state.bindingStates;
		int* controlIndexToBindingIndex = state.controlIndexToBindingIndex;
		int actionIndexInState = action.m_ActionIndexInState;
		for (int i = 0; i < totalControlCount; i++)
		{
			if (controls[i] == control)
			{
				int num = controlIndexToBindingIndex[i];
				if (bindingStates[num].actionIndex == actionIndexInState)
				{
					int bindingIndexInMap = state.GetBindingIndexInMap(num);
					return action.BindingIndexOnMapToBindingIndexOnAction(bindingIndexInMap);
				}
			}
		}
		return -1;
	}

	public static string GetBindingDisplayString(this InputAction action, InputBinding.DisplayStringOptions options = (InputBinding.DisplayStringOptions)0, string group = null)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		InputBinding bindingMask;
		if (!string.IsNullOrEmpty(group))
		{
			bindingMask = InputBinding.MaskByGroup(group);
		}
		else
		{
			InputBinding? inputBinding = action.FindEffectiveBindingMask();
			bindingMask = ((!inputBinding.HasValue) ? default(InputBinding) : inputBinding.Value);
		}
		return action.GetBindingDisplayString(bindingMask, options);
	}

	public static string GetBindingDisplayString(this InputAction action, InputBinding bindingMask, InputBinding.DisplayStringOptions options = (InputBinding.DisplayStringOptions)0)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		string text = string.Empty;
		ReadOnlyArray<InputBinding> bindings = action.bindings;
		for (int i = 0; i < bindings.Count; i++)
		{
			if (!bindings[i].isPartOfComposite && bindingMask.Matches(bindings[i]))
			{
				string bindingDisplayString = action.GetBindingDisplayString(i, options);
				text = ((!(text != "")) ? bindingDisplayString : (text + " | " + bindingDisplayString));
			}
		}
		return text;
	}

	public static string GetBindingDisplayString(this InputAction action, int bindingIndex, InputBinding.DisplayStringOptions options = (InputBinding.DisplayStringOptions)0)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		string deviceLayoutName;
		string controlPath;
		return action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, options);
	}

	public unsafe static string GetBindingDisplayString(this InputAction action, int bindingIndex, out string deviceLayoutName, out string controlPath, InputBinding.DisplayStringOptions options = (InputBinding.DisplayStringOptions)0)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		deviceLayoutName = null;
		controlPath = null;
		ReadOnlyArray<InputBinding> bindings = action.bindings;
		int count = bindings.Count;
		if (bindingIndex < 0 || bindingIndex >= count)
		{
			throw new ArgumentOutOfRangeException($"Binding index {bindingIndex} is out of range on action '{action}' with {bindings.Count} bindings", "bindingIndex");
		}
		if (bindings[bindingIndex].isComposite)
		{
			string name = NameAndParameters.Parse(bindings[bindingIndex].effectivePath).name;
			int firstPartIndex = bindingIndex + 1;
			int i;
			for (i = firstPartIndex; i < count && bindings[i].isPartOfComposite; i++)
			{
			}
			int partCount = i - firstPartIndex;
			string[] partStrings = new string[partCount];
			for (int j = 0; j < partCount; j++)
			{
				string text = action.GetBindingDisplayString(firstPartIndex + j, options);
				if (string.IsNullOrEmpty(text))
				{
					text = " ";
				}
				partStrings[j] = text;
			}
			string displayFormatString = InputBindingComposite.GetDisplayFormatString(name);
			if (string.IsNullOrEmpty(displayFormatString))
			{
				return StringHelpers.Join("/", partStrings);
			}
			return StringHelpers.ExpandTemplateString(displayFormatString, delegate(string fragment)
			{
				string text2 = string.Empty;
				for (int k = 0; k < partCount; k++)
				{
					if (string.Equals(bindings[firstPartIndex + k].name, fragment, StringComparison.InvariantCultureIgnoreCase))
					{
						text2 = (string.IsNullOrEmpty(text2) ? partStrings[k] : (text2 + "|" + partStrings[k]));
					}
				}
				if (string.IsNullOrEmpty(text2))
				{
					text2 = " ";
				}
				return text2;
			});
		}
		InputControl control = null;
		InputActionMap orCreateActionMap = action.GetOrCreateActionMap();
		orCreateActionMap.ResolveBindingsIfNecessary();
		InputActionState state = orCreateActionMap.m_State;
		int bindingIndexInMap = action.BindingIndexOnActionToBindingIndexOnMap(bindingIndex);
		int bindingIndexInState = state.GetBindingIndexInState(orCreateActionMap.m_MapIndexInState, bindingIndexInMap);
		InputActionState.BindingState* ptr = state.bindingStates + bindingIndexInState;
		if (ptr->controlCount > 0)
		{
			control = state.controls[ptr->controlStartIndex];
		}
		InputBinding inputBinding = bindings[bindingIndex];
		if (string.IsNullOrEmpty(inputBinding.effectiveInteractions))
		{
			inputBinding.overrideInteractions = action.interactions;
		}
		else if (!string.IsNullOrEmpty(action.interactions))
		{
			inputBinding.overrideInteractions = inputBinding.effectiveInteractions + ";action.interactions";
		}
		return inputBinding.ToDisplayString(out deviceLayoutName, out controlPath, options, control);
	}

	public static void ApplyBindingOverride(this InputAction action, string newPath, string group = null, string path = null)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		action.ApplyBindingOverride(new InputBinding
		{
			overridePath = newPath,
			groups = group,
			path = path
		});
	}

	public static void ApplyBindingOverride(this InputAction action, InputBinding bindingOverride)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		bindingOverride.action = action.name;
		action.GetOrCreateActionMap().ApplyBindingOverride(bindingOverride);
	}

	public static void ApplyBindingOverride(this InputAction action, int bindingIndex, InputBinding bindingOverride)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		int bindingIndex2 = action.BindingIndexOnActionToBindingIndexOnMap(bindingIndex);
		bindingOverride.action = action.name;
		action.GetOrCreateActionMap().ApplyBindingOverride(bindingIndex2, bindingOverride);
	}

	public static void ApplyBindingOverride(this InputAction action, int bindingIndex, string path)
	{
		if (path == null)
		{
			throw new ArgumentException("Binding path cannot be null", "path");
		}
		action.ApplyBindingOverride(bindingIndex, new InputBinding
		{
			overridePath = path
		});
	}

	public static int ApplyBindingOverride(this InputActionMap actionMap, InputBinding bindingOverride)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		InputBinding[] bindings = actionMap.m_Bindings;
		if (bindings == null)
		{
			return 0;
		}
		int num = bindings.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (bindingOverride.Matches(ref bindings[i]))
			{
				bindings[i].overridePath = bindingOverride.overridePath;
				bindings[i].overrideInteractions = bindingOverride.overrideInteractions;
				bindings[i].overrideProcessors = bindingOverride.overrideProcessors;
				num2++;
			}
		}
		if (num2 > 0)
		{
			actionMap.ClearCachedActionData();
			actionMap.LazyResolveBindings();
		}
		return num2;
	}

	public static void ApplyBindingOverride(this InputActionMap actionMap, int bindingIndex, InputBinding bindingOverride)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		InputBinding[] bindings = actionMap.m_Bindings;
		int num = ((bindings != null) ? bindings.Length : 0);
		if (bindingIndex < 0 || bindingIndex >= num)
		{
			throw new ArgumentOutOfRangeException("bindingIndex", $"Cannot apply override to binding at index {bindingIndex} in map '{actionMap}' with only {num} bindings");
		}
		actionMap.m_Bindings[bindingIndex].overridePath = bindingOverride.overridePath;
		actionMap.m_Bindings[bindingIndex].overrideInteractions = bindingOverride.overrideInteractions;
		actionMap.m_Bindings[bindingIndex].overrideProcessors = bindingOverride.overrideProcessors;
		actionMap.ClearCachedActionData();
		actionMap.LazyResolveBindings();
	}

	public static void RemoveBindingOverride(this InputAction action, int bindingIndex)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		action.ApplyBindingOverride(bindingIndex, default(InputBinding));
	}

	public static void RemoveBindingOverride(this InputAction action, InputBinding bindingMask)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		bindingMask.overridePath = null;
		bindingMask.overrideInteractions = null;
		bindingMask.overrideProcessors = null;
		action.ApplyBindingOverride(bindingMask);
	}

	private static void RemoveBindingOverride(this InputActionMap actionMap, InputBinding bindingMask)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		bindingMask.overridePath = null;
		bindingMask.overrideInteractions = null;
		bindingMask.overrideProcessors = null;
		actionMap.ApplyBindingOverride(bindingMask);
	}

	public static void RemoveAllBindingOverrides(this IInputActionCollection2 actions)
	{
		if (actions == null)
		{
			throw new ArgumentNullException("actions");
		}
		using (DeferBindingResolution())
		{
			foreach (InputAction action in actions)
			{
				InputActionMap orCreateActionMap = action.GetOrCreateActionMap();
				InputBinding[] bindings = orCreateActionMap.m_Bindings;
				int num = bindings.LengthSafe();
				for (int i = 0; i < num; i++)
				{
					ref InputBinding reference = ref bindings[i];
					if (reference.TriggersAction(action))
					{
						reference.RemoveOverrides();
					}
				}
				orCreateActionMap.ClearCachedActionData();
				orCreateActionMap.LazyResolveBindings();
			}
		}
	}

	public static void RemoveAllBindingOverrides(this InputAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		string name = action.name;
		InputActionMap orCreateActionMap = action.GetOrCreateActionMap();
		InputBinding[] bindings = orCreateActionMap.m_Bindings;
		if (bindings == null)
		{
			return;
		}
		int num = bindings.Length;
		for (int i = 0; i < num; i++)
		{
			if (string.Compare(bindings[i].action, name, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				bindings[i].overridePath = null;
				bindings[i].overrideInteractions = null;
				bindings[i].overrideProcessors = null;
			}
		}
		orCreateActionMap.ClearCachedActionData();
		orCreateActionMap.LazyResolveBindings();
	}

	public static void ApplyBindingOverrides(this InputActionMap actionMap, IEnumerable<InputBinding> overrides)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		foreach (InputBinding @override in overrides)
		{
			actionMap.ApplyBindingOverride(@override);
		}
	}

	public static void RemoveBindingOverrides(this InputActionMap actionMap, IEnumerable<InputBinding> overrides)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		foreach (InputBinding @override in overrides)
		{
			actionMap.RemoveBindingOverride(@override);
		}
	}

	public static int ApplyBindingOverridesOnMatchingControls(this InputAction action, InputControl control)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		ReadOnlyArray<InputBinding> bindings = action.bindings;
		int count = bindings.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			InputControl inputControl = InputControlPath.TryFindControl(control, bindings[i].path);
			if (inputControl != null)
			{
				action.ApplyBindingOverride(i, inputControl.path);
				num++;
			}
		}
		return num;
	}

	public static int ApplyBindingOverridesOnMatchingControls(this InputActionMap actionMap, InputControl control)
	{
		if (actionMap == null)
		{
			throw new ArgumentNullException("actionMap");
		}
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		ReadOnlyArray<InputAction> actions = actionMap.actions;
		int count = actions.Count;
		int result = 0;
		for (int i = 0; i < count; i++)
		{
			result = actions[i].ApplyBindingOverridesOnMatchingControls(control);
		}
		return result;
	}

	public static string SaveBindingOverridesAsJson(this IInputActionCollection2 actions)
	{
		if (actions == null)
		{
			throw new ArgumentNullException("actions");
		}
		List<InputActionMap.BindingOverrideJson> list = new List<InputActionMap.BindingOverrideJson>();
		foreach (InputBinding binding in actions.bindings)
		{
			actions.AddBindingOverrideJsonTo(binding, list);
		}
		if (list.Count == 0)
		{
			return string.Empty;
		}
		return JsonUtility.ToJson(new InputActionMap.BindingOverrideListJson
		{
			bindings = list
		});
	}

	public static string SaveBindingOverridesAsJson(this InputAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		bool isSingletonAction = action.isSingletonAction;
		InputActionMap orCreateActionMap = action.GetOrCreateActionMap();
		List<InputActionMap.BindingOverrideJson> list = new List<InputActionMap.BindingOverrideJson>();
		foreach (InputBinding binding in action.bindings)
		{
			if (isSingletonAction || binding.TriggersAction(action))
			{
				orCreateActionMap.AddBindingOverrideJsonTo(binding, list, isSingletonAction ? action : null);
			}
		}
		if (list.Count == 0)
		{
			return string.Empty;
		}
		return JsonUtility.ToJson(new InputActionMap.BindingOverrideListJson
		{
			bindings = list
		});
	}

	private static void AddBindingOverrideJsonTo(this IInputActionCollection2 actions, InputBinding binding, List<InputActionMap.BindingOverrideJson> list, InputAction action = null)
	{
		if (binding.hasOverrides)
		{
			if (action == null)
			{
				action = actions.FindAction(binding.action);
			}
			InputActionMap.BindingOverrideJson item = new InputActionMap.BindingOverrideJson
			{
				action = ((action != null && !action.isSingletonAction) ? (action.actionMap.name + "/" + action.name) : null),
				id = binding.id.ToString(),
				path = binding.overridePath,
				interactions = binding.overrideInteractions,
				processors = binding.overrideProcessors
			};
			list.Add(item);
		}
	}

	public static void LoadBindingOverridesFromJson(this IInputActionCollection2 actions, string json, bool removeExisting = true)
	{
		if (actions == null)
		{
			throw new ArgumentNullException("actions");
		}
		using (DeferBindingResolution())
		{
			if (removeExisting)
			{
				actions.RemoveAllBindingOverrides();
			}
			actions.LoadBindingOverridesFromJsonInternal(json);
		}
	}

	public static void LoadBindingOverridesFromJson(this InputAction action, string json, bool removeExisting = true)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		using (DeferBindingResolution())
		{
			if (removeExisting)
			{
				action.RemoveAllBindingOverrides();
			}
			action.GetOrCreateActionMap().LoadBindingOverridesFromJsonInternal(json);
		}
	}

	private static void LoadBindingOverridesFromJsonInternal(this IInputActionCollection2 actions, string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return;
		}
		foreach (InputActionMap.BindingOverrideJson binding in JsonUtility.FromJson<InputActionMap.BindingOverrideListJson>(json).bindings)
		{
			if (!string.IsNullOrEmpty(binding.id))
			{
				InputAction action;
				int num = actions.FindBinding(new InputBinding
				{
					m_Id = binding.id
				}, out action);
				if (num != -1)
				{
					action.ApplyBindingOverride(num, new InputBinding
					{
						overridePath = binding.path,
						overrideInteractions = binding.interactions,
						overrideProcessors = binding.processors
					});
					continue;
				}
			}
			throw new NotImplementedException();
		}
	}

	public static RebindingOperation PerformInteractiveRebinding(this InputAction action, int bindingIndex = -1)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		RebindingOperation rebindingOperation = new RebindingOperation().WithAction(action).OnMatchWaitForAnother(0.05f).WithControlsExcluding("<Pointer>/delta")
			.WithControlsExcluding("<Pointer>/position")
			.WithControlsExcluding("<Touchscreen>/touch*/position")
			.WithControlsExcluding("<Touchscreen>/touch*/delta")
			.WithControlsExcluding("<Mouse>/clickCount")
			.WithMatchingEventsBeingSuppressed();
		if (rebindingOperation.expectedControlType != "Button")
		{
			rebindingOperation.WithCancelingThrough("<Keyboard>/escape");
		}
		if (bindingIndex >= 0)
		{
			ReadOnlyArray<InputBinding> bindings = action.bindings;
			if (bindingIndex >= bindings.Count)
			{
				throw new ArgumentOutOfRangeException($"Binding index {bindingIndex} is out of range for action '{action}' with {bindings.Count} bindings", "bindings");
			}
			if (bindings[bindingIndex].isComposite)
			{
				throw new InvalidOperationException($"Cannot perform rebinding on composite binding '{bindings[bindingIndex]}' of '{action}'");
			}
			rebindingOperation.WithTargetBinding(bindingIndex);
		}
		return rebindingOperation;
	}

	internal static DeferBindingResolutionWrapper DeferBindingResolution()
	{
		if (s_DeferBindingResolutionWrapper == null)
		{
			s_DeferBindingResolutionWrapper = new DeferBindingResolutionWrapper();
		}
		s_DeferBindingResolutionWrapper.Acquire();
		return s_DeferBindingResolutionWrapper;
	}
}
