using UnityEngine;
using System.Collections;

public class RegularMemoryCleaning : MonoBehaviour
{
    [SerializeField,Header("间隔时间，小时")]
    float _intervalTime = 1;

    float _intervalTimeS;
    float _lastCleanTime;

    void Awake ()
    {
        _intervalTimeS = _intervalTime * 3600;
        _lastCleanTime = Time.realtimeSinceStartup;
        DontDestroyOnLoad(gameObject);
	}
	
	void Update ()
    {
        if (Time.realtimeSinceStartup - _lastCleanTime > _intervalTimeS)
        {
            _lastCleanTime = Time.realtimeSinceStartup;
            CleanMemory();
        }
    }

    [ContextMenu("CleanMemory")]
    void CleanMemory()
    {
        Debug.Log("Auto Clean Memory");
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
