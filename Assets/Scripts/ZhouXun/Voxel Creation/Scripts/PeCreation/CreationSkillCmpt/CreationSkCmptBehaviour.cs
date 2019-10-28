//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Pathea;
//using SkillSystem;

//public abstract partial class CreationSkCmpt : PeCmpt
//{
//	protected virtual void Update ()
//	{
////		// Check
////		if ( ObjectID == 0 ) return;
////		
////		// Particle
////		UpdateDamageParticles();
////		
////		// Destruct Tick
////		UpdateDestructTick();
////		
////		// Debug
////		if ( Application.isEditor )
////		{
////			m_DebugStr = "HP: " + HP.ToString("0") + " / " + MaxHP.ToString("0") + "    " +
////				"Fuel: " + m_CreationData.m_Fuel.ToString("0") + " / " +
////					m_CreationData.m_Attribute.m_MaxFuel.ToString("0");
////		}
//	}
	
//	// Particle
//	private List<VCParticlePlayer> m_DamageParticlePlayers = null;
//	private List<VCParticlePlayer> m_ExplodeParticlePlayers = null;
//	private void UpdateDamageParticles ()
//	{
////		foreach ( VCParticlePlayer pp in m_DamageParticlePlayers )
////		{
////			if ( HP <= 0 )
////			{
////				if ( UnityEngine.Random.value < (pp.ReferenceValue - 0.3f) * 0.05f )
////					pp.Effect = VCParticleMgr.GetEffect("Wreckage Spurt", m_SceneSetting);
////			}
////			else if ( HP < pp.ReferenceValue * MaxHP * .25F )
////			{
////				pp.Effect = VCParticleMgr.GetEffect("Fire", m_SceneSetting);
////			}
////			else if ( HP < pp.ReferenceValue * MaxHP * .5F )
////			{
////				pp.Effect = VCParticleMgr.GetEffect("Smoke", m_SceneSetting);
////			}
////			else
////			{
////				pp.Effect = null;
////			}
////		}
//	}
	
//	//
//	// Explode
//	//
	
//	private List<Rigidbody> m_ThrowoutList = null;
//	private float m_DestructTick = 120.0f;
//	private bool m_Sinked = false;
//	private void UpdateDestructTick ()
//	{
////		// Destruct creation if dead
////		if ( HP < 0 )
////			m_DestructTick -= Time.deltaTime;
////		else
////			m_DestructTick = 120.0f;
////		if ( m_DestructTick < 30.0f )
////		{
////			GetComponent<Rigidbody>().isKinematic = false;
////		}
////		if ( m_DestructTick < 0 )
////		{
////			GameObject.Destroy(this.gameObject);
////		}		
//	}
//	protected virtual void FixedUpdate ()
//	{
////		// Update Explosion Physics
////		if ( m_ThrowoutList != null )
////		{
////			// A. Throw out rigidbodies
////			if ( m_ThrowoutList.Count > 0 )
////			{
////				ThrowOutRigidbodies();
////				m_ThrowoutList.Clear();
////			}
////			
////			// B. Destruct sink
////			// Dead, wreckage, and last 30 sec about to destruct
////			if ( m_DestructTick < 30.0f && !m_Sinked )
////			{
////				SinkWreckage();
////				m_Sinked = true;
////			}
////		}
//	}
//	// Throw out rigidbodies - For explosion
//	private void ThrowOutRigidbodies()
//	{
//		Vector3 explode_center = transform.position;
//		if ( m_ThrowoutList.Count > 0 )
//		{
//			float cnt = 0;
//			explode_center = Vector3.zero;
//			Vector3 refer = transform.position;
//			if ( m_ThrowoutList[0] != null )
//				refer = m_ThrowoutList[0].transform.position;
//			foreach ( Rigidbody r in m_ThrowoutList )
//			{
//				if ( r != null )
//				{
//					explode_center += (r.transform.position - refer);
//					cnt += 1.0f;
//				}
//			}
//			explode_center /= cnt;
//			explode_center += refer;
//		}
//		foreach ( Rigidbody r in m_ThrowoutList )
//		{
//			if ( r != null )
//			{
//				Vector3 throw_direction = ((r.transform.position - explode_center).normalized * Random.value + Random.insideUnitSphere * .3F);
//				float throw_strength = Mathf.Pow(m_SceneSetting.EditorWorldSize.sqrMagnitude, 0.2f) * 5f;
				
//				r.velocity = throw_direction * throw_strength;
//			}
//		}
//	}
	
//	// Destruct sink
//	// Wreckage, and last 30 sec about to destruct
//	private void SinkWreckage()
//	{
//		// Alter rigidbodies' physics properties
//		Rigidbody [] rbs = GetComponentsInChildren<Rigidbody>(true);
//		foreach ( Rigidbody rb in rbs )
//		{
//			if ( rb != GetComponent<Rigidbody>() )
//			{
//				// Turn off normal physics
//				rb.detectCollisions = false;
//				rb.useGravity = false;
//				rb.angularVelocity = Vector3.zero;
				
//				float sink_speed = SceneSetting.m_VoxelSize * 3f;
//				rb.velocity = Vector3.down * sink_speed;
//			}
//		}
//		// Destroy Colliders
//		Collider [] cs = GetComponentsInChildren<Collider>(true);
//		foreach ( Collider c in cs )
//		{
//			Component.Destroy(c);
//		}
//	}
//	private void Explode ()
//	{
//		// 1. Play explode particles
//		foreach ( VCParticlePlayer pp in m_ExplodeParticlePlayers )
//			pp.Effect = VCParticleMgr.GetEffect("Explode", m_SceneSetting);
		
//		// 2. Add all MeshRenderers and Templates
//		List<GameObject> explode_gos = new List<GameObject> ();
//		List<GameObject> part_gos = new List<GameObject> ();
//		MeshRenderer [] mrs = GetComponentsInChildren<MeshRenderer>(false);
//		foreach ( MeshRenderer mr in mrs )
//		{
//			if ( !explode_gos.Contains(mr.gameObject) )
//				explode_gos.Add(mr.gameObject);
//		}
//		VCPartFunc [] pfs = GetComponentsInChildren<VCPartFunc>(true);
//		foreach ( VCPartFunc pf in pfs )
//		{
//			if ( !part_gos.Contains(pf.gameObject) )
//				part_gos.Add(pf.gameObject);
//		}
//		SkinnedMeshRenderer [] smrs = GetComponentsInChildren<SkinnedMeshRenderer>(false);
//		foreach ( SkinnedMeshRenderer smr in smrs )
//		{
//			GameObject.Destroy(smr);
//		}
		
//		// 3. Destroy all living-functions on these GameObjects and main GameObject
//		for ( int t = 0; t < 3; ++t )
//		{
//			foreach ( GameObject go in part_gos )
//			{
//				MonoBehaviour [] mbs = go.GetComponents<MonoBehaviour>();
//				foreach ( MonoBehaviour mb in mbs )
//				{
//					if ( mb is VCParticlePlayer )
//						continue;
//					MonoBehaviour.Destroy(mb);
//				}
//			}
//			MonoBehaviour [] mmbs = GetComponents<MonoBehaviour>();
//			foreach ( MonoBehaviour mmb in mmbs )
//			{
//				if ( mmb is CreationSkillRunner )
//				{
//					continue;
//				}
//				if ( mmb is CreationController )
//				{
//					(mmb as CreationController).m_Active = false;
//					mmb.enabled = false;
//					continue;
//				}
//				if ( mmb is CreationBound )
//				{
//					mmb.enabled = false;
//					continue;
//				}
//				//if ( mmb is DistanceTrigger )
//				//{
//				//    mmb.enabled = false;
//				//    continue;
//				//}
//				MonoBehaviour.Destroy(mmb);
//			}
//		}
//		Light[] lts = GetComponentsInChildren<Light>();
//		foreach ( Light lt in lts )
//			Light.Destroy(lt);
//		WheelCollider[] wcs = GetComponentsInChildren<WheelCollider>();
//		foreach ( WheelCollider wc in wcs )
//			WheelCollider.Destroy(wc);
//		AudioSource[] sounds = GetComponentsInChildren<AudioSource>();
//		foreach ( AudioSource sound in sounds )
//			AudioSource.Destroy(sound);
//		CreationWaterMask[] wmasks = GetComponentsInChildren<CreationWaterMask>();
//		foreach ( CreationWaterMask wmask in wmasks )
//			GameObject.Destroy(wmask.m_WaterMask);
		
//		// 4. Disable the top Rigidbody
//		if ( GetComponent<Rigidbody>() != null )
//		{
//			GetComponent<Rigidbody>().isKinematic = true;
//			GetComponent<Rigidbody>().useGravity = false;
//			GetComponent<Rigidbody>().detectCollisions = false;
//		}
//		else
//		{
//			gameObject.AddComponent<Rigidbody>();
//			GetComponent<Rigidbody>().isKinematic = true;
//			GetComponent<Rigidbody>().useGravity = false;
//			GetComponent<Rigidbody>().detectCollisions = false;
//		}
		
//		// 5. Modify name
//		name = "Creation [" + ObjectID.ToString() + "]\'s Wreckage";
		
//		// 6. Add Rigidbody & Collider for each GameObject
//		float mass_total = m_CreationData.m_Attribute.m_Weight;
//		foreach ( GameObject go in explode_gos )
//		{
//			if ( go.GetComponent<Collider>() == null )
//				go.AddComponent<BoxCollider>();
//			else
//				go.GetComponent<Collider>().enabled = true;
//			if ( go.GetComponent<Collider>() is MeshCollider )
//				((MeshCollider)go.GetComponent<Collider>()).convex = true;
//			if ( go.GetComponent<Rigidbody>() == null )
//			{
//				if ( Random.value < .7F )
//					go.AddComponent<Rigidbody>();
//			}
//			if ( go.GetComponent<Rigidbody>() != null )
//			{
//				go.GetComponent<Rigidbody>().mass = mass_total / (float)(explode_gos.Count) * .7f;
//			}
//		}
		
//		// 7. Throw them out
//		m_ThrowoutList = new List<Rigidbody> ();
//		foreach ( GameObject go in explode_gos )
//		{
//			if ( go.GetComponent<Rigidbody>() != null )
//			{
//				go.GetComponent<Rigidbody>().isKinematic = false;
//				m_ThrowoutList.Add(go.GetComponent<Rigidbody>());
//			}
//			else
//			{
//				GameObject.Destroy(go);
//			}
//		}
//	}
//}