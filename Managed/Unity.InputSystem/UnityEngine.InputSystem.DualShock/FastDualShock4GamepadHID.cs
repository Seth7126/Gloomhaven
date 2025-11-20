using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Processors;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.DualShock;

internal class FastDualShock4GamepadHID : DualShock4GamepadHID
{
	public const string metadata = "StickDeadzone;AxisDeadzone;Stick;Vector2;Dpad;Button;Axis;DpadAxis;DiscreteButton;DualShock4GamepadHID;DualShockGamepad;Gamepad";

	public FastDualShock4GamepadHID()
	{
		InputControlExtensions.DeviceBuilder deviceBuilder = this.Setup(37, 11, 8).WithName("DualShock4GamepadHID").WithDisplayName("PlayStation Controller")
			.WithChildren(0, 19)
			.WithLayout(new InternedString("DualShock4GamepadHID"))
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1212761120),
				sizeInBits = 80u
			});
		InternedString kStickLayout = new InternedString("Stick");
		InternedString kDpadLayout = new InternedString("Dpad");
		InternedString kButtonLayout = new InternedString("Button");
		InternedString kAxisLayout = new InternedString("Axis");
		InternedString kDpadAxisLayout = new InternedString("DpadAxis");
		InternedString kDiscreteButtonLayout = new InternedString("DiscreteButton");
		StickControl stickControl = Initialize_ctrlDualShock4GamepadHIDleftStick(kStickLayout, this);
		StickControl stickControl2 = Initialize_ctrlDualShock4GamepadHIDrightStick(kStickLayout, this);
		DpadControl dpadControl = Initialize_ctrlDualShock4GamepadHIDdpad(kDpadLayout, this);
		ButtonControl control = Initialize_ctrlDualShock4GamepadHIDbuttonWest(kButtonLayout, this);
		ButtonControl control2 = Initialize_ctrlDualShock4GamepadHIDbuttonSouth(kButtonLayout, this);
		ButtonControl control3 = Initialize_ctrlDualShock4GamepadHIDbuttonEast(kButtonLayout, this);
		ButtonControl buttonControl = Initialize_ctrlDualShock4GamepadHIDbuttonNorth(kButtonLayout, this);
		ButtonControl l = Initialize_ctrlDualShock4GamepadHIDleftShoulder(kButtonLayout, this);
		ButtonControl r = Initialize_ctrlDualShock4GamepadHIDrightShoulder(kButtonLayout, this);
		ButtonControl buttonControl2 = Initialize_ctrlDualShock4GamepadHIDleftTriggerButton(kButtonLayout, this);
		ButtonControl buttonControl3 = Initialize_ctrlDualShock4GamepadHIDrightTriggerButton(kButtonLayout, this);
		ButtonControl buttonControl4 = Initialize_ctrlDualShock4GamepadHIDselect(kButtonLayout, this);
		ButtonControl control4 = Initialize_ctrlDualShock4GamepadHIDstart(kButtonLayout, this);
		ButtonControl l2 = Initialize_ctrlDualShock4GamepadHIDleftStickPress(kButtonLayout, this);
		ButtonControl r2 = Initialize_ctrlDualShock4GamepadHIDrightStickPress(kButtonLayout, this);
		ButtonControl buttonControl5 = Initialize_ctrlDualShock4GamepadHIDsystemButton(kButtonLayout, this);
		ButtonControl buttonControl6 = Initialize_ctrlDualShock4GamepadHIDtouchpadButton(kButtonLayout, this);
		ButtonControl buttonControl7 = Initialize_ctrlDualShock4GamepadHIDleftTrigger(kButtonLayout, this);
		ButtonControl buttonControl8 = Initialize_ctrlDualShock4GamepadHIDrightTrigger(kButtonLayout, this);
		ButtonControl up = Initialize_ctrlDualShock4GamepadHIDleftStickup(kButtonLayout, stickControl);
		AxisControl x = Initialize_ctrlDualShock4GamepadHIDleftStickx(kAxisLayout, stickControl);
		AxisControl y = Initialize_ctrlDualShock4GamepadHIDleftSticky(kAxisLayout, stickControl);
		ButtonControl down = Initialize_ctrlDualShock4GamepadHIDleftStickdown(kButtonLayout, stickControl);
		ButtonControl left = Initialize_ctrlDualShock4GamepadHIDleftStickleft(kButtonLayout, stickControl);
		ButtonControl right = Initialize_ctrlDualShock4GamepadHIDleftStickright(kButtonLayout, stickControl);
		ButtonControl up2 = Initialize_ctrlDualShock4GamepadHIDrightStickup(kButtonLayout, stickControl2);
		AxisControl x2 = Initialize_ctrlDualShock4GamepadHIDrightStickx(kAxisLayout, stickControl2);
		AxisControl y2 = Initialize_ctrlDualShock4GamepadHIDrightSticky(kAxisLayout, stickControl2);
		ButtonControl down2 = Initialize_ctrlDualShock4GamepadHIDrightStickdown(kButtonLayout, stickControl2);
		ButtonControl left2 = Initialize_ctrlDualShock4GamepadHIDrightStickleft(kButtonLayout, stickControl2);
		ButtonControl right2 = Initialize_ctrlDualShock4GamepadHIDrightStickright(kButtonLayout, stickControl2);
		DpadControl.DpadAxisControl x3 = Initialize_ctrlDualShock4GamepadHIDdpadx(kDpadAxisLayout, dpadControl);
		DpadControl.DpadAxisControl y3 = Initialize_ctrlDualShock4GamepadHIDdpady(kDpadAxisLayout, dpadControl);
		DiscreteButtonControl up3 = Initialize_ctrlDualShock4GamepadHIDdpadup(kDiscreteButtonLayout, dpadControl);
		DiscreteButtonControl down3 = Initialize_ctrlDualShock4GamepadHIDdpaddown(kDiscreteButtonLayout, dpadControl);
		DiscreteButtonControl left3 = Initialize_ctrlDualShock4GamepadHIDdpadleft(kDiscreteButtonLayout, dpadControl);
		DiscreteButtonControl right3 = Initialize_ctrlDualShock4GamepadHIDdpadright(kDiscreteButtonLayout, dpadControl);
		deviceBuilder.WithControlUsage(0, new InternedString("Primary2DMotion"), stickControl);
		deviceBuilder.WithControlUsage(1, new InternedString("Secondary2DMotion"), stickControl2);
		deviceBuilder.WithControlUsage(2, new InternedString("Hatswitch"), dpadControl);
		deviceBuilder.WithControlUsage(3, new InternedString("SecondaryAction"), control);
		deviceBuilder.WithControlUsage(4, new InternedString("PrimaryAction"), control2);
		deviceBuilder.WithControlUsage(5, new InternedString("Submit"), control2);
		deviceBuilder.WithControlUsage(6, new InternedString("Back"), control3);
		deviceBuilder.WithControlUsage(7, new InternedString("Cancel"), control3);
		deviceBuilder.WithControlUsage(8, new InternedString("Menu"), control4);
		deviceBuilder.WithControlUsage(9, new InternedString("SecondaryTrigger"), buttonControl7);
		deviceBuilder.WithControlUsage(10, new InternedString("SecondaryTrigger"), buttonControl8);
		deviceBuilder.WithControlAlias(0, new InternedString("x"));
		deviceBuilder.WithControlAlias(1, new InternedString("square"));
		deviceBuilder.WithControlAlias(2, new InternedString("a"));
		deviceBuilder.WithControlAlias(3, new InternedString("cross"));
		deviceBuilder.WithControlAlias(4, new InternedString("b"));
		deviceBuilder.WithControlAlias(5, new InternedString("circle"));
		deviceBuilder.WithControlAlias(6, new InternedString("y"));
		deviceBuilder.WithControlAlias(7, new InternedString("triangle"));
		base.leftTriggerButton = buttonControl2;
		base.rightTriggerButton = buttonControl3;
		base.playStationButton = buttonControl5;
		base.touchpadButton = buttonControl6;
		base.optionsButton = control4;
		base.shareButton = buttonControl4;
		base.L1 = l;
		base.R1 = r;
		base.L2 = buttonControl7;
		base.R2 = buttonControl8;
		base.L3 = l2;
		base.R3 = r2;
		base.buttonWest = control;
		base.buttonNorth = buttonControl;
		base.buttonSouth = control2;
		base.buttonEast = control3;
		base.leftStickButton = l2;
		base.rightStickButton = r2;
		base.startButton = control4;
		base.selectButton = buttonControl4;
		base.dpad = dpadControl;
		base.leftShoulder = l;
		base.rightShoulder = r;
		base.leftStick = stickControl;
		base.rightStick = stickControl2;
		base.leftTrigger = buttonControl7;
		base.rightTrigger = buttonControl8;
		stickControl.up = up;
		stickControl.down = down;
		stickControl.left = left;
		stickControl.right = right;
		stickControl.x = x;
		stickControl.y = y;
		stickControl2.up = up2;
		stickControl2.down = down2;
		stickControl2.left = left2;
		stickControl2.right = right2;
		stickControl2.x = x2;
		stickControl2.y = y2;
		dpadControl.up = up3;
		dpadControl.down = down3;
		dpadControl.left = left3;
		dpadControl.right = right3;
		dpadControl.x = x3;
		dpadControl.y = y3;
		deviceBuilder.WithStateOffsetToControlIndexMap(new uint[34]
		{
			4202516u, 4202519u, 4202520u, 8396819u, 8396821u, 8396822u, 12591130u, 12591133u, 12591134u, 16785433u,
			16785435u, 16785436u, 20975647u, 20975648u, 20975649u, 20975650u, 20975651u, 20975652u, 23069699u, 23593988u,
			24118277u, 24642566u, 25166855u, 25691144u, 26215433u, 26739722u, 27264011u, 27788300u, 28312589u, 28836878u,
			29361167u, 29885456u, 33562641u, 37756946u
		});
		deviceBuilder.Finish();
	}

	private StickControl Initialize_ctrlDualShock4GamepadHIDleftStick(InternedString kStickLayout, InputControl parent)
	{
		StickControl stickControl = new StickControl();
		stickControl.Setup().At(this, 0).WithParent(parent)
			.WithChildren(19, 6)
			.WithName("leftStick")
			.WithDisplayName("Left Stick")
			.WithShortDisplayName("LS")
			.WithLayout(kStickLayout)
			.WithUsages(0, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447244354),
				byteOffset = 1u,
				bitOffset = 0u,
				sizeInBits = 16u
			})
			.WithProcessor<InputProcessor<Vector2>, Vector2>(new StickDeadzoneProcessor())
			.Finish();
		return stickControl;
	}

	private StickControl Initialize_ctrlDualShock4GamepadHIDrightStick(InternedString kStickLayout, InputControl parent)
	{
		StickControl stickControl = new StickControl();
		stickControl.Setup().At(this, 1).WithParent(parent)
			.WithChildren(25, 6)
			.WithName("rightStick")
			.WithDisplayName("Right Stick")
			.WithShortDisplayName("RS")
			.WithLayout(kStickLayout)
			.WithUsages(1, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447244354),
				byteOffset = 3u,
				bitOffset = 0u,
				sizeInBits = 16u
			})
			.WithProcessor<InputProcessor<Vector2>, Vector2>(new StickDeadzoneProcessor())
			.Finish();
		return stickControl;
	}

	private DpadControl Initialize_ctrlDualShock4GamepadHIDdpad(InternedString kDpadLayout, InputControl parent)
	{
		DpadControl dpadControl = new DpadControl();
		dpadControl.Setup().At(this, 2).WithParent(parent)
			.WithChildren(31, 6)
			.WithName("dpad")
			.WithDisplayName("D-Pad")
			.WithLayout(kDpadLayout)
			.WithUsages(2, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.WithDefaultState(8)
			.Finish();
		return dpadControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDbuttonWest(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 3).WithParent(parent)
			.WithName("buttonWest")
			.WithDisplayName("Square")
			.WithShortDisplayName("Square")
			.WithLayout(kButtonLayout)
			.WithUsages(3, 1)
			.WithAliases(0, 2)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDbuttonSouth(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 4).WithParent(parent)
			.WithName("buttonSouth")
			.WithDisplayName("Cross")
			.WithShortDisplayName("Cross")
			.WithLayout(kButtonLayout)
			.WithUsages(4, 2)
			.WithAliases(2, 2)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 5u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDbuttonEast(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 5).WithParent(parent)
			.WithName("buttonEast")
			.WithDisplayName("Circle")
			.WithShortDisplayName("Circle")
			.WithLayout(kButtonLayout)
			.WithUsages(6, 2)
			.WithAliases(4, 2)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 6u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDbuttonNorth(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 6).WithParent(parent)
			.WithName("buttonNorth")
			.WithDisplayName("Triangle")
			.WithShortDisplayName("Triangle")
			.WithLayout(kButtonLayout)
			.WithAliases(6, 2)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 7u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftShoulder(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 7).WithParent(parent)
			.WithName("leftShoulder")
			.WithDisplayName("L1")
			.WithShortDisplayName("L1")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightShoulder(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 8).WithParent(parent)
			.WithName("rightShoulder")
			.WithDisplayName("R1")
			.WithShortDisplayName("R1")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 1u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftTriggerButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 9).WithParent(parent)
			.WithName("leftTriggerButton")
			.WithDisplayName("leftTriggerButton")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 2u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightTriggerButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 10).WithParent(parent)
			.WithName("rightTriggerButton")
			.WithDisplayName("rightTriggerButton")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 3u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDselect(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 11).WithParent(parent)
			.WithName("select")
			.WithDisplayName("Share")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDstart(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 12).WithParent(parent)
			.WithName("start")
			.WithDisplayName("Options")
			.WithLayout(kButtonLayout)
			.WithUsages(8, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 5u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftStickPress(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 13).WithParent(parent)
			.WithName("leftStickPress")
			.WithDisplayName("L3")
			.WithShortDisplayName("L3")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 6u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightStickPress(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 14).WithParent(parent)
			.WithName("rightStickPress")
			.WithDisplayName("R3")
			.WithShortDisplayName("R3")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 6u,
				bitOffset = 7u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDsystemButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 15).WithParent(parent)
			.WithName("systemButton")
			.WithDisplayName("System")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 7u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDtouchpadButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 16).WithParent(parent)
			.WithName("touchpadButton")
			.WithDisplayName("Touchpad Press")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 7u,
				bitOffset = 1u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftTrigger(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 17).WithParent(parent)
			.WithName("leftTrigger")
			.WithDisplayName("L2")
			.WithShortDisplayName("L2")
			.WithLayout(kButtonLayout)
			.WithUsages(9, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 8u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightTrigger(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 18).WithParent(parent)
			.WithName("rightTrigger")
			.WithDisplayName("R2")
			.WithShortDisplayName("R2")
			.WithLayout(kButtonLayout)
			.WithUsages(10, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 9u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftStickup(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMax = 0.5f,
			invert = true,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 19).WithParent(parent)
			.WithName("up")
			.WithDisplayName("Left Stick Up")
			.WithShortDisplayName("LS Up")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 2u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private AxisControl Initialize_ctrlDualShock4GamepadHIDleftStickx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl obj = new AxisControl
		{
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 20).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Left Stick X")
			.WithShortDisplayName("LS X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 1u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(-1, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private AxisControl Initialize_ctrlDualShock4GamepadHIDleftSticky(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl obj = new AxisControl
		{
			invert = true,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 21).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Left Stick Y")
			.WithShortDisplayName("LS Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 2u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(-1, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftStickdown(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMin = 0.5f,
			clampMax = 1f,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 22).WithParent(parent)
			.WithName("down")
			.WithDisplayName("Left Stick Down")
			.WithShortDisplayName("LS Down")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 2u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftStickleft(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMax = 0.5f,
			invert = true,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 23).WithParent(parent)
			.WithName("left")
			.WithDisplayName("Left Stick Left")
			.WithShortDisplayName("LS Left")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 1u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDleftStickright(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMin = 0.5f,
			clampMax = 1f,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 24).WithParent(parent)
			.WithName("right")
			.WithDisplayName("Left Stick Right")
			.WithShortDisplayName("LS Right")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 1u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightStickup(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMax = 0.5f,
			invert = true,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 25).WithParent(parent)
			.WithName("up")
			.WithDisplayName("Right Stick Up")
			.WithShortDisplayName("RS Up")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 4u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private AxisControl Initialize_ctrlDualShock4GamepadHIDrightStickx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl obj = new AxisControl
		{
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 26).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Right Stick X")
			.WithShortDisplayName("RS X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 3u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(-1, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private AxisControl Initialize_ctrlDualShock4GamepadHIDrightSticky(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl obj = new AxisControl
		{
			invert = true,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 27).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Right Stick Y")
			.WithShortDisplayName("RS Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 4u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(-1, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightStickdown(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMin = 0.5f,
			clampMax = 1f,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 28).WithParent(parent)
			.WithName("down")
			.WithDisplayName("Right Stick Down")
			.WithShortDisplayName("RS Down")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 4u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightStickleft(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMax = 0.5f,
			invert = true,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 29).WithParent(parent)
			.WithName("left")
			.WithDisplayName("Right Stick Left")
			.WithShortDisplayName("RS Left")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 3u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private ButtonControl Initialize_ctrlDualShock4GamepadHIDrightStickright(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl obj = new ButtonControl
		{
			clamp = AxisControl.Clamp.BeforeNormalize,
			clampMin = 0.5f,
			clampMax = 1f,
			normalize = true,
			normalizeMax = 1f,
			normalizeZero = 0.5f
		};
		obj.Setup().At(this, 30).WithParent(parent)
			.WithName("right")
			.WithDisplayName("Right Stick Right")
			.WithShortDisplayName("RS Right")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 3u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithDefaultState(127)
			.WithMinAndMax(0, 1)
			.WithProcessor<InputProcessor<float>, float>(new AxisDeadzoneProcessor())
			.Finish();
		return obj;
	}

	private DpadControl.DpadAxisControl Initialize_ctrlDualShock4GamepadHIDdpadx(InternedString kDpadAxisLayout, InputControl parent)
	{
		DpadControl.DpadAxisControl dpadAxisControl = new DpadControl.DpadAxisControl();
		dpadAxisControl.Setup().At(this, 31).WithParent(parent)
			.WithName("x")
			.WithDisplayName("D-Pad X")
			.WithShortDisplayName("D-Pad X")
			.WithLayout(kDpadAxisLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.Finish();
		dpadAxisControl.component = 0;
		return dpadAxisControl;
	}

	private DpadControl.DpadAxisControl Initialize_ctrlDualShock4GamepadHIDdpady(InternedString kDpadAxisLayout, InputControl parent)
	{
		DpadControl.DpadAxisControl dpadAxisControl = new DpadControl.DpadAxisControl();
		dpadAxisControl.Setup().At(this, 32).WithParent(parent)
			.WithName("y")
			.WithDisplayName("D-Pad Y")
			.WithShortDisplayName("D-Pad Y")
			.WithLayout(kDpadAxisLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.Finish();
		dpadAxisControl.component = 1;
		return dpadAxisControl;
	}

	private DiscreteButtonControl Initialize_ctrlDualShock4GamepadHIDdpadup(InternedString kDiscreteButtonLayout, InputControl parent)
	{
		DiscreteButtonControl obj = new DiscreteButtonControl
		{
			minValue = 7,
			maxValue = 1,
			wrapAtValue = 7,
			nullValue = 8
		};
		obj.Setup().At(this, 33).WithParent(parent)
			.WithName("up")
			.WithDisplayName("D-Pad Up")
			.WithShortDisplayName("D-Pad Up")
			.WithLayout(kDiscreteButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return obj;
	}

	private DiscreteButtonControl Initialize_ctrlDualShock4GamepadHIDdpaddown(InternedString kDiscreteButtonLayout, InputControl parent)
	{
		DiscreteButtonControl obj = new DiscreteButtonControl
		{
			minValue = 3,
			maxValue = 5
		};
		obj.Setup().At(this, 34).WithParent(parent)
			.WithName("down")
			.WithDisplayName("D-Pad Down")
			.WithShortDisplayName("D-Pad Down")
			.WithLayout(kDiscreteButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return obj;
	}

	private DiscreteButtonControl Initialize_ctrlDualShock4GamepadHIDdpadleft(InternedString kDiscreteButtonLayout, InputControl parent)
	{
		DiscreteButtonControl obj = new DiscreteButtonControl
		{
			minValue = 5,
			maxValue = 7
		};
		obj.Setup().At(this, 35).WithParent(parent)
			.WithName("left")
			.WithDisplayName("D-Pad Left")
			.WithShortDisplayName("D-Pad Left")
			.WithLayout(kDiscreteButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return obj;
	}

	private DiscreteButtonControl Initialize_ctrlDualShock4GamepadHIDdpadright(InternedString kDiscreteButtonLayout, InputControl parent)
	{
		DiscreteButtonControl obj = new DiscreteButtonControl
		{
			minValue = 1,
			maxValue = 3
		};
		obj.Setup().At(this, 36).WithParent(parent)
			.WithName("right")
			.WithDisplayName("D-Pad Right")
			.WithShortDisplayName("D-Pad Right")
			.WithLayout(kDiscreteButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 5u,
				bitOffset = 0u,
				sizeInBits = 4u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return obj;
	}
}
