using UnityEngine;
using System.Collections;

public class UITreeGridDebugger : MonoBehaviour
{
	public string m_NodeRes;
	public UITreeGrid m_Target;
	public Transform m_ChildrenGroup;

	public bool m_AddChild = false;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_AddChild )
		{
			m_AddChild = false;
			AddChild();
		}
	}

	void AddChild ()
	{
		GameObject node = GameObject.Instantiate(Resources.Load(m_NodeRes) as GameObject) as GameObject;
		node.transform.parent = m_ChildrenGroup;
		node.transform.localPosition = Vector3.zero;
		node.transform.localRotation = Quaternion.identity;
		node.transform.localScale = Vector3.one;
		m_Target.m_Children.Add(node.GetComponent<UITreeGrid>());
		m_Target.transform.root.GetComponentInChildren<UITreeGrid>().Reposition();
	}
}
