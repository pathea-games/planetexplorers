using UnityEngine;
using System.Collections;

public class WaterDetect : MonoBehaviour 
{
	public Transform WaterChuncksRoot;

	public float DectectDistance = 100;

	const float c_SqrDistance = 100 * 100;
	
	/* Use s_chunkLod0Cnt/s_chunkLodxCnt instead of water detection
	 * 
	// Use this for initialization
	void Start () 
	{
	}

	// Update is called once per frame
	void Update () 
	{
		if (WaterChuncksRoot == null)
			return;
	
		if (WaterReflection.InstanceSets == null || WaterReflection.InstanceSets.Count == 0)
			return;

		if (WaterReflection.ReflectionSetting == 1)
		{
			foreach (WaterReflection wr in WaterReflection.InstanceSets)
			{
				if (wr.CurCam == null )
					continue;

				Vector3 cameraPos = wr.CurCam.transform.position;
				for (int i = 0; i < WaterChuncksRoot.childCount; i++)
				{
					Transform trans = WaterChuncksRoot.GetChild(i);
					
					if ((cameraPos - trans.position).sqrMagnitude < c_SqrDistance)
					{
						wr.WaterChunkInRange = true;
						break;
					}
				}
			}
		}
		else if (WaterReflection.ReflectionSetting == 2)
		{
		}
	}

	void OnWillRenderObject ()
	{
	}
	 */
}
