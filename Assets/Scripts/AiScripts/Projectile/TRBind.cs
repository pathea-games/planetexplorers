using UnityEngine;
using System.Collections;

public class TRBind : Trajectory
{
	public float delayTime;
	public Transform effect;

	void Start()
	{
		Emit(m_Emitter);
	}

    public void Emit(Transform shooter)
    {
//		transform.parent = shooter;
//		transform.localPosition = Vector3.zero;
//		transform.localRotation = Quaternion.identity;

		if(delayTime > 0f)
			StartCoroutine(LengthUp());
//		StartCoroutine(RefreshQueue());
    }

//    public void Update()
//    {
//        if (effect != null && transform.parent != null)
//        {
//            effect.position = transform.parent.position;
//            effect.rotation = transform.parent.rotation;
//        }
//    }
	
//	IEnumerator RefreshQueue(){
//		Queue locus = new Queue();
//		float currentTime = -0.1f;
//		while(true){
//			currentTime += 0.1f;
//			locus.Enqueue(transform.parent.position);
//			locus.Enqueue(transform.parent.rotation);
//			if(currentTime >= delayTime){
//				this.transform.position = (Vector3)locus.Dequeue();
//				this.transform.rotation = (Quaternion)locus.Dequeue();
//			}		
//			yield return new WaitForSeconds(0.1f);
//		}
//	}
	
	IEnumerator LengthUp(){
		CapsuleCollider coli = this.GetComponent<Collider>() as CapsuleCollider;
		if(coli != null)
		{
			float maxLength = coli.height;		
			float unit = (coli.height - coli.radius) / (delayTime / 0.1f);
			
			coli.height = coli.radius - unit;
			while(true){
				coli.height += unit;
				coli.center = new Vector3(0f, 0f, coli.height / 2f);
				if(coli.height > maxLength){
					coli.height = maxLength;
					coli.center = new Vector3(0f, 0f, maxLength / 2f);
					break;
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}
