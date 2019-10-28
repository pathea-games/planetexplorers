using UnityEngine;
using System.Collections;

public class VCParticlePlayer : MonoBehaviour
{
	// Function Tags ------------------
	public const int ftDamaged = 1;
	public const int ftExplode = 2;
	// --------------------------------
	
	public int FunctionTag = 0;
	public float ReferenceValue = 0;
	
	public Vector3 LocalPosition = Vector3.zero;
	[SerializeField] private GameObject PlayingObject = null;
	private GameObject ResourceObject = null;
	
	public GameObject Effect 
	{
		get
		{
			return PlayingObject;
		}
		set
		{
			if ( value == ResourceObject )
				return;
			ResourceObject = value;
			if ( PlayingObject != null )
				GameObject.Destroy(PlayingObject);
			if ( value == null )
			{
				PlayingObject = null;
			}
			else
			{
				PlayingObject = Instantiate(value) as GameObject;
				if (PlayingObject)
				{
					PlayingObject.transform.parent = transform;
					PlayingObject.transform.localPosition = LocalPosition;
					PlayingObject.transform.localRotation = Quaternion.identity;
					PlayingObject.transform.localScale = Vector3.one;
					ParticleSystem[] pss = PlayingObject.GetComponentsInChildren<ParticleSystem>(true);
					if (PlayingObject.GetComponent<ParticleSystem>() != null)
						PlayingObject.GetComponent<ParticleSystem>().Play();
					foreach (ParticleSystem ps in pss)
					{
						ps.Play();
					}
				}
			}
		}
	}
	
	// Use this for initialization
	void Start ()
	{
		Effect = null;
		ReferenceValue = Random.value;
	}
}
