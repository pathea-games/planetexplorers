using UnityEngine;
using System.Collections.Generic;
using Pathea;
using WhiteCat;

public class CreationDamageController : MonoBehaviour
{
	VCESceneSetting m_SceneSetting = null;
	BehaviourController m_Controller = null;

	List<VCParticlePlayer> m_DamageParticlePlayers = null;
	List<VCParticlePlayer> m_ExplodeParticlePlayers = null;
	List<Rigidbody> m_ThrowoutList = null;

	float m_DestructTick;
	bool m_IsWreckage;
	bool m_Sinked;


	public virtual void Init(BehaviourController controller)
	{
		m_Controller = controller;
		m_SceneSetting = controller.creationController.creationData.m_IsoData.m_HeadInfo.FindSceneSetting();

		m_DamageParticlePlayers = new List<VCParticlePlayer>();
		m_ExplodeParticlePlayers = new List<VCParticlePlayer>();

		VCParticlePlayer[] pps = GetComponentsInChildren<VCParticlePlayer>(false);

		foreach ( VCParticlePlayer pp in pps )
		{
			if ( pp.FunctionTag == VCParticlePlayer.ftDamaged )
				m_DamageParticlePlayers.Add(pp);
			else if ( pp.FunctionTag == VCParticlePlayer.ftExplode )
				m_ExplodeParticlePlayers.Add(pp);
		}

		m_DestructTick = 120.0f;
		m_IsWreckage = false;
		m_Sinked = false;
	}


	protected virtual void Update()
	{
		// Particle
		UpdateDamageParticles();
		
		// Destruct Tick
		UpdateDestructTick();
	}


	private void UpdateDamageParticles()
	{
		if (!m_IsWreckage && m_Controller.isDead)
		{
			Explode();
			m_IsWreckage = true;
		}

		VCParticlePlayer pp;
		for (int i=0; i < m_DamageParticlePlayers.Count; i++)
		{
			pp = m_DamageParticlePlayers[i];
			if (pp)
			{
				if (m_Controller.isDead)
				{
					if (UnityEngine.Random.value < (pp.ReferenceValue - 0.3f) * 0.05f)
						pp.Effect = VCParticleMgr.GetEffect("Wreckage Spurt", m_SceneSetting);
				}
				else if (m_Controller.hpPercentage < pp.ReferenceValue * .25F)
				{
					pp.Effect = VCParticleMgr.GetEffect("Fire", m_SceneSetting);
				}
				else if (m_Controller.hpPercentage < pp.ReferenceValue * .5F)
				{
					pp.Effect = VCParticleMgr.GetEffect("Smoke", m_SceneSetting);
				}
				else
				{
					pp.Effect = null;
				}
			}
		}
	}


	private void UpdateDestructTick()
	{
		// Destruct creation if dead
		if (m_IsWreckage)
			m_DestructTick -= Time.deltaTime;
		else
			m_DestructTick = 120.0f;
		if (m_DestructTick < 30.0f)
		{
			GetComponent<Rigidbody>().isKinematic = false;
		}
		if (m_DestructTick < 0)
		{
			m_Controller.Destroy();
		}
	}


	protected virtual void FixedUpdate()
	{
		// Update Explosion Physics
		if (m_ThrowoutList != null)
		{
			// A. Throw out rigidbodies
			if (m_ThrowoutList.Count > 0)
			{
				ThrowOutRigidbodies();
				m_ThrowoutList.Clear();
			}

			// B. Destruct sink
			// Dead, wreckage, and last 30 sec about to destruct
			if (m_DestructTick < 30.0f && !m_Sinked)
			{
				SinkWreckage();
				m_Sinked = true;
			}
		}
	}



	// Throw out rigidbodies - For explosion
	private void ThrowOutRigidbodies()
	{
		Vector3 explode_center = transform.position;
		if ( m_ThrowoutList.Count > 0 )
		{
			float cnt = 0;
			explode_center = Vector3.zero;
			Vector3 refer = transform.position;
			if ( m_ThrowoutList[0] != null )
				refer = m_ThrowoutList[0].transform.position;
			foreach ( Rigidbody r in m_ThrowoutList )
			{
				if ( r != null )
				{
					explode_center += (r.transform.position - refer);
					cnt += 1.0f;
				}
			}
			explode_center /= cnt;
			explode_center += refer;
		}
		foreach ( Rigidbody r in m_ThrowoutList )
		{
			if ( r != null )
			{
				Vector3 throw_direction = ((r.transform.position - explode_center).normalized * UnityEngine.Random.value + UnityEngine.Random.insideUnitSphere * .3F);
				float throw_strength = Mathf.Pow(m_SceneSetting.EditorWorldSize.sqrMagnitude, 0.2f) * 5f;
		
				r.velocity = throw_direction * throw_strength;
			}
		}
	}


	// Destruct sink
	// Wreckage, and last 30 sec about to destruct
	private void SinkWreckage()
	{
		// Alter rigidbodies' physics properties
		Rigidbody [] rbs = GetComponentsInChildren<Rigidbody>(true);
		foreach ( Rigidbody rb in rbs )
		{
			if ( rb != GetComponent<Rigidbody>() )
			{
				// Turn off normal physics
				rb.detectCollisions = false;
				rb.useGravity = false;
				rb.angularVelocity = Vector3.zero;

				float sink_speed = m_SceneSetting.m_VoxelSize * 3f;
				rb.velocity = Vector3.down * sink_speed;
			}
		}
		// Destroy Colliders
		Collider [] cs = GetComponentsInChildren<Collider>(true);
		foreach ( Collider c in cs )
		{
			Component.Destroy(c);
		}
	}


	public void Explode()
	{
		// 0 删除粒子效果
		var particles = m_Controller.creationController.partRoot.GetComponentsInChildren<ParticleSystem>();
		if (particles != null)
		{
			for (int i=0; i<particles.Length; i++)
			{
				Destroy(particles[i]);
			}
		}

		// 1. Play explode particles
		foreach (VCParticlePlayer pp in m_ExplodeParticlePlayers)
			pp.Effect = VCParticleMgr.GetEffect("Explode", m_SceneSetting);

		// 2. Add all MeshRenderers and Templates
		List<GameObject> explode_gos = new List<GameObject>();
		List<GameObject> part_gos = new List<GameObject>();
		MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>(false);
		foreach (MeshRenderer mr in mrs)
		{
			if (!explode_gos.Contains(mr.gameObject))
				explode_gos.Add(mr.gameObject);
		}
		VCPart[] pfs = GetComponentsInChildren<VCPart>(true);
		foreach (VCPart pf in pfs)
		{
			if (!part_gos.Contains(pf.gameObject))
				part_gos.Add(pf.gameObject);
		}
		SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>(false);
		foreach (SkinnedMeshRenderer smr in smrs)
		{
			GameObject.Destroy(smr);
		}

		// 
		MonoBehaviour[] mbs = GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour mb in mbs)
		{
			if (mb is CreationDamageController)
			{
				continue;
			}
			if (mb is CreationController)
			{
				mb.enabled = false;
				continue;
			}
			if (mb is PeEntity)
			{
				continue;
			}
			if (mb is BehaviourController)
			{
				mb.enabled = false;
				(mb as BehaviourController).audioSource.PlayOneShot(
					PEVCConfig.instance.explotionSound,
					PEVCConfig.instance.explotionVolume * SystemSettingData.Instance.AbsEffectVolume);
				continue;
			}

			mb.enabled = false;
			Destroy(mb);
		}

		// 3. Destroy all living-functions on these GameObjects and main GameObject
		for (int t = 0; t < 3; ++t)
		{
			foreach (GameObject go in part_gos)
			{
				mbs = go.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour mb in mbs)
				{
					if (mb is VCParticlePlayer)
						continue;
					MonoBehaviour.Destroy(mb);
				}
			}
		}

		Light[] lts = GetComponentsInChildren<Light>();
		foreach (Light lt in lts)
			Destroy(lt);
		WhiteCat.ParticlePlayer[] pps = GetComponentsInChildren<WhiteCat.ParticlePlayer>();
		foreach (var pp in pps)
			Destroy(pp);
		WheelCollider[] wcs = GetComponentsInChildren<WheelCollider>(true);
		foreach (WheelCollider wc in wcs)
			Destroy(wc);

		AudioSource[] sounds = GetComponentsInChildren<AudioSource>(true);
		var controller = GetComponent<BehaviourController>();
		AudioSource carrierSource = null;
		if (controller) carrierSource = controller.audioSource;
		foreach (AudioSource sound in sounds)
		{
			if (sound != carrierSource)
			{
				sound.enabled = false;
			}
		}

		CreationWaterMask[] wmasks = GetComponentsInChildren<CreationWaterMask>(true);
		foreach (CreationWaterMask wmask in wmasks)
			GameObject.Destroy(wmask.m_WaterMask);

		// 4. Disable the top Rigidbody
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().detectCollisions = false;
		}
		else
		{
			gameObject.AddComponent<Rigidbody>();
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().detectCollisions = false;
		}

		// 5. Modify name
		//name = "Creation [" + m_CreationData.m_ObjectID.ToString() + "]\'s Wreckage";

		// 6. Add Rigidbody & Collider for each GameObject
		foreach (GameObject go in explode_gos)
		{
			if (go.GetComponent<Collider>() == null)
				go.AddComponent<BoxCollider>();
			else
				go.GetComponent<Collider>().enabled = true;
			if (go.GetComponent<Collider>() is MeshCollider)
				((MeshCollider)go.GetComponent<Collider>()).convex = true;
			if (go.GetComponent<Rigidbody>() == null)
			{
				if (UnityEngine.Random.value < .7F)
					go.AddComponent<Rigidbody>();
			}
			if (go.GetComponent<Rigidbody>() != null)
			{
				go.GetComponent<Rigidbody>().mass = 10f;
			}
		}

		// 7. Throw them out
		m_ThrowoutList = new List<Rigidbody>();
		foreach (GameObject go in explode_gos)
		{
			if (go.GetComponent<Rigidbody>() != null)
			{
				go.GetComponent<Rigidbody>().isKinematic = false;
				m_ThrowoutList.Add(go.GetComponent<Rigidbody>());
			}
			else
			{
				GameObject.Destroy(go);
			}
		}
	}
}
