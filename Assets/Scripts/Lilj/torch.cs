using UnityEngine;
using System.Collections;
using ItemAsset;

public class torch : MonoBehaviour
{
    [SerializeField]
    GameObject m_fire;

    bool mBurningEnv = true;

    public void SetBurning(bool v)
    {
        if (m_fire == null)
        {
            return;
        }

        m_fire.SetActive(v && mBurningEnv);
    }

    public bool IsBurning()
    {
        if (m_fire == null)
        {
            return false;
        }

        return m_fire.activeSelf;
    }

    void Start ()
	{
        SetBurning(true);
	}

    bool CheckBurningEnv()
    {
        return !(PETools.PE.PointInWater(transform.position) > 0.5f);

        //if(null != VFVoxelWater.self && VFVoxelWater.self.IsInWater(transform.position.x, transform.position.y, transform.position.z))
        //{
        //    return false;
        //}

		// [Edit by zx]
//        if (null != WeatherManager.Instance &&
//                    (WeatherManager.Instance.CurrentWeather == UniSkyWeather.USW_SprinkleRain ||
//                     WeatherManager.Instance.CurrentWeather == UniSkyWeather.USW_TorrentialRain))
//        {
//            return false;
//        }

        //return true;
    }

	void Update ()
	{
        mBurningEnv = CheckBurningEnv();

        if (IsBurning() && !mBurningEnv)
        {
            SetBurning(false);
        }
	}
}
