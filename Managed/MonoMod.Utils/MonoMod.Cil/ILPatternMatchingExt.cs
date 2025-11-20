using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.Cil;

public static class ILPatternMatchingExt
{
	public static bool Match(this Instruction instr, OpCode opcode)
	{
		return instr.OpCode == opcode;
	}

	public static bool Match<T>(this Instruction instr, OpCode opcode, T value)
	{
		if (instr.Match<T>(opcode, out var value2))
		{
			if (value2 == null)
			{
				return value == null;
			}
			object obj = value;
			return value2.Equals(obj);
		}
		return false;
	}

	public static bool Match<T>(this Instruction instr, OpCode opcode, out T value)
	{
		if (instr.OpCode == opcode)
		{
			value = (T)instr.Operand;
			return false;
		}
		value = default(T);
		return false;
	}

	public static bool MatchNop(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Nop)
		{
			return true;
		}
		return false;
	}

	public static bool MatchBreak(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Break)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdarg instead.", true)]
	public static bool MatchLdarg0(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldarg_0)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdarg instead.", true)]
	public static bool MatchLdarg1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldarg_1)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdarg instead.", true)]
	public static bool MatchLdarg2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldarg_2)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdarg instead.", true)]
	public static bool MatchLdarg3(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldarg_3)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdloc instead.", true)]
	public static bool MatchLdloc0(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldloc_0)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdloc instead.", true)]
	public static bool MatchLdloc1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldloc_1)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdloc instead.", true)]
	public static bool MatchLdloc2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldloc_2)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchLdloc instead.", true)]
	public static bool MatchLdloc3(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldloc_3)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchStloc instead.", true)]
	public static bool MatchStloc0(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stloc_0)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchStloc instead.", true)]
	public static bool MatchStloc1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stloc_1)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchStloc instead.", true)]
	public static bool MatchStloc2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stloc_2)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MatchStloc instead.", true)]
	public static bool MatchStloc3(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stloc_3)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdnull(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldnull)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdcI4(this Instruction instr, int value)
	{
		if (instr.MatchLdcI4(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdcI4(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Ldc_I4)
		{
			value = (int)instr.Operand;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_S)
		{
			value = (sbyte)instr.Operand;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_M1)
		{
			value = -1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_3)
		{
			value = 3;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_4)
		{
			value = 4;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_5)
		{
			value = 5;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_6)
		{
			value = 6;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_7)
		{
			value = 7;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_8)
		{
			value = 8;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdcI8(this Instruction instr, long value)
	{
		if (instr.MatchLdcI8(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdcI8(this Instruction instr, out long value)
	{
		if (instr.OpCode == OpCodes.Ldc_I8)
		{
			value = (long)instr.Operand;
			return true;
		}
		value = 0L;
		return false;
	}

	public static bool MatchLdcR4(this Instruction instr, float value)
	{
		if (instr.MatchLdcR4(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdcR4(this Instruction instr, out float value)
	{
		if (instr.OpCode == OpCodes.Ldc_R4)
		{
			value = (float)instr.Operand;
			return true;
		}
		value = 0f;
		return false;
	}

	public static bool MatchLdcR8(this Instruction instr, double value)
	{
		if (instr.MatchLdcR8(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdcR8(this Instruction instr, out double value)
	{
		if (instr.OpCode == OpCodes.Ldc_R8)
		{
			value = (double)instr.Operand;
			return true;
		}
		value = 0.0;
		return false;
	}

	public static bool MatchDup(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Dup)
		{
			return true;
		}
		return false;
	}

	public static bool MatchPop(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Pop)
		{
			return true;
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchJmp(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchJmp<T>(this Instruction instr, string name)
	{
		if (instr.MatchJmp(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, Type type, string name)
	{
		if (instr.MatchJmp(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, MethodBase value)
	{
		if (instr.MatchJmp(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, MethodReference value)
	{
		if (instr.MatchJmp(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Jmp)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCall(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchCall(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchCall<T>(this Instruction instr, string name)
	{
		if (instr.MatchCall(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, Type type, string name)
	{
		if (instr.MatchCall(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, MethodBase value)
	{
		if (instr.MatchCall(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, MethodReference value)
	{
		if (instr.MatchCall(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Call)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchCallvirt(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchCallvirt<T>(this Instruction instr, string name)
	{
		if (instr.MatchCallvirt(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, Type type, string name)
	{
		if (instr.MatchCallvirt(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, MethodBase value)
	{
		if (instr.MatchCallvirt(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, MethodReference value)
	{
		if (instr.MatchCallvirt(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Callvirt)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchCallOrCallvirt(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt<T>(this Instruction instr, string name)
	{
		if (instr.MatchCallOrCallvirt(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, Type type, string name)
	{
		if (instr.MatchCallOrCallvirt(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, MethodBase value)
	{
		if (instr.MatchCallOrCallvirt(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, MethodReference value)
	{
		if (instr.MatchCallOrCallvirt(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCalli(this Instruction instr, IMethodSignature value)
	{
		if (instr.MatchCalli(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchCalli(this Instruction instr, out IMethodSignature value)
	{
		if (instr.OpCode == OpCodes.Calli)
		{
			value = (IMethodSignature)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchRet(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ret)
		{
			return true;
		}
		return false;
	}

	public static bool MatchBr(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBr(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBr(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Br || instr.OpCode == OpCodes.Br_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBrfalse(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBrfalse(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBrfalse(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Brfalse || instr.OpCode == OpCodes.Brfalse_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBrtrue(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBrtrue(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBrtrue(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brtrue_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBeq(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBeq(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBeq(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Beq || instr.OpCode == OpCodes.Beq_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBge(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBge(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBge(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Bge || instr.OpCode == OpCodes.Bge_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBgt(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBgt(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBgt(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Bgt || instr.OpCode == OpCodes.Bgt_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBle(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBle(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBle(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Ble || instr.OpCode == OpCodes.Ble_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBlt(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBlt(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBlt(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Blt || instr.OpCode == OpCodes.Blt_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBneUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBneUn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBneUn(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Bne_Un || instr.OpCode == OpCodes.Bne_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBgeUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBgeUn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBgeUn(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Bge_Un || instr.OpCode == OpCodes.Bge_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBgtUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBgtUn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBgtUn(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Bgt_Un || instr.OpCode == OpCodes.Bgt_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBleUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBleUn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBleUn(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Ble_Un || instr.OpCode == OpCodes.Ble_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBltUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBltUn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBltUn(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Blt_Un || instr.OpCode == OpCodes.Blt_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchSwitch(this Instruction instr, ILLabel[] value)
	{
		if (instr.MatchSwitch(out var value2))
		{
			return value2.SequenceEqual(value);
		}
		return false;
	}

	public static bool MatchSwitch(this Instruction instr, out ILLabel[] value)
	{
		if (instr.OpCode == OpCodes.Switch)
		{
			value = (ILLabel[])instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdindI1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_I1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindU1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_U1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindI2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_I2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindU2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_U2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindI4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_I4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindU4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_U4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindI8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_I8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindI(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_I)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindR4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_R4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindR8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_R8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdindRef(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldind_Ref)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindRef(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_Ref)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindI1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_I1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindI2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_I2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindI4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_I4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindI8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_I8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindR4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_R4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStindR8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_R8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchAdd(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Add)
		{
			return true;
		}
		return false;
	}

	public static bool MatchSub(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Sub)
		{
			return true;
		}
		return false;
	}

	public static bool MatchMul(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Mul)
		{
			return true;
		}
		return false;
	}

	public static bool MatchDiv(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Div)
		{
			return true;
		}
		return false;
	}

	public static bool MatchDivUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Div_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchRem(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Rem)
		{
			return true;
		}
		return false;
	}

	public static bool MatchRemUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Rem_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchAnd(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.And)
		{
			return true;
		}
		return false;
	}

	public static bool MatchOr(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Or)
		{
			return true;
		}
		return false;
	}

	public static bool MatchXor(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Xor)
		{
			return true;
		}
		return false;
	}

	public static bool MatchShl(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Shl)
		{
			return true;
		}
		return false;
	}

	public static bool MatchShr(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Shr)
		{
			return true;
		}
		return false;
	}

	public static bool MatchShrUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Shr_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchNeg(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Neg)
		{
			return true;
		}
		return false;
	}

	public static bool MatchNot(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Not)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvI1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_I1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvI2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_I2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvI4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_I4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvI8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_I8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvR4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_R4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvR8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_R8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvU4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_U4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvU8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_U8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchCpobj(this Instruction instr, string fullName)
	{
		if (instr.MatchCpobj(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchCpobj<T>(this Instruction instr)
	{
		if (instr.MatchCpobj(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchCpobj(this Instruction instr, Type value)
	{
		if (instr.MatchCpobj(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchCpobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchCpobj(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchCpobj(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Cpobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdobj(this Instruction instr, string fullName)
	{
		if (instr.MatchLdobj(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchLdobj<T>(this Instruction instr)
	{
		if (instr.MatchLdobj(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchLdobj(this Instruction instr, Type value)
	{
		if (instr.MatchLdobj(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchLdobj(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdobj(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Ldobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdstr(this Instruction instr, string value)
	{
		if (instr.MatchLdstr(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdstr(this Instruction instr, out string value)
	{
		if (instr.OpCode == OpCodes.Ldstr)
		{
			value = (string)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, string typeFullName)
	{
		if (instr.MatchNewobj(out var value))
		{
			return value.DeclaringType.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchNewobj<T>(this Instruction instr)
	{
		if (instr.MatchNewobj(out var value))
		{
			return value.DeclaringType.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, Type type)
	{
		if (instr.MatchNewobj(out var value))
		{
			return value.DeclaringType.Is(type);
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, MethodBase value)
	{
		if (instr.MatchNewobj(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, MethodReference value)
	{
		if (instr.MatchNewobj(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Newobj)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, string fullName)
	{
		if (instr.MatchCastclass(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchCastclass<T>(this Instruction instr)
	{
		if (instr.MatchCastclass(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, Type value)
	{
		if (instr.MatchCastclass(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, TypeReference value)
	{
		if (instr.MatchCastclass(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Castclass)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, string fullName)
	{
		if (instr.MatchIsinst(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchIsinst<T>(this Instruction instr)
	{
		if (instr.MatchIsinst(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, Type value)
	{
		if (instr.MatchIsinst(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, TypeReference value)
	{
		if (instr.MatchIsinst(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Isinst)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchConvRUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_R_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, string fullName)
	{
		if (instr.MatchUnbox(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchUnbox<T>(this Instruction instr)
	{
		if (instr.MatchUnbox(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, Type value)
	{
		if (instr.MatchUnbox(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, TypeReference value)
	{
		if (instr.MatchUnbox(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Unbox)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchThrow(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Throw)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdfld(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdfld<T>(this Instruction instr, string name)
	{
		if (instr.MatchLdfld(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdfld(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdfld(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdfld(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, out FieldReference value)
	{
		if (instr.OpCode == OpCodes.Ldfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdflda(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdflda<T>(this Instruction instr, string name)
	{
		if (instr.MatchLdflda(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdflda(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdflda(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdflda(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, out FieldReference value)
	{
		if (instr.OpCode == OpCodes.Ldflda)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchStfld(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchStfld<T>(this Instruction instr, string name)
	{
		if (instr.MatchStfld(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchStfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchStfld(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchStfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchStfld(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchStfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchStfld(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchStfld(this Instruction instr, out FieldReference value)
	{
		if (instr.OpCode == OpCodes.Stfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdsfld(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdsfld<T>(this Instruction instr, string name)
	{
		if (instr.MatchLdsfld(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdsfld(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdsfld(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdsfld(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, out FieldReference value)
	{
		if (instr.OpCode == OpCodes.Ldsfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdsflda(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdsflda<T>(this Instruction instr, string name)
	{
		if (instr.MatchLdsflda(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdsflda(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdsflda(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdsflda(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, out FieldReference value)
	{
		if (instr.OpCode == OpCodes.Ldsflda)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchStsfld(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchStsfld<T>(this Instruction instr, string name)
	{
		if (instr.MatchStsfld(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchStsfld(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchStsfld(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchStsfld(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, out FieldReference value)
	{
		if (instr.OpCode == OpCodes.Stsfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStobj(this Instruction instr, string fullName)
	{
		if (instr.MatchStobj(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchStobj<T>(this Instruction instr)
	{
		if (instr.MatchStobj(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchStobj(this Instruction instr, Type value)
	{
		if (instr.MatchStobj(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchStobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchStobj(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchStobj(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Stobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchConvOvfI1Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I1_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfI2Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I2_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfI4Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I4_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfI8Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I8_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU1Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U1_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU2Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U2_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU4Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U4_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU8Un(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U8_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfIUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfUUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchBox(this Instruction instr, string fullName)
	{
		if (instr.MatchBox(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchBox<T>(this Instruction instr)
	{
		if (instr.MatchBox(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchBox(this Instruction instr, Type value)
	{
		if (instr.MatchBox(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchBox(this Instruction instr, TypeReference value)
	{
		if (instr.MatchBox(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchBox(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Box)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchNewarr(this Instruction instr, string fullName)
	{
		if (instr.MatchNewarr(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchNewarr<T>(this Instruction instr)
	{
		if (instr.MatchNewarr(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchNewarr(this Instruction instr, Type value)
	{
		if (instr.MatchNewarr(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchNewarr(this Instruction instr, TypeReference value)
	{
		if (instr.MatchNewarr(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchNewarr(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Newarr)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdlen(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldlen)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelema(this Instruction instr, string fullName)
	{
		if (instr.MatchLdelema(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchLdelema<T>(this Instruction instr)
	{
		if (instr.MatchLdelema(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchLdelema(this Instruction instr, Type value)
	{
		if (instr.MatchLdelema(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdelema(this Instruction instr, TypeReference value)
	{
		if (instr.MatchLdelema(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdelema(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Ldelema)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdelemI1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_I1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemU1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_U1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemI2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_I2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemU2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_U2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemI4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_I4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemU4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_U4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemI8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_I8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemI(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_I)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemR4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_R4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemR8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_R8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemRef(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ldelem_Ref)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemI(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_I)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemI1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_I1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemI2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_I2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemI4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_I4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemI8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_I8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemR4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_R4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemR8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_R8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchStelemRef(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stelem_Ref)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, string fullName)
	{
		if (instr.MatchLdelemAny(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchLdelemAny<T>(this Instruction instr)
	{
		if (instr.MatchLdelemAny(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, Type value)
	{
		if (instr.MatchLdelemAny(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, TypeReference value)
	{
		if (instr.MatchLdelemAny(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Ldelem_Any)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, string fullName)
	{
		if (instr.MatchStelemAny(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchStelemAny<T>(this Instruction instr)
	{
		if (instr.MatchStelemAny(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, Type value)
	{
		if (instr.MatchStelemAny(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, TypeReference value)
	{
		if (instr.MatchStelemAny(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Stelem_Any)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, string fullName)
	{
		if (instr.MatchUnboxAny(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchUnboxAny<T>(this Instruction instr)
	{
		if (instr.MatchUnboxAny(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, Type value)
	{
		if (instr.MatchUnboxAny(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, TypeReference value)
	{
		if (instr.MatchUnboxAny(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Unbox_Any)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchConvOvfI1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfI2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfI4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU4(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U4)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfI8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvOvfU8(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U8)
		{
			return true;
		}
		return false;
	}

	public static bool MatchRefanyval(this Instruction instr, string fullName)
	{
		if (instr.MatchRefanyval(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchRefanyval<T>(this Instruction instr)
	{
		if (instr.MatchRefanyval(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchRefanyval(this Instruction instr, Type value)
	{
		if (instr.MatchRefanyval(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchRefanyval(this Instruction instr, TypeReference value)
	{
		if (instr.MatchRefanyval(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchRefanyval(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Refanyval)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCkfinite(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ckfinite)
		{
			return true;
		}
		return false;
	}

	public static bool MatchMkrefany(this Instruction instr, string fullName)
	{
		if (instr.MatchMkrefany(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchMkrefany<T>(this Instruction instr)
	{
		if (instr.MatchMkrefany(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchMkrefany(this Instruction instr, Type value)
	{
		if (instr.MatchMkrefany(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchMkrefany(this Instruction instr, TypeReference value)
	{
		if (instr.MatchMkrefany(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchMkrefany(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Mkrefany)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdtoken(this Instruction instr, IMetadataTokenProvider value)
	{
		if (instr.MatchLdtoken(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdtoken(this Instruction instr, out IMetadataTokenProvider value)
	{
		if (instr.OpCode == OpCodes.Ldtoken)
		{
			value = (IMetadataTokenProvider)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchConvU2(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_U2)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvU1(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_U1)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvI(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_I)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConv_OvfI(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_I)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConv_OvfU(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_Ovf_U)
		{
			return true;
		}
		return false;
	}

	public static bool MatchAddOvf(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Add_Ovf)
		{
			return true;
		}
		return false;
	}

	public static bool MatchAdd_OvfUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Add_Ovf_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchMulOvf(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Mul_Ovf)
		{
			return true;
		}
		return false;
	}

	public static bool MatchMulOvfUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Mul_Ovf_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchSubOvf(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Sub_Ovf)
		{
			return true;
		}
		return false;
	}

	public static bool MatchSubOvfUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Sub_Ovf_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchEndfinally(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Endfinally)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLeave(this Instruction instr, ILLabel value)
	{
		if (instr.MatchLeave(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLeave(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Leave)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLeaveS(this Instruction instr, ILLabel value)
	{
		if (instr.MatchLeaveS(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLeaveS(this Instruction instr, out ILLabel value)
	{
		if (instr.OpCode == OpCodes.Leave_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStindI(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Stind_I)
		{
			return true;
		}
		return false;
	}

	public static bool MatchConvU(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Conv_U)
		{
			return true;
		}
		return false;
	}

	public static bool MatchArglist(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Arglist)
		{
			return true;
		}
		return false;
	}

	public static bool MatchCeq(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Ceq)
		{
			return true;
		}
		return false;
	}

	public static bool MatchCgt(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Cgt)
		{
			return true;
		}
		return false;
	}

	public static bool MatchCgtUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Cgt_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchClt(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Clt)
		{
			return true;
		}
		return false;
	}

	public static bool MatchCltUn(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Clt_Un)
		{
			return true;
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdftn(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdftn<T>(this Instruction instr, string name)
	{
		if (instr.MatchLdftn(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdftn(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, MethodBase value)
	{
		if (instr.MatchLdftn(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, MethodReference value)
	{
		if (instr.MatchLdftn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Ldftn)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdvirtftn(out var value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdvirtftn<T>(this Instruction instr, string name)
	{
		if (instr.MatchLdvirtftn(out var value))
		{
			return value.Is(typeof(T), name);
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdvirtftn(out var value))
		{
			return value.Is(type, name);
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, MethodBase value)
	{
		if (instr.MatchLdvirtftn(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, MethodReference value)
	{
		if (instr.MatchLdvirtftn(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, out MethodReference value)
	{
		if (instr.OpCode == OpCodes.Ldvirtftn)
		{
			value = instr.Operand as MethodReference;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdarg(this Instruction instr, int value)
	{
		if (instr.MatchLdarg(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdarg(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Ldarg || instr.OpCode == OpCodes.Ldarg_S)
		{
			value = ((ParameterReference)instr.Operand).Index;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_3)
		{
			value = 3;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdarga(this Instruction instr, int value)
	{
		if (instr.MatchLdarga(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdarga(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Ldarga || instr.OpCode == OpCodes.Ldarga_S)
		{
			value = ((ParameterReference)instr.Operand).Index;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchStarg(this Instruction instr, int value)
	{
		if (instr.MatchStarg(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchStarg(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Starg || instr.OpCode == OpCodes.Starg_S)
		{
			value = ((ParameterReference)instr.Operand).Index;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdloc(this Instruction instr, int value)
	{
		if (instr.MatchLdloc(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdloc(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Ldloc || instr.OpCode == OpCodes.Ldloc_S)
		{
			value = ((VariableReference)instr.Operand).Index;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_3)
		{
			value = 3;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdloca(this Instruction instr, int value)
	{
		if (instr.MatchLdloca(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchLdloca(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Ldloca || instr.OpCode == OpCodes.Ldloca_S)
		{
			value = ((VariableReference)instr.Operand).Index;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchStloc(this Instruction instr, int value)
	{
		if (instr.MatchStloc(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchStloc(this Instruction instr, out int value)
	{
		if (instr.OpCode == OpCodes.Stloc || instr.OpCode == OpCodes.Stloc_S)
		{
			value = ((VariableReference)instr.Operand).Index;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_3)
		{
			value = 3;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLocalloc(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Localloc)
		{
			return true;
		}
		return false;
	}

	public static bool MatchEndfilter(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Endfilter)
		{
			return true;
		}
		return false;
	}

	public static bool MatchUnaligned(this Instruction instr, sbyte value)
	{
		if (instr.MatchUnaligned(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchUnaligned(this Instruction instr, out sbyte value)
	{
		if (instr.OpCode == OpCodes.Unaligned)
		{
			value = (sbyte)instr.Operand;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchVolatile(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Volatile)
		{
			return true;
		}
		return false;
	}

	public static bool MatchTail(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Tail)
		{
			return true;
		}
		return false;
	}

	public static bool MatchInitobj(this Instruction instr, string fullName)
	{
		if (instr.MatchInitobj(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchInitobj<T>(this Instruction instr)
	{
		if (instr.MatchInitobj(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchInitobj(this Instruction instr, Type value)
	{
		if (instr.MatchInitobj(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchInitobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchInitobj(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchInitobj(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Initobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchConstrained(this Instruction instr, string fullName)
	{
		if (instr.MatchConstrained(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchConstrained<T>(this Instruction instr)
	{
		if (instr.MatchConstrained(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchConstrained(this Instruction instr, Type value)
	{
		if (instr.MatchConstrained(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchConstrained(this Instruction instr, TypeReference value)
	{
		if (instr.MatchConstrained(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchConstrained(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Constrained)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCpblk(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Cpblk)
		{
			return true;
		}
		return false;
	}

	public static bool MatchInitblk(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Initblk)
		{
			return true;
		}
		return false;
	}

	public static bool MatchNo(this Instruction instr, sbyte value)
	{
		if (instr.MatchNo(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchNo(this Instruction instr, out sbyte value)
	{
		if (instr.OpCode == OpCodes.No)
		{
			value = (sbyte)instr.Operand;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchRethrow(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Rethrow)
		{
			return true;
		}
		return false;
	}

	public static bool MatchSizeof(this Instruction instr, string fullName)
	{
		if (instr.MatchSizeof(out var value))
		{
			return value.Is(fullName);
		}
		return false;
	}

	public static bool MatchSizeof<T>(this Instruction instr)
	{
		if (instr.MatchSizeof(out var value))
		{
			return value.Is(typeof(T));
		}
		return false;
	}

	public static bool MatchSizeof(this Instruction instr, Type value)
	{
		if (instr.MatchSizeof(out var value2))
		{
			return value2.Is(value);
		}
		return false;
	}

	public static bool MatchSizeof(this Instruction instr, TypeReference value)
	{
		if (instr.MatchSizeof(out var value2))
		{
			return value2 == value;
		}
		return false;
	}

	public static bool MatchSizeof(this Instruction instr, out TypeReference value)
	{
		if (instr.OpCode == OpCodes.Sizeof)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchRefanytype(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Refanytype)
		{
			return true;
		}
		return false;
	}

	public static bool MatchReadonly(this Instruction instr)
	{
		if (instr.OpCode == OpCodes.Readonly)
		{
			return true;
		}
		return false;
	}
}
