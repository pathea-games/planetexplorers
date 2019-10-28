#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace WhiteCat.Internal
{
	public class RootLocator : ScriptableObject
	{
		static MonoScript _script;
		static string _directory;


		public static string directory
		{
			get
			{
				if (_script == null)
				{
					var instance = CreateInstance<RootLocator>();
					_script = MonoScript.FromScriptableObject(instance);
					DestroyImmediate(instance, false);

					_directory = AssetDatabase.GetAssetPath(_script);
					_directory = _directory.Substring(0, _directory.Length - 15);
				}
				return _directory;
			}
		}
	}
}
#endif