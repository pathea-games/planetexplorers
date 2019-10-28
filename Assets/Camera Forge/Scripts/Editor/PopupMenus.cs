using UnityEngine;
using UnityEditor;
using System.Collections;
using CameraForge;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CameraForgeEditor
{
	public class MenuTypeDesc
	{
		public Type type;
		public string name;
		public int order;
		
		public static int Compare (MenuTypeDesc lhs, MenuTypeDesc rhs)
		{
			return lhs.order - rhs.order;
		}
	}

	public static class ModifierEditorMenu
	{
		static GenericMenu menu;
		static GenericMenu delete_menu;
		static bool inited = false;
		static Vector2 pos;
		static Node del_node;
		
		static void Init ()
		{
			menu = new GenericMenu();
			
			Assembly asm = Assembly.GetAssembly(typeof(Node));
			Type[] types = asm.GetTypes(); 

			List<MenuTypeDesc> menu_types = new List<MenuTypeDesc> ();
			foreach (Type t in types)
			{
				object[] attrs = t.GetCustomAttributes(false);
				if (attrs != null && attrs.Length > 0)
				{
					MenuAttribute attr = attrs[0] as MenuAttribute;
					if (attr != null)
					{
						MenuTypeDesc desc = new MenuTypeDesc ();
						desc.name = attr.Name;
						desc.order = attr.Order;
						desc.type = t;
						menu_types.Add(desc);
					}
				}
			}

			menu_types.Sort(MenuTypeDesc.Compare);

			foreach (MenuTypeDesc desc in menu_types)
			{
				Type t = desc.type;
				if (t.IsSubclassOf(typeof(OutputNode)))
				{
					if (t == CameraForgeWindow.modifier.output.GetType())
						menu.AddItem(new GUIContent("Output Type/" + desc.name), true, () => {});
					else
						menu.AddItem(new GUIContent("Output Type/" + desc.name), false, MenuFunc, t);
				}
				else
				{
					menu.AddItem(new GUIContent("Add Node/" + desc.name), false, MenuFunc, t);
				}
			}
			
			delete_menu = new GenericMenu ();
			delete_menu.AddItem(new GUIContent("Delete"), false, () => { CameraForgeWindow.modifier.DeleteNode(del_node); });

			inited = true;
		}
		
		static void MenuFunc (object t)
		{
			Type _t = t as Type;
			
			Node node = System.Activator.CreateInstance(_t) as Node;
			if (node != null)
			{
				node.editorPos = pos;
				node.modifier = CameraForgeWindow.modifier;
				if (!(node is OutputNode))
				{
					CameraForgeWindow.modifier.AddNode(node);
				}
				else
				{
					CameraForgeWindow.modifier.output = node as OutputNode;
				}
			}
		}
		
		public static void Popup(Vector2 position, Vector2 nodepos)
		{
			/*if (!inited)*/ Init();
			pos = nodepos;
			menu.DropDown(new Rect(position.x, position.y, 0,0));
		}
		public static void PopupDelete(Vector2 position, Node _del_node)
		{
			if (!inited) Init();
			del_node = _del_node;
			delete_menu.DropDown(new Rect(position.x, position.y, 0,0));
		}
	}

	public static class ControllerEditorMenu
	{
		static GenericMenu menu;
		static GenericMenu delete_menu;
		static bool inited = false;
		static Vector2 pos;
		static Node del_node;
		static PoseNode del_pose_node;

		static void Init ()
		{
			menu = new GenericMenu();
			
			Assembly asm = Assembly.GetAssembly(typeof(Node));
			Type[] types = asm.GetTypes(); 
			
			List<MenuTypeDesc> menu_types = new List<MenuTypeDesc> ();
			foreach (Type t in types)
			{
				object[] attrs = t.GetCustomAttributes(false);
				if (attrs != null && attrs.Length > 0)
				{
					MenuAttribute attr = attrs[0] as MenuAttribute;
					if (attr != null)
					{
						MenuTypeDesc desc = new MenuTypeDesc ();
						desc.name = attr.Name;
						desc.order = attr.Order;
						desc.type = t;
						menu_types.Add(desc);
					}
				}
			}
			
			menu_types.Sort(MenuTypeDesc.Compare);
			
			foreach (MenuTypeDesc desc in menu_types)
			{
				Type t = desc.type;
				if (!t.IsSubclassOf(typeof(OutputNode)))
				{
					menu.AddItem(new GUIContent("Add Node/" + desc.name), false, MenuFunc, t);
				}
			}

			foreach (Type t in types)
			{
				if (t.IsAbstract)
					continue;
				if (t.IsSubclassOf(typeof(MediaPoseNode)))
				{
					menu.AddItem(new GUIContent("Add Pose Node/Variable/" + t.Name), false, MenuFunc, t);
				}
				else if (t == typeof(Modifier))
				{
					menu.AddItem(new GUIContent("Add Modifier"), false, MenuFunc, t);
				}
				else if (t.IsSubclassOf(typeof(ScriptModifier)))
				{
					menu.AddItem(new GUIContent("Add Script Modifier/" + t.Name), false, MenuFunc, t);
				}
				else if (t.IsSubclassOf(typeof(PoseNode)) && t != typeof(FinalPose) && t != typeof(Controller))
				{
					menu.AddItem(new GUIContent("Add Pose Node/" + t.Name), false, MenuFunc, t);
				}
			}
			
			delete_menu = new GenericMenu ();

			delete_menu.AddItem(new GUIContent("Delete"), false, () =>
			{
				if (del_node != null)
					CameraForgeWindow.controller.DeleteNode(del_node);
				else if (del_pose_node != null)
					CameraForgeWindow.controller.DeletePoseNode(del_pose_node);

				del_node = null;
				del_pose_node = null;
			});
			
			inited = true;
		}
		
		static void MenuFunc (object t)
		{
			Type _t = t as Type;
			object obj = System.Activator.CreateInstance(_t);
			Node node = obj as Node;
			if (node != null)
			{
				node.editorPos = pos;
				node.modifier = CameraForgeWindow.controller;
				if (!(node is OutputNode))
				{
					CameraForgeWindow.controller.AddNode(node);
				}
			}
			PoseNode posenode = obj as PoseNode;
			if (posenode != null)
			{
				posenode.editorPos = pos;
				posenode.controller = CameraForgeWindow.controller;
				if (!(posenode is FinalPose))
				{
					CameraForgeWindow.controller.AddPoseNode(posenode);
				}
			}
		}
		
		public static void Popup(Vector2 position, Vector2 nodepos)
		{
			if (!inited) Init();
			pos = nodepos;
			menu.DropDown(new Rect(position.x, position.y, 0,0));
		}
		public static void PopupDelete(Vector2 position, Node _del_node, PoseNode _del_pose_node)
		{
			if (!inited) Init();
			del_node = _del_node;
			del_pose_node = _del_pose_node;
			delete_menu.DropDown(new Rect(position.x, position.y, 0,0));
		}
	}
}