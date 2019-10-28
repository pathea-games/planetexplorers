using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LightMgr
{
    static LightMgr _instance = null;

    public static LightMgr Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LightMgr();
            }

            return _instance;
        }
    }

	bool _isFastLightingMode = false;
    public List<LightUnit> lights = new List<LightUnit>();

    public void Registerlight(LightUnit light)
    {
        if (!lights.Contains(light))
        {
            lights.Add(light);
			if (_isFastLightingMode) {
				light.lamp.shadows = LightShadows.None;
				light.lamp.renderMode = LightRenderMode.ForceVertex;
			}
        }
    }

    public void RemoveLight(LightUnit light)
    {
        if (lights.Contains(light))
        {
            lights.Remove(light);
        }
    }

    public LightUnit GetLight(Vector3 point)
    {
        int n = lights.Count;
        for (int i = 0; i < n; i++)
        {
            if (lights[i] != null && lights[i].IsInLight(point))
                return lights[i];
        }

        return null;
    }

    public LightUnit GetLight(Transform tr)
    {
        int n = lights.Count;
        for (int i = 0; i < n; i++)
        {
            if (lights[i] != null && lights[i].IsInLight(tr))
                return lights[i];
        }

        return null;
    }

    public LightUnit GetLight(Transform tr, Bounds bounds)
    {
        int n = lights.Count;
        for (int i = 0; i < n; i++)
        {
            if (lights[i] != null && lights[i].IsInLight(tr, bounds))
                return lights[i];
        }

        return null;
    }

	public void SetLightMode(bool fastMode)
	{
		_isFastLightingMode = fastMode;

		int n = lights.Count;
		if (_isFastLightingMode) {
			for (int i = 0; i < n; i++) {
				lights [i].lamp.shadows = LightShadows.None;
				lights [i].lamp.renderMode = LightRenderMode.ForceVertex;
			}
		} else {
			for (int i = 0; i < n; i++) {
				lights [i].lamp.shadows = lights [i].shadowsBak;
				lights [i].lamp.renderMode = lights [i].renderModeBak;
			}
		}
	}
}
