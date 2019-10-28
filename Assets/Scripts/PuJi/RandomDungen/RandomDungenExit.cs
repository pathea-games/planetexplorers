using UnityEngine;
using System.Collections;

public class RandomDungenExit:MonoBehaviour
{
	bool isShow = false;
	
	void OnTriggerEnter(Collider target){
		//if(Application.isEditor)
		{
			Debug.Log("Exit dungen"); 
			if (null == target.GetComponentInParent<Pathea.MainPlayerCmpt>())
				return;
			if (isShow == true)
				return;
			
			isShow = true;
			MessageBox_N.ShowYNBox(PELocalization.GetString(DungenMessage.EXIT_DUNGEN), SceneTranslate, SetFalse);
		}
	}

	public void SceneTranslate()
	{
		RandomDungenMgr.Instance.ExitDungen();
	}
	public void SetFalse() { isShow = false; }
}

