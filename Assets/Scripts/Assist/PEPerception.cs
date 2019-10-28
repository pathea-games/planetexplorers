using UnityEngine;
using System.Collections;

public class PEPerception : MonoBehaviour 
{
    internal int blockLayer;

    public void Awake()
    {
        blockLayer = 1 << Pathea.Layer.SceneStatic
                        | 1 << Pathea.Layer.VFVoxelTerrain
                        | 1 << Pathea.Layer.Unwalkable
                        | 1 << Pathea.Layer.TreeStatic
                        | 1 << Pathea.Layer.Building
                        | 1 << Pathea.Layer.NearTreePhysics;
    }
}
