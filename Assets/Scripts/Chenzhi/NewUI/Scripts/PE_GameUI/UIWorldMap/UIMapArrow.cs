using UnityEngine;
using System.Collections;

public class UIMapArrow : MonoBehaviour
{
	public enum EArrowType
	{
		Main,
		Other
	}

	//private EArrowType m_ArrowType = EArrowType.Main;

	public PeMap.ILabel trackLabel;

	public float visualWidth = 190;
	public float visualHeight = 200;

    //const float c_ErrorX = 20;
    //const float c_ErrorY = 30;
    //public float relMinX { get { return -visualWidth * 0.5f + c_ErrorX; } }
    //public float relMaxX { get { return visualWidth * 0.5f - (c_ErrorX + 5); } }
    //public float relMinY { get { return -visualWidth * 0.5f + c_ErrorY; } }
    //public float relMaxY { get { return visualWidth * 0.5f - c_ErrorY + 5; } }

    public GameObject content;

	public void SetLabel (PeMap.ILabel label, EArrowType arrow_type)
	{
		//m_ArrowType = arrow_type;
		 trackLabel = label;
	}

	void UpdateTrans()
	{
		Vector3 dis =  trackLabel.GetPos() - GameUI.Instance.mMainPlayer.position;
		Vector3 dir = dis;
		dir.y = 0;



        // screen pos pos
        float s_pos_x = dis.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x;
		float s_pos_z = dis.z * GameUI.Instance.mUIMinMapCtrl.mMapScale.y;


        float radius =  5;//trackLabel.GetRadius() / GameUI.Instance.mUIMinMapCtrl.mMapScale.x;
        if (s_pos_x >= -visualWidth * 0.5f - radius && s_pos_x <= visualWidth * 0.5f + radius
            && s_pos_z >= -visualHeight * 0.5f - radius && s_pos_z <= visualHeight * 0.5f + radius)
        {
            if (content.activeSelf)
                content.SetActive(false);
        }
        else
        {
            if (!content.activeSelf)
                content.SetActive(true);
        }


        float pos_x = 0;
        float pos_z = 0;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            Vector3 _dir = dir;
            _dir.x = Mathf.Sign(_dir.x);
            _dir.z = dir.z / (_dir.x * dir.x);

            pos_x = _dir.x < 0 ? (visualWidth * 0.5f - 10) * _dir.x: (visualWidth * 0.5f - 45) * _dir.x;
            pos_z = _dir.z < 0 ? (visualHeight * 0.5f - 20) *_dir.z: (visualHeight * 0.5f - 20) * _dir.z;
        }
        else if (Mathf.Abs(dir.x) < Mathf.Abs(dir.z))
        {
            Vector3 _dir = dir;

            _dir.z = Mathf.Sign(_dir.z);
            _dir.x = dir.x / (_dir.z * dir.z);
            pos_x = _dir.x < 0 ? (visualWidth * 0.5f - 10) * _dir.x: (visualWidth * 0.5f - 45) * _dir.x;
            pos_z = _dir.z < 0 ? (visualHeight * 0.5f - 20) * _dir.z: (visualHeight * 0.5f - 20) * _dir.z;
        }
        else
        {
            pos_x = 0;
            pos_z = 0;
        }


        transform.localPosition = new Vector3(pos_x, pos_z, 0);

		Vector3 euler = Quaternion.LookRotation(dir).eulerAngles;
		transform.localRotation =  Quaternion.Euler( new Vector3(0, 0, Mathf.FloorToInt( -euler.y)) );


	}

	#region UNITY_INNER_FUNC

	void Update ()
	{
		if (trackLabel != null && GameUI.Instance.mMainPlayer != null)
		{
			UpdateTrans();

		}
	}

	#endregion
}
