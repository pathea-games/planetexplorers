using System.IO;
using UnityEngine;

public class DragCreationAgent : DragArticleAgent, ISceneObjAgent
{
	//WhiteCat.CreationController _creationController;

	Vector3 ISceneObjAgent.Pos
	{
		get
		{
			return position;// + Vector3.up * 40f;
		}
	}


	public DragCreationAgent()
	{
	}


	public DragCreationAgent(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
        :base(drag, pos, scl, rot, id, net)
    {
	}
}
