using UnityEngine;
using System.Collections;

public static class UIMouseEvent
{
	// Mouse on any GUI
	private static int lastCheckOnAnyGUIFrame = -1;
	private static bool lastOnAnyGUI = false;
	public static bool onAnyGUI
	{
		get
		{
			if (Time.frameCount == lastCheckOnAnyGUIFrame)
				return lastOnAnyGUI;
			lastCheckOnAnyGUIFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				lastOnAnyGUI = Physics.Raycast(ray, 512f, 1 << Pathea.Layer.GUI);
				return lastOnAnyGUI;
			}
			else
			{
				lastOnAnyGUI = false;
				return false;
			}
		}
	}

	private static int lastCheckOpAnyGUIFrame = -1;
	private static bool lastOpAnyGUI = false;
	public static bool opAnyGUI
	{
		get
		{
			if (Time.frameCount == lastCheckOpAnyGUIFrame)
				return lastOpAnyGUI;
			if (Input.GetMouseButton(0))
				return lastOpAnyGUI;
			if (Input.GetMouseButton(1))
				return lastOpAnyGUI;
			if (Input.GetMouseButton(2))
				return lastOpAnyGUI;
			lastCheckOpAnyGUIFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				lastOpAnyGUI = Physics.Raycast(ray, 512f, 1 << Pathea.Layer.GUI);
				return lastOpAnyGUI;
			}
			else
			{
				lastOpAnyGUI = false;
				return false;
			}
		}
	}
	
	// Mouse on any Scroll GUI
	private static int lastCheckOnAnyScrollFrame = -1;
	private static bool lastOnAnyScroll = false;
	public static bool onAnyScroll
	{
		get
		{
			if (Time.frameCount == lastCheckOnAnyScrollFrame)
				return lastOnAnyScroll;
			lastCheckOnAnyScrollFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit rch;
				if (Physics.Raycast(ray, out rch, 512f, 1 << Pathea.Layer.GUI))
				{
					if (rch.collider.GetComponent<UIOnScrollMouse>() != null)
						lastOnAnyScroll = true;
					else
						lastOnAnyScroll = false;
				}
				else
				{
					lastOnAnyScroll = false;
				}
				return lastOnAnyScroll;
			}
			else
			{
				lastOnAnyScroll = false;
				return false;
			}
		}
	}
	
	// Mouse op any Scroll GUI
	private static int lastCheckOpAnyScrollFrame = -1;
	private static bool lastOpAnyScroll = false;
	public static bool opAnyScroll
	{
		get
		{
			if (Time.frameCount == lastCheckOpAnyScrollFrame)
				return lastOpAnyScroll;
			if (Input.GetMouseButton(0))
				return lastOpAnyScroll;
			if (Input.GetMouseButton(1))
				return lastOpAnyScroll;
			if (Input.GetMouseButton(2))
				return lastOpAnyScroll;
			lastCheckOpAnyScrollFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit rch;
				if (Physics.Raycast(ray, out rch, 512f, 1 << Pathea.Layer.GUI))
				{
					if (rch.collider.GetComponent<UIOnScrollMouse>() != null)
						lastOpAnyScroll = true;
					else
						lastOpAnyScroll = false;
				}
				else
				{
					lastOpAnyScroll = false;
				}
				return lastOpAnyScroll;
			}
			else
			{
				lastOpAnyScroll = false;
				return false;
			}
		}
	}
}
