using UnityEngine;
using Pathea;

public class PlayerCameraTarget : MonoBehaviour
{
	private Transform _pelvis;
	private Transform _custom_camtar;

    public Transform CustomCameraTarget
    {
		get
		{
			_pelvis = Utils.GetChild(this.transform, "Bip01 Pelvis");
			string trans_name = "CamTar";
			Transform trans = transform.FindChild(trans_name);
			if (null == trans)
			{
				GameObject obj = new GameObject(trans_name);
				trans = obj.transform;
				trans.parent = transform;
				
				trans.localPosition = new Vector3(0f, 0f, 0f);
				trans.localRotation = Quaternion.identity;
				_custom_camtar = trans;
			}
			return trans;
		}
	}

	public Transform ShootCameraTarget
    {
		get
		{
			string trans_name = "ShootCamTar";
			Transform trans = transform.FindChild(trans_name);
	        if (null == trans)
	        {
				GameObject obj = new GameObject(trans_name);
	            trans = obj.transform;
	            trans.parent = transform;

	            trans.localPosition = new Vector3(0.557f, 1.727f, 0.592f);
				trans.localRotation = Quaternion.identity;
	        }

	        return trans;
		}
    }

	public Transform ClimbCameraTarget
	{
		get
		{
			return Utils.GetChild(this.transform, "Bip01 Spine3");
		}
	}

	void Update ()
	{
		if (_pelvis != null && _custom_camtar != null )
		{
			Vector3 bias = _pelvis.position - transform.position;
			bias.x *= 0.5f;
			bias.y *= 0.5f;
			float rise = 1.23f;
			float forward = 0.0f;
			if (PECameraMan.Instance.ControlType == 3)
			{
				rise = 1.58f;
				forward = 0.3f;
			}
			_custom_camtar.position = transform.position + bias * 0.1f + Vector3.up * rise + transform.forward * forward;
			_custom_camtar.rotation = transform.rotation;
		}
	}
}