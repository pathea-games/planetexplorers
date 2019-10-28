using UnityEngine;
using System.Collections;

public class WeaponEffect : MonoBehaviour {
	
	public LayerMask aiLayer;
	public Transform start;
	public Transform end;

    public static int[] EffectID = new int[] { 24, 25, 26, 93};
	
	private bool isPlay = false;
	public bool mAttackStart = false;

    //Player player;
	//AiObject aiObject;

	void Start()
	{
        //player = VCUtils.GetComponentOrOnParent<Player>(gameObject);
		//aiObject = VCUtils.GetComponentOrOnParent<AiObject>(gameObject);
	}

	void LateUpdate () 
	{
        ImpactEffectByCollision();
	}
	
	public void ImpactEffectByCollision()
	{
		float time = 0.0f;

        //if(player != null)
        //    time = player.AttackAnimTime();
        //else if(aiObject != null)
        //    time = aiObject.animationTime;

		if(mAttackStart && time > 0.2f && time < 0.8f)
		{
			if(start != null && end != null && !isPlay){
				RaycastHit hit;
				Ray ray = new Ray(start.position, end.position - start.position);
				float dist = Vector3.Distance(end.position, start.position);
				if(Physics.Raycast(ray, out hit, dist, aiLayer))
				{
                    //if(hit.transform.GetComponent<AiNpcObject>() != null)
                    //    return;
					
					isPlay = true;
                    int _id = 0;
                    if (Random.value < 0.3f)
                        _id = EffectID[EffectID.Length - 1];
                    else
                        _id = EffectID[Random.Range(0, EffectID.Length - 1)];

                    if (_id > 0)
                        EffectManager.Instance.Instantiate(_id, hit.point, Quaternion.identity, null);
				}
			}
		}
		else
		{
		 	isPlay = false;	
			if(mAttackStart && time > 0.8f)
				mAttackStart = false;
		}
	}
}
