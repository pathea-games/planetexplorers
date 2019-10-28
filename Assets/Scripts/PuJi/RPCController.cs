using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RPCController : MonoBehaviour

{
    public static Dictionary<string, bool> RPCEnable = new Dictionary<string, bool>();

    void Awake(){
        RPCEnable.Clear();
        RPCEnable.Add("RPC_C2S_GetShop", true);
    }

    void Start() { 
    }

    void Update()
    {

    } 

    public static void Reset(string rpcName)
    {
        RPCEnable[rpcName] = true;
    }
}
