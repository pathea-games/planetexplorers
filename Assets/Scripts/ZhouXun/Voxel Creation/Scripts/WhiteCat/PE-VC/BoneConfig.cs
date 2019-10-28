using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WhiteCat
{
	public class BoneConfig : ScriptableObject
	{
		public string[] boneNames;
		public string[] searchFolders;


		static BoneConfig _instance;
		public static BoneConfig instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<BoneConfig>("BoneConfig");
				}
				return _instance;
			}
		}


#if UNITY_EDITOR

		[CustomEditor(typeof(BoneConfig))]
		class BoneConfigEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();

				if (GUILayout.Button("Reset All Prefabs *"))
				{
					var guids = AssetDatabase.FindAssets("t:GameObject", (target as BoneConfig).searchFolders);
					foreach (var guid in guids)
					{
						var prefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(GameObject)) as GameObject;
						var bc = prefab.GetComponent<BoneCollector>();
						if (bc != null) Undo.DestroyObjectImmediate(bc);

						bc = Undo.AddComponent<BoneCollector>(prefab);
						if (!bc.isValid) Undo.DestroyObjectImmediate(bc);
						EditorUtility.SetDirty(prefab);
					}
				}
			}
		}


		[MenuItem("Assets/Create/Bone Config")]
		static void CreateConfig()
		{
			AssetDatabase.CreateAsset(CreateInstance<BoneConfig>(), Internal.RootLocator.directory + "/PE-VC/Resources/BoneConfig.asset");
		}

#endif
	}
}