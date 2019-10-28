using System;
using System.Collections;
using UnityEngine;

namespace WhiteCat.UnityExtension
{
	/// <summary>
	/// Unity 内置类型扩展
	/// </summary>
	public static class UnityExtension
	{
		/// <summary>
		/// 安全获取组件，如果物体上没有组件，自动添加新组件
		/// </summary>
		public static T SafeGetComponent<T>(this GameObject gameObject) where T : Component
		{
			T component = gameObject.GetComponent<T>();
			if (!component) component = gameObject.AddComponent<T>();
			return component;
		}


		/// <summary>
		/// 销毁 Transform 所有子级的游戏对象
		/// </summary>
		/// <param name="parent">要销毁所有子物体的父级 Transform</param>
		public static void DestroyAllChildren(this Transform parent)
		{
			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				GameObject.Destroy(parent.GetChild(i).gameObject);
			}
		}


		/// <summary>
		/// 将本地旋转换到世界旋转，转换受 Transform 的旋转和缩放影响
		/// </summary>
		/// <param name="localRotation">本地旋转</param>
		/// <returns>世界旋转</returns>
		public static Quaternion TransformRotation(this Transform parent, Quaternion localRotation)
		{
			return Quaternion.LookRotation(
				parent.TransformVector(localRotation * Vector3.forward),
				parent.TransformVector(localRotation * Vector3.up)
				);
		}


		/// <summary>
		/// 将世界旋转换到本地旋转，转换受 Transform 的旋转和缩放影响
		/// </summary>
		/// <param name="rotation">世界旋转</param>
		/// <returns>本地旋转</returns>
		public static Quaternion InverseTransformRotation(this Transform parent, Quaternion rotation)
		{
			return Quaternion.LookRotation(
				parent.InverseTransformVector(rotation * Vector3.forward),
				parent.InverseTransformVector(rotation * Vector3.up)
				);
		}


		/// <summary>
		/// (深度优先)遍历 Transform 层级, 对每一个访问的节点执行一个自定义的操作
		/// </summary>
		/// <param name="root">遍历开始的根部 Transform 对象</param>
		/// <param name="operate">遍历到每一个节点时将调用此方法; 参数1: 当前访问的对象; 参数2: 包括本层次在内的剩余深度限制</param>
		/// <param name="depthLimit">遍历深度限制, 负值表示不限制, 0 表示只访问 root 本身而不访问其子级, 正值表示最多访问的子级层数</param>
		public static void TraverseHierarchy(this Transform root, Action<Transform, int> operate, int depthLimit = -1)
		{
			if (operate != null)
			{
				operate(root, depthLimit);
				if (depthLimit == 0) return;

				for (int i = root.childCount - 1; i >= 0; i--)
				{
					TraverseHierarchy(root.GetChild(i), operate, depthLimit - 1);
				}
			}
		}


		/// <summary>
		/// (深度优先)遍历 Transform 层级, 对每一个访问的节点执行一个自定义的操作, 如果此操作返回一个非空引用将终止遍历过程, 并且此返回值最终作为遍历方法的返回值
		/// </summary>
		/// <param name="root">遍历开始的根部 Transform 对象</param>
		/// <param name="operate">遍历到每一个节点时将调用此方法; 参数1: 当前访问的对象; 参数2: 包括本层次在内的剩余深度限制; 返回值: null 表示继续遍历, 非空引用将终止遍历过程, 并且此返回值最终作为遍历方法的返回值</param>
		/// <param name="depthLimit">遍历深度限制, 负值表示不限制, 0 表示只访问 root 本身而不访问其子级, 正值表示最多访问的子级层数</param>
		/// <returns>第一次 operate 返回的非空引用或 null</returns>
		public static object TraverseHierarchy(this Transform root, Func<Transform, int, object> operate, int depthLimit = -1)
		{
			if (operate != null)
			{
				object obj = operate(root, depthLimit);
				if (obj != null || depthLimit == 0) return obj;

				for (int i = root.childCount - 1; i >= 0; i--)
				{
					obj = TraverseHierarchy(root.GetChild(i), operate, depthLimit - 1);
					if (obj != null) return obj;
				}
			}
			return null;
		}


		/// <summary>
		/// 延时调用指定的方法
		/// </summary>
		/// <param name="monoBehaviour">协程附着的脚本对象</param>
		/// <param name="seconds">延迟的秒数</param>
		/// <param name="method">延时结束调用的方法</param>
		public static void Invoke(this MonoBehaviour monoBehaviour, float seconds, Action method)
		{
			if (method != null)
			{
				monoBehaviour.StartCoroutine(DelayCall(seconds, method));
			}
		}


		static IEnumerator DelayCall(float seconds, Action method)
		{
			yield return new WaitForSeconds(seconds);
			method();
		}


		/// <summary>
		/// 计算正交相机屏幕上，每像素的世界单位（单位：米）大小
		/// </summary>
		/// <param name="camera">正交相机对象</param>
		/// <returns>每像素的世界单位大小</returns>
		public static float UnitsPerPixel(this Camera camera)
		{
			return camera.orthographicSize * 2 / Screen.height;
		}


		/// <summary>
		/// 计算正交相机屏幕上，每世界单位（单位：米）的像素大小
		/// </summary>
		/// <param name="camera">正交相机对象</param>
		/// <returns>每世界单位的像素大小</returns>
		public static float PixelsPerUnit(this Camera camera)
		{
			return Screen.height * 0.5f / camera.orthographicSize;
		}



	}
}
