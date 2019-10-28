using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Pathea
{
    public class PeGameMgrRunner : MonoBehaviour
    {
        void Awake()
        {
            ClearSingleton();

            PeGameMgr.Run();

            Destroy(gameObject);
        }

        static void ClearSingleton()
        {
            //avoid some singleton be used before game, creation eg.
            MonoLikeSingletonMgr.Instance.Clear();
        }
    }
}