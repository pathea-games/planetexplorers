using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public static class Utils
	{
		public static GameObject CreateGameObject (GameObject res, string name, Transform parent)
		{
			if (res != null)
			{
				GameObject go = GameObject.Instantiate(res) as GameObject;
				go.name = name;
				go.transform.SetParent(parent, false);
				go.transform.localPosition = Vector3.zero;
				if (parent != null)
					go.layer = parent.gameObject.layer;
				return go;
			}
			else
			{
				GameObject go = new GameObject (name);
				go.transform.SetParent(parent, false);
				go.transform.localPosition = Vector3.zero;
				if (parent != null)
					go.layer = parent.gameObject.layer;
				return go;
			}
		}

		public static T CreateGameObject<T> (GameObject res, string name, Transform parent) where T : Component
		{
			if (res != null)
			{
				GameObject go = GameObject.Instantiate(res) as GameObject;
				go.name = name;
				go.transform.SetParent(parent, false);
				go.transform.localPosition = Vector3.zero;
				if (parent != null)
					go.layer = parent.gameObject.layer;
				T t = go.GetComponent<T>();
				if (t == null)
					GameObject.Destroy(go);
				return t;
			}
			else
			{
				GameObject go = new GameObject (name);
				go.transform.SetParent(parent, false);
				go.transform.localPosition = Vector3.zero;
				if (parent != null)
					go.layer = parent.gameObject.layer;
				T t = go.AddComponent<T>();
				return t;
			}
		}

		public static GameObject CreateGameObject (PrimitiveType primitive, string name, Transform parent)
		{
			GameObject go = GameObject.CreatePrimitive(primitive);
			go.name = name;
			go.transform.SetParent(parent, false);
			go.transform.localPosition = Vector3.zero;
			if (parent != null)
				go.layer = parent.gameObject.layer;
			if (go.GetComponent<Collider>() != null)
				Collider.Destroy(go.GetComponent<Collider>());
			return go;
		}

		public static void DestroyChild (GameObject gameObject)
		{
			for (int i = 0; i < gameObject.transform.childCount; ++i)
			{
				GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
			}
		}

		public static float NormalizeDegree (float deg)
		{
			while ( deg > 180 )
				deg -= 360;
			while ( deg < -180 )
				deg += 360;
			return deg;
		}

		public static Color ColorSaturate (Color c, float saturate)
		{
			float grayscale = c.grayscale;
			Color target = new Color(grayscale, grayscale, grayscale, 1);
			return Color.Lerp(target, c, saturate);
		}
	}
}
