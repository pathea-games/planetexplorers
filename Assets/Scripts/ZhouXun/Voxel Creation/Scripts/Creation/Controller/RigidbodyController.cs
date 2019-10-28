//#define PLANET_EXPLORERS
//using UnityEngine;
//using System.Collections;

//namespace OLD
//{
//	public abstract class RigidbodyController : CreationController
//	{
//		public Rigidbody m_Rigidbody;
//		public float m_Gravity = 9.806f;
//		public string m_GroundLayerName = "";

//		public override void Init(CreationData crd, int itemInstanceId)
//		{
//			m_CreationData = crd;
//			_creationID = crd.m_ObjectID;
//			m_Rigidbody = gameObject.AddComponent<Rigidbody>();
//			m_Rigidbody.mass = m_CreationData.m_Attribute.m_Weight;
//			m_Rigidbody.centerOfMass = new Vector3(0, m_CreationData.m_Attribute.m_CenterOfMass.y, 0);
//			m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
//			m_Rigidbody.interpolation = RigidbodyInterpolation.None;
//			m_Rigidbody.sleepThreshold = 0.01f;
//			m_Rigidbody.useGravity = false;
//			m_Rigidbody.isKinematic = true;
//#if PLANET_EXPLORERS
//			m_GroundLayerName = "VFVoxelTerrain";
//#endif
//		}
//		protected override void Update()
//		{
//			if (m_Rigidbody == null) return;
//			m_Rigidbody.isKinematic = !m_Active;
//			//m_Rigidbody.detectCollisions = ( m_NetChar == ENetCharacter.nrOwner );
//		}
//		protected override void FixedUpdate()
//		{
//			if (m_Rigidbody == null) return;
//			if (m_Active)
//			{
//				if (m_NetChar == ENetCharacter.nrOwner)
//				{
//					m_Rigidbody.AddForce(new Vector3(0, -m_Gravity * m_Rigidbody.mass, 0));
//				}
//			}
//		}
//		public virtual bool SafePosition(float max_ray_distance, out Vector3 new_pos)
//		{
//			Vector3 old_pos = transform.position;
//			new_pos = old_pos;
//			CreationBound cb = GetComponent<CreationBound>();
//			if (cb == null)
//				return false;

//			Bounds bound = cb.m_Bound;
//			Vector3[] vert = new Vector3[8];
//			for (int i = 0; i < 8; i++)
//			{
//				vert[i] = bound.center;
//				if ((i & 1) == 0)
//					vert[i] -= bound.extents.x * new Vector3(1, 0, 0);
//				else
//					vert[i] += bound.extents.x * new Vector3(1, 0, 0);
//				if ((i & 2) == 0)
//					vert[i] -= bound.extents.y * new Vector3(0, 1, 0);
//				else
//					vert[i] += bound.extents.y * new Vector3(0, 1, 0);
//				if ((i & 4) == 0)
//					vert[i] -= bound.extents.z * new Vector3(0, 0, 1);
//				else
//					vert[i] += bound.extents.z * new Vector3(0, 0, 1);
//				vert[i] = transform.TransformPoint(vert[i]);
//			}
//			float max_rise = 0.0f;
//			for (int i = 0; i < 8; i++)
//			{
//				Ray ray1 = new Ray(vert[i], Vector3.up);
//				Ray ray2 = new Ray(vert[i] + Vector3.up * max_ray_distance, Vector3.down);
//				RaycastHit rch;

//				if (Physics.Raycast(ray1, out rch, max_ray_distance,
//					 1 << LayerMask.NameToLayer(m_GroundLayerName)))
//				{
//					Vector3 rise_vec = rch.point - vert[i];
//					if (max_rise < rise_vec.magnitude)
//					{
//						max_rise = rise_vec.magnitude;
//					}
//				}
//				if (Physics.Raycast(ray2, out rch, max_ray_distance,
//					 1 << LayerMask.NameToLayer(m_GroundLayerName)))
//				{
//					Vector3 rise_vec = rch.point - vert[i];
//					if (max_rise < rise_vec.magnitude)
//					{
//						max_rise = rise_vec.magnitude;
//					}
//				}
//			}
//			if (max_rise > 0.00001f)
//			{
//				new_pos = old_pos + Vector3.up * (max_rise);
//				return true;
//			}
//			else
//			{
//				new_pos = old_pos;
//				return false;
//			}
//		}
//	}

//}