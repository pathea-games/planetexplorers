using UnityEngine;
using UnityEditor;
using System;
using CameraForge;

namespace CameraForgeEditor
{
	public static class ControllerEditor
	{
		public static CameraForgeWindow cfwindow;
		public static Controller controller { get { return CameraForgeWindow.current as Controller; } }

		static Rect[] nodeRects;
		static bool[] nodeOutputs;

		static Rect[] posenodeRects;
		static bool[] posenodeOutputs;

		static bool mouseDown = false;
		static Vector2 mouseDownPos = Vector2.zero;
		static Vector2 mouseDownCenter = Vector2.zero;
		
		static Slot draggingSlot = null;
		static int draggingSlotIndex = -1;
		static Node dragSlotExcludeNode = null;
		
		static PoseSlot draggingPoseSlot = null;
		static int draggingPoseSlotIndex = -1;
		static PoseNode dragPoseSlotExcludeNode = null;
		
		static Slot editingSlot = null;
		static PoseBlend editingPoseBlend = null;
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
					mouseDownCenter = controller.graphCenter;
				}
				if (Event.current.rawType == EventType.mouseUp)
				{
					mouseDown = false;
				}
				if (draggingSlot == null && draggingPoseSlot == null && editingSlot == null && mouseDown && Event.current.button == 2)
				{
					Vector2 offset = Event.current.mousePosition - mouseDownPos;
					controller.graphCenter = mouseDownCenter - offset;
				}
			}
		}

		static void DragModifierInto ()
		{
			if (Event.current == null)
				return;
			var eventType = Event.current.type;
			if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
			{
				// Show a copy icon on the drag
				if (DragAndDrop.objectReferences.Length != 1)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
				}
				else
				{
					object drag_obj = DragAndDrop.objectReferences[0];
					ModifierAsset drag_asset = drag_obj as ModifierAsset;
					if (drag_asset != null)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					
						if (eventType == EventType.DragPerform)
						{
							DragAndDrop.AcceptDrag();
							ModifierAsset copied_asset = UnityEngine.Object.Instantiate<ModifierAsset>(drag_asset);
							copied_asset.Load(true);
							Modifier modifier = copied_asset.modifier;
							modifier.editorPos = Content2Node(Event.current.mousePosition);
							controller.AddPoseNode(modifier);
							ModifierAsset.DestroyImmediate(copied_asset);
						}
					}
					else
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					}
				}
				Event.current.Use();
			}
			if ( eventType == EventType.DragExited )
			{
				DragAndDrop.PrepareStartDrag();
			}

//			EditorGUILayout.BeginHorizontal ();
//			EditorGUILayout.LabelField ("特效路径", GUILayout.Width (80));
//			Rect sfxPathRect = EditorGUILayout.GetControlRect(GUILayout.Width (250));
//			string sfxPathText = EditorGUI.TextField (sfxPathRect, "");
//
//			if (Event.current != null)
//			{
//				if ( Event.current.type == EventType.DragExited )
//				{
//					Debug.Log(Event.current.mousePosition);
//					if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
//					{
//						DragAndDrop.AcceptDrag ();
//						foreach (string path in DragAndDrop.paths)
//						{
//							if (!string.IsNullOrEmpty(path))
//							{
//								if (Event.current.type == EventType.DragExited)
//								{
//									Debug.Log(path);
//								}
//							}
//						}
//					}
//				}
//			}
//			EditorGUILayout.EndHorizontal ();
//
		}

		// Init Temp Arrays
		static void InitTempArrays ()
		{
			int ncnt = controller.nodes.Count;
			int stncnt = controller.posenodes.Count;
			nodeRects = new Rect[ncnt] ;
			nodeOutputs = new bool[ncnt] ;
			posenodeRects = new Rect[stncnt + 1] ;
			posenodeOutputs = new bool[stncnt + 1] ;

			// Collect nodeOutputs
			for (int n = 0; n < ncnt; ++n)
			{
				Node node = null;
				node = controller.nodes[n];
				if (node != null)
				{
					for (int i = 0; i < node.slots.Length; ++i)
					{
						if (node.slots[i].input != null)
						{
							for (int k = 0; k < ncnt; ++k)
							{
								if (controller.nodes[k] == node.slots[i].input)
								{
									nodeOutputs[k] = true;
									break;
								}
							}
						}
					}
				}
			}

			// Collect posenodeOutputs
			for (int n = 0; n <= stncnt; ++n)
			{
				PoseNode posenode = null;
				if (n < stncnt)
					posenode = controller.posenodes[n];
				else
					posenode = controller.final;
				if (posenode != null)
				{
					for (int i = 0; i < posenode.slots.Length; ++i)
					{
						if (posenode.slots[i].input != null)
						{
							for (int k = 0; k < ncnt; ++k)
							{
								if (controller.nodes[k] == posenode.slots[i].input)
								{
									nodeOutputs[k] = true;
									break;
								}
							}
						}
					}
					for (int i = 0; i < posenode.poseslots.Length; ++i)
					{
						if (posenode.poseslots[i].input != null)
						{
							for (int k = 0; k < stncnt; ++k)
							{
								if (controller.posenodes[k] == posenode.poseslots[i].input)
								{
									posenodeOutputs[k] = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		// Node Windows
		const int poseStartID = 10000;
		static void NodeWindows ()
		{
			int ncnt = controller.nodes.Count;
			int stncnt = controller.posenodes.Count;
			cfwindow.BeginWindows();

			// Normal Node
			for (int n = 0; n < ncnt; ++n)
			{
				GUI.color = NodeColor(n);
				DrawNode(controller.nodes[n], n);
				GUI.color = Color.white;
			}
			// Pose Node
			for (int n = 0; n < stncnt; ++n)
			{
				GUI.color = NodeColor(poseStartID+n);
				DrawPoseNode(controller.posenodes[n], n);
				GUI.color = Color.white;
			}
			// Final Node
			if (controller.final != null)
			{
				GUI.color = NodeColor(controller.final);
				DrawPoseNode(controller.final, stncnt);
				GUI.color = Color.white;
			}

			cfwindow.EndWindows();
		}

		// Draw Bezier curve
		static void DrawBezierCurve (Vector2 thispos, Vector2 targetpos, int index)
		{
			
			float tgl = Vector2.Distance(targetpos, thispos) * 0.5f;
			tgl = Mathf.Min(100, tgl);
			
			Vector2 thistg = thispos - Vector2.right * tgl;
			Vector2 targettg = targetpos + Vector2.right * tgl;
			
			Handles.DrawBezier(thispos, targetpos, thistg, targettg, new Color(1,1,1,0.2f), null, 3f);
			
			float t = ((float)((cfwindow.frame + index * 45) % 400)) / 400f;
			
			if (CameraForgeWindow.showDirection)
			{
				t -= 0.025f;
				Vector3 p1 = Vector3.Lerp(Vector3.Lerp(Vector3.Lerp(targetpos, targettg, t), Vector3.Lerp(targettg, thistg, t), t), 
				                          Vector3.Lerp(Vector3.Lerp(targettg, thistg, t), Vector3.Lerp(thistg, thispos, t), t), t);
				t += 0.05f;
				Vector3 p2 = Vector3.Lerp(Vector3.Lerp(Vector3.Lerp(targetpos, targettg, t), Vector3.Lerp(targettg, thistg, t), t), 
				                          Vector3.Lerp(Vector3.Lerp(targettg, thistg, t), Vector3.Lerp(thistg, thispos, t), t), t);
				
				Color c = NodeColor(index);
				c = Color.Lerp(c, Color.white, 0.5f);
				//c.a = 0.5f;
				Handles.color = c;
				Handles.DrawLine(p1, p2);
				Handles.color = Color.white;
			}
		}

		// Draw Connection lines
		static void DrawConnectionLines ()
		{
			int ncnt = controller.nodes.Count;
			int stncnt = controller.posenodes.Count;

			// Draw node line
			for (int n = 0; n < ncnt; ++n)
			{
				Node node = null;
				node = controller.nodes[n];
				if (node == null)
					continue;

				for (int i = 0; i < node.slots.Length; ++i)
				{
					if (node.slots[i].input != null)
					{
						for (int k = 0; k < ncnt; ++k)
						{
							if (controller.nodes[k] == node.slots[i].input)
							{
								nodeOutputs[k] = true;
								Vector2 thispos = Node2Content(node.editorPos);
								thispos.y += (30 + i * 18);
								thispos.x += 8;
								Vector2 targetpos = new Vector2(nodeRects[k].xMax - 1, nodeRects[k].yMin + 8);
								DrawBezierCurve(thispos, targetpos, k);
								break;
							}
						}
					}
				}
			}	// End 'Draw node line'

			// Draw posenode line
			for (int n = 0; n <= stncnt; ++n)
			{
				PoseNode posenode = null;
				if (n < stncnt)
					posenode = controller.posenodes[n];
				else
					posenode = controller.final;
				if (posenode != null)
				{
					for (int i = 0; i < posenode.slots.Length; ++i)
					{
						if (posenode.slots[i].input != null)
						{
							for (int k = 0; k < ncnt; ++k)
							{
								if (controller.nodes[k] == posenode.slots[i].input)
								{
									nodeOutputs[k] = true;
									Vector2 thispos = Node2Content(posenode.editorPos);
									thispos.y += (30 + i * 18);
									thispos.x += 8;
									Vector2 targetpos = new Vector2(nodeRects[k].xMax - 1, nodeRects[k].yMin + 8);
									DrawBezierCurve(thispos, targetpos, k);
									break;
								}
							}
						}
					}
					for (int i = 0; i < posenode.poseslots.Length; ++i)
					{
						if (posenode.poseslots[i].input != null)
						{
							for (int k = 0; k < stncnt; ++k)
							{
								if (controller.posenodes[k] == posenode.poseslots[i].input)
								{
									posenodeOutputs[k] = true;
									Vector2 thispos = Node2Content(posenode.editorPos);
									thispos.y += (30 + (i + posenode.slots.Length) * 18);
									thispos.x += 8;
									Vector2 targetpos = new Vector2(posenodeRects[k].xMax - 1, posenodeRects[k].yMin + 8);
									DrawBezierCurve(thispos, targetpos, k + poseStartID);
									break;
								}
							}
						}
					}
				}
			}	// End 'Draw posenode line'

		}

		// Drag Slot
		static void DragSlot ()
		{
			int ncnt = controller.nodes.Count;
			
			// Drag slot
			if (draggingSlot != null && Event.current != null)
			{
				for (int n = 0; n < ncnt; ++n)
				{
					Node node = controller.nodes[n];
					if (dragSlotExcludeNode != null && dragSlotExcludeNode.IsDependencyOf(node))
						continue;
//					if (dragPoseSlotExcludeNode != null && ...)
//						continue;
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
				Vector2 thispos = Vector2.zero;
				if (dragSlotExcludeNode != null)
					thispos = Node2Content(dragSlotExcludeNode.editorPos);
				else if (dragPoseSlotExcludeNode != null)
					thispos = Node2Content(dragPoseSlotExcludeNode.editorPos);
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
					dragPoseSlotExcludeNode = null;
				}
			} // End 'Drag slot'
		}
		// Drag Pose Slot
		static void DragPoseSlot ()
		{
			int stncnt = controller.posenodes.Count;
			
			// Drag pose slot
			if (draggingPoseSlot != null && Event.current != null)
			{
				for (int n = 0; n < stncnt; ++n)
				{
					PoseNode posenode = controller.posenodes[n];

					if (posenode == dragPoseSlotExcludeNode)
						continue;
//					if (dragPoseSlotExcludeNode.IsDependencyOf(node))
//						continue;

					Rect outputRect = posenodeRects[n];
					outputRect.xMin = outputRect.xMax - 20;
					outputRect.yMin = outputRect.yMin - 3;
					outputRect.width = 24;
					outputRect.height = 24;
					
					bool inRect = outputRect.Contains(Event.current.mousePosition);
					
					if (!mouseDown)
					{
						if (inRect)
						{
							draggingPoseSlot.input = posenode;
							draggingPoseSlot.value = Pose.Default;
						}
					}
					
					Color c = NodeColor(posenode);
					c.a = inRect ? 1.0f : 0.4f;
					Color.Lerp(c, Color.white, inRect ? 0f : 0.5f);
					GUI.color = c;
					GUI.Label(outputRect, cfwindow.texTargetArea);
					GUI.color = Color.white;
				}
				
				// Dragging , draw line
				Vector2 thispos = Node2Content(dragPoseSlotExcludeNode.editorPos);
				thispos.y += (30 + (draggingPoseSlotIndex + dragPoseSlotExcludeNode.slots.Length) * 18);
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
					draggingPoseSlot = null;
					draggingPoseSlotIndex = -1;
					dragSlotExcludeNode = null;
					dragPoseSlotExcludeNode = null;
				}
			} // End 'Drag pose slot'
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
					ControllerEditorMenu.Popup(Event.current.mousePosition, Content2Node(Event.current.mousePosition));
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
			if (index >= 0 && index < controller.nodes.Count)
				nodeRects[index] = rct;
			if (!CameraForgeWindow.opening)
				node.editorPos = Content2Node(rct.position);
		}
		
		static void DrawPoseNode (PoseNode node, int index)
		{
			Vector2 nodepos = Node2Content(node.editorPos);
			Rect rct = new Rect(nodepos.x, nodepos.y, 30, 30);
			string type_name = node.GetType().Name;
			string window_name = type_name;
			if (node is FinalPose)
				window_name = "Final Pose";
			else if (node is HistoryPose)
				window_name = "History Pose";
			else if (node is Modifier)
				window_name = (node as Modifier).Name.value.value_str;
			else if (node is PoseBlend)
				window_name = (node as PoseBlend).Name.value.value_str;

			rct = GUILayout.Window(poseStartID + index, rct, NodeWindowFunc, window_name);
			if (index >= 0 && index < controller.posenodes.Count)
				posenodeRects[index] = rct;
			if (!CameraForgeWindow.opening)
				node.editorPos = Content2Node(rct.position);
		}
		
		static void NodeWindowFunc (int windowID)
		{
			if (windowID < poseStartID)
				NodeWindow(windowID);
			else
				PoseNodeWindow(windowID-poseStartID);
		}

		static void NodeWindow (int windowID)
		{
			Node node = null;
			if (windowID < controller.nodes.Count)
				node = controller.nodes[windowID];
			if (node == null)
			{
				GUI.DragWindow();
				return;
			}
			
			// Delete this node
			if (draggingSlot == null && draggingPoseSlot == null && editingSlot == null 
			    && Event.current != null && Event.current.type == EventType.mouseDown)
			{
				if (Event.current.button == 1 && Event.current.mousePosition.y < 16)
				{
					ControllerEditorMenu.PopupDelete(Event.current.mousePosition, node, null);
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
								dragPoseSlotExcludeNode = null;
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
				
				GUILayout.BeginVertical(GUILayout.MinWidth(80));
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

		public static object nextCurrent = null;
		static void PoseNodeWindow (int windowID)
		{
			PoseNode posenode = null;
			if (windowID < controller.posenodes.Count)
				posenode = controller.posenodes[windowID];
			else
				posenode = controller.final;
			if (posenode == null)
			{
				GUI.DragWindow();
				return;
			}
			
			// Delete this posenode
			if (draggingSlot == null && editingSlot == null && Event.current != null && Event.current.type == EventType.mouseDown)
			{
				if (Event.current.button == 1 && Event.current.mousePosition.y < 16)
				{
					ControllerEditorMenu.PopupDelete(Event.current.mousePosition, null, posenode);
				}
			}
			
			// Multiple-click this posenode
			if (draggingSlot == null && editingSlot == null && Event.current != null && Event.current.type == EventType.mouseDown)
			{
				if (Event.current.button == 0 && Event.current.clickCount == 2 && Event.current.mousePosition.y < 16)
				{
					if (posenode is Modifier)
					{
						CameraForgeWindow.nextCurrent = posenode;
						mouseDown = false;
					}
				}
			}

			GUILayout.BeginHorizontal(GUILayout.MinHeight(10));
			
			if (posenode.slots.Length + posenode.poseslots.Length > 0)
			{
				GUILayout.BeginVertical();
				GUILayout.Space(1);
				for (int i = 0; i < posenode.slots.Length; ++i)
				{
					//GUILayout.Space(-1);
					Rect rct = EditorGUILayout.BeginHorizontal();
					
					GUI.color = posenode.slots[i].input == null ? new Color(1,1,1,0.2f) : Color.white;
					if (posenode.slots[i].name == "Name" || posenode.slots[i].name == "Count")
						GUI.color = Color.clear;
					GUILayout.Label(posenode.slots[i].input == null ? cfwindow.texInputSlot : cfwindow.texInputSlotActive);
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
					Color c = NodeColor(posenode);
					c.a = 0.7f;
					GUI.color = c;
					if (posenode.slots[i].name != "Name" && posenode.slots[i].name != "Count")
					{
						if (Event.current != null && rct.Contains(Event.current.mousePosition))
						{
							GUI.Label(rct, "", gs);
							// Begin drag slot
							if (Event.current.type == EventType.MouseDown)
							{
								if (Event.current.button == 0)
								{
									draggingSlot = posenode.slots[i];
									draggingSlotIndex = i;
									draggingSlot.input = null;
									dragSlotExcludeNode = null;
									dragPoseSlotExcludeNode = posenode;
								}
							}
						}
					}
					GUI.color = Color.white;
				}
				for (int i = 0; i < posenode.poseslots.Length; ++i)
				{
					//GUILayout.Space(-1);
					Rect rct = EditorGUILayout.BeginHorizontal();
					
					GUI.color = posenode.poseslots[i].input == null ? new Color(1,1,1,0.2f) : Color.white;
					GUILayout.Label(posenode.poseslots[i].input == null ? cfwindow.texInputSlot : cfwindow.texInputSlotActive);
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
					Color c = NodeColor(posenode);
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
								draggingPoseSlot = posenode.poseslots[i];
								draggingPoseSlotIndex = i;
								draggingPoseSlot.input = null;
								dragSlotExcludeNode = null;
								dragPoseSlotExcludeNode = posenode;
							}
						}
					}
					GUI.color = Color.white;
				}
				GUILayout.Space(3);
				GUILayout.EndVertical();
				
				
				Rect nameRect = EditorGUILayout.BeginVertical(GUILayout.MinWidth(30), GUILayout.MaxWidth(200));
				GUILayout.Space(2);
				for (int i = 0; i < posenode.slots.Length; ++i)
				{
					if (posenode.slots[i] == editingSlot)
						GUI.color = Color.clear;
					GUILayout.Label(posenode.slots[i].name);
					GUI.color = Color.white;
				}
				for (int i = 0; i < posenode.poseslots.Length; ++i)
				{
					GUILayout.Label(posenode.poseslots[i].name);
				}
				EditorGUILayout.EndVertical();
				
				GUILayout.BeginVertical(GUILayout.MinWidth(20), GUILayout.MaxWidth(200));
				GUILayout.Space(2);
				for (int i = 0; i < posenode.slots.Length; ++i)
				{
					string value = "";
					if (!posenode.slots[i].value.isNull)
						value = posenode.slots[i].value.ToEditString(false);
					
					Rect rct = EditorGUILayout.BeginHorizontal();
					
					GUIStyle gs = new GUIStyle (EditorStyles.label);
					gs.alignment = TextAnchor.MiddleRight;
					gs.padding = new RectOffset (gs.padding.left, 5, gs.padding.top, gs.padding.bottom);
					
					Color c = NodeColor(posenode);
					c.a = 0.25f;
					
					if (posenode.slots[i].input != null)
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
						if (!posenode.slots[i].value.isNull)
							gbs.normal.background = null;
						if (posenode.slots[i] != editingSlot && GUI.Button(btn_rct, "", gbs))
						{
							if (Event.current != null && Event.current.button == 0)
							{
								EndEditingSlot();
								editingSlot = posenode.slots[i];
								editingSlotValue = posenode.slots[i].value.ToEditString(true);
								if (posenode is PoseBlend)
									editingPoseBlend = posenode as PoseBlend;
								else
									editingPoseBlend = null;
								GUI.FocusControl("");
							}
						}
						GUI.color = Color.white;
						
						if (posenode.slots[i].value.isNull)
							value = "?";
						
						if (posenode.slots[i] == editingSlot)
							GUI.color = Color.clear;
						GUILayout.Label(value,gs);
						GUI.color = Color.white;
						
						if (posenode.slots[i] == editingSlot)
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
				for (int i = 0; i < posenode.poseslots.Length; ++i)
				{
					string value = "Camera Pose";

					if (posenode is PoseBlend)
					{
						value = "Weight = " + (posenode as PoseBlend).GetWeight(i).ToString("0.000000");
					}
					
					EditorGUILayout.BeginHorizontal();
					
					GUIStyle gs = new GUIStyle (EditorStyles.label);
					gs.alignment = TextAnchor.MiddleRight;
					gs.padding = new RectOffset (gs.padding.left, 5, gs.padding.top, gs.padding.bottom);
					
					Color c = NodeColor(posenode);
					c.a = 0.25f;
					
					if (posenode.poseslots[i].input != null)
					{
						GUI.color = c;
						GUILayout.Label(value,gs);
						GUI.color = Color.white;
					}
					else
					{
						GUI.color = c;
						GUILayout.Label("Default Pose",gs);
						GUI.color = Color.white;
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
				
				GUILayout.BeginVertical(GUILayout.MinWidth(80));
				GUILayout.Space(0);
				Color c = NodeColor(posenode);
				c.a = 0.25f;
				GUI.color = c;
				GUIStyle gs = new GUIStyle ();
				gs.normal.textColor = Color.white;
				gs.alignment = TextAnchor.LowerRight;
				GUILayout.Label("<Empty>", gs);
				GUI.color = Color.white;
				GUILayout.Space(1);
				GUILayout.EndVertical();
			}
			
			if (!(posenode is FinalPose))
			{
				GUILayout.Space(-13);
				GUILayout.BeginVertical();
				GUILayout.Space(-21);
				GUI.color = NodeColor(posenode);
				if (!posenodeOutputs[windowID])
					GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, 0.2f);
				if (posenode == dragPoseSlotExcludeNode)
					GUI.color = Color.clear;
				GUILayout.Label(posenodeOutputs[windowID] ? cfwindow.texOutputSlotActive : cfwindow.texOutputSlot);
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
			
			if (dragPoseSlotExcludeNode != posenode)
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
					if (editingSlot.name == "Count" && editingPoseBlend != null)
					{
						int count = Mathf.RoundToInt(editingSlot.value.value_f);
						editingPoseBlend.UpdateCount(count);
					}
				}
			}
			editingSlot = null;
			editingPoseBlend = null;
			editingSlotValue = "";
		}
		
		// ---------------------------------

		// Utilities
		static Vector2 Node2Content (Vector2 pos)
		{
			Vector2 center = cfwindow.contentRect.size * 0.5f;
			center.x = (int)(center.x);
			center.y = (int)(center.y);
			
			return center + pos - controller.graphCenter;
		}
		
		static Vector2 Content2Node (Vector2 pos)
		{
			Vector2 center = cfwindow.contentRect.size * 0.5f;
			center.x = (int)(center.x);
			center.y = (int)(center.y);
			
			return pos + controller.graphCenter - center;
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
		
		static Color NodeColor (PoseNode posenode)
		{
			if (posenode is Modifier)
				return (posenode as Modifier).Col.value.value_c;
			else if (posenode is ScriptModifier)
				return (posenode as ScriptModifier).Col.value.value_c;
			else if (posenode is MediaPoseNode)
				return Color.white;
			else if (posenode is PoseBlend)
				return Color.white;
			else
				return Color.white;
		}
		
		static Color NodeColor (int i)
		{
			if (i >= poseStartID)
				return NodeColor(controller.posenodes[i-poseStartID]);
			else
				return NodeColor(controller.nodes[i]);
		}

		// ----------------------------------
		//               On GUI
		// ----------------------------------

		public static void OnGUI ()
		{
			DragGraph();
			InitTempArrays();
			NodeWindows();
			DrawConnectionLines();
			DragSlot();
			DragPoseSlot();
			EditSlot();
			PopupMenu();
			DragModifierInto();
		}
	}
}
