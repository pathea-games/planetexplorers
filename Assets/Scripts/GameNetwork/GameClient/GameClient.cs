using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class GameClient : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);

		LogManager.InitLogManager();
        ClientConfig.InitClientConfig();
        //PositionsConfig.InitTerrainConfig();
		MapsConfig.InitMapConfig();
    }
}
