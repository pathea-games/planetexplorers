using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class EmitlineRayCaster : MonoBehaviour
	{
		GameObject eff_true;//绿色代表圆圈
		GameObject eff_false;//红色代表XX
		Transform receiver1;
		Transform receiver2;
		LineRenderer lr;
		bool isX;
		RaycastHit rh;
		EmitlineTask et;

		void Start()
		{
			eff_true = transform.FindChild("effect_true").gameObject;
			eff_false = transform.FindChild("effect_false").gameObject;
			et = EmitlineTask.Instance;
			receiver1 = et.receivers[0].colreceiver;//竖着的那个
			receiver2 = et.receivers[1].colreceiver;//平放的那个
			lr = GetComponent<LineRenderer>();
			//lr.material = et.matLine;
		}
		void Update()
		{
			Physics.Raycast(transform.position - transform.right * 15, transform.right, out rh, 20f, 1 << Pathea.Layer.VFVoxelTerrain);
			if(EmitlineTask.Instance.missionComplete)
				isX = false;
			else
				isX = rh.transform == receiver1 || rh.transform == receiver2;//只要碰到接收的Collider,就为true
			UpdateLine();
		}
		void UpdateLine()
		{
			Vector3 newPos = (Vector3.Distance(rh.point, transform.position) - 0.01f) * Vector3.right;
			lr.SetPosition(0, newPos);
			if(isX)			
			{
				eff_false.transform.localPosition = newPos;
				et.testScore -= 100;
			}
			else			
			{
				eff_true.transform.localPosition = newPos;
				et.testScore += 1;
			}
			eff_true.SetActive(!isX);
			eff_false.SetActive(isX);
		}
	}

}
