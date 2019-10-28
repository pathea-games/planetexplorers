using UnityEngine;
using UnityEditor;
using CameraForge;

namespace CameraForgeEditor
{
	public class CameraForgeWindow : EditorWindow
	{
		public int frame = 0;
		void Update ()
		{
			frame++;
//			if (modifier != null)
//				modifier.output.Output();
			Repaint();

			if (nextCurrent != null)
			{
				current = nextCurrent;
				nextCurrent = null;
			}

			if (Application.isPlaying)
			{
				if (current == null)
				{
					if (CameraController.debugController != null)
					{
						current = CameraController.debugController;
						asset = null;
						Init();
					}
				}
			}
		}
		
		public Rect windowRect;
		public Rect contentRect;

		void OnGUI ()
		{
			LoadTextures();

			// Rect
			windowRect = new Rect(0,0,position.width,position.height);

			// Top
			DrawLayoutBoxes();
			DrawGrid();
			DrawFunctionButtons();
			DrawLogo();

			// Content
			if (controller != null)
			{
				DrawName(controller.Name.value.value_str, Color.white);
				GUI.BeginGroup(contentRect);
				ControllerEditor.OnGUI();
				GUI.EndGroup();
			}
			else if (modifier != null)
			{
				if (modifier.controller != null)
					DrawName(modifier.controller.Name.value.value_str + " / " + modifier.Name.value.value_str, new Color(0.9f, 0.8f, 0.7f, 1.0f));
				else
					DrawName(modifier.Name.value.value_str, new Color(0.9f, 0.8f, 0.7f, 1.0f));

				GUI.BeginGroup(contentRect);
				ModifierEditor.OnGUI();
				GUI.EndGroup();
			}
			else if (current != null)
			{
				DrawName("Camera Forge cannot edit this object", Color.red);
			}
			else
			{
				DrawName("Please select a Camera Controller or Modifier and load", Color.gray);
			}
			opening = false;
		}

		// ---------------------------------
		void DrawLayoutBoxes ()
		{
			contentRect = windowRect;
			contentRect.xMin += 2;
			contentRect.xMax -= 2;
			contentRect.yMin += 40;
			contentRect.yMax -= 2;
			GUI.color = Color.black;
			GUI.Box(contentRect, "");
			GUI.Box(contentRect, "");
			GUI.Box(contentRect, "");
			GUI.color = Color.white;
		}
		
		void DrawFunctionButtons ()
		{
			if (modifier != null || controller != null)
			{
				GUIStyle gs = new GUIStyle ();
				gs.normal.background = texSave;
				if (GUI.Button(new Rect(windowRect.xMax - 38, 1, 36, 36), "X"))
				{
					current = null;
					asset = null;
					if (Application.isPlaying)
						CameraController.debugController = null;
				}
				if (asset != null)
				{
					if (GUI.Button(new Rect(windowRect.xMax - 76, 1, 36, 36), texSave))
						Save();
				}

				if (controller != null)
				{
					if (GUI.Button(new Rect(windowRect.xMax - 114, 1, 36, 36), "SvAs"))
					{
						if (EditorUtility.DisplayDialog("Save as", "Save this controller as a ControllerAsset ?", "OK", "Cancel"))
							EditorMenus.CreateController(controller);
					}
				}

				if (modifier != null && modifier.controller != null)
				{
					if (GUI.Button(new Rect(windowRect.xMax - 114, 1, 36, 36), "SvAs"))
					{
						if (EditorUtility.DisplayDialog("Save as", "Save this modifier as a ModifierAsset ?", "OK", "Cancel"))
						    EditorMenus.CreateModifier(modifier);
					}
					if (GUI.Button(new Rect(windowRect.xMax - 152, 1, 36, 36), "<<<"))
						CameraForgeWindow.current = modifier.controller;
				}
			}
		}

		public void DrawName (string name, Color color)
		{
			Rect nameRect = new Rect (8,8,windowRect.width, 30);
			GUIStyle gs = new GUIStyle (EditorStyles.largeLabel);
			gs.fontStyle = FontStyle.Bold;
			gs.fontSize = 20;
			gs.normal.textColor = Color.black;
			GUI.Label(nameRect, name, gs);
			nameRect.yMin -= 2;
			nameRect.xMin -= 2;
			gs.normal.textColor = color;
			GUI.Label(nameRect, name, gs);
		}

		public void DrawLogo ()
		{
			GUI.DrawTexture(new Rect(windowRect.width - 256, windowRect.height - 64, 256, 64), texLogo);
		}

		Vector2 Content2Node (Vector2 pos)
		{
			Vector2 center = contentRect.size * 0.5f;
			center.x = (int)(center.x);
			center.y = (int)(center.y);
			
			return pos + modifier.graphCenter - center;
		}

		public void DrawGrid ()
		{
			if (modifier != null)
			{
				Rect coord = new Rect ();
				Vector2 nodepos = Content2Node(Vector2.zero);
				coord.x = nodepos.x / 128.0f;
				coord.y = nodepos.y / 128.0f;
				coord.width = contentRect.width / 128.0f;
				coord.height = -contentRect.height / 128.0f;
				GUI.color = new Color(1f,1f,1f,0.1f);
				GUI.DrawTextureWithTexCoords(contentRect, texGrid, coord);
				GUI.color = Color.white;
			}
		}

		// Texture Resourses
		public Texture2D texLogo;
		public Texture2D texInputSlot;
		public Texture2D texInputSlotActive;
		public Texture2D texOutputSlot;
		public Texture2D texOutputSlotActive;
		public Texture2D texTargetArea;
		public Texture2D texSave;
		public Texture2D texGrid;

		public void LoadTextures()
		{
			if (texLogo == null)
				texLogo = Resources.Load("cf_logo") as Texture2D;
			if (texInputSlot == null)
				texInputSlot = Resources.Load("cf_input_slot") as Texture2D;
			if (texInputSlotActive == null)
				texInputSlotActive = Resources.Load("cf_input_slot_active") as Texture2D;
			if (texOutputSlot == null)
				texOutputSlot = Resources.Load("cf_output_slot") as Texture2D;
			if (texOutputSlotActive == null)
				texOutputSlotActive = Resources.Load("cf_output_slot_active") as Texture2D;
			if (texTargetArea == null)
				texTargetArea = Resources.Load("cf_target_area") as Texture2D;
			if (texSave == null)
				texSave = Resources.Load("cf_save") as Texture2D;
			if (texGrid == null)
				texGrid = Resources.Load("cf_grid") as Texture2D;
		}
		


		//
		// Static
		//

		public static ScriptableObject asset;
		public static object current;
		public static object nextCurrent;
		public static Modifier modifier { get { return current as Modifier; } }
		public static Controller controller { get { return current as Controller; } }
		public static bool showDirection = true;

		public static bool opening = false;
		[MenuItem ("Window/Camera Forge")]
		public static void Init ()
		{
			// Get existing open window or if none, make a new one:
			CameraForgeWindow window = (CameraForgeWindow)EditorWindow.GetWindow(typeof(CameraForgeWindow));
			ModifierEditor.cfwindow = window;
			ControllerEditor.cfwindow = window;
			window.titleContent = new GUIContent("Camera Forge");
			window.Show();
			opening = true;
		}

		public static void Save ()
		{
			if (asset != null)
			{
				if (asset is ModifierAsset)
				{
					(asset as ModifierAsset).Save();
					AssetDatabase.SaveAssets();
					EditorUtility.SetDirty(asset);
				}
				if (asset is ControllerAsset)
				{
					(asset as ControllerAsset).Save();
					AssetDatabase.SaveAssets();
					EditorUtility.SetDirty(asset);
				}
			}
		}
	}
}
