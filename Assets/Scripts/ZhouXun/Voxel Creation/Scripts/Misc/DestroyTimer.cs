using UnityEngine;
using System.Collections;

public class DestroyTimer : MonoBehaviour
{
	public float m_LifeTime = 0f;
	public bool m_DisableDestroy = false;
	
	// Update is called once per frame
	void Update ()
	{
		m_LifeTime -= Time.deltaTime;
		if ( m_LifeTime < 0 )
		{
			GameObject.Destroy(this.gameObject);
		}
	}

	void OnDisable ()
	{
		if ( m_DisableDestroy )
			GameObject.Destroy(this.gameObject);
	}
}
