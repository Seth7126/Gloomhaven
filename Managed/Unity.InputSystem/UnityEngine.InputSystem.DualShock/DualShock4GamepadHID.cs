using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock.LowLevel;
using UnityEngine.InputSystem.Layouts;

namespace UnityEngine.InputSystem.DualShock;

[InputControlLayout(stateType = typeof(DualShock4HIDInputReport), hideInUI = true)]
public class DualShock4GamepadHID : DualShockGamepad
{
	private float? m_LowFrequencyMotorSpeed;

	private float? m_HighFrequenceyMotorSpeed;

	private Color? m_LightBarColor;

	public ButtonControl leftTriggerButton { get; protected set; }

	public ButtonControl rightTriggerButton { get; protected set; }

	public ButtonControl playStationButton { get; protected set; }

	protected override void FinishSetup()
	{
		leftTriggerButton = GetChildControl<ButtonControl>("leftTriggerButton");
		rightTriggerButton = GetChildControl<ButtonControl>("rightTriggerButton");
		playStationButton = GetChildControl<ButtonControl>("systemButton");
		base.FinishSetup();
	}

	public override void PauseHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue || m_LightBarColor.HasValue)
		{
			DualShockHIDOutputReport command = DualShockHIDOutputReport.Create();
			command.SetMotorSpeeds(0f, 0f);
			if (m_LightBarColor.HasValue)
			{
				command.SetColor(Color.black);
			}
			ExecuteCommand(ref command);
		}
	}

	public override void ResetHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue || m_LightBarColor.HasValue)
		{
			DualShockHIDOutputReport command = DualShockHIDOutputReport.Create();
			command.SetMotorSpeeds(0f, 0f);
			if (m_LightBarColor.HasValue)
			{
				command.SetColor(Color.black);
			}
			ExecuteCommand(ref command);
			m_HighFrequenceyMotorSpeed = null;
			m_LowFrequencyMotorSpeed = null;
			m_LightBarColor = null;
		}
	}

	public override void ResumeHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue || m_LightBarColor.HasValue)
		{
			DualShockHIDOutputReport command = DualShockHIDOutputReport.Create();
			if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue)
			{
				command.SetMotorSpeeds(m_LowFrequencyMotorSpeed.Value, m_HighFrequenceyMotorSpeed.Value);
			}
			if (m_LightBarColor.HasValue)
			{
				command.SetColor(m_LightBarColor.Value);
			}
			ExecuteCommand(ref command);
		}
	}

	public override void SetLightBarColor(Color color)
	{
		DualShockHIDOutputReport command = DualShockHIDOutputReport.Create();
		command.SetColor(color);
		ExecuteCommand(ref command);
		m_LightBarColor = color;
	}

	public override void SetMotorSpeeds(float lowFrequency, float highFrequency)
	{
		DualShockHIDOutputReport command = DualShockHIDOutputReport.Create();
		command.SetMotorSpeeds(lowFrequency, highFrequency);
		ExecuteCommand(ref command);
		m_LowFrequencyMotorSpeed = lowFrequency;
		m_HighFrequenceyMotorSpeed = highFrequency;
	}

	public bool SetMotorSpeedsAndLightBarColor(float lowFrequency, float highFrequency, Color color)
	{
		DualShockHIDOutputReport command = DualShockHIDOutputReport.Create();
		command.SetMotorSpeeds(lowFrequency, highFrequency);
		command.SetColor(color);
		long num = ExecuteCommand(ref command);
		m_LowFrequencyMotorSpeed = lowFrequency;
		m_HighFrequenceyMotorSpeed = highFrequency;
		m_LightBarColor = color;
		return num >= 0;
	}
}
