using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EpsilonIndi
{
	public class CloudEmitter : MonoBehaviour 
	{
		[SerializeField] GameObject cloudFrag;
		[SerializeField] Material[] cloudSample;
		[SerializeField] float maxSpeed = 2f;
		[SerializeField] float minSpeed = -2f;
		[SerializeField] float maxAngle = 180f;
		[SerializeField] float minAngle = 0f;
		[SerializeField] float cdTime = 0.75f;
		[HideInInspector] public List<CloudMotion> cms = new List<CloudMotion>();
		[HideInInspector] public float relativeRotY;
		float ctime;
		static float cloud1prob = 0.005f;
		static float cloud2prob = 0.005f;
		
		void Start()
		{
			int startCloudMount = (int)(360 / GetComponent<SelfRotation>().selfSpeed / cdTime);
			for(int i = 0; i < startCloudMount; i++)
				CreateACloud(360f * i / startCloudMount, -2f, 2f);
		}
		
		void Update()
		{	
			ctime += Time.deltaTime;
			if(ctime > cdTime)
			{
				ctime -= cdTime;
				CreateACloud(relativeRotY, -2f, 2f);
			}
		}
		float GetRandom1(float mi, float ma)
		{
			float x = Random.value;
			x *= (x * x - 1.5f * x + 1.5f);
			return Mathf.Lerp(mi, ma, x);
		}
		
		int GetRandomID(int cloudNumMax)
		{
			float randomSeed = Random.value;
			if(randomSeed < cloud1prob)
				return 0;
			else if(randomSeed < cloud2prob)
				return 1;
			else			
				return Random.Range(2, cloudNumMax);			
		}
		
		void CreateACloud(float correctRotY, float minAngleOffset, float maxAngleOffset)
		{
			Transform fragUnit = (GameObject.Instantiate(cloudFrag) as GameObject).transform;
			fragUnit.parent = transform;
			fragUnit.localPosition = Vector3.zero;
			fragUnit.localRotation = Quaternion.identity;
			fragUnit.localScale = Vector3.one * Random.Range(0.0502f, 0.0505f);
			int cloudID = GetRandomID(cloudSample.Length - 1);
			fragUnit.GetComponent<MeshRenderer>().material = cloudSample[cloudID];
			CloudMotion cm = fragUnit.GetComponent<CloudMotion>();
			cms.Add(cm);
			if(cloudID <= 1)
				cm.InitProp(GetRandom1(minSpeed, maxSpeed), GetRandom1(minAngle, maxAngle), correctRotY + Random.Range(minAngleOffset, maxAngleOffset), cloudID - 4f);
			else
				cm.InitProp(GetRandom1(minSpeed, maxSpeed), GetRandom1(minAngle, maxAngle), correctRotY + Random.Range(minAngleOffset, maxAngleOffset), 0f);
		}
	}
}
	