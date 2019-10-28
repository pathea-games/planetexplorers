using UnityEngine;
using System.Collections;

public class UIAlphaGroup : MonoBehaviour
{
    // Use this for initialization
    [SerializeField]
    float mAlpha = 1;
    // 0 : Active 
    // 1 : deActive
    [SerializeField]
    float[] mAlphaGroup;

    [Range(1, 30)]
    public int m_Speed = 1;//变化速度

    public float Alpha { get { return mAlpha; } }

    float newAlpha = 1;
    public int State { set { newAlpha = (value < mAlphaGroup.Length) ? mAlphaGroup[value] : 1; } }

    void Update()
    {
        if (newAlpha != mAlpha)
        {
            mAlpha = Mathf.Lerp(mAlpha, newAlpha, Time.deltaTime * 2 * m_Speed);
            if (Mathf.Abs(mAlpha - newAlpha) < 0.02f)
                mAlpha = newAlpha;
        }
    }
}
