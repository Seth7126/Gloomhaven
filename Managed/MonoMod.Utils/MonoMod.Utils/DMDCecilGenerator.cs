using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MonoMod.Utils;

public sealed class DMDCecilGenerator : DMDGenerator<DMDCecilGenerator>
{
	protected override MethodInfo _Generate(DynamicMethodDefinition dmd, object context)
	{
		MethodDefinition definition = dmd.Definition;
		TypeDefinition typeDefinition = context as TypeDefinition;
		bool flag = false;
		ModuleDefinition module = typeDefinition?.Module;
		HashSet<string> hashSet = null;
		if (typeDefinition == null)
		{
			flag = true;
			hashSet = new HashSet<string>();
			string dumpName = dmd.GetDumpName("Cecil");
			module = ModuleDefinition.CreateModule(dumpName, new ModuleParameters
			{
				Kind = ModuleKind.Dll,
				ReflectionImporterProvider = MMReflectionImporter.ProviderNoDefault
			});
			module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference(DynamicMethodDefinition.c_UnverifiableCodeAttribute)));
			if (dmd.Debug)
			{
				CustomAttribute customAttribute = new CustomAttribute(module.ImportReference(DynamicMethodDefinition.c_DebuggableAttribute));
				customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference(typeof(DebuggableAttribute.DebuggingModes)), DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations));
				module.Assembly.CustomAttributes.Add(customAttribute);
			}
			typeDefinition = new TypeDefinition("", $"DMD<{dmd.OriginalMethod?.Name?.Replace('.', '_')}>?{GetHashCode()}", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed)
			{
				BaseType = module.TypeSystem.Object
			};
			module.Types.Add(typeDefinition);
		}
		try
		{
			Relinker relinker = (IMetadataTokenProvider mtp2, IGenericParameterProvider ctx) => module.ImportReference(mtp2);
			MethodDefinition methodDefinition = new MethodDefinition("_" + definition.Name.Replace('.', '_'), definition.Attributes, module.TypeSystem.Void)
			{
				MethodReturnType = definition.MethodReturnType,
				Attributes = (Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.HideBySig),
				ImplAttributes = Mono.Cecil.MethodImplAttributes.IL,
				DeclaringType = typeDefinition,
				NoInlining = true
			};
			foreach (ParameterDefinition parameter in definition.Parameters)
			{
				methodDefinition.Parameters.Add(parameter.Clone().Relink(relinker, methodDefinition));
			}
			methodDefinition.ReturnType = definition.ReturnType.Relink(relinker, methodDefinition);
			typeDefinition.Methods.Add(methodDefinition);
			methodDefinition.HasThis = definition.HasThis;
			Mono.Cecil.Cil.MethodBody methodBody = (methodDefinition.Body = definition.Body.Clone(methodDefinition));
			Mono.Cecil.Cil.MethodBody methodBody3 = methodBody;
			foreach (VariableDefinition variable in methodDefinition.Body.Variables)
			{
				variable.VariableType = variable.VariableType.Relink(relinker, methodDefinition);
			}
			foreach (ExceptionHandler exceptionHandler in methodDefinition.Body.ExceptionHandlers)
			{
				if (exceptionHandler.CatchType != null)
				{
					exceptionHandler.CatchType = exceptionHandler.CatchType.Relink(relinker, methodDefinition);
				}
			}
			for (int num = 0; num < methodBody3.Instructions.Count; num++)
			{
				Instruction instruction = methodBody3.Instructions[num];
				object obj = instruction.Operand;
				if (obj is ParameterDefinition parameterDefinition)
				{
					obj = methodDefinition.Parameters[parameterDefinition.Index];
				}
				else if (obj is IMetadataTokenProvider mtp)
				{
					obj = mtp.Relink(relinker, methodDefinition);
				}
				_ = obj is DynamicMethodReference;
				if (hashSet != null && obj is MemberReference memberReference)
				{
					IMetadataScope metadataScope = (memberReference as TypeReference)?.Scope ?? memberReference.DeclaringType.Scope;
					if (!hashSet.Contains(metadataScope.Name))
					{
						CustomAttribute item = new CustomAttribute(module.ImportReference(DynamicMethodDefinition.c_IgnoresAccessChecksToAttribute))
						{
							ConstructorArguments = 
							{
								new CustomAttributeArgument(module.ImportReference(typeof(DebuggableAttribute.DebuggingModes)), metadataScope.Name)
							}
						};
						module.Assembly.CustomAttributes.Add(item);
						hashSet.Add(metadataScope.Name);
					}
				}
				instruction.Operand = obj;
			}
			methodDefinition.HasThis = false;
			if (definition.HasThis)
			{
				TypeReference typeReference = definition.DeclaringType;
				if (typeReference.IsValueType)
				{
					typeReference = new ByReferenceType(typeReference);
				}
				methodDefinition.Parameters.Insert(0, new ParameterDefinition("<>_this", Mono.Cecil.ParameterAttributes.None, typeReference.Relink(relinker, methodDefinition)));
			}
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONOMOD_DMD_DUMP")))
			{
				string fullPath = Path.GetFullPath(Environment.GetEnvironmentVariable("MONOMOD_DMD_DUMP"));
				string path = module.Name + ".dll";
				string path2 = Path.Combine(fullPath, path);
				fullPath = Path.GetDirectoryName(path2);
				if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
				using Stream stream = File.OpenWrite(path2);
				module.Write(stream);
			}
			return ReflectionHelper.Load(module).GetType(typeDefinition.FullName.Replace("+", "\\+"), throwOnError: false, ignoreCase: false).GetMethod(methodDefinition.Name, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}
		finally
		{
			if (flag)
			{
				module.Dispose();
			}
		}
	}
}
