using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem;

internal class FastTouchscreen : Touchscreen
{
	public const string metadata = "AutoWindowSpace;Touch;Vector2;Analog;TouchPress;Button;Axis;Integer;TouchPhase;Double;Touchscreen;Pointer";

	public FastTouchscreen()
	{
		InputControlExtensions.DeviceBuilder deviceBuilder = this.Setup(242, 5, 0).WithName("Touchscreen").WithDisplayName("Touchscreen")
			.WithChildren(0, 16)
			.WithLayout(new InternedString("Touchscreen"))
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414742866),
				sizeInBits = 4928u
			});
		InternedString kTouchLayout = new InternedString("Touch");
		InternedString kVector2Layout = new InternedString("Vector2");
		InternedString kAnalogLayout = new InternedString("Analog");
		InternedString kTouchPressLayout = new InternedString("TouchPress");
		InternedString kIntegerLayout = new InternedString("Integer");
		InternedString kAxisLayout = new InternedString("Axis");
		InternedString kTouchPhaseLayout = new InternedString("TouchPhase");
		InternedString kButtonLayout = new InternedString("Button");
		InternedString kDoubleLayout = new InternedString("Double");
		TouchControl touchControl = Initialize_ctrlTouchscreenprimaryTouch(kTouchLayout, this);
		Vector2Control vector2Control = Initialize_ctrlTouchscreenposition(kVector2Layout, this);
		Vector2Control vector2Control2 = Initialize_ctrlTouchscreendelta(kVector2Layout, this);
		AxisControl control = Initialize_ctrlTouchscreenpressure(kAnalogLayout, this);
		Vector2Control vector2Control3 = Initialize_ctrlTouchscreenradius(kVector2Layout, this);
		TouchPressControl touchPressControl = Initialize_ctrlTouchscreenpress(kTouchPressLayout, this);
		TouchControl touchControl2 = Initialize_ctrlTouchscreentouch0(kTouchLayout, this);
		TouchControl touchControl3 = Initialize_ctrlTouchscreentouch1(kTouchLayout, this);
		TouchControl touchControl4 = Initialize_ctrlTouchscreentouch2(kTouchLayout, this);
		TouchControl touchControl5 = Initialize_ctrlTouchscreentouch3(kTouchLayout, this);
		TouchControl touchControl6 = Initialize_ctrlTouchscreentouch4(kTouchLayout, this);
		TouchControl touchControl7 = Initialize_ctrlTouchscreentouch5(kTouchLayout, this);
		TouchControl touchControl8 = Initialize_ctrlTouchscreentouch6(kTouchLayout, this);
		TouchControl touchControl9 = Initialize_ctrlTouchscreentouch7(kTouchLayout, this);
		TouchControl touchControl10 = Initialize_ctrlTouchscreentouch8(kTouchLayout, this);
		TouchControl touchControl11 = Initialize_ctrlTouchscreentouch9(kTouchLayout, this);
		IntegerControl touchId = Initialize_ctrlTouchscreenprimaryTouchtouchId(kIntegerLayout, touchControl);
		Vector2Control vector2Control4 = Initialize_ctrlTouchscreenprimaryTouchposition(kVector2Layout, touchControl);
		Vector2Control vector2Control5 = Initialize_ctrlTouchscreenprimaryTouchdelta(kVector2Layout, touchControl);
		AxisControl axisControl = Initialize_ctrlTouchscreenprimaryTouchpressure(kAxisLayout, touchControl);
		Vector2Control vector2Control6 = Initialize_ctrlTouchscreenprimaryTouchradius(kVector2Layout, touchControl);
		TouchPhaseControl phase = Initialize_ctrlTouchscreenprimaryTouchphase(kTouchPhaseLayout, touchControl);
		TouchPressControl touchPressControl2 = Initialize_ctrlTouchscreenprimaryTouchpress(kTouchPressLayout, touchControl);
		IntegerControl tapCount = Initialize_ctrlTouchscreenprimaryTouchtapCount(kIntegerLayout, touchControl);
		ButtonControl indirectTouch = Initialize_ctrlTouchscreenprimaryTouchindirectTouch(kButtonLayout, touchControl);
		ButtonControl buttonControl = Initialize_ctrlTouchscreenprimaryTouchtap(kButtonLayout, touchControl);
		DoubleControl startTime = Initialize_ctrlTouchscreenprimaryTouchstartTime(kDoubleLayout, touchControl);
		Vector2Control vector2Control7 = Initialize_ctrlTouchscreenprimaryTouchstartPosition(kVector2Layout, touchControl);
		AxisControl x = Initialize_ctrlTouchscreenprimaryTouchpositionx(kAxisLayout, vector2Control4);
		AxisControl y = Initialize_ctrlTouchscreenprimaryTouchpositiony(kAxisLayout, vector2Control4);
		AxisControl x2 = Initialize_ctrlTouchscreenprimaryTouchdeltax(kAxisLayout, vector2Control5);
		AxisControl y2 = Initialize_ctrlTouchscreenprimaryTouchdeltay(kAxisLayout, vector2Control5);
		AxisControl x3 = Initialize_ctrlTouchscreenprimaryTouchradiusx(kAxisLayout, vector2Control6);
		AxisControl y3 = Initialize_ctrlTouchscreenprimaryTouchradiusy(kAxisLayout, vector2Control6);
		AxisControl x4 = Initialize_ctrlTouchscreenprimaryTouchstartPositionx(kAxisLayout, vector2Control7);
		AxisControl y4 = Initialize_ctrlTouchscreenprimaryTouchstartPositiony(kAxisLayout, vector2Control7);
		AxisControl x5 = Initialize_ctrlTouchscreenpositionx(kAxisLayout, vector2Control);
		AxisControl y5 = Initialize_ctrlTouchscreenpositiony(kAxisLayout, vector2Control);
		AxisControl x6 = Initialize_ctrlTouchscreendeltax(kAxisLayout, vector2Control2);
		AxisControl y6 = Initialize_ctrlTouchscreendeltay(kAxisLayout, vector2Control2);
		AxisControl x7 = Initialize_ctrlTouchscreenradiusx(kAxisLayout, vector2Control3);
		AxisControl y7 = Initialize_ctrlTouchscreenradiusy(kAxisLayout, vector2Control3);
		IntegerControl touchId2 = Initialize_ctrlTouchscreentouch0touchId(kIntegerLayout, touchControl2);
		Vector2Control vector2Control8 = Initialize_ctrlTouchscreentouch0position(kVector2Layout, touchControl2);
		Vector2Control vector2Control9 = Initialize_ctrlTouchscreentouch0delta(kVector2Layout, touchControl2);
		AxisControl axisControl2 = Initialize_ctrlTouchscreentouch0pressure(kAxisLayout, touchControl2);
		Vector2Control vector2Control10 = Initialize_ctrlTouchscreentouch0radius(kVector2Layout, touchControl2);
		TouchPhaseControl phase2 = Initialize_ctrlTouchscreentouch0phase(kTouchPhaseLayout, touchControl2);
		TouchPressControl touchPressControl3 = Initialize_ctrlTouchscreentouch0press(kTouchPressLayout, touchControl2);
		IntegerControl tapCount2 = Initialize_ctrlTouchscreentouch0tapCount(kIntegerLayout, touchControl2);
		ButtonControl indirectTouch2 = Initialize_ctrlTouchscreentouch0indirectTouch(kButtonLayout, touchControl2);
		ButtonControl tap = Initialize_ctrlTouchscreentouch0tap(kButtonLayout, touchControl2);
		DoubleControl startTime2 = Initialize_ctrlTouchscreentouch0startTime(kDoubleLayout, touchControl2);
		Vector2Control vector2Control11 = Initialize_ctrlTouchscreentouch0startPosition(kVector2Layout, touchControl2);
		AxisControl x8 = Initialize_ctrlTouchscreentouch0positionx(kAxisLayout, vector2Control8);
		AxisControl y8 = Initialize_ctrlTouchscreentouch0positiony(kAxisLayout, vector2Control8);
		AxisControl x9 = Initialize_ctrlTouchscreentouch0deltax(kAxisLayout, vector2Control9);
		AxisControl y9 = Initialize_ctrlTouchscreentouch0deltay(kAxisLayout, vector2Control9);
		AxisControl x10 = Initialize_ctrlTouchscreentouch0radiusx(kAxisLayout, vector2Control10);
		AxisControl y10 = Initialize_ctrlTouchscreentouch0radiusy(kAxisLayout, vector2Control10);
		AxisControl x11 = Initialize_ctrlTouchscreentouch0startPositionx(kAxisLayout, vector2Control11);
		AxisControl y11 = Initialize_ctrlTouchscreentouch0startPositiony(kAxisLayout, vector2Control11);
		IntegerControl touchId3 = Initialize_ctrlTouchscreentouch1touchId(kIntegerLayout, touchControl3);
		Vector2Control vector2Control12 = Initialize_ctrlTouchscreentouch1position(kVector2Layout, touchControl3);
		Vector2Control vector2Control13 = Initialize_ctrlTouchscreentouch1delta(kVector2Layout, touchControl3);
		AxisControl axisControl3 = Initialize_ctrlTouchscreentouch1pressure(kAxisLayout, touchControl3);
		Vector2Control vector2Control14 = Initialize_ctrlTouchscreentouch1radius(kVector2Layout, touchControl3);
		TouchPhaseControl phase3 = Initialize_ctrlTouchscreentouch1phase(kTouchPhaseLayout, touchControl3);
		TouchPressControl touchPressControl4 = Initialize_ctrlTouchscreentouch1press(kTouchPressLayout, touchControl3);
		IntegerControl tapCount3 = Initialize_ctrlTouchscreentouch1tapCount(kIntegerLayout, touchControl3);
		ButtonControl indirectTouch3 = Initialize_ctrlTouchscreentouch1indirectTouch(kButtonLayout, touchControl3);
		ButtonControl tap2 = Initialize_ctrlTouchscreentouch1tap(kButtonLayout, touchControl3);
		DoubleControl startTime3 = Initialize_ctrlTouchscreentouch1startTime(kDoubleLayout, touchControl3);
		Vector2Control vector2Control15 = Initialize_ctrlTouchscreentouch1startPosition(kVector2Layout, touchControl3);
		AxisControl x12 = Initialize_ctrlTouchscreentouch1positionx(kAxisLayout, vector2Control12);
		AxisControl y12 = Initialize_ctrlTouchscreentouch1positiony(kAxisLayout, vector2Control12);
		AxisControl x13 = Initialize_ctrlTouchscreentouch1deltax(kAxisLayout, vector2Control13);
		AxisControl y13 = Initialize_ctrlTouchscreentouch1deltay(kAxisLayout, vector2Control13);
		AxisControl x14 = Initialize_ctrlTouchscreentouch1radiusx(kAxisLayout, vector2Control14);
		AxisControl y14 = Initialize_ctrlTouchscreentouch1radiusy(kAxisLayout, vector2Control14);
		AxisControl x15 = Initialize_ctrlTouchscreentouch1startPositionx(kAxisLayout, vector2Control15);
		AxisControl y15 = Initialize_ctrlTouchscreentouch1startPositiony(kAxisLayout, vector2Control15);
		IntegerControl touchId4 = Initialize_ctrlTouchscreentouch2touchId(kIntegerLayout, touchControl4);
		Vector2Control vector2Control16 = Initialize_ctrlTouchscreentouch2position(kVector2Layout, touchControl4);
		Vector2Control vector2Control17 = Initialize_ctrlTouchscreentouch2delta(kVector2Layout, touchControl4);
		AxisControl axisControl4 = Initialize_ctrlTouchscreentouch2pressure(kAxisLayout, touchControl4);
		Vector2Control vector2Control18 = Initialize_ctrlTouchscreentouch2radius(kVector2Layout, touchControl4);
		TouchPhaseControl phase4 = Initialize_ctrlTouchscreentouch2phase(kTouchPhaseLayout, touchControl4);
		TouchPressControl touchPressControl5 = Initialize_ctrlTouchscreentouch2press(kTouchPressLayout, touchControl4);
		IntegerControl tapCount4 = Initialize_ctrlTouchscreentouch2tapCount(kIntegerLayout, touchControl4);
		ButtonControl indirectTouch4 = Initialize_ctrlTouchscreentouch2indirectTouch(kButtonLayout, touchControl4);
		ButtonControl tap3 = Initialize_ctrlTouchscreentouch2tap(kButtonLayout, touchControl4);
		DoubleControl startTime4 = Initialize_ctrlTouchscreentouch2startTime(kDoubleLayout, touchControl4);
		Vector2Control vector2Control19 = Initialize_ctrlTouchscreentouch2startPosition(kVector2Layout, touchControl4);
		AxisControl x16 = Initialize_ctrlTouchscreentouch2positionx(kAxisLayout, vector2Control16);
		AxisControl y16 = Initialize_ctrlTouchscreentouch2positiony(kAxisLayout, vector2Control16);
		AxisControl x17 = Initialize_ctrlTouchscreentouch2deltax(kAxisLayout, vector2Control17);
		AxisControl y17 = Initialize_ctrlTouchscreentouch2deltay(kAxisLayout, vector2Control17);
		AxisControl x18 = Initialize_ctrlTouchscreentouch2radiusx(kAxisLayout, vector2Control18);
		AxisControl y18 = Initialize_ctrlTouchscreentouch2radiusy(kAxisLayout, vector2Control18);
		AxisControl x19 = Initialize_ctrlTouchscreentouch2startPositionx(kAxisLayout, vector2Control19);
		AxisControl y19 = Initialize_ctrlTouchscreentouch2startPositiony(kAxisLayout, vector2Control19);
		IntegerControl touchId5 = Initialize_ctrlTouchscreentouch3touchId(kIntegerLayout, touchControl5);
		Vector2Control vector2Control20 = Initialize_ctrlTouchscreentouch3position(kVector2Layout, touchControl5);
		Vector2Control vector2Control21 = Initialize_ctrlTouchscreentouch3delta(kVector2Layout, touchControl5);
		AxisControl axisControl5 = Initialize_ctrlTouchscreentouch3pressure(kAxisLayout, touchControl5);
		Vector2Control vector2Control22 = Initialize_ctrlTouchscreentouch3radius(kVector2Layout, touchControl5);
		TouchPhaseControl phase5 = Initialize_ctrlTouchscreentouch3phase(kTouchPhaseLayout, touchControl5);
		TouchPressControl touchPressControl6 = Initialize_ctrlTouchscreentouch3press(kTouchPressLayout, touchControl5);
		IntegerControl tapCount5 = Initialize_ctrlTouchscreentouch3tapCount(kIntegerLayout, touchControl5);
		ButtonControl indirectTouch5 = Initialize_ctrlTouchscreentouch3indirectTouch(kButtonLayout, touchControl5);
		ButtonControl tap4 = Initialize_ctrlTouchscreentouch3tap(kButtonLayout, touchControl5);
		DoubleControl startTime5 = Initialize_ctrlTouchscreentouch3startTime(kDoubleLayout, touchControl5);
		Vector2Control vector2Control23 = Initialize_ctrlTouchscreentouch3startPosition(kVector2Layout, touchControl5);
		AxisControl x20 = Initialize_ctrlTouchscreentouch3positionx(kAxisLayout, vector2Control20);
		AxisControl y20 = Initialize_ctrlTouchscreentouch3positiony(kAxisLayout, vector2Control20);
		AxisControl x21 = Initialize_ctrlTouchscreentouch3deltax(kAxisLayout, vector2Control21);
		AxisControl y21 = Initialize_ctrlTouchscreentouch3deltay(kAxisLayout, vector2Control21);
		AxisControl x22 = Initialize_ctrlTouchscreentouch3radiusx(kAxisLayout, vector2Control22);
		AxisControl y22 = Initialize_ctrlTouchscreentouch3radiusy(kAxisLayout, vector2Control22);
		AxisControl x23 = Initialize_ctrlTouchscreentouch3startPositionx(kAxisLayout, vector2Control23);
		AxisControl y23 = Initialize_ctrlTouchscreentouch3startPositiony(kAxisLayout, vector2Control23);
		IntegerControl touchId6 = Initialize_ctrlTouchscreentouch4touchId(kIntegerLayout, touchControl6);
		Vector2Control vector2Control24 = Initialize_ctrlTouchscreentouch4position(kVector2Layout, touchControl6);
		Vector2Control vector2Control25 = Initialize_ctrlTouchscreentouch4delta(kVector2Layout, touchControl6);
		AxisControl axisControl6 = Initialize_ctrlTouchscreentouch4pressure(kAxisLayout, touchControl6);
		Vector2Control vector2Control26 = Initialize_ctrlTouchscreentouch4radius(kVector2Layout, touchControl6);
		TouchPhaseControl phase6 = Initialize_ctrlTouchscreentouch4phase(kTouchPhaseLayout, touchControl6);
		TouchPressControl touchPressControl7 = Initialize_ctrlTouchscreentouch4press(kTouchPressLayout, touchControl6);
		IntegerControl tapCount6 = Initialize_ctrlTouchscreentouch4tapCount(kIntegerLayout, touchControl6);
		ButtonControl indirectTouch6 = Initialize_ctrlTouchscreentouch4indirectTouch(kButtonLayout, touchControl6);
		ButtonControl tap5 = Initialize_ctrlTouchscreentouch4tap(kButtonLayout, touchControl6);
		DoubleControl startTime6 = Initialize_ctrlTouchscreentouch4startTime(kDoubleLayout, touchControl6);
		Vector2Control vector2Control27 = Initialize_ctrlTouchscreentouch4startPosition(kVector2Layout, touchControl6);
		AxisControl x24 = Initialize_ctrlTouchscreentouch4positionx(kAxisLayout, vector2Control24);
		AxisControl y24 = Initialize_ctrlTouchscreentouch4positiony(kAxisLayout, vector2Control24);
		AxisControl x25 = Initialize_ctrlTouchscreentouch4deltax(kAxisLayout, vector2Control25);
		AxisControl y25 = Initialize_ctrlTouchscreentouch4deltay(kAxisLayout, vector2Control25);
		AxisControl x26 = Initialize_ctrlTouchscreentouch4radiusx(kAxisLayout, vector2Control26);
		AxisControl y26 = Initialize_ctrlTouchscreentouch4radiusy(kAxisLayout, vector2Control26);
		AxisControl x27 = Initialize_ctrlTouchscreentouch4startPositionx(kAxisLayout, vector2Control27);
		AxisControl y27 = Initialize_ctrlTouchscreentouch4startPositiony(kAxisLayout, vector2Control27);
		IntegerControl touchId7 = Initialize_ctrlTouchscreentouch5touchId(kIntegerLayout, touchControl7);
		Vector2Control vector2Control28 = Initialize_ctrlTouchscreentouch5position(kVector2Layout, touchControl7);
		Vector2Control vector2Control29 = Initialize_ctrlTouchscreentouch5delta(kVector2Layout, touchControl7);
		AxisControl axisControl7 = Initialize_ctrlTouchscreentouch5pressure(kAxisLayout, touchControl7);
		Vector2Control vector2Control30 = Initialize_ctrlTouchscreentouch5radius(kVector2Layout, touchControl7);
		TouchPhaseControl phase7 = Initialize_ctrlTouchscreentouch5phase(kTouchPhaseLayout, touchControl7);
		TouchPressControl touchPressControl8 = Initialize_ctrlTouchscreentouch5press(kTouchPressLayout, touchControl7);
		IntegerControl tapCount7 = Initialize_ctrlTouchscreentouch5tapCount(kIntegerLayout, touchControl7);
		ButtonControl indirectTouch7 = Initialize_ctrlTouchscreentouch5indirectTouch(kButtonLayout, touchControl7);
		ButtonControl tap6 = Initialize_ctrlTouchscreentouch5tap(kButtonLayout, touchControl7);
		DoubleControl startTime7 = Initialize_ctrlTouchscreentouch5startTime(kDoubleLayout, touchControl7);
		Vector2Control vector2Control31 = Initialize_ctrlTouchscreentouch5startPosition(kVector2Layout, touchControl7);
		AxisControl x28 = Initialize_ctrlTouchscreentouch5positionx(kAxisLayout, vector2Control28);
		AxisControl y28 = Initialize_ctrlTouchscreentouch5positiony(kAxisLayout, vector2Control28);
		AxisControl x29 = Initialize_ctrlTouchscreentouch5deltax(kAxisLayout, vector2Control29);
		AxisControl y29 = Initialize_ctrlTouchscreentouch5deltay(kAxisLayout, vector2Control29);
		AxisControl x30 = Initialize_ctrlTouchscreentouch5radiusx(kAxisLayout, vector2Control30);
		AxisControl y30 = Initialize_ctrlTouchscreentouch5radiusy(kAxisLayout, vector2Control30);
		AxisControl x31 = Initialize_ctrlTouchscreentouch5startPositionx(kAxisLayout, vector2Control31);
		AxisControl y31 = Initialize_ctrlTouchscreentouch5startPositiony(kAxisLayout, vector2Control31);
		IntegerControl touchId8 = Initialize_ctrlTouchscreentouch6touchId(kIntegerLayout, touchControl8);
		Vector2Control vector2Control32 = Initialize_ctrlTouchscreentouch6position(kVector2Layout, touchControl8);
		Vector2Control vector2Control33 = Initialize_ctrlTouchscreentouch6delta(kVector2Layout, touchControl8);
		AxisControl axisControl8 = Initialize_ctrlTouchscreentouch6pressure(kAxisLayout, touchControl8);
		Vector2Control vector2Control34 = Initialize_ctrlTouchscreentouch6radius(kVector2Layout, touchControl8);
		TouchPhaseControl phase8 = Initialize_ctrlTouchscreentouch6phase(kTouchPhaseLayout, touchControl8);
		TouchPressControl touchPressControl9 = Initialize_ctrlTouchscreentouch6press(kTouchPressLayout, touchControl8);
		IntegerControl tapCount8 = Initialize_ctrlTouchscreentouch6tapCount(kIntegerLayout, touchControl8);
		ButtonControl indirectTouch8 = Initialize_ctrlTouchscreentouch6indirectTouch(kButtonLayout, touchControl8);
		ButtonControl tap7 = Initialize_ctrlTouchscreentouch6tap(kButtonLayout, touchControl8);
		DoubleControl startTime8 = Initialize_ctrlTouchscreentouch6startTime(kDoubleLayout, touchControl8);
		Vector2Control vector2Control35 = Initialize_ctrlTouchscreentouch6startPosition(kVector2Layout, touchControl8);
		AxisControl x32 = Initialize_ctrlTouchscreentouch6positionx(kAxisLayout, vector2Control32);
		AxisControl y32 = Initialize_ctrlTouchscreentouch6positiony(kAxisLayout, vector2Control32);
		AxisControl x33 = Initialize_ctrlTouchscreentouch6deltax(kAxisLayout, vector2Control33);
		AxisControl y33 = Initialize_ctrlTouchscreentouch6deltay(kAxisLayout, vector2Control33);
		AxisControl x34 = Initialize_ctrlTouchscreentouch6radiusx(kAxisLayout, vector2Control34);
		AxisControl y34 = Initialize_ctrlTouchscreentouch6radiusy(kAxisLayout, vector2Control34);
		AxisControl x35 = Initialize_ctrlTouchscreentouch6startPositionx(kAxisLayout, vector2Control35);
		AxisControl y35 = Initialize_ctrlTouchscreentouch6startPositiony(kAxisLayout, vector2Control35);
		IntegerControl touchId9 = Initialize_ctrlTouchscreentouch7touchId(kIntegerLayout, touchControl9);
		Vector2Control vector2Control36 = Initialize_ctrlTouchscreentouch7position(kVector2Layout, touchControl9);
		Vector2Control vector2Control37 = Initialize_ctrlTouchscreentouch7delta(kVector2Layout, touchControl9);
		AxisControl axisControl9 = Initialize_ctrlTouchscreentouch7pressure(kAxisLayout, touchControl9);
		Vector2Control vector2Control38 = Initialize_ctrlTouchscreentouch7radius(kVector2Layout, touchControl9);
		TouchPhaseControl phase9 = Initialize_ctrlTouchscreentouch7phase(kTouchPhaseLayout, touchControl9);
		TouchPressControl touchPressControl10 = Initialize_ctrlTouchscreentouch7press(kTouchPressLayout, touchControl9);
		IntegerControl tapCount9 = Initialize_ctrlTouchscreentouch7tapCount(kIntegerLayout, touchControl9);
		ButtonControl indirectTouch9 = Initialize_ctrlTouchscreentouch7indirectTouch(kButtonLayout, touchControl9);
		ButtonControl tap8 = Initialize_ctrlTouchscreentouch7tap(kButtonLayout, touchControl9);
		DoubleControl startTime9 = Initialize_ctrlTouchscreentouch7startTime(kDoubleLayout, touchControl9);
		Vector2Control vector2Control39 = Initialize_ctrlTouchscreentouch7startPosition(kVector2Layout, touchControl9);
		AxisControl x36 = Initialize_ctrlTouchscreentouch7positionx(kAxisLayout, vector2Control36);
		AxisControl y36 = Initialize_ctrlTouchscreentouch7positiony(kAxisLayout, vector2Control36);
		AxisControl x37 = Initialize_ctrlTouchscreentouch7deltax(kAxisLayout, vector2Control37);
		AxisControl y37 = Initialize_ctrlTouchscreentouch7deltay(kAxisLayout, vector2Control37);
		AxisControl x38 = Initialize_ctrlTouchscreentouch7radiusx(kAxisLayout, vector2Control38);
		AxisControl y38 = Initialize_ctrlTouchscreentouch7radiusy(kAxisLayout, vector2Control38);
		AxisControl x39 = Initialize_ctrlTouchscreentouch7startPositionx(kAxisLayout, vector2Control39);
		AxisControl y39 = Initialize_ctrlTouchscreentouch7startPositiony(kAxisLayout, vector2Control39);
		IntegerControl touchId10 = Initialize_ctrlTouchscreentouch8touchId(kIntegerLayout, touchControl10);
		Vector2Control vector2Control40 = Initialize_ctrlTouchscreentouch8position(kVector2Layout, touchControl10);
		Vector2Control vector2Control41 = Initialize_ctrlTouchscreentouch8delta(kVector2Layout, touchControl10);
		AxisControl axisControl10 = Initialize_ctrlTouchscreentouch8pressure(kAxisLayout, touchControl10);
		Vector2Control vector2Control42 = Initialize_ctrlTouchscreentouch8radius(kVector2Layout, touchControl10);
		TouchPhaseControl phase10 = Initialize_ctrlTouchscreentouch8phase(kTouchPhaseLayout, touchControl10);
		TouchPressControl touchPressControl11 = Initialize_ctrlTouchscreentouch8press(kTouchPressLayout, touchControl10);
		IntegerControl tapCount10 = Initialize_ctrlTouchscreentouch8tapCount(kIntegerLayout, touchControl10);
		ButtonControl indirectTouch10 = Initialize_ctrlTouchscreentouch8indirectTouch(kButtonLayout, touchControl10);
		ButtonControl tap9 = Initialize_ctrlTouchscreentouch8tap(kButtonLayout, touchControl10);
		DoubleControl startTime10 = Initialize_ctrlTouchscreentouch8startTime(kDoubleLayout, touchControl10);
		Vector2Control vector2Control43 = Initialize_ctrlTouchscreentouch8startPosition(kVector2Layout, touchControl10);
		AxisControl x40 = Initialize_ctrlTouchscreentouch8positionx(kAxisLayout, vector2Control40);
		AxisControl y40 = Initialize_ctrlTouchscreentouch8positiony(kAxisLayout, vector2Control40);
		AxisControl x41 = Initialize_ctrlTouchscreentouch8deltax(kAxisLayout, vector2Control41);
		AxisControl y41 = Initialize_ctrlTouchscreentouch8deltay(kAxisLayout, vector2Control41);
		AxisControl x42 = Initialize_ctrlTouchscreentouch8radiusx(kAxisLayout, vector2Control42);
		AxisControl y42 = Initialize_ctrlTouchscreentouch8radiusy(kAxisLayout, vector2Control42);
		AxisControl x43 = Initialize_ctrlTouchscreentouch8startPositionx(kAxisLayout, vector2Control43);
		AxisControl y43 = Initialize_ctrlTouchscreentouch8startPositiony(kAxisLayout, vector2Control43);
		IntegerControl touchId11 = Initialize_ctrlTouchscreentouch9touchId(kIntegerLayout, touchControl11);
		Vector2Control vector2Control44 = Initialize_ctrlTouchscreentouch9position(kVector2Layout, touchControl11);
		Vector2Control vector2Control45 = Initialize_ctrlTouchscreentouch9delta(kVector2Layout, touchControl11);
		AxisControl axisControl11 = Initialize_ctrlTouchscreentouch9pressure(kAxisLayout, touchControl11);
		Vector2Control vector2Control46 = Initialize_ctrlTouchscreentouch9radius(kVector2Layout, touchControl11);
		TouchPhaseControl phase11 = Initialize_ctrlTouchscreentouch9phase(kTouchPhaseLayout, touchControl11);
		TouchPressControl touchPressControl12 = Initialize_ctrlTouchscreentouch9press(kTouchPressLayout, touchControl11);
		IntegerControl tapCount11 = Initialize_ctrlTouchscreentouch9tapCount(kIntegerLayout, touchControl11);
		ButtonControl indirectTouch11 = Initialize_ctrlTouchscreentouch9indirectTouch(kButtonLayout, touchControl11);
		ButtonControl tap10 = Initialize_ctrlTouchscreentouch9tap(kButtonLayout, touchControl11);
		DoubleControl startTime11 = Initialize_ctrlTouchscreentouch9startTime(kDoubleLayout, touchControl11);
		Vector2Control vector2Control47 = Initialize_ctrlTouchscreentouch9startPosition(kVector2Layout, touchControl11);
		AxisControl x44 = Initialize_ctrlTouchscreentouch9positionx(kAxisLayout, vector2Control44);
		AxisControl y44 = Initialize_ctrlTouchscreentouch9positiony(kAxisLayout, vector2Control44);
		AxisControl x45 = Initialize_ctrlTouchscreentouch9deltax(kAxisLayout, vector2Control45);
		AxisControl y45 = Initialize_ctrlTouchscreentouch9deltay(kAxisLayout, vector2Control45);
		AxisControl x46 = Initialize_ctrlTouchscreentouch9radiusx(kAxisLayout, vector2Control46);
		AxisControl y46 = Initialize_ctrlTouchscreentouch9radiusy(kAxisLayout, vector2Control46);
		AxisControl x47 = Initialize_ctrlTouchscreentouch9startPositionx(kAxisLayout, vector2Control47);
		AxisControl y47 = Initialize_ctrlTouchscreentouch9startPositiony(kAxisLayout, vector2Control47);
		deviceBuilder.WithControlUsage(0, new InternedString("PrimaryAction"), buttonControl);
		deviceBuilder.WithControlUsage(1, new InternedString("Point"), vector2Control);
		deviceBuilder.WithControlUsage(2, new InternedString("Secondary2DMotion"), vector2Control2);
		deviceBuilder.WithControlUsage(3, new InternedString("Pressure"), control);
		deviceBuilder.WithControlUsage(4, new InternedString("Radius"), vector2Control3);
		base.touchControlArray = new TouchControl[10];
		base.touchControlArray[0] = touchControl2;
		base.touchControlArray[1] = touchControl3;
		base.touchControlArray[2] = touchControl4;
		base.touchControlArray[3] = touchControl5;
		base.touchControlArray[4] = touchControl6;
		base.touchControlArray[5] = touchControl7;
		base.touchControlArray[6] = touchControl8;
		base.touchControlArray[7] = touchControl9;
		base.touchControlArray[8] = touchControl10;
		base.touchControlArray[9] = touchControl11;
		base.primaryTouch = touchControl;
		base.position = vector2Control;
		base.delta = vector2Control2;
		base.radius = vector2Control3;
		base.pressure = control;
		base.press = touchPressControl;
		touchControl.press = touchPressControl2;
		touchControl.touchId = touchId;
		touchControl.position = vector2Control4;
		touchControl.delta = vector2Control5;
		touchControl.pressure = axisControl;
		touchControl.radius = vector2Control6;
		touchControl.phase = phase;
		touchControl.indirectTouch = indirectTouch;
		touchControl.tap = buttonControl;
		touchControl.tapCount = tapCount;
		touchControl.startTime = startTime;
		touchControl.startPosition = vector2Control7;
		vector2Control.x = x5;
		vector2Control.y = y5;
		vector2Control2.x = x6;
		vector2Control2.y = y6;
		vector2Control3.x = x7;
		vector2Control3.y = y7;
		touchControl2.press = touchPressControl3;
		touchControl2.touchId = touchId2;
		touchControl2.position = vector2Control8;
		touchControl2.delta = vector2Control9;
		touchControl2.pressure = axisControl2;
		touchControl2.radius = vector2Control10;
		touchControl2.phase = phase2;
		touchControl2.indirectTouch = indirectTouch2;
		touchControl2.tap = tap;
		touchControl2.tapCount = tapCount2;
		touchControl2.startTime = startTime2;
		touchControl2.startPosition = vector2Control11;
		touchControl3.press = touchPressControl4;
		touchControl3.touchId = touchId3;
		touchControl3.position = vector2Control12;
		touchControl3.delta = vector2Control13;
		touchControl3.pressure = axisControl3;
		touchControl3.radius = vector2Control14;
		touchControl3.phase = phase3;
		touchControl3.indirectTouch = indirectTouch3;
		touchControl3.tap = tap2;
		touchControl3.tapCount = tapCount3;
		touchControl3.startTime = startTime3;
		touchControl3.startPosition = vector2Control15;
		touchControl4.press = touchPressControl5;
		touchControl4.touchId = touchId4;
		touchControl4.position = vector2Control16;
		touchControl4.delta = vector2Control17;
		touchControl4.pressure = axisControl4;
		touchControl4.radius = vector2Control18;
		touchControl4.phase = phase4;
		touchControl4.indirectTouch = indirectTouch4;
		touchControl4.tap = tap3;
		touchControl4.tapCount = tapCount4;
		touchControl4.startTime = startTime4;
		touchControl4.startPosition = vector2Control19;
		touchControl5.press = touchPressControl6;
		touchControl5.touchId = touchId5;
		touchControl5.position = vector2Control20;
		touchControl5.delta = vector2Control21;
		touchControl5.pressure = axisControl5;
		touchControl5.radius = vector2Control22;
		touchControl5.phase = phase5;
		touchControl5.indirectTouch = indirectTouch5;
		touchControl5.tap = tap4;
		touchControl5.tapCount = tapCount5;
		touchControl5.startTime = startTime5;
		touchControl5.startPosition = vector2Control23;
		touchControl6.press = touchPressControl7;
		touchControl6.touchId = touchId6;
		touchControl6.position = vector2Control24;
		touchControl6.delta = vector2Control25;
		touchControl6.pressure = axisControl6;
		touchControl6.radius = vector2Control26;
		touchControl6.phase = phase6;
		touchControl6.indirectTouch = indirectTouch6;
		touchControl6.tap = tap5;
		touchControl6.tapCount = tapCount6;
		touchControl6.startTime = startTime6;
		touchControl6.startPosition = vector2Control27;
		touchControl7.press = touchPressControl8;
		touchControl7.touchId = touchId7;
		touchControl7.position = vector2Control28;
		touchControl7.delta = vector2Control29;
		touchControl7.pressure = axisControl7;
		touchControl7.radius = vector2Control30;
		touchControl7.phase = phase7;
		touchControl7.indirectTouch = indirectTouch7;
		touchControl7.tap = tap6;
		touchControl7.tapCount = tapCount7;
		touchControl7.startTime = startTime7;
		touchControl7.startPosition = vector2Control31;
		touchControl8.press = touchPressControl9;
		touchControl8.touchId = touchId8;
		touchControl8.position = vector2Control32;
		touchControl8.delta = vector2Control33;
		touchControl8.pressure = axisControl8;
		touchControl8.radius = vector2Control34;
		touchControl8.phase = phase8;
		touchControl8.indirectTouch = indirectTouch8;
		touchControl8.tap = tap7;
		touchControl8.tapCount = tapCount8;
		touchControl8.startTime = startTime8;
		touchControl8.startPosition = vector2Control35;
		touchControl9.press = touchPressControl10;
		touchControl9.touchId = touchId9;
		touchControl9.position = vector2Control36;
		touchControl9.delta = vector2Control37;
		touchControl9.pressure = axisControl9;
		touchControl9.radius = vector2Control38;
		touchControl9.phase = phase9;
		touchControl9.indirectTouch = indirectTouch9;
		touchControl9.tap = tap8;
		touchControl9.tapCount = tapCount9;
		touchControl9.startTime = startTime9;
		touchControl9.startPosition = vector2Control39;
		touchControl10.press = touchPressControl11;
		touchControl10.touchId = touchId10;
		touchControl10.position = vector2Control40;
		touchControl10.delta = vector2Control41;
		touchControl10.pressure = axisControl10;
		touchControl10.radius = vector2Control42;
		touchControl10.phase = phase10;
		touchControl10.indirectTouch = indirectTouch10;
		touchControl10.tap = tap9;
		touchControl10.tapCount = tapCount10;
		touchControl10.startTime = startTime10;
		touchControl10.startPosition = vector2Control43;
		touchControl11.press = touchPressControl12;
		touchControl11.touchId = touchId11;
		touchControl11.position = vector2Control44;
		touchControl11.delta = vector2Control45;
		touchControl11.pressure = axisControl11;
		touchControl11.radius = vector2Control46;
		touchControl11.phase = phase11;
		touchControl11.indirectTouch = indirectTouch11;
		touchControl11.tap = tap10;
		touchControl11.tapCount = tapCount11;
		touchControl11.startTime = startTime11;
		touchControl11.startPosition = vector2Control47;
		vector2Control4.x = x;
		vector2Control4.y = y;
		vector2Control5.x = x2;
		vector2Control5.y = y2;
		vector2Control6.x = x3;
		vector2Control6.y = y3;
		vector2Control7.x = x4;
		vector2Control7.y = y4;
		vector2Control8.x = x8;
		vector2Control8.y = y8;
		vector2Control9.x = x9;
		vector2Control9.y = y9;
		vector2Control10.x = x10;
		vector2Control10.y = y10;
		vector2Control11.x = x11;
		vector2Control11.y = y11;
		vector2Control12.x = x12;
		vector2Control12.y = y12;
		vector2Control13.x = x13;
		vector2Control13.y = y13;
		vector2Control14.x = x14;
		vector2Control14.y = y14;
		vector2Control15.x = x15;
		vector2Control15.y = y15;
		vector2Control16.x = x16;
		vector2Control16.y = y16;
		vector2Control17.x = x17;
		vector2Control17.y = y17;
		vector2Control18.x = x18;
		vector2Control18.y = y18;
		vector2Control19.x = x19;
		vector2Control19.y = y19;
		vector2Control20.x = x20;
		vector2Control20.y = y20;
		vector2Control21.x = x21;
		vector2Control21.y = y21;
		vector2Control22.x = x22;
		vector2Control22.y = y22;
		vector2Control23.x = x23;
		vector2Control23.y = y23;
		vector2Control24.x = x24;
		vector2Control24.y = y24;
		vector2Control25.x = x25;
		vector2Control25.y = y25;
		vector2Control26.x = x26;
		vector2Control26.y = y26;
		vector2Control27.x = x27;
		vector2Control27.y = y27;
		vector2Control28.x = x28;
		vector2Control28.y = y28;
		vector2Control29.x = x29;
		vector2Control29.y = y29;
		vector2Control30.x = x30;
		vector2Control30.y = y30;
		vector2Control31.x = x31;
		vector2Control31.y = y31;
		vector2Control32.x = x32;
		vector2Control32.y = y32;
		vector2Control33.x = x33;
		vector2Control33.y = y33;
		vector2Control34.x = x34;
		vector2Control34.y = y34;
		vector2Control35.x = x35;
		vector2Control35.y = y35;
		vector2Control36.x = x36;
		vector2Control36.y = y36;
		vector2Control37.x = x37;
		vector2Control37.y = y37;
		vector2Control38.x = x38;
		vector2Control38.y = y38;
		vector2Control39.x = x39;
		vector2Control39.y = y39;
		vector2Control40.x = x40;
		vector2Control40.y = y40;
		vector2Control41.x = x41;
		vector2Control41.y = y41;
		vector2Control42.x = x42;
		vector2Control42.y = y42;
		vector2Control43.x = x43;
		vector2Control43.y = y43;
		vector2Control44.x = x44;
		vector2Control44.y = y44;
		vector2Control45.x = x45;
		vector2Control45.y = y45;
		vector2Control46.x = x46;
		vector2Control46.y = y46;
		vector2Control47.x = x47;
		vector2Control47.y = y47;
		deviceBuilder.WithStateOffsetToControlIndexMap(new uint[184]
		{
			32784u, 16810012u, 16810020u, 33587229u, 33587237u, 50364446u, 50364454u, 67141663u, 67141671u, 83918851u,
			83918867u, 100696096u, 100696104u, 117473313u, 117473321u, 134225925u, 134225941u, 134225942u, 138420247u, 146801688u,
			148898841u, 167837722u, 201359394u, 218136611u, 234913834u, 251691062u, 268468279u, 285245496u, 302022713u, 318799917u,
			335577146u, 352354363u, 369106991u, 369106992u, 373301297u, 381682738u, 383779891u, 402718772u, 436240444u, 453017661u,
			469794878u, 486572106u, 503349323u, 520126540u, 536903757u, 553680961u, 570458190u, 587235407u, 603988035u, 603988036u,
			608182341u, 616563782u, 618660935u, 637599816u, 671121488u, 687898705u, 704675922u, 721453150u, 738230367u, 755007584u,
			771784801u, 788562005u, 805339234u, 822116451u, 838869079u, 838869080u, 843063385u, 851444826u, 853541979u, 872480860u,
			906002532u, 922779749u, 939556966u, 956334194u, 973111411u, 989888628u, 1006665845u, 1023443049u, 1040220278u, 1056997495u,
			1073750123u, 1073750124u, 1077944429u, 1086325870u, 1088423023u, 1107361904u, 1140883576u, 1157660793u, 1174438010u, 1191215238u,
			1207992455u, 1224769672u, 1241546889u, 1258324093u, 1275101322u, 1291878539u, 1308631167u, 1308631168u, 1312825473u, 1321206914u,
			1323304067u, 1342242948u, 1375764620u, 1392541837u, 1409319054u, 1426096282u, 1442873499u, 1459650716u, 1476427933u, 1493205137u,
			1509982366u, 1526759583u, 1543512211u, 1543512212u, 1547706517u, 1556087958u, 1558185111u, 1577123992u, 1610645664u, 1627422881u,
			1644200098u, 1660977326u, 1677754543u, 1694531760u, 1711308977u, 1728086181u, 1744863410u, 1761640627u, 1778393255u, 1778393256u,
			1782587561u, 1790969002u, 1793066155u, 1812005036u, 1845526708u, 1862303925u, 1879081142u, 1895858370u, 1912635587u, 1929412804u,
			1946190021u, 1962967225u, 1979744454u, 1996521671u, 2013274299u, 2013274300u, 2017468605u, 2025850046u, 2027947199u, 2046886080u,
			2080407752u, 2097184969u, 2113962186u, 2130739414u, 2147516631u, 2164293848u, 2181071065u, 2197848269u, 2214625498u, 2231402715u,
			2248155343u, 2248155344u, 2252349649u, 2260731090u, 2262828243u, 2281767124u, 2315288796u, 2332066013u, 2348843230u, 2365620458u,
			2382397675u, 2399174892u, 2415952109u, 2432729313u, 2449506542u, 2466283759u, 2483036387u, 2483036388u, 2487230693u, 2495612134u,
			2497709287u, 2516648168u, 2550169840u, 2566947057u
		});
		deviceBuilder.Finish();
	}

	private TouchControl Initialize_ctrlTouchscreenprimaryTouch(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 0).WithParent(parent)
			.WithChildren(16, 12)
			.WithName("primaryTouch")
			.WithDisplayName("Primary Touch")
			.WithLayout(kTouchLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 0u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private Vector2Control Initialize_ctrlTouchscreenposition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 1).WithParent(parent)
			.WithChildren(36, 2)
			.WithName("position")
			.WithDisplayName("Position")
			.WithLayout(kVector2Layout)
			.WithUsages(1, 1)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 4u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreendelta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 2).WithParent(parent)
			.WithChildren(38, 2)
			.WithName("delta")
			.WithDisplayName("Delta")
			.WithLayout(kVector2Layout)
			.WithUsages(2, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 12u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreenpressure(InternedString kAnalogLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 3).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Pressure")
			.WithLayout(kAnalogLayout)
			.WithUsages(3, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 20u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.WithDefaultState(1)
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreenradius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 4).WithParent(parent)
			.WithChildren(40, 2)
			.WithName("radius")
			.WithDisplayName("Radius")
			.WithLayout(kVector2Layout)
			.WithUsages(4, 1)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 24u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPressControl Initialize_ctrlTouchscreenpress(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 5).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Press")
			.WithLayout(kTouchPressLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 32u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch0(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 6).WithParent(parent)
			.WithChildren(42, 12)
			.WithName("touch0")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 56u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch1(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 7).WithParent(parent)
			.WithChildren(62, 12)
			.WithName("touch1")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 112u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch2(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 8).WithParent(parent)
			.WithChildren(82, 12)
			.WithName("touch2")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 168u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch3(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 9).WithParent(parent)
			.WithChildren(102, 12)
			.WithName("touch3")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 224u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch4(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 10).WithParent(parent)
			.WithChildren(122, 12)
			.WithName("touch4")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 280u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch5(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 11).WithParent(parent)
			.WithChildren(142, 12)
			.WithName("touch5")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 336u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch6(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 12).WithParent(parent)
			.WithChildren(162, 12)
			.WithName("touch6")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 392u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch7(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 13).WithParent(parent)
			.WithChildren(182, 12)
			.WithName("touch7")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 448u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch8(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 14).WithParent(parent)
			.WithChildren(202, 12)
			.WithName("touch8")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 504u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private TouchControl Initialize_ctrlTouchscreentouch9(InternedString kTouchLayout, InputControl parent)
	{
		TouchControl touchControl = new TouchControl();
		touchControl.Setup().At(this, 15).WithParent(parent)
			.WithChildren(222, 12)
			.WithName("touch9")
			.WithDisplayName("Touch")
			.WithLayout(kTouchLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1414485315),
				byteOffset = 560u,
				bitOffset = 0u,
				sizeInBits = 448u
			})
			.Finish();
		return touchControl;
	}

	private IntegerControl Initialize_ctrlTouchscreenprimaryTouchtouchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 16).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Primary Touch Touch ID")
			.WithShortDisplayName("Primary Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 0u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreenprimaryTouchposition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 17).WithParent(parent)
			.WithChildren(28, 2)
			.WithName("position")
			.WithDisplayName("Primary Touch Position")
			.WithShortDisplayName("Primary Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 4u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreenprimaryTouchdelta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 18).WithParent(parent)
			.WithChildren(30, 2)
			.WithName("delta")
			.WithDisplayName("Primary Touch Delta")
			.WithShortDisplayName("Primary Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 12u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchpressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 19).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Primary Touch Pressure")
			.WithShortDisplayName("Primary Touch Pressure")
			.WithLayout(kAxisLayout)
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

	private Vector2Control Initialize_ctrlTouchscreenprimaryTouchradius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 20).WithParent(parent)
			.WithChildren(32, 2)
			.WithName("radius")
			.WithDisplayName("Primary Touch Radius")
			.WithShortDisplayName("Primary Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 24u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreenprimaryTouchphase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 21).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Primary Touch Touch Phase")
			.WithShortDisplayName("Primary Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 32u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreenprimaryTouchpress(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 22).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Primary Touch Touch Contact?")
			.WithShortDisplayName("Primary Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 32u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreenprimaryTouchtapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 23).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Primary Touch Tap Count")
			.WithShortDisplayName("Primary Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 33u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreenprimaryTouchindirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 24).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Primary Touch Indirect Touch?")
			.WithShortDisplayName("Primary Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 35u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreenprimaryTouchtap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 25).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Primary Touch Tap")
			.WithShortDisplayName("Primary Touch Tap")
			.WithLayout(kButtonLayout)
			.WithUsages(0, 1)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 35u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreenprimaryTouchstartTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 26).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Primary Touch Start Time")
			.WithShortDisplayName("Primary Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 40u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreenprimaryTouchstartPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 27).WithParent(parent)
			.WithChildren(34, 2)
			.WithName("startPosition")
			.WithDisplayName("Primary Touch Start Position")
			.WithShortDisplayName("Primary Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 48u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchpositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 28).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Primary Touch Primary Touch Position X")
			.WithShortDisplayName("Primary Touch Primary Touch Position X")
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

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchpositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 29).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Primary Touch Primary Touch Position Y")
			.WithShortDisplayName("Primary Touch Primary Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
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

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchdeltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 30).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Primary Touch Primary Touch Delta X")
			.WithShortDisplayName("Primary Touch Primary Touch Delta X")
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

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchdeltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 31).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Primary Touch Primary Touch Delta Y")
			.WithShortDisplayName("Primary Touch Primary Touch Delta Y")
			.WithLayout(kAxisLayout)
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

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchradiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 32).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Primary Touch Primary Touch Radius X")
			.WithShortDisplayName("Primary Touch Primary Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 24u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchradiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 33).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Primary Touch Primary Touch Radius Y")
			.WithShortDisplayName("Primary Touch Primary Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 28u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchstartPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 34).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Primary Touch Primary Touch Start Position X")
			.WithShortDisplayName("Primary Touch Primary Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 48u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreenprimaryTouchstartPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 35).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Primary Touch Primary Touch Start Position Y")
			.WithShortDisplayName("Primary Touch Primary Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 52u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreenpositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 36).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Position X")
			.WithShortDisplayName("Position X")
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

	private AxisControl Initialize_ctrlTouchscreenpositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 37).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Position Y")
			.WithShortDisplayName("Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
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

	private AxisControl Initialize_ctrlTouchscreendeltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 38).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Delta X")
			.WithShortDisplayName("Delta X")
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

	private AxisControl Initialize_ctrlTouchscreendeltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 39).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Delta Y")
			.WithShortDisplayName("Delta Y")
			.WithLayout(kAxisLayout)
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

	private AxisControl Initialize_ctrlTouchscreenradiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 40).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Radius X")
			.WithShortDisplayName("Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 24u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreenradiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 41).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Radius Y")
			.WithShortDisplayName("Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 28u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch0touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 42).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 56u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch0position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 43).WithParent(parent)
			.WithChildren(54, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 60u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch0delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 44).WithParent(parent)
			.WithChildren(56, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 68u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 45).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 76u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch0radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 46).WithParent(parent)
			.WithChildren(58, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 80u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch0phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 47).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 88u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch0press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 48).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 88u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch0tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 49).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 89u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch0indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 50).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 91u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch0tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 51).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 91u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch0startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 52).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 96u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch0startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 53).WithParent(parent)
			.WithChildren(60, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 104u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 54).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 60u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 55).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 64u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 56).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 68u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 57).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 72u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 58).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 80u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 59).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 84u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 60).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 104u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch0startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 61).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 108u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch1touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 62).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 112u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch1position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 63).WithParent(parent)
			.WithChildren(74, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 116u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch1delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 64).WithParent(parent)
			.WithChildren(76, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 124u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 65).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 132u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch1radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 66).WithParent(parent)
			.WithChildren(78, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 136u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch1phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 67).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 144u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch1press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 68).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 144u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch1tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 69).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 145u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch1indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 70).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 147u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch1tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 71).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 147u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch1startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 72).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 152u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch1startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 73).WithParent(parent)
			.WithChildren(80, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 160u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 74).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 116u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 75).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 120u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 76).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 124u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 77).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 128u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 78).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 136u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 79).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 140u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 80).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 160u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch1startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 81).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 164u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch2touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 82).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 168u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch2position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 83).WithParent(parent)
			.WithChildren(94, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 172u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch2delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 84).WithParent(parent)
			.WithChildren(96, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 180u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 85).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 188u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch2radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 86).WithParent(parent)
			.WithChildren(98, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 192u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch2phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 87).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 200u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch2press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 88).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 200u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch2tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 89).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 201u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch2indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 90).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 203u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch2tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 91).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 203u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch2startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 92).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 208u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch2startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 93).WithParent(parent)
			.WithChildren(100, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 216u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 94).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 172u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 95).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 176u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 96).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 180u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 97).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 184u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 98).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 192u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 99).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 196u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 100).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 216u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch2startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 101).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 220u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch3touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 102).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 224u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch3position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 103).WithParent(parent)
			.WithChildren(114, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 228u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch3delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 104).WithParent(parent)
			.WithChildren(116, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 236u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 105).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 244u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch3radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 106).WithParent(parent)
			.WithChildren(118, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 248u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch3phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 107).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 256u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch3press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 108).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 256u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch3tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 109).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 257u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch3indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 110).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 259u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch3tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 111).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 259u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch3startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 112).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 264u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch3startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 113).WithParent(parent)
			.WithChildren(120, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 272u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 114).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 228u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 115).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 232u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 116).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 236u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 117).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 240u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 118).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 248u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 119).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 252u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 120).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 272u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch3startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 121).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 276u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch4touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 122).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 280u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch4position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 123).WithParent(parent)
			.WithChildren(134, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 284u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch4delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 124).WithParent(parent)
			.WithChildren(136, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 292u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 125).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 300u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch4radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 126).WithParent(parent)
			.WithChildren(138, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 304u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch4phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 127).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 312u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch4press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 128).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 312u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch4tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 129).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 313u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch4indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 130).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 315u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch4tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 131).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 315u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch4startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 132).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 320u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch4startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 133).WithParent(parent)
			.WithChildren(140, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 328u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 134).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 284u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 135).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 288u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 136).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 292u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 137).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 296u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 138).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 304u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 139).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 308u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 140).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 328u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch4startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 141).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 332u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch5touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 142).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 336u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch5position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 143).WithParent(parent)
			.WithChildren(154, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 340u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch5delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 144).WithParent(parent)
			.WithChildren(156, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 348u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 145).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 356u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch5radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 146).WithParent(parent)
			.WithChildren(158, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 360u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch5phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 147).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 368u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch5press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 148).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 368u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch5tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 149).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 369u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch5indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 150).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 371u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch5tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 151).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 371u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch5startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 152).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 376u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch5startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 153).WithParent(parent)
			.WithChildren(160, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 384u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 154).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 340u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 155).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 344u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 156).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 348u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 157).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 352u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 158).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 360u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 159).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 364u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 160).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 384u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch5startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 161).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 388u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch6touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 162).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 392u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch6position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 163).WithParent(parent)
			.WithChildren(174, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 396u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch6delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 164).WithParent(parent)
			.WithChildren(176, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 404u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 165).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 412u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch6radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 166).WithParent(parent)
			.WithChildren(178, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 416u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch6phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 167).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 424u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch6press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 168).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 424u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch6tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 169).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 425u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch6indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 170).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 427u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch6tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 171).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 427u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch6startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 172).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 432u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch6startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 173).WithParent(parent)
			.WithChildren(180, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 440u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 174).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 396u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 175).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 400u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 176).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 404u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 177).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 408u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 178).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 416u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 179).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 420u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 180).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 440u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch6startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 181).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 444u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch7touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 182).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 448u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch7position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 183).WithParent(parent)
			.WithChildren(194, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 452u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch7delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 184).WithParent(parent)
			.WithChildren(196, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 460u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 185).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 468u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch7radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 186).WithParent(parent)
			.WithChildren(198, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 472u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch7phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 187).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 480u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch7press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 188).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 480u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch7tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 189).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 481u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch7indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 190).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 483u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch7tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 191).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 483u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch7startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 192).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 488u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch7startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 193).WithParent(parent)
			.WithChildren(200, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 496u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 194).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 452u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 195).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 456u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 196).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 460u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 197).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 464u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 198).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 472u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 199).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 476u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 200).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 496u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch7startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 201).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 500u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch8touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 202).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 504u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch8position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 203).WithParent(parent)
			.WithChildren(214, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 508u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch8delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 204).WithParent(parent)
			.WithChildren(216, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 516u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 205).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 524u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch8radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 206).WithParent(parent)
			.WithChildren(218, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 528u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch8phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 207).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 536u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch8press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 208).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 536u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch8tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 209).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 537u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch8indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 210).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 539u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch8tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 211).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 539u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch8startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 212).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 544u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch8startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 213).WithParent(parent)
			.WithChildren(220, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 552u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 214).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 508u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 215).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 512u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 216).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 516u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 217).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 520u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 218).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 528u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 219).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 532u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 220).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 552u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch8startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 221).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 556u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch9touchId(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 222).WithParent(parent)
			.WithName("touchId")
			.WithDisplayName("Touch Touch ID")
			.WithShortDisplayName("Touch Touch ID")
			.WithLayout(kIntegerLayout)
			.IsSynthetic(value: true)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1229870112),
				byteOffset = 560u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return integerControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch9position(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 223).WithParent(parent)
			.WithChildren(234, 2)
			.WithName("position")
			.WithDisplayName("Touch Position")
			.WithShortDisplayName("Touch Position")
			.WithLayout(kVector2Layout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 564u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch9delta(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 224).WithParent(parent)
			.WithChildren(236, 2)
			.WithName("delta")
			.WithDisplayName("Touch Delta")
			.WithShortDisplayName("Touch Delta")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 572u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9pressure(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 225).WithParent(parent)
			.WithName("pressure")
			.WithDisplayName("Touch Pressure")
			.WithShortDisplayName("Touch Pressure")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 580u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch9radius(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 226).WithParent(parent)
			.WithChildren(238, 2)
			.WithName("radius")
			.WithDisplayName("Touch Radius")
			.WithShortDisplayName("Touch Radius")
			.WithLayout(kVector2Layout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 584u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private TouchPhaseControl Initialize_ctrlTouchscreentouch9phase(InternedString kTouchPhaseLayout, InputControl parent)
	{
		TouchPhaseControl touchPhaseControl = new TouchPhaseControl();
		touchPhaseControl.Setup().At(this, 227).WithParent(parent)
			.WithName("phase")
			.WithDisplayName("Touch Touch Phase")
			.WithShortDisplayName("Touch Touch Phase")
			.WithLayout(kTouchPhaseLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 592u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return touchPhaseControl;
	}

	private TouchPressControl Initialize_ctrlTouchscreentouch9press(InternedString kTouchPressLayout, InputControl parent)
	{
		TouchPressControl touchPressControl = new TouchPressControl();
		touchPressControl.Setup().At(this, 228).WithParent(parent)
			.WithName("press")
			.WithDisplayName("Touch Touch Contact?")
			.WithShortDisplayName("Touch Touch Contact?")
			.WithLayout(kTouchPressLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 592u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return touchPressControl;
	}

	private IntegerControl Initialize_ctrlTouchscreentouch9tapCount(InternedString kIntegerLayout, InputControl parent)
	{
		IntegerControl integerControl = new IntegerControl();
		integerControl.Setup().At(this, 229).WithParent(parent)
			.WithName("tapCount")
			.WithDisplayName("Touch Tap Count")
			.WithShortDisplayName("Touch Tap Count")
			.WithLayout(kIntegerLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1113150533),
				byteOffset = 593u,
				bitOffset = 0u,
				sizeInBits = 8u
			})
			.Finish();
		return integerControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch9indirectTouch(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 230).WithParent(parent)
			.WithName("indirectTouch")
			.WithDisplayName("Touch Indirect Touch?")
			.WithShortDisplayName("Touch Indirect Touch?")
			.WithLayout(kButtonLayout)
			.IsSynthetic(value: true)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 595u,
				bitOffset = 0u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private ButtonControl Initialize_ctrlTouchscreentouch9tap(InternedString kButtonLayout, InputControl parent)
	{
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Setup().At(this, 231).WithParent(parent)
			.WithName("tap")
			.WithDisplayName("Touch Tap")
			.WithShortDisplayName("Touch Tap")
			.WithLayout(kButtonLayout)
			.IsButton(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1112101920),
				byteOffset = 595u,
				bitOffset = 4u,
				sizeInBits = 1u
			})
			.WithMinAndMax(0, 1)
			.Finish();
		return buttonControl;
	}

	private DoubleControl Initialize_ctrlTouchscreentouch9startTime(InternedString kDoubleLayout, InputControl parent)
	{
		DoubleControl doubleControl = new DoubleControl();
		doubleControl.Setup().At(this, 232).WithParent(parent)
			.WithName("startTime")
			.WithDisplayName("Touch Start Time")
			.WithShortDisplayName("Touch Start Time")
			.WithLayout(kDoubleLayout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1145195552),
				byteOffset = 600u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return doubleControl;
	}

	private Vector2Control Initialize_ctrlTouchscreentouch9startPosition(InternedString kVector2Layout, InputControl parent)
	{
		Vector2Control vector2Control = new Vector2Control();
		vector2Control.Setup().At(this, 233).WithParent(parent)
			.WithChildren(240, 2)
			.WithName("startPosition")
			.WithDisplayName("Touch Start Position")
			.WithShortDisplayName("Touch Start Position")
			.WithLayout(kVector2Layout)
			.IsSynthetic(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1447379762),
				byteOffset = 608u,
				bitOffset = 0u,
				sizeInBits = 64u
			})
			.Finish();
		return vector2Control;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9positionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 234).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Position X")
			.WithShortDisplayName("Touch Touch Position X")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 564u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9positiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 235).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Position Y")
			.WithShortDisplayName("Touch Touch Position Y")
			.WithLayout(kAxisLayout)
			.DontReset(value: true)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 568u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9deltax(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 236).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Delta X")
			.WithShortDisplayName("Touch Touch Delta X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 572u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9deltay(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 237).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Delta Y")
			.WithShortDisplayName("Touch Touch Delta Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 576u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9radiusx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 238).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Radius X")
			.WithShortDisplayName("Touch Touch Radius X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 584u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9radiusy(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 239).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Radius Y")
			.WithShortDisplayName("Touch Touch Radius Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 588u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9startPositionx(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 240).WithParent(parent)
			.WithName("x")
			.WithDisplayName("Touch Touch Start Position X")
			.WithShortDisplayName("Touch Touch Start Position X")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 608u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}

	private AxisControl Initialize_ctrlTouchscreentouch9startPositiony(InternedString kAxisLayout, InputControl parent)
	{
		AxisControl axisControl = new AxisControl();
		axisControl.Setup().At(this, 241).WithParent(parent)
			.WithName("y")
			.WithDisplayName("Touch Touch Start Position Y")
			.WithShortDisplayName("Touch Touch Start Position Y")
			.WithLayout(kAxisLayout)
			.WithStateBlock(new InputStateBlock
			{
				format = new FourCC(1179407392),
				byteOffset = 612u,
				bitOffset = 0u,
				sizeInBits = 32u
			})
			.Finish();
		return axisControl;
	}
}
