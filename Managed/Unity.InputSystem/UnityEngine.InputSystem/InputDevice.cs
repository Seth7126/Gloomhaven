using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem;

public class InputDevice : InputControl
{
	[Serializable]
	[Flags]
	internal enum DeviceFlags
	{
		UpdateBeforeRender = 1,
		HasStateCallbacks = 2,
		HasControlsWithDefaultState = 4,
		HasDontResetControls = 0x400,
		HasEventMerger = 0x2000,
		HasEventPreProcessor = 0x4000,
		Remote = 8,
		Native = 0x10,
		DisabledInFrontend = 0x20,
		DisabledInRuntime = 0x80,
		DisabledWhileInBackground = 0x100,
		DisabledStateHasBeenQueriedFromRuntime = 0x40,
		CanRunInBackground = 0x800,
		CanRunInBackgroundHasBeenQueried = 0x1000
	}

	public const int InvalidDeviceId = 0;

	internal const int kLocalParticipantId = 0;

	internal const int kInvalidDeviceIndex = -1;

	internal DeviceFlags m_DeviceFlags;

	internal int m_DeviceId;

	internal int m_ParticipantId;

	internal int m_DeviceIndex;

	internal InputDeviceDescription m_Description;

	internal double m_LastUpdateTimeInternal;

	internal uint m_CurrentUpdateStepCount;

	internal InternedString[] m_AliasesForEachControl;

	internal InternedString[] m_UsagesForEachControl;

	internal InputControl[] m_UsageToControl;

	internal InputControl[] m_ChildrenForEachControl;

	internal uint[] m_StateOffsetToControlMap;

	internal const int kControlIndexBits = 10;

	internal const int kStateOffsetBits = 13;

	internal const int kStateSizeBits = 9;

	public InputDeviceDescription description => m_Description;

	public bool enabled
	{
		get
		{
			if ((m_DeviceFlags & (DeviceFlags.DisabledInFrontend | DeviceFlags.DisabledWhileInBackground)) != 0)
			{
				return false;
			}
			return QueryEnabledStateFromRuntime();
		}
	}

	public bool canRunInBackground
	{
		get
		{
			if ((m_DeviceFlags & DeviceFlags.CanRunInBackgroundHasBeenQueried) != 0)
			{
				return (m_DeviceFlags & DeviceFlags.CanRunInBackground) != 0;
			}
			QueryCanRunInBackground command = QueryCanRunInBackground.Create();
			m_DeviceFlags |= DeviceFlags.CanRunInBackgroundHasBeenQueried;
			if (ExecuteCommand(ref command) >= 0 && command.canRunInBackground)
			{
				m_DeviceFlags |= DeviceFlags.CanRunInBackground;
				return true;
			}
			m_DeviceFlags &= ~DeviceFlags.CanRunInBackground;
			return false;
		}
	}

	public bool added => m_DeviceIndex != -1;

	public bool remote => (m_DeviceFlags & DeviceFlags.Remote) == DeviceFlags.Remote;

	public bool native => (m_DeviceFlags & DeviceFlags.Native) == DeviceFlags.Native;

	public bool updateBeforeRender => (m_DeviceFlags & DeviceFlags.UpdateBeforeRender) == DeviceFlags.UpdateBeforeRender;

	public int deviceId => m_DeviceId;

	public double lastUpdateTime => m_LastUpdateTimeInternal - InputRuntime.s_CurrentTimeOffsetToRealtimeSinceStartup;

	public bool wasUpdatedThisFrame => m_CurrentUpdateStepCount == InputUpdate.s_UpdateStepCount;

	public ReadOnlyArray<InputControl> allControls => new ReadOnlyArray<InputControl>(m_ChildrenForEachControl);

	public override Type valueType => typeof(byte[]);

	public override int valueSizeInBytes => (int)m_StateBlock.alignedSizeInBytes;

	[Obsolete("Use 'InputSystem.devices' instead. (UnityUpgradable) -> InputSystem.devices", false)]
	public static ReadOnlyArray<InputDevice> all => InputSystem.devices;

	internal bool disabledInFrontend
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.DisabledInFrontend) != 0;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.DisabledInFrontend;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.DisabledInFrontend;
			}
		}
	}

	internal bool disabledInRuntime
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.DisabledInRuntime) != 0;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.DisabledInRuntime;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.DisabledInRuntime;
			}
		}
	}

	internal bool disabledWhileInBackground
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.DisabledWhileInBackground) != 0;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.DisabledWhileInBackground;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.DisabledWhileInBackground;
			}
		}
	}

	internal bool hasControlsWithDefaultState
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.HasControlsWithDefaultState) == DeviceFlags.HasControlsWithDefaultState;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.HasControlsWithDefaultState;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.HasControlsWithDefaultState;
			}
		}
	}

	internal bool hasDontResetControls
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.HasDontResetControls) == DeviceFlags.HasDontResetControls;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.HasDontResetControls;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.HasDontResetControls;
			}
		}
	}

	internal bool hasStateCallbacks
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.HasStateCallbacks) == DeviceFlags.HasStateCallbacks;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.HasStateCallbacks;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.HasStateCallbacks;
			}
		}
	}

	internal bool hasEventMerger
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.HasEventMerger) == DeviceFlags.HasEventMerger;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.HasEventMerger;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.HasEventMerger;
			}
		}
	}

	internal bool hasEventPreProcessor
	{
		get
		{
			return (m_DeviceFlags & DeviceFlags.HasEventPreProcessor) == DeviceFlags.HasEventPreProcessor;
		}
		set
		{
			if (value)
			{
				m_DeviceFlags |= DeviceFlags.HasEventPreProcessor;
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.HasEventPreProcessor;
			}
		}
	}

	public InputDevice()
	{
		m_DeviceId = 0;
		m_ParticipantId = 0;
		m_DeviceIndex = -1;
	}

	public unsafe override object ReadValueFromBufferAsObject(void* buffer, int bufferSize)
	{
		throw new NotImplementedException();
	}

	public unsafe override object ReadValueFromStateAsObject(void* statePtr)
	{
		if (m_DeviceIndex == -1)
		{
			return null;
		}
		uint alignedSizeInBytes = base.stateBlock.alignedSizeInBytes;
		byte[] array = new byte[alignedSizeInBytes];
		fixed (byte* destination = array)
		{
			byte* source = (byte*)statePtr + m_StateBlock.byteOffset;
			UnsafeUtility.MemCpy(destination, source, alignedSizeInBytes);
		}
		return array;
	}

	public unsafe override void ReadValueFromStateIntoBuffer(void* statePtr, void* bufferPtr, int bufferSize)
	{
		if (statePtr == null)
		{
			throw new ArgumentNullException("statePtr");
		}
		if (bufferPtr == null)
		{
			throw new ArgumentNullException("bufferPtr");
		}
		if (bufferSize < valueSizeInBytes)
		{
			throw new ArgumentException($"Buffer too small (expected: {valueSizeInBytes}, actual: {bufferSize}");
		}
		byte* source = (byte*)statePtr + m_StateBlock.byteOffset;
		UnsafeUtility.MemCpy(bufferPtr, source, m_StateBlock.alignedSizeInBytes);
	}

	public unsafe override bool CompareValue(void* firstStatePtr, void* secondStatePtr)
	{
		if (firstStatePtr == null)
		{
			throw new ArgumentNullException("firstStatePtr");
		}
		if (secondStatePtr == null)
		{
			throw new ArgumentNullException("secondStatePtr");
		}
		byte* ptr = (byte*)firstStatePtr + m_StateBlock.byteOffset;
		byte* ptr2 = (byte*)firstStatePtr + m_StateBlock.byteOffset;
		return UnsafeUtility.MemCmp(ptr, ptr2, m_StateBlock.alignedSizeInBytes) == 0;
	}

	internal void NotifyConfigurationChanged()
	{
		base.isConfigUpToDate = false;
		for (int i = 0; i < m_ChildrenForEachControl.Length; i++)
		{
			m_ChildrenForEachControl[i].isConfigUpToDate = false;
		}
		m_DeviceFlags &= ~DeviceFlags.DisabledStateHasBeenQueriedFromRuntime;
		OnConfigurationChanged();
	}

	public virtual void MakeCurrent()
	{
	}

	protected virtual void OnAdded()
	{
	}

	protected virtual void OnRemoved()
	{
	}

	protected virtual void OnConfigurationChanged()
	{
	}

	public unsafe long ExecuteCommand<TCommand>(ref TCommand command) where TCommand : struct, IInputDeviceCommandInfo
	{
		InputDeviceCommand* command2 = (InputDeviceCommand*)UnsafeUtility.AddressOf(ref command);
		InputManager s_Manager = InputSystem.s_Manager;
		s_Manager.m_DeviceCommandCallbacks.LockForChanges();
		for (int i = 0; i < s_Manager.m_DeviceCommandCallbacks.length; i++)
		{
			try
			{
				long? num = s_Manager.m_DeviceCommandCallbacks[i](this, command2);
				if (num.HasValue)
				{
					return num.Value;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.GetType().Name + " while executing 'InputSystem.onDeviceCommand' callbacks");
				Debug.LogException(ex);
			}
		}
		s_Manager.m_DeviceCommandCallbacks.UnlockForChanges();
		return ExecuteCommand((InputDeviceCommand*)UnsafeUtility.AddressOf(ref command));
	}

	protected unsafe virtual long ExecuteCommand(InputDeviceCommand* commandPtr)
	{
		return InputRuntime.s_Instance.DeviceCommand(deviceId, commandPtr);
	}

	internal bool QueryEnabledStateFromRuntime()
	{
		if ((m_DeviceFlags & DeviceFlags.DisabledStateHasBeenQueriedFromRuntime) == 0)
		{
			QueryEnabledStateCommand command = QueryEnabledStateCommand.Create();
			if (ExecuteCommand(ref command) >= 0)
			{
				if (command.isEnabled)
				{
					m_DeviceFlags &= ~DeviceFlags.DisabledInRuntime;
				}
				else
				{
					m_DeviceFlags |= DeviceFlags.DisabledInRuntime;
				}
			}
			else
			{
				m_DeviceFlags &= ~DeviceFlags.DisabledInRuntime;
			}
			m_DeviceFlags |= DeviceFlags.DisabledStateHasBeenQueriedFromRuntime;
		}
		return (m_DeviceFlags & DeviceFlags.DisabledInRuntime) == 0;
	}

	internal static uint EncodeStateOffsetToControlMapEntry(uint controlIndex, uint stateOffsetInBits, uint stateSizeInBits)
	{
		return (stateOffsetInBits << 19) | (stateSizeInBits << 10) | controlIndex;
	}

	internal static void DecodeStateOffsetToControlMapEntry(uint entry, out uint controlIndex, out uint stateOffset, out uint stateSize)
	{
		controlIndex = entry & 0x3FF;
		stateOffset = entry >> 19;
		stateSize = (entry >> 10) & 0x1FF;
	}

	internal void AddDeviceUsage(InternedString usage)
	{
		int count = m_UsageToControl.LengthSafe() + m_UsageCount;
		if (m_UsageCount == 0)
		{
			m_UsageStartIndex = count;
		}
		ArrayHelpers.AppendWithCapacity(ref m_UsagesForEachControl, ref count, usage);
		m_UsageCount++;
	}

	internal void RemoveDeviceUsage(InternedString usage)
	{
		int count = m_UsageToControl.LengthSafe() + m_UsageCount;
		int num = m_UsagesForEachControl.IndexOfValue(usage, m_UsageStartIndex, count);
		if (num != -1)
		{
			m_UsagesForEachControl.EraseAtWithCapacity(ref count, num);
			m_UsageCount--;
			if (m_UsageCount == 0)
			{
				m_UsageStartIndex = 0;
			}
		}
	}

	internal void ClearDeviceUsages()
	{
		for (int i = m_UsageStartIndex; i < m_UsageCount; i++)
		{
			m_UsagesForEachControl[i] = default(InternedString);
		}
		m_UsageCount = 0;
	}

	internal bool RequestSync()
	{
		RequestSyncCommand command = RequestSyncCommand.Create();
		return base.device.ExecuteCommand(ref command) >= 0;
	}

	internal bool RequestReset()
	{
		RequestResetCommand command = RequestResetCommand.Create();
		return base.device.ExecuteCommand(ref command) >= 0;
	}

	internal bool ExecuteEnableCommand()
	{
		EnableDeviceCommand command = EnableDeviceCommand.Create();
		return base.device.ExecuteCommand(ref command) >= 0;
	}

	internal bool ExecuteDisableCommand()
	{
		DisableDeviceCommand command = DisableDeviceCommand.Create();
		return base.device.ExecuteCommand(ref command) >= 0;
	}

	internal void NotifyAdded()
	{
		OnAdded();
	}

	internal void NotifyRemoved()
	{
		OnRemoved();
	}

	internal static TDevice Build<TDevice>(string layoutName = null, string layoutVariants = null, InputDeviceDescription deviceDescription = default(InputDeviceDescription), bool noPrecompiledLayouts = false) where TDevice : InputDevice
	{
		InternedString key = new InternedString(layoutName);
		if (key.IsEmpty())
		{
			key = InputControlLayout.s_Layouts.TryFindLayoutForType(typeof(TDevice));
			if (key.IsEmpty())
			{
				key = new InternedString(typeof(TDevice).Name);
			}
		}
		if (!noPrecompiledLayouts && string.IsNullOrEmpty(layoutVariants) && InputControlLayout.s_Layouts.precompiledLayouts.TryGetValue(key, out var value))
		{
			return (TDevice)value.factoryMethod();
		}
		using (InputDeviceBuilder.Ref())
		{
			InputDeviceBuilder.instance.Setup(key, new InternedString(layoutVariants), deviceDescription);
			InputDevice inputDevice = InputDeviceBuilder.instance.Finish();
			if (!(inputDevice is TDevice result))
			{
				throw new ArgumentException("Expected device of type '" + typeof(TDevice).Name + "' but got device of type '" + inputDevice.GetType().Name + "' instead", "TDevice");
			}
			return result;
		}
	}
}
