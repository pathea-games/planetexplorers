using UnityEngine;
using System.Collections;

namespace PeMap
{
    public class MapRunner : MonoBehaviour
    {
        static bool updateTile = false;
        Pathea.PeTrans mTrans = null;

        public static bool HasMaskTile
        {
            get
            {
                return updateTile;
            }
        }

        public static void UpdateTile(bool value)
        {
            updateTile = value;
        }

        void Start()
        {
            StartCoroutine(Run());
        }

        bool GetPlayerPos(out Vector3 pos)
        {
            if (mTrans == null)
            {
                Pathea.PeEntity entity = Pathea.PeCreature.Instance.mainPlayer;
                if (null != entity)
                {
                    mTrans = entity.GetCmpt<Pathea.PeTrans>();
                }
            }

            if (null == mTrans)
            {
                pos = Vector3.zero;
                return false;
            }

            pos = mTrans.position;
            return true;
        }

        IEnumerator Run()
        {
            while (true)
            {
                Vector3 pos;
                if (GetPlayerPos(out pos))
                {
                    StaticPoint.Mgr.Instance.Tick(pos);
                }

                yield return new WaitForSeconds(1f);

                if (updateTile)
                {
                    if (GetPlayerPos(out pos))
                    {
                        MaskTile.Mgr.Instance.Tick(pos);
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }

}