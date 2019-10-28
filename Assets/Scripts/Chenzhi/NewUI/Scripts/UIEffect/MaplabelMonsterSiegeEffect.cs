using UnityEngine;
using System.Collections;

public class MaplabelMonsterSiegeEffect : MonoBehaviour {

    [SerializeField]
    private AnimationCurve m_AlphaCurve;
    [SerializeField]
    private AnimationCurve m_ScaleCurve;
    [SerializeField]
    private Vector3 m_MaxScale = new Vector3(120,120,1);
    [SerializeField]
    private bool m_Loop=true;
    [SerializeField]
    private float m_Time = 0.6f;
    [SerializeField]
    private UISprite m_Sprite;

    private float m_StartTime;
    private float m_DeltaTime;
    private Color m_CurColor;
    private Vector3 m_CurScale;
    
    public bool Run = true;

    void Start () {
        m_DeltaTime = 0;
        m_StartTime = Time.realtimeSinceStartup;
        m_CurColor = m_Sprite.color;
        m_CurScale = m_Sprite.transform.localScale;
    }
	
	void Update () {

        if (Run)
        {
            m_DeltaTime += Time.deltaTime;
            m_CurScale.x = m_MaxScale.x * m_ScaleCurve.Evaluate(m_DeltaTime);
            m_CurScale.y = m_MaxScale.y * m_ScaleCurve.Evaluate(m_DeltaTime);
            m_Sprite.transform.localScale = m_CurScale;
            m_CurColor.a = m_AlphaCurve.Evaluate(m_DeltaTime);
            m_Sprite.color = m_CurColor;
            if (m_Loop)
            {
                if (Time.realtimeSinceStartup - m_StartTime > m_Time)
                {
                    m_DeltaTime = 0;
                    m_StartTime = Time.realtimeSinceStartup;
                }
            }
            else
            {
                Run = false;
            }
        }
	}
}
