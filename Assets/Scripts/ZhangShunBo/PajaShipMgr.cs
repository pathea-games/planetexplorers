using UnityEngine;
using System.Collections;

public class PajaShipMgr : MonoBehaviour
{
	public Light directionLight;

	public MeshRenderer waterRenderer;


    void Start ()
    {

    }
    void Update ()
    {
		if (PEWaveSystem.Self != null && waterRenderer != null && waterRenderer.material != null && PEWaveSystem.Self.Target != null)
        {
            waterRenderer.material.SetTexture("_WaveMap", PEWaveSystem.Self.Target);
        }

    }
}

