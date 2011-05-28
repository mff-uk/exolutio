/*
 * Source: CodeProject article Dynamic Code Generation vs Reflection 
 * Author: Herbrandson
 * http://www.codeproject.com/KB/cs/Dynamic_Code_Generation.aspx
 */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Exolutio.SupportingClasses.Reflection
{
	public delegate object GetHandler(object source);
	
	public delegate void SetHandler(object source, object value);
	
	public delegate object InstantiateObjectHandler();

	public static class DynamicMethodCompiler
	{
		// CreateInstantiateObjectDelegate
        public static InstantiateObjectHandler CreateInstantiateObjectHandler(Type type)
		{
			ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public |
				   BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);

			if (constructorInfo == null)
			{
				throw new Exception(string.Format("Missing nonparametric constructor", type));
			}

			DynamicMethod dynamicMethod = new DynamicMethod("InstantiateObject",
					MethodAttributes.Static |
				  MethodAttributes.Public, CallingConventions.Standard, typeof(object),
					null, type, true);

			ILGenerator generator = dynamicMethod.GetILGenerator();
			generator.Emit(OpCodes.Newobj, constructorInfo);
			generator.Emit(OpCodes.Ret);
			return (InstantiateObjectHandler)dynamicMethod.CreateDelegate
					(typeof(InstantiateObjectHandler));
		}

		// CreateGetDelegate
        public static GetHandler CreateGetHandler(Type type, PropertyInfo propertyInfo)
		{
			MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
			DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
			ILGenerator getGenerator = dynamicGet.GetILGenerator();

			getGenerator.Emit(OpCodes.Ldarg_0);
			getGenerator.Emit(OpCodes.Call, getMethodInfo);
			BoxIfNeeded(getMethodInfo.ReturnType, getGenerator);
			getGenerator.Emit(OpCodes.Ret);

			return (GetHandler)dynamicGet.CreateDelegate(typeof(GetHandler));
		}

		// CreateGetDelegate
		public static GetHandler CreateGetHandler(Type type, FieldInfo fieldInfo)
		{
			DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
			ILGenerator getGenerator = dynamicGet.GetILGenerator();

			getGenerator.Emit(OpCodes.Ldarg_0);
			getGenerator.Emit(OpCodes.Ldfld, fieldInfo);
			BoxIfNeeded(fieldInfo.FieldType, getGenerator);
			getGenerator.Emit(OpCodes.Ret);

			return (GetHandler)dynamicGet.CreateDelegate(typeof(GetHandler));
		}

		// CreateSetDelegate
        public static SetHandler CreateSetHandler(Type type, PropertyInfo propertyInfo)
		{
			MethodInfo setMethodInfo = propertyInfo.GetSetMethod(true);
			DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
			ILGenerator setGenerator = dynamicSet.GetILGenerator();

			setGenerator.Emit(OpCodes.Ldarg_0);
			setGenerator.Emit(OpCodes.Ldarg_1);
			UnboxIfNeeded(setMethodInfo.GetParameters()[0].ParameterType, setGenerator);
			setGenerator.Emit(OpCodes.Call, setMethodInfo);
			setGenerator.Emit(OpCodes.Ret);

			return (SetHandler)dynamicSet.CreateDelegate(typeof(SetHandler));
		}

		// CreateSetDelegate
        public static SetHandler CreateSetHandler(Type type, FieldInfo fieldInfo)
		{
			DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
			ILGenerator setGenerator = dynamicSet.GetILGenerator();

			setGenerator.Emit(OpCodes.Ldarg_0);
			setGenerator.Emit(OpCodes.Ldarg_1);
			UnboxIfNeeded(fieldInfo.FieldType, setGenerator);
			setGenerator.Emit(OpCodes.Stfld, fieldInfo);
			setGenerator.Emit(OpCodes.Ret);

			return (SetHandler)dynamicSet.CreateDelegate(typeof(SetHandler));
		}

		// CreateGetDynamicMethod
        public static DynamicMethod CreateGetDynamicMethod(Type type)
		{
			return new DynamicMethod("DynamicGet", typeof(object),
				  new Type[] { typeof(object) }, type, true);
		}

		// CreateSetDynamicMethod
        public static DynamicMethod CreateSetDynamicMethod(Type type)
		{
			return new DynamicMethod("DynamicSet", typeof(void),
				  new Type[] { typeof(object), typeof(object) }, type, true);
		}

		// BoxIfNeeded
        public static void BoxIfNeeded(Type type, ILGenerator generator)
		{
			if (type.IsValueType)
			{
				generator.Emit(OpCodes.Box, type);
			}
		}

		// UnboxIfNeeded
        public static void UnboxIfNeeded(Type type, ILGenerator generator)
		{
			if (type.IsValueType)
			{
				generator.Emit(OpCodes.Unbox_Any, type);
			}
		}
	}
}