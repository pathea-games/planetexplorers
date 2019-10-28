using System;
using System.Reflection;

namespace WhiteCat.ReflectionExtension
{
	/// <summary>
	/// 反射扩展
	/// </summary>
	public static class ReflectionExtension
	{
		/// <summary>
		/// 获取对象的字段信息。从最终类型开始向上查找，忽略字段的可见性。
		/// </summary>
		public static FieldInfo GetFieldInfo(this object instance, string fieldName)
		{
			Type type = instance.GetType();
			FieldInfo info = null;
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			while(type != null)
			{
				info = type.GetField(fieldName, flags);
				if (info != null) return info;
				type = type.BaseType;
			}

			return null;
		}


		/// <summary>
		/// 获取对象的属性信息。从最终类型开始向上查找，忽略属性的可见性。
		/// </summary>
		public static PropertyInfo GetPropertyInfo(this object instance, string propertyName)
		{
			Type type = instance.GetType();
			PropertyInfo info = null;
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			while (type != null)
			{
				info = type.GetProperty(propertyName, flags);
				if (info != null) return info;
				type = type.BaseType;
			}

			return null;
		}


		/// <summary>
		/// 获取对象的方法信息。从最终类型开始向上查找，忽略方法的可见性。
		/// </summary>
		public static MethodInfo GetMethodInfo(this object instance, string methodName)
		{
			Type type = instance.GetType();
			MethodInfo info = null;
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			while (type != null)
			{
				info = type.GetMethod(methodName, flags);
				if (info != null) return info;
				type = type.BaseType;
			}

			return null;
		}


		/// <summary>
		/// 获取对象的字段值。从最终类型开始向上查找，忽略字段的可见性。
		/// </summary>
		public static object GetFieldValue(this object instance, string fieldName)
		{
			return instance.GetFieldInfo(fieldName).GetValue(instance);
		}


		/// <summary>
		/// 设置对象的字段值。从最终类型开始向上查找，忽略字段的可见性。
		/// </summary>
		public static void SetFieldValue(this object instance, string fieldName, object value)
		{
			instance.GetFieldInfo(fieldName).SetValue(instance, value);
		}


		/// <summary>
		/// 获取对象的属性值。从最终类型开始向上查找，忽略属性的可见性。
		/// </summary>
		public static object GetPropertyValue(this object instance, string propertyName)
		{
			return instance.GetPropertyInfo(propertyName).GetValue(instance, null); 
		}


		/// <summary>
		/// 设置对象的属性值。从最终类型开始向上查找，忽略属性的可见性。
		/// </summary>
		public static void SetPropertyValue(this object instance, string propertyName, object value)
		{
			instance.GetPropertyInfo(propertyName).SetValue(instance, value, null);
		}


		/// <summary>
		/// 调用对象的方法。从最终类型开始向上查找，忽略方法的可见性。
		/// </summary>
		public static object InvokeMethod(this object instance, string methodName, params object[] param)
		{
			return instance.GetMethodInfo(methodName).Invoke(instance, param);
		}
	}
}
