using UnityEngine;
using System.Collections;
using UnityEditor;


public class UIComponents : ScriptableObject 
{
	// need set Components prefab path
	const string mComponentPath = "Prefab/UICompoment/";
		
	// add obj to selection Transform
	static void AddComponent(GameObject component)
	{
		Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);  
		
		foreach(Transform transform in transforms)  
		{  
			GameObject newGameObject = GameObject.Instantiate(component) as GameObject;
			newGameObject.transform.parent = transform;  
			newGameObject.layer = newGameObject.transform.parent.gameObject.layer;
			newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localScale = Vector3.one;
		} 
	}


	// add Components to Unity menu, you can add compoment in here
	[MenuItem ("GameUI/Add BaseWnd")] 
	static void AddBaseWnd()  
	{  
		string path = mComponentPath + "BaseWnd";
		GameObject obj = Resources.Load(path)  as GameObject;  
		if (obj != null)
			AddComponent(obj);
		else
			Debug.LogError("Components prefab  path error! Path detail:" + path);
	} 

	[MenuItem ("GameUI/Add MidWnd")] 
	static void AddMidWnd()  
	{  
		string path = mComponentPath + "MidWnd";
		GameObject obj = Resources.Load(path)  as GameObject;  
		if (obj != null)
			AddComponent(obj);
		else
			Debug.LogError("Components prefab  path error! Path detail:" + path);
	} 


	[MenuItem ("GameUI/Add N_ImageButton")] 
	static void AddN_ImageButton()  
	{  
		string path = mComponentPath + "N_ImageButton";
		GameObject obj = Resources.Load(path)  as GameObject;  
		if (obj != null)
			AddComponent(obj);
		else
			Debug.LogError("Components prefab  path error! Path detail:" + path);
	} 

	[MenuItem ("GameUI/Add N_Button")] 
	static void AddN_Button()  
	{  
		string path = mComponentPath + "N_Button";
		GameObject obj = Resources.Load(path)  as GameObject;  
		if (obj != null)
			AddComponent(obj);
		else
			Debug.LogError("Components prefab  path error! Path detail:" + path);
	} 

	[MenuItem ("GameUI/Add MenuList")]  
	static void AddMenuList()  
	{  
		string path = mComponentPath + "MenuList";
		GameObject obj = Resources.Load(path)  as GameObject;  
		if (obj != null)
			AddComponent(obj);
		else
			Debug.LogError("Components prefab  path error! Path detail:" + path);
	} 
	[MenuItem ("GameUI/Add MissionTree")]  
	static void AddMessionTree()
	{
		string path = mComponentPath + "MissionTree";
		GameObject obj = Resources.Load(path)  as GameObject;  
		if (obj != null)
			AddComponent(obj);
		else
			Debug.LogError("Components prefab  path error! Path detail:" + path);
	}


//	[MenuItem ("GameUI/Add NewGrid_N")]
//	static void AddNewGrid_N()  
//	{  
//		string path = mComponentPath + "NewGrid_N";
//		GameObject newMenuList = Resources.Load(path)  as GameObject;  
//		if (newMenuList != null)
//			AddComponent(newMenuList);
//		else
//			Debug.LogError("Components prefab  path error! Path detail:" + path);
//	} 


}