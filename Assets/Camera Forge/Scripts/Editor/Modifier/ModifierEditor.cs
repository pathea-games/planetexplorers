using UnityEngine;
using UnityEditor;
using System;
using CameraForge;

namespace CameraForgeEditor
{
	public static class ModifierEditor
	{
		public static CameraForgeWindow cfwindow;
		public static Modifier modifier { get { return CameraForgeWindow.current as Modifier; } }

		static Rect[] nodeRects;
		static bool[] nodeOutputs;
		
		static bool mouseDown = false;
		static Vector2 mouseDownPos = Vector2.zero;
		static Vector2 mouseDownCenter = Vector2.zero;
		
		static Slot draggingSlot = null;
		static int draggingSlotIndex = -1;
		static Node dragSlotExcludeNode = null;
		
		static Slot editingSlot = null;
		static string editingSlotValue = "";
		static bool editingEntering = false;

		// Drag Graph
		static void DragGraph ()
		{
			if (Event.current != null)
			{
				if (Event.current.type == EventType.MouseDown)
				{
					mouseDown = true;
					mouseDownPos = Event.current.mousePosition;
					mouseDownCenter = modifier.graphCenter;
				}
				if (Event.current.rawType == EventType.mouseUp)
				{
					mouseDown = false;
				}
				if (draggingSlot == null && editingSlot == null && mouseDown && Event.current.button == 2)
				{
					Vector2 offset = Event.current.mousePosition - mouseDownPos;
					modifier.graphCenter = mouseDownCenter - offset;
				}
			}
		}

		// Init Temp Arrays
		static void InitTempArrays ()
		{
			int ncnt = modifier.nodes.Count;
			nodeRects = new Rect[ncnt + 1] ;
			nodeOutputs = new bool[ncnt + 1] ;

			// Collect nodeOutputs
			for (int n = 0; n <= ncnt; ++n)
			{
				Node node = null;
				if (n < ncnt)
					node = modifier.nodes[n];
				else
					node = modifier.output;
				if (node != null)
				{
					for (int i = 0; i < node.slots.Length; ++i)
					{
						if (node.slots[i].input != null)
						{
							for (int k = 0; k < ncnt; ++k)
							{
								if (modifier.nodes[k] == node.slots[i].input)
								{
									nodeOutputs[k] = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		// Node Windows
		static void NodeWindows ()
		{
			int ncnt = modifier.nodes.Count;
			cfwindow.BeginWindows();

			// Normal Node
			for (int n = 0; n < ncnt; ++n)
			{
				GUI.color = NodeColor(n);
				DrawNode(modifier.nodes[n], n);
				GUI.color = Color.white;
			}
			// Output Node
			if (modifier.output != null)
			{
				GUI.color = NodeColor(modifier.output);
				DrawNode(modifier.output, ncnt);
				GUI.color = Color.white;
			}

			cfwindow.EndWindows();
		}

		// Draw Connection lines
		static void DrawConnectionLines ()
		{
			int ncnt = modifier.nodes.Count;

			// Draw bezier lines
			for (int n = 0; n <= ncnt; ++n)
			{
				Node node = null;
				if (n < ncnt)
					node = modifier.nodes[n];
				else
					node = modifier.output;
				if (node == null)
					continue;

				for (int i = 0; i < node.slots.Length; ++i)
				{
					if (node.slots[i].input != null)
					{
						for (int k = 0; k < ncnt; ++k)
						{
							if (modifier.nodes[k] == node.slots[i].input)
							{
								nodeOutputs[k] = true;
								Vector2 thispos = Node2Content(node.editorPos);
								thispos.y += (30 + i * 18);
								thispos.x += 8;
								Vector2 targetpos = new Vector2(nodeRects[k].xMax - 1, nodeRects[k].yMin + 8);
								
								float tgl = Vector2.Distance(targetpos, thispos) * 0.5f;
								tgl = Mathf.Min(100, tgl);
								
								Vector2 thistg = thispos - Vector2.right * tgl;
								Vector2 targettg = targetpos + Vector2.right * tgl;
								
								Handles.DrawBezier(thispos, targetpos, thistg, targettg, new Color(1,1,1,0.2f), null, 3f);
								
								float t = ((float)((cfwindow.frame + k * 45) % 400)) / 400f;
								
								if (CameraForgeWindow.showDirection)
								{
									t -= 0.025f;
									Vector3 p1 = Vector3.Lerp(Vector3.Lerp(Vector3.Lerp(targetpos, targettg, t), Vector3.Lerp(targettg, thistg, t), t), 
									                          Vector3.Lerp(Vector3.Lerp(targettg, thistg, t), Vector3.Lerp(thistg, thispos, t), t), t);
									t += 0.05f;
									Vector3 p2 = Vector3.Lerp(Vector3.Lerp(Vector3.Lerp(targetpos, targettg, t), Vector3.Lerp(targettg, thistg, t), t), 
									                          Vector3.Lerp(Vector3.Lerp(targettg, thistg, t), Vector3.Lerp(thistg, thispos, t), t), t);
									
									Color c = NodeColor(k);
									c = Color.Lerp(c, Color.white, 0.5f);
									//c.a = 0.5f;
									Handles.color = c;
									Handles.DrawLine(p1, p2);
									Handles.color = Color.white;
								}
								break;
							}
						}
					}
				}
			}	// End 'Draw bezier line'
		}

		// Drag Slot
		static void DragSlot ()
		{
			int ncnt = modifier.nodes.Count;
			
			// Drag slot
			if (draggingSlot != null && Event.current != null)
			{
				for (int n = 0; n < ncnt; ++n)
				{
					Node node = modifier.nodes[n];
					if (dragSlotExcludeNode.IsDependencyOf(node))
						continue;
					Rect outputRect = nodeRects[n];
					outputRect.xMin = outputRect.xMax - 20;
					outputRect.yMin = outputRect.yMin - 3;
					outputRect.width = 24;
					outputRect.height = 24;
					
					bool inRect = outputRect.Contains(Event.current.mousePosition);
					
					if (!mouseDown)
					{
						if (inRect)
						{
							draggingSlot.input = node;
							draggingSlot.value = Var.Null;
						}
					}
					
					Color c = NodeColor(node);
					c.a = inRect ? 1.0f : 0.4f;
					Color.Lerp(c, Color.white, inRect ? 0f : 0.5f);
					GUI.color = c;
					GUI.Label(outputRect, cfwindow.texTargetArea);
					GUI.color = Color.white;
				}
				
				// Dragging , draw line
				Vector2 thispos = Node2Content(dragSlotExcludeNode.editorPos);
				thispos.y += (30 + draggingSlotIndex * 18);
				thispos.x += 8;
				Vector2 targetpos = Event.current.mousePosition;
				
				float tgl = Vector2.Distance(targetpos, thispos) * 0.5f;
				tgl = Mathf.Min(100, tgl);
				
				Vector2 thistg = thispos - Vector2.right * tgl;
				Vector2 targettg = targetpos + Vector2.right * tgl;
				
				if (Vector3.Distance(thispos, targetpos) > 8)
					Handles.DrawBezier(thispos, targetpos, thistg, targettg, new Color(1,1,1,0.2f), null, 3f);
				
				if (!mouseDown)
				{
					draggingSlot = null;
					draggingSlotIndex = -1;
					dragSlotExcludeNode = null;
				}
			} // End 'Drag slot'
		}

		// Edit Slot
		static void EditSlot ()
		{
			// When editing slot
			if (editingSlot != null && Event.current != null)
			{
				if (Event.current.rawType == EventType.MouseUp)
				{
					if (GUI.GetNameOfFocusedControl() != "EditSlot")
					{
						EndEditingSlot();
					}
				}
				if (Event.current.type == EventType.keyDown)
				{
					if (Event.current.keyCode == KeyCode.Return || 
					    Event.current.keyCode == KeyCode.KeypadEnter ||
					    Event.current.character == '\n')
					{
						if (GUI.GetNameOfFocusedControl() == "EditSlot")
						{
							if (!editingEntering)
								EndEditingSlot();
							editingEntering = false;
						}
						else
						{
							GUI.FocusControl("EditSlot");
							editingEntering = true;
						}
					}
				}
			}
		}

		// Menu
		static void PopupMenu ()
		{
			if (Event.current != null && Event.current.type == EventType.mouseDown)
			{
				if (Event.current.button == 1)
					ModifierEditorMenu.Popup(Event.current.mousePosition, Content2Node(Event.current.mousePosition));
			}
		}

		// ---------------------------------
		// ---------------------------------
		// ---------------------------------
		// ---------------------------------


		// Node Window
		static void DrawNode (Node node, int index)
		{
			Vector2 nodepos = Node2Content(node.editorPos);
			Rect rct = new Rect(nodepos.x, nodepos.y, 30, 30);
			string type_name = node.GetType().Name;
			
			rct = GUILayout.Window(index, rct, NodeWindowFunc, type_name);
			if (index >= 0 && index < modifier.nodes.Count)
				nodeRects[index] = rct;
			if (!CameraForgeWindow.opening)
				node.editorPos = Content2Node(rct.position);
		}
		
		static void NodeWindowFunc (int windowID)
		{
			Node node = null;
			if (windowID < modifier.nodes.Count)
				node = modifier.nodes[windowID];
			else
				node = modifier.output;
			if (node == null)
			{
				GUI.DragWindow();
				return;
			}
			
			// Delete this node
			if (draggingSlot == null && editingSlot == null && Event.current != null && Event.current.type == EventType.mouseDown)
			{
				if (Event.current.button == 1 && Event.current.mousePosition.y < 16)
				{
					ModifierEditorMenu.PopupDelete(Event.current.mousePosition, node);
				}
			}
			
			GUILayout.BeginHorizontal(GUILayout.MinHeight(10));
			
			if (node.slots.Length > 0)
			{
				GUILayout.BeginVertical();
				GUILayout.Space(1);
				for (int i = 0; i < node.slots.Length; ++i)
				{
					//GUILayout.Space(-1);
					Rect rct = EditorGUILayout.BeginHorizontal();
					
					GUI.color = node.slots[i].input == null ? new Color(1,1,1,0.2f) : Color.white;
					GUILayout.Label(node.slots[i].input == null ? cfwindow.texInputSlot : cfwindow.texInputSlotActive);
					GUI.color = Color.white;
					EditorGUILayout.EndHorizontal(); 
					GUILayout.Space(-3);
					
					// Draw "drag slot area"
					GUIStyle gs = new GUIStyle ();
					gs.normal.background = cfwindow.texInputSlot;
					gs.hover.background = cfwindow.texInputSlot;
					gs.active.background = cfwindow.texInputSlot;
					
					rct.x += 2;
					rct.y += 1;
					rct.width = 16;
					rct.height = 16;
					Color c = NodeColor(node);
					c.a = 0.7f;
					GUI.color = c;
					if (Event.current != null && rct.Contains(Event.current.mousePosition))
					{
						GUI.Label(rct, "", gs);
						// Begin drag slot
						if (Event.current.type == EventType.MouseDown)
						{
							if (Event.current.button == 0)
							{
								draggingSlot = node.slots[i];
								draggingSlotIndex = i;
								draggingSlot.input = null;
								dragSlotExcludeNode = node;
							}
						}
					}
					GUI.color = Color.white;
				}
				GUILayout.Space(3);
				GUILayout.EndVertical();
				
				
				Rect nameRect = EditorGUILayout.BeginVertical(GUILayout.MinWidth(30), GUILayout.MaxWidth(200));
				GUILayout.Space(2);
				for (int i = 0; i < node.slots.Length; ++i)
				{
					if (node.slots[i] == editingSlot)
						GUI.color = Color.clear;
					GUILayout.Label(node.slots[i].name);
					GUI.color = Color.white;
				}
				EditorGUILayout.EndVertical();
				
				GUILayout.BeginVertical(GUILayout.MinWidth(20), GUILayout.MaxWidth(200));
				GUILayout.Space(2);
				for (int i = 0; i < node.slots.Length; ++i)
				{
					string value = "";
					if (!node.slots[i].value.isNull)
						value = node.slots[i].value.ToEditString(false);
					
					Rect rct = EditorGUILayout.BeginHorizontal();
					
					GUIStyle gs = new GUIStyle (EditorStyles.label);
					gs.alignment = TextAnchor.MiddleRight;
					gs.padding = new RectOffset (gs.padding.left, 5, gs.padding.top, gs.padding.bottom);
					
					Color c = NodeColor(node);
					c.a = 0.25f;
					
					if (node.slots[i].input != null)
					{
						GUI.color = c;
						GUILayout.Label(value,gs);
						GUI.color = Color.white;
					}
					else
					{
						GUIStyle gbs = new GUIStyle (EditorStyles.miniButton);
						gbs.hover.background = gbs.normal.background;
						
						Rect btn_rct = new Rect (rct.xMin - (nameRect.width + 8), rct.yMin, rct.width + (nameRect.width + 8), rct.height - 1);
						
						c.a = 0.5f;
						GUI.color = c;
						if (!node.slots[i].value.isNull)
							gbs.normal.background = null;
						if (node.slots[i] != editingSlot && GUI.Button(btn_rct, "", gbs))
						{
							if (Event.current != null && Event.current.button == 0)
							{
								EndEditingSlot();
								editingSlot = node.slots[i];
								editingSlotValue = node.slots[i].value.ToEditString(true);
								GUI.FocusControl("");
							}
						}
						GUI.color = Color.white;
						
						if (node.slots[i].value.isNull)
							value = "?";
						
						if (node.slots[i] == editingSlot)
							GUI.color = Color.clear;
						GUILayout.Label(value,gs);
						GUI.color = Color.white;
						
						if (node.slots[i] == editingSlot)
						{
							GUIStyle gfs = new GUIStyle (EditorStyles.textField);
							gfs.alignment = TextAnchor.MiddleRight;
							gfs.padding = new RectOffset (gfs.padding.left, 5, gfs.padding.top, gfs.padding.bottom);
							gfs.fontSize = 10;
							c.a = 1;
							//GUI.color = c;
							GUI.SetNextControlName("EditSlot");
							editingSlotValue = GUI.TextField(btn_rct, editingSlotValue, gfs);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			else
			{
				GUILayout.BeginVertical(GUILayout.MaxWidth(5));
				GUILayout.Space(2);
				GUILayout.EndVertical();
				
				GUILayout.BeginVertical(GUILayout.MaxWidth(5));
				GUILayout.Space(2);
				GUILayout.EndVertical();
				
				GUILayout.BeginVertical(GUILayout.MinWidth(90));
				GUILayout.Space(0);
				Color c = NodeColor(node);
				c.a = 0.25f;
				GUI.color = c;
				GUIStyle gs = new GUIStyle ();
				gs.normal.textColor = Color.white;
				gs.alignment = TextAnchor.LowerRight;
				GUILayout.Label(node.Calculate().ToEditString(false), gs);
				GUI.color = Color.white;
				GUILayout.Space(1);
				GUILayout.EndVertical();
			}
			
			if (!(node is OutputNode))
			{
				GUILayout.Space(-13);
				GUILayout.BeginVertical();
				GUILayout.Space(-21);
				GUI.color = NodeColor(node);
				if (!nodeOutputs[windowID])
					GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, 0.2f);
				if (node == dragSlotExcludeNode)
					GUI.color = Color.clear;
				GUILayout.Label(nodeOutputs[windowID] ? cfwindow.texOutputSlotActive : cfwindow.texOutputSlot);
				GUI.color = Color.white;
				GUILayout.EndVertical();
				GUILayout.Space(-7);
			}
			else
			{
				GUILayout.BeginVertical();
				GUILayout.EndVertical();
			}
			
			GUILayout.EndHorizontal();
			
			if (dragSlotExcludeNode != node)
				GUI.DragWindow ();
		}

		// End Edit Slot
		static void EndEditingSlot ()
		{
			if (editingSlot != null)
			{
				Var v = Var.Parse(editingSlotValue);
				if (v.isNull)
				{
					EditorUtility.DisplayDialog("Invalid value", "Please specify a valid value", "OK");
				}
				else
				{
					editingSlot.input = null;
					editingSlot.value = v;
				}
			}
			editingSlot = null;
			editingSlotValue = "";
		}
		
		// ---------------------------------

		// Utilities
		static Vector2 Node2Content (Vector2 pos)
		{
			Vector2 center = cfwindow.contentRect.size * 0.5f;
			center.x = (int)(center.x);
			center.y = (int)(center.y);
			
			return center + pos - modifier.graphCenter;
		}
		
		static Vector2 Content2Node (Vector2 pos)
		{
			Vector2 center = cfwindow.contentRect.size * 0.5f;
			center.x = (int)(center.x);
			center.y = (int)(center.y);
			
			return pos + modifier.graphCenter - center;
		}
		
		static Color NodeColor (Node node)
		{
			if (node is FunctionNode)
				return new Color (0.3f,0.5f,1.0f,1.0f);
			else if (node is InputNode)
				return new Color (1.0f,0.0f,0.0f,1.0f);
			else if (node is VarNode)
				return new Color (0.9f,0.8f,0.7f,1.0f);
			else if (node is MediaNode)
				return Color.white;
			else if (node is OutputNode)
				return new Color (0.3f,1.0f,0.0f,1.0f);
			else
				return Color.white;
		}
		
		static Color NodeColor (int i)
		{
			return NodeColor(modifier.nodes[i]);
		}

		// ----------------------------------
		//             On GUI
		// ----------------------------------

		public static void OnGUI ()
		{
			DragGraph();
			InitTempArrays();
			NodeWindows();
			DrawConnectionLines();
			DragSlot();
			EditSlot();
			PopupMenu();
		}
	}
}
