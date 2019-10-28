using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;

namespace Pathea
{
    public class ImpactAtkTriggrer : MonoBehaviour
    {
        PeEntity m_Entity;
        Rigidbody m_RigidBody;

        void Awake()
        {
            m_Entity = GetComponentInParent<PeEntity>();
            m_RigidBody = GetComponent<Rigidbody>();
        }

		public static float MinCmpt(Vector3 v)
		{
			float min = v.x;
			if (v.y < min) min = v.y;
			if (v.z < min) min = v.z;
			return min;
		}


		void OnCollisionEnter(Collision colInfo)
        {
            GameObject go = colInfo.collider.gameObject;
			if (go.layer == Layer.TreeStatic)
            {
                // Proceed one each time
                /*
                ContactPoint point = colInfo.contacts[0];
                Vector3 vPt = point.point;
                int x = Mathf.CeilToInt(vPt.x);
                int y = Mathf.CeilToInt(vPt.y);
                int z = Mathf.CeilToInt(vPt.z);
                for(int dx = 0; dx < 2; dx++) {
                    for(int dy = 0; dx < 2; dx++) {
                        for(int dx = 0; dx < 2; dx++) {
                        }
                    }
                }
                */
            }
			else if (go.layer == Layer.NearTreePhysics)
            {
				if (Pathea.PeGameMgr.IsStory || Pathea.PeGameMgr.IsAdventure)
				{
             		PeEntity entity = GetComponentInParent<PeEntity>();

					if (Pathea.PeGameMgr.IsStory)
					{
						if (entity != null)
						{
							if (entity.Field == MovementField.Sky && entity.commonCmpt.entityProto.proto == EEntityProto.Monster)
							{
								if (entity.maxRadius < MinCmpt(colInfo.collider.bounds.extents) * 2f)
									return;
							}
							
							else if (entity.carrier){
								if (colInfo.relativeVelocity.magnitude * entity.carrier.rigidbody.mass
								    < MinCmpt(colInfo.collider.bounds.extents) * PEVCConfig.instance.treeHardness)
									return;
							}

							GlobalTreeInfo tree = LSubTerrainMgr.TryGetTreeInfo(go);
							if (tree != null)
							{
								LSubTerrainMgr.DeleteTree(go);
								LSubTerrainMgr.RefreshAllLayerTerrains();
								SkEntitySubTerrain.Instance.SetTreeHp(tree.WorldPos, 0);
								StroyManager.Instance.TreeCutDown(entity.position, tree._treeInfo, tree.WorldPos);
							}
						}
					}
					else // Pathea.PeGameMgr.IsAdventure
					{
						if (entity != null)
						{
							if (entity.Field == MovementField.Sky && entity.commonCmpt.entityProto.proto == EEntityProto.Monster)
							{
								if (entity.maxRadius < MinCmpt(colInfo.collider.bounds.extents) * 2f)
									return;
							}

							else if (entity.carrier){
								if (colInfo.relativeVelocity.magnitude * entity.carrier.rigidbody.mass
								    < MinCmpt(colInfo.collider.bounds.extents) * PEVCConfig.instance.treeHardness)
									return;
							}

							TreeInfo tree = RSubTerrainMgr.TryGetTreeInfo(go);
							if (tree != null)
							{
								RSubTerrainMgr.DeleteTree(go);
								RSubTerrainMgr.RefreshAllLayerTerrains();
								SkEntitySubTerrain.Instance.SetTreeHp(tree.m_pos, 0);
								StroyManager.Instance.TreeCutDown(entity.position, tree, tree.m_pos);
							}
						}
					}
				}
            }
            //50吨以15米/秒速度撞击的伤害缩放为1 50 * 15 * 15 = 11250
            else if(go.layer == Layer.AIPlayer || go.layer == Layer.Player)
            {
                PeEntity target = go.GetComponentInParent<PeEntity>();
                if(target != null && m_Entity != null && m_Entity.carrier != null && colInfo.relativeVelocity.sqrMagnitude > 2.0f*2.0f)
                {
                    NetCmpt net = m_Entity.GetComponent<NetCmpt>();
                    if (net == null || net.IsController)
                    {
                        float scale = 1.0f / 11250.0f;
                        float mass = m_Entity.carrier.creationController.creationData.m_Attribute.m_Weight * 0.001f;
                        float speed = colInfo.relativeVelocity.sqrMagnitude;

                        float damageScale = Mathf.Clamp(mass * speed * scale, 0.0f, 2.0f);
                        //Debug.LogError("Mass:" + mass + " Speed:" + speed + " Scale:" + damageScale);
                        if (m_RigidBody == null || m_RigidBody.velocity.sqrMagnitude < 5.0f*5.0f || damageScale < 0.01)
                            damageScale = 0.0f;
                        else
                        {
                            PECapsuleHitResult hitResult = new PECapsuleHitResult();
                            hitResult.selfTrans = transform;
                            hitResult.hitTrans = go.transform;
                            hitResult.hitPos = colInfo.contacts[0].point;
                            hitResult.hitDir = -colInfo.contacts[0].normal;
                            hitResult.damageScale = damageScale;

                            m_Entity.skEntity.CollisionCheck(hitResult);

                            //if(go.layer == Layer.Player && target.NpcCmpt != null && speed > 10.0f*10.0f)
                            //{
                            //    if(target.biologyViewCmpt != null)
                            //    {
                            //        RagdollHitInfo hitInfo = new RagdollHitInfo();
                            //        hitInfo.hitTransform = target.biologyViewCmpt.monoRagdollCtrlr.ragdollRootBone;
                            //        hitInfo.hitPoint = colInfo.contacts[0].point;
                            //        hitInfo.hitNormal = colInfo.contacts[0].normal;
                            //        hitInfo.hitForce = colInfo.impulse;
                            //        target.biologyViewCmpt.ActivateRagdoll(hitInfo);
                            //    }
                            //}
                        }
                    }
                }
            }

            //50吨以15米/秒速度撞击的伤害缩放为1 50 * 15 * 15 = 11250
            if (   go.layer == Layer.VFVoxelTerrain 
                || go.layer == Layer.TreeStatic 
                //|| go.layer == Layer.NearTreePhysics
                || go.layer == Layer.Building 
                || go.layer == Layer.SceneStatic 
                || go.layer == Layer.Unwalkable
                || go.layer == Layer.GIEProductLayer)
            {
                bool isWheel = false;
                if (go.layer == Layer.VFVoxelTerrain)
                {
                    for (int i = 0; i < colInfo.contacts.Length; i++)
                    {
                        if (colInfo.contacts[i].thisCollider.gameObject.GetComponentInParent<VCPVehicleWheel>() != null)
                            isWheel = true;
                    }
                }

                if (!isWheel && m_Entity != null && m_Entity.carrier != null && colInfo.relativeVelocity.sqrMagnitude > 2.0f * 2.0f)
                {
                    NetCmpt net = m_Entity.GetComponent<NetCmpt>();
                    if (net == null || net.IsController)
                    {
                        float scale = 1.0f / 11250.0f;
                        float mass = m_Entity.carrier.creationController.creationData.m_Attribute.m_Weight * 0.001f;
                        float speed = colInfo.relativeVelocity.sqrMagnitude;

                        float damageScale = Mathf.Clamp(mass * speed * scale, 0.0f, 2.0f);
                        Vector3 v = m_RigidBody.velocity;
                        float curSpeed = Mathf.Sqrt(v.x*v.x + v.z*v.z) * 3.6f;
                        //Debug.LogError("Mass:" + mass + " Speed:" + speed + " Scale:" + damageScale);
                        if (curSpeed < 45f || damageScale < 0.01)
                            damageScale = 0.0f;
                        else
                        {
                            SkillSystem.SkEntity.MountBuff(m_Entity.skEntity, 30200174, new List<int> { 0 }, new List<float>() { damageScale });

                            //PECapsuleHitResult hitResult = new PECapsuleHitResult();
                            //hitResult.selfTrans = transform;
                            //hitResult.hitTrans = go.transform;
                            //hitResult.hitPos = colInfo.contacts[0].point;
                            //hitResult.hitDir = -colInfo.contacts[0].normal;
                            //hitResult.damageScale = damageScale;

                            //m_Entity.skEntity.CollisionCheck(hitResult);

                            //Debug.LogError(go.name + " --> " + colInfo.relativeVelocity.magnitude);
                        }
                    }
                }
            }
        }
    }
}

