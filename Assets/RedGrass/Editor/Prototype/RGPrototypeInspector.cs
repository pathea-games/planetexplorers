using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace RedGrass
{
	[CustomEditor(typeof(RGPrototypeMgr), true)]
	public class RGPrototypeInspector : Editor
	{
		int offset = 0;
		int count_per_page = 10;
		static bool expand = false;
		
		private void RegisterModify(string name)
		{
			Undo.RecordObject(target, name);
			EditorUtility.SetDirty(target);
		}
		
		public override void OnInspectorGUI ()
		{
			RGPrototypeMgr manager = target as RGPrototypeMgr;

			GUILayout.BeginVertical();
			EditorGUIUtility.labelWidth = 100;
			EditorGUIUtility.fieldWidth = 190;
			manager.m_DiffuseMapFileName = EditorGUILayout.TextField("Diffuse output", manager.m_DiffuseMapFileName);
			manager.m_ParticleMapFileName = EditorGUILayout.TextField("Particle output", manager.m_ParticleMapFileName);
			manager.m_PropertyMapFileName = EditorGUILayout.TextField("Property output", manager.m_PropertyMapFileName);
			GUILayout.Space(20);
			GUILayout.Label("Diffuse Map      Particle Map      Property Map");
			GUILayout.BeginHorizontal();
			GUILayout.Space(7);
			GUILayout.BeginVertical(GUILayout.Width(88));
			EditorGUIUtility.labelWidth = 1;
			EditorGUIUtility.fieldWidth = 58;
			manager.m_DiffuseMap = EditorGUILayout.ObjectField("", manager.m_DiffuseMap, typeof(Texture2D), false, GUILayout.ExpandWidth(false)) as Texture2D;
			GUILayout.EndVertical();
			GUILayout.BeginVertical(GUILayout.Width(88));
			EditorGUIUtility.labelWidth = 1;
			EditorGUIUtility.fieldWidth = 58;
			manager.m_ParticleMap = EditorGUILayout.ObjectField("", manager.m_ParticleMap, typeof(Texture2D), false, GUILayout.ExpandWidth(false)) as Texture2D;
			GUILayout.EndVertical();
			GUILayout.BeginVertical(GUILayout.Width(88));
			EditorGUIUtility.labelWidth = 1;
			EditorGUIUtility.fieldWidth = 58;
			manager.m_PropertyMap = EditorGUILayout.ObjectField("", manager.m_PropertyMap, typeof(Texture2D), false, GUILayout.ExpandWidth(false)) as Texture2D;
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.Space(20); 
			
			GUI.backgroundColor = expand ? Color.red : Color.green;
	        if (GUILayout.Button("Expand/Collapse prototypes", GUILayout.Width(200f), GUILayout.Height(21f)))
	            expand = !expand;
			GUILayout.Space(10); 
			
			if ( manager.m_Prototypes == null )
			{
				manager.m_Prototypes = new List<RGPrototype>();
				RegisterModify("New prototypes list");
			}
			while ( manager.m_Prototypes.Count < RGPrototypeMgr.s_PrototypeCount )
			{
				manager.m_Prototypes.Add(new RGPrototype());
				RegisterModify("Fill Prototypes");
			}
			
			if ( expand )
			{
				GUILayout.Label("Select Page: ");
				GUILayout.BeginHorizontal();
				for ( int i = 0; i < RGPrototypeMgr.s_PrototypeCount; i += count_per_page )
				{
					if ( offset == i )
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				        if (GUILayout.Button(i.ToString("00"), GUILayout.Width(24f), GUILayout.Height(21f)))
				        {
				            offset = i;
				        }
					}
					else
					{
						GUI.backgroundColor = Color.blue;
				        if (GUILayout.Button(i.ToString("00"), GUILayout.Width(24f), GUILayout.Height(21f)))
				        {
				            offset = i;
				        }
					}

				}
				GUI.backgroundColor = Color.white;
				GUILayout.EndHorizontal();
				
				for ( int i = offset; i < offset + count_per_page && i < RGPrototypeMgr.s_PrototypeCount; ++i )
				{
					GUILayout.Label("-------------------------------------------------------------------------");
					GUI.color = Color.yellow;
					GUILayout.Label("Prototype " + i.ToString("00"));
					GUI.color = Color.white;
					GUILayout.Space(2);
					GUILayout.Label("Diffuse         Particle        Properties");
					GUILayout.BeginHorizontal();
					GUILayout.Space(7);
					GUILayout.BeginVertical(GUILayout.Width(71));
					EditorGUIUtility.labelWidth = 1;
					EditorGUIUtility.fieldWidth = 58;
					Texture2D diffuse = EditorGUILayout.ObjectField("", manager.m_Prototypes[i].m_Diffuse, typeof(Texture2D), false, GUILayout.ExpandWidth(false)) as Texture2D;
					if ( diffuse != manager.m_Prototypes[i].m_Diffuse )
					{
						manager.m_Prototypes[i].m_Diffuse = diffuse;
						RegisterModify("Set diffuse");
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical(GUILayout.Width(71));
					EditorGUIUtility.labelWidth = 1;
					EditorGUIUtility.fieldWidth = 58;
					Texture2D particle = EditorGUILayout.ObjectField("", manager.m_Prototypes[i].m_Particle, typeof(Texture2D), false, GUILayout.ExpandWidth(false)) as Texture2D;
					if ( particle != manager.m_Prototypes[i].m_Particle )
					{
						manager.m_Prototypes[i].m_Particle = particle;
						RegisterModify("Set particle");
					}
					EditorGUIUtility.labelWidth = 25;
					manager.m_Prototypes[i].m_ParticleTintColor = EditorGUILayout.ColorField("Tint", manager.m_Prototypes[i].m_ParticleTintColor, GUILayout.Width(67));
					GUILayout.EndVertical();
					GUILayout.Space(-4);
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 54;
					manager.m_Prototypes[i].m_MinSize.x = EditorGUILayout.FloatField("Size Min:", manager.m_Prototypes[i].m_MinSize.x, GUILayout.Width(84));
					manager.m_Prototypes[i].m_MinSize.y = EditorGUILayout.FloatField(manager.m_Prototypes[i].m_MinSize.y, GUILayout.Width(30));
					EditorGUIUtility.labelWidth = 30;
					manager.m_Prototypes[i].m_MaxSize.x = EditorGUILayout.FloatField("Max:", manager.m_Prototypes[i].m_MaxSize.x, GUILayout.Width(60));
					manager.m_Prototypes[i].m_MaxSize.y = EditorGUILayout.FloatField(manager.m_Prototypes[i].m_MaxSize.y, GUILayout.Width(30));
					GUILayout.EndHorizontal();
					EditorGUIUtility.labelWidth = 80;
					manager.m_Prototypes[i].m_BendFactor = EditorGUILayout.FloatField("Bend Factor:", manager.m_Prototypes[i].m_BendFactor, GUILayout.Width(130));
					EditorGUIUtility.labelWidth = 80;
					manager.m_Prototypes[i].m_LODBias = EditorGUILayout.FloatField("LOD Bias:", manager.m_Prototypes[i].m_LODBias, GUILayout.Width(130));
					
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				GUILayout.Label("-------------------------------------------------------------------------");
				GUI.backgroundColor = Color.green;
				GUILayout.Space(10);
				if ( GUILayout.Button("Generate Textures", GUILayout.Width(250f), GUILayout.Height(31f)) )
				{
//					manager.GenerateTextures();
					GenerateTextures(manager);
					EditorUtility.DisplayDialog("Generate Textures Complete", "Diffuse map, particle map, property map\r\nDone!", "OK");
				}
				GUILayout.Space(20);
			}
			GUILayout.EndVertical();
			if ( GUI.changed ) RegisterModify("Set value");
		}

		void GenerateTextures (RGPrototypeMgr manager)
		{
			for ( int i = 0; i <  RGPrototypeMgr.s_PrototypeCount; ++i )
			{
				if ( manager.m_Prototypes[i].m_Diffuse != null )
				{
					manager.m_DiffuseMap.SetPixels((i%RGPrototypeMgr.s_PrototypeRowCount)*RGPrototypeMgr.s_MapResolution, 
					                               (i/RGPrototypeMgr.s_PrototypeRowCount)*RGPrototypeMgr.s_MapResolution, RGPrototypeMgr.s_MapResolution, RGPrototypeMgr.s_MapResolution, 
					                               manager.m_Prototypes[i].m_Diffuse.GetPixels(0,0,RGPrototypeMgr.s_MapResolution,RGPrototypeMgr.s_MapResolution));
				}
				else
				{
					manager.m_DiffuseMap.SetPixels((i%RGPrototypeMgr.s_PrototypeRowCount)*RGPrototypeMgr.s_MapResolution, 
					                               (i/RGPrototypeMgr.s_PrototypeRowCount)*RGPrototypeMgr.s_MapResolution, RGPrototypeMgr.s_MapResolution, RGPrototypeMgr.s_MapResolution, 
					                                new Color[RGPrototypeMgr.s_MapResolution*RGPrototypeMgr.s_MapResolution]);
				}
			}

			for ( int i = 0; i < RGPrototypeMgr.s_PrototypeCount; ++i )
			{
				manager.m_PropertyMap.SetPixel(i,0,new Color(manager.m_Prototypes[i].m_MinSize.x*0.5f, manager.m_Prototypes[i].m_MinSize.y*0.5f, manager.m_Prototypes[i].m_MaxSize.x*0.5f, manager.m_Prototypes[i].m_MaxSize.y*0.5f));
				manager.m_PropertyMap.SetPixel(i,1,new Color(manager.m_Prototypes[i].m_BendFactor*2, (manager.m_Prototypes[i].m_LODBias + 4.0f)/8.0f, 0, 1));
				manager.m_PropertyMap.SetPixel(i,2,manager.m_Prototypes[i].m_ParticleTintColor);
				manager.m_PropertyMap.SetPixel(i,3,new Color(0,0,0,1));
			}

			manager.m_DiffuseMap.Apply();
			manager.m_PropertyMap.Apply();

			byte[] pixels = null;
			pixels = manager.m_DiffuseMap.EncodeToPNG();
			string path = AssetDatabase.GetAssetPath(manager.m_DiffuseMap);
			System.IO.File.WriteAllBytes(path, pixels);
			pixels = manager.m_PropertyMap.EncodeToPNG();
			path = AssetDatabase.GetAssetPath(manager.m_PropertyMap);
			System.IO.File.WriteAllBytes(path, pixels);
		}
	}
}
