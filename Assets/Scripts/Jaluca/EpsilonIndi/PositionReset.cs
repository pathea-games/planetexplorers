using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EpsilonIndi
{
	public class PositionReset : MonoBehaviour {
		public bool test;
		[SerializeField]Transform sightpoint;
		[SerializeField]Transform shipsight;
		[SerializeField]Transform sun;
		[SerializeField]Transform[] planets;
		ShipSightPath sp;
		ShipSightRotate sr;
		float dt;
		//float rotateX;
		float rotateY;
		float ssrotate;
		List<OrbitalPath> ops = new List<OrbitalPath>();
		List<SelfRotation> srs = new List<SelfRotation>();
		Quaternion obj2cmr;

		void Start () {
			sp = shipsight.GetComponent<ShipSightPath>();
			sr = shipsight.GetComponent<ShipSightRotate>();
			foreach(Transform t in planets)
			{
				if(t.GetComponent<OrbitalPath>() != null)
					ops.Add(t.GetComponent<OrbitalPath>());
				if(t.GetComponent<SelfRotation>() != null)
					srs.Add(t.GetComponent<SelfRotation>());
			}
		}
		void FixedUpdate()
		{
			if(test)
			{
				TestUniverse();
				return;
			}
			dt = Time.deltaTime;
			obj2cmr = Quaternion.Inverse(sr.m_rotation * sightpoint.rotation);
			foreach(OrbitalPath v in ops)
				v.transform.position = obj2cmr * (v.UpdatePosition(dt) - sp.m_position) + sightpoint.position;
			foreach(SelfRotation v in srs)
			{
				Quaternion m_rotateX = v.m_rotationX;	//origin planet rotateX
				Quaternion m_rotateY = v.UpdateRotateY(dt);	//origin planet rotateY
				v.transform.rotation = obj2cmr * (m_rotateX * m_rotateY);	//actual planet rotate
				CloudEmitter ce = v.GetComponent<CloudEmitter>();
				if(ce)
				{
					rotateY = ce.transform.GetComponent<SelfRotation>().s_angle;
					//rotateX = ce.transform.GetComponent<SelfRotation>().rotationAngle;
					ssrotate = sr.rotateY;
					ce.relativeRotY = ssrotate - rotateY;
					List<CloudMotion> delList = new List<CloudMotion>();
					foreach(CloudMotion cm in ce.cms)
					{
						cm.transform.localRotation = cm.UpdateRotate(dt, rotateY);//, rotateX);	//actual cloud rotate
						cm.transform.GetComponent<MeshRenderer>().material.SetVector("_SunDirect", sun.forward);
						if(cm.OutOfSight(ssrotate))
						{
							delList.Add(cm);
						}
					}
					foreach(CloudMotion del in delList)
					{
						ce.cms.Remove(del);
						GameObject.Destroy(del.gameObject);
					}
					delList.Clear();
				}
			}
		}
		void TestUniverse() 
		{
			dt = Time.deltaTime;
			obj2cmr = Quaternion.Inverse(sr.m_rotation * sightpoint.rotation);
			foreach(OrbitalPath v in ops)
			{
				v.UpdatePosition(dt);
				v.TestUpdate();
			}				
			foreach(SelfRotation v in srs)
			{
//				Quaternion m_rotateX = v.m_rotationX;	//origin planet rotateX
//				Quaternion m_rotateY = v.UpdateRotateY(dt);	//origin planet rotateY
				v.TestUpdate();
				CloudEmitter ce = v.GetComponent<CloudEmitter>();
				if(ce)
				{
					rotateY = ce.transform.GetComponent<SelfRotation>().s_angle;
					//rotateX = ce.transform.GetComponent<SelfRotation>().rotationAngle;
					ssrotate = sr.rotateY;
					ce.relativeRotY = ssrotate - rotateY;
					foreach(CloudMotion cm in ce.cms)
					{
						cm.UpdateRotate(dt, rotateY);
						cm.TestUpdate();
						if(cm.OutOfSight(ssrotate))
						{
							ce.cms.Remove(cm);
							GameObject.Destroy(cm.gameObject);
						}
					}
				}
			}
		}
	}

}
