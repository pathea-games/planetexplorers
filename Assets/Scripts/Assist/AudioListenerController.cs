using UnityEngine;
using System.Collections;
using Pathea;

public class AudioListenerController : MonoBehaviour 
{
    Transform mainPlayerTrans;

    void Update()
    {
		if(PETools.PEUtil.MainCamTransform != null)
		{
			Transform tCam = PETools.PEUtil.MainCamTransform;
			transform.position = tCam.position;
			transform.rotation = tCam.rotation;
		}
//        if (mainPlayerTrans == null && PeCreature.Instance.mainPlayer != null)
//            mainPlayerTrans = PeCreature.Instance.mainPlayer.tr;
//
//        if(mainPlayerTrans != null)
//        {
//            transform.position = mainPlayerTrans.position + mainPlayerTrans.up * 1.8f;
//            transform.rotation = mainPlayerTrans.rotation;
//        }
//        else
//        {
//			Transform tCam = PETools.PEUtil.MainCamTransform;
//			if (tCam != null)
//		    {
//				transform.position = tCam.position + tCam.forward;
//				transform.rotation = tCam.rotation;
//		    }
//        }
    }
}
