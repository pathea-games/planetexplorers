using UnityEngine;
using System.Collections.Generic;
using PeCustom;

public class UIStopwatchList : MonoBehaviour
{
	[SerializeField] UIStopwatchNode NodePrefab;
	[SerializeField] Transform NodeGroup;

	bool mgrInited
	{
		get
		{
			if (PeCustomScene.Self == null)
				return false;
			if (PeCustomScene.Self.scenario == null)
				return false;
			if (PeCustomScene.Self.scenario.stopwatchMgr == null)
				return false;
			return true;
		}
	}
	StopwatchMgr mgr { get { return PeCustomScene.Self.scenario.stopwatchMgr; } }

	Dictionary<int, UIStopwatchNode> nodes;

	// Use this for initialization
	void Start ()
	{
		nodes = new Dictionary<int, UIStopwatchNode>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (mgrInited)
		{
			if (mgr.stopwatches.Count > 0)
			{
				foreach (var kvp in mgr.stopwatches)
				{
					if (!nodes.ContainsKey(kvp.Key))
					{
						UIStopwatchNode newnode = UIStopwatchNode.Instantiate(NodePrefab) as UIStopwatchNode;
						newnode.transform.parent = NodeGroup;  
						newnode.transform.localScale = Vector3.one;
						newnode.List = this;
						newnode.StopwatchId = kvp.Key;
						newnode.Index = nodes.Count;
						newnode.gameObject.SetActive(true);
						nodes.Add(kvp.Key, newnode);
					}
				}
			}

			if (nodes.Count > 0)
			{
				int idx = 0;
				foreach (var kvp in nodes)
				{
					kvp.Value.Index = idx++;
				}
			}
		}
	}

	public void DeleteNode (int id)
	{
		if (nodes.ContainsKey(id))
		{
			GameObject.Destroy(nodes[id].gameObject);
			nodes.Remove(id);
		}
	}
}
