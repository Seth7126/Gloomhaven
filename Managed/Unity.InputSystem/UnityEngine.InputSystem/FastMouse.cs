using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem;

internal class FastMouse : Mouse, IInputStateCallbackReceiver, IEventMerger
{
	public const string metadata = "AutoWindowSpace;Vector2;Button;Axis;Digital;Integer;Mouse;Pointer";

	public FastMouse()
	{
		InputControlExtensions.DeviceBuilder deviceBuilder = this.Setup(21, 10, 2).WithName("Mouse").WithDisplayName("Mouse")
			.WithChildren(0, 13)
			.WithLayout(new InternedString("Mouse"))
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1297044819),
				sizeInBits = 392u
			});
		InternedString kVector2Layout = new InternedString("Vector2");
		InternedString kButtonLayout = new InternedString("Button");
		InternedString kAxisLayout = new InternedString("Axis");
		InternedString kDigitalLayout = new InternedString("Digital");
		InternedString kIntegerLayout = new InternedString("Integer");
		Vector2Control vector2Control = Initialize_ctrlMouseposition(kVector2Layout, this);
		Vector2Control vector2Control2 = Initialize_ctrlMousedelta(kVector2Layout, this);
		Vector2Control vector2Control3 = Initialize_ctrlMousescroll(kVector2Layout, this);
		ButtonControl buttonControl = Initialize_ctrlMousepress(kButtonLayout, this);
		ButtonControl control = Initialize_ctrlMouseleftButton(kButtonLayout, this);
		ButtonControl control2 = Initialize_ctrlMouserightButton(kButtonLayout, this);
		ButtonControl buttonControl2 = Initialize_ctrlMousemiddleButton(kButtonLayout, this);
		ButtonControl control3 = Initialize_ctrlMouseforwardButton(kButtonLayout, this);
		ButtonControl control4 = Initialize_ctrlMousebackButton(kButtonLayout, this);
		AxisControl control5 = Initialize_ctrlMousepressure(kAxisLayout, this);
		Vector2Control vector2Control4 = Initialize_ctrlMouseradius(kVector2Layout, this);
		Initialize_ctrlMousepointerId(kDigitalLayout, this);
		IntegerControl integerControl = Initialize_ctrlMouseclickCount(kIntegerLayout, this);
		AxisControl x = Initialize_ctrlMousepositionx(kAxisLayout, vector2Control);
		AxisControl y = Initialize_ctrlMousepositiony(kAxisLayout, vector2Control);
		AxisControl x2 = Initialize_ctrlMousedeltax(kAxisLayout, vector2Control2);
		AxisControl y2 = Initialize_ctrlMousedeltay(kAxisLayout, vector2Control2);
		AxisControl axisControl = Initialize_ctrlMousescrollx(kAxisLayout, vector2Control3);
		AxisControl axisControl2 = Initialize_ctrlMousescrolly(kAxisLayout, vector2Control3);
		AxisControl x3 = Initialize_ctrlMouseradiusx(kAxisLayout, vector2Control4);
		AxisControl y3 = Initialize_ctrlMouseradiusy(kAxisLayout, vector2Control4);
		deviceBuilder.WithControlUsage(0, new InternedString("Point"), vector2Control);
		deviceBuilder.WithControlUsage(1, new InternedString("Secondary2DMotion"), vector2Control2);
		deviceBuilder.WithControlUsage(2, new InternedString("ScrollHorizontal"), axisControl);
		deviceBuilder.WithControlUsage(3, new InternedString("ScrollVertical"), axisControl2);
		deviceBuilder.WithControlUsage(4, new InternedString("PrimaryAction"), control);
		deviceBuilder.WithControlUsage(5, new InternedString("SecondaryAction"), control2);
		deviceBuilder.WithControlUsage(6, new InternedString("Forward"), control3);
		deviceBuilder.WithControlUsage(7, new InternedString("Back"), control4);
		deviceBuilder.WithControlUsage(8, new InternedString("Pressure"), control5);
		deviceBuilder.WithControlUsage(9, new InternedString("Radius"), vector2Control4);
		deviceBuilder.WithControlAlias(0, new InternedString("horizontal"));
		deviceBuilder.WithControlAlias(1, new InternedString("vertical"));
		base.scroll = vector2Control3;
		base.leftButton = control;
		base.middleButton = buttonControl2;
		base.rightButton = control2;
		base.backButton = control4;
		base.forwardButton = control3;
		base.clickCount = integerControl;
		base.position = vector2Control;
		base.delta = vector2Control2;
		base.radius = vector2Control4;
		base.pressure = control5;
		base.press = buttonControl;
		vector2Control.x = x;
		vector2Control.y = y;
		vector2Control2.x = x2;
		vector2Control2.y = y2;
		vector2Control3.x = axisControl;
		vector2Control3.y = axisControl2;
		vector2Control4.x = x3;
		vector2Control4.y = y3;
		deviceBuilder.WithStateOffsetToControlIndexMap(new uint[17]
		{
			32781u, 16809998u, 33587215u, 50364432u, 67141649u, 83918866u, 100664323u, 100664324u, 101188613u, 101712902u,
			102237191u, 102761480u, 117456908u, 134250505u, 167804947u, 184582164u, 201327627u
		});
		deviceBuilder.Finish();
	}

	private Vector2Control Initialize_ctrlMouseposition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 0).WithParent(parent)
			.WithChildren(13, 2)
			.WithName("position")
			.WithDisplayName("Position")
			.WithLayout(kVector2Layout)
			.WithUsages(0, 1)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 0u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlMousedelta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 1).WithParent(parent)
			.WithChildren(15, 2)
			.WithName("delta")
			.WithDisplayName("Delta")
			.WithLayout(kVector2Layout)
			.WithUsages(1, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 8u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlMousescroll(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 2).WithParent(parent)
			.WithChildren(17, 2)
			.WithName("scroll")
			.WithDisplayName("Scroll")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 16u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private ButtonControl Initialize_ctrlMousepress(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 3).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Press")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 24u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlMouseleftButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 4).WithParent(parent)
			.WithName("leftButton")
			.WithDisplayName("Left Button")
			.WithShortDisplayName("LMB")
			.WithLayout(kButtonLayout)
			.WithUsages(4, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 24u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlMouserightButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 5).WithParent(parent)
			.WithName("rightButton")
			.WithDisplayName("Right Button")
			.WithShortDisplayName("RMB")
			.WithLayout(kButtonLayout)
			.WithUsages(5, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 24u,
				bitOffset = 1u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlMousemiddleButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 6).WithParent(parent)
			.WithName("middleButton")
			.WithDisplayName("Middle Button")
			.WithShortDisplayName("MMB")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 24u,
				bitOffset = 2u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlMouseforwardButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 7).WithParent(parent)
			.WithName("forwardButton")
			.WithDisplayName("Forward")
			.WithLayout(kButtonLayout)
			.WithUsages(6, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 24u,
				bitOffset = 3u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlMousebackButton(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 8).WithParent(parent)
			.WithName("backButton")
			.WithDisplayName("Back")
			.WithLayout(kButtonLayout)
			.WithUsages(7, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 24u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private AxisControl Initialize_ctrlMousepressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 9).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Pressure")
			.WithLayout(kAxisLayout)
			.WithUsages(8, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 32u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.WithDefaultState(1)
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlMouseradius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 10).WithParent(parent)
			.WithChildren(19, 2)
			.WithName("radius")
			.WithDisplayName("Radius")
			.WithLayout(kVector2Layout)
			.WithUsages(9, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 40u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private IntegerControl Initialize_ctrlMousepointerId(InternedString kDigitalLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 11).WithParent(parent)
			.WithName("pointerId")
			.WithDisplayName("pointerId")
			.WithLayout(kDigitalLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 48u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.Finish();
		return integerControl;
	}

	private IntegerControl Initialize_ctrlMouseclickCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 12).WithParent(parent)
			.WithName("clickCount")
			.WithDisplayName("Click Count")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1431521364),
				byteOffset = 28u,
				bitOffset = 0u,
				sizeInBits = 16u
			})
			.Finish();
		return integerControl;
	}

	private AxisControl Initialize_ctrlMousepositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 13).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Position X")
			.WithShortDisplayName("Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 0u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMousepositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 14).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Position Y")
			.WithShortDisplayName("Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 4u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMousedeltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 15).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Delta X")
			.WithShortDisplayName("Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 8u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMousedeltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 16).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Delta Y")
			.WithShortDisplayName("Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 12u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMousescrollx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 17).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Scroll Left/Right")
			.WithShortDisplayName("Scroll Left/Right")
			.WithLayout(kAxisLayout)
			.WithUsages(2, 1)
			.WithAliases(0, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 16u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMousescrolly(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 18).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Scroll Up/Down")
			.WithShortDisplayName("Scroll Wheel")
			.WithLayout(kAxisLayout)
			.WithUsages(3, 1)
			.WithAliases(1, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 20u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMouseradiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 19).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Radius X")
			.WithShortDisplayName("Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 40u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlMouseradiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 20).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Radius Y")
			.WithShortDisplayName("Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 44u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	protected new void OnNextUpdate()
	{
		InputState.Change(base.delta, Vector2.zero, InputState.currentUpdateType);
		InputState.Change(base.scroll, Vector2.zero, InputState.currentUpdateType);
	}

	protected new unsafe void OnStateEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type != 1398030676)
		{
			base.OnStateEvent(eventPtr);
			return;
		}
		StateEvent* ptr = StateEvent.FromUnchecked(eventPtr);
		if (ptr->stateFormat != MouseState.Format)
		{
			base.OnStateEvent(eventPtr);
			return;
		}
		MouseState state = *(MouseState*)ptr->state;
		MouseState* ptr2 = (MouseState*)((byte*)base.currentStatePtr + m_StateBlock.byteOffset);
		state.delta += ptr2->delta;
		state.scroll += ptr2->scroll;
		InputState.Change(this, ref state, InputState.currentUpdateType, eventPtr);
	}

	void IInputStateCallbackReceiver.OnNextUpdate()
	{
		OnNextUpdate();
	}

	void IInputStateCallbackReceiver.OnStateEvent(InputEventPtr eventPtr)
	{
		OnStateEvent(eventPtr);
	}

	internal unsafe static bool MergeForward(InputEventPtr currentEventPtr, InputEventPtr nextEventPtr)
	{
		if (currentEventPtr.type != 1398030676 || nextEventPtr.type != 1398030676)
		{
			return false;
		}
		StateEvent* ptr = StateEvent.FromUnchecked(currentEventPtr);
		StateEvent* ptr2 = StateEvent.FromUnchecked(nextEventPtr);
		if (ptr->stateFormat != MouseState.Format || ptr2->stateFormat != MouseState.Format)
		{
			return false;
		}
		MouseState* state = (MouseState*)ptr->state;
		MouseState* state2 = (MouseState*)ptr2->state;
		if (state->buttons != state2->buttons || state->clickCount != state2->clickCount)
		{
			return false;
		}
		state2->delta += state->delta;
		state2->scroll += state->scroll;
		return true;
	}

	bool IEventMerger.MergeForward(InputEventPtr currentEventPtr, InputEventPtr nextEventPtr)
	{
		return MergeForward(currentEventPtr, nextEventPtr);
	}
}
