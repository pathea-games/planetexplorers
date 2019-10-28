using System.Collections.Generic;
using UnityEngine;

namespace PeMap
{
    public class MaskTileRunner : Pathea.MonoLikeSingleton<MaskTileRunner>
    {
        //bool mUpdateTile = false;

        public void StartUpdate()
        {
            //mUpdateTile = true;
        }

        Pathea.PeTrans mTrans = null;

        Vector3 playerPos
        {
            get
            {
                if (mTrans == null)
                {
                    mTrans = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PeTrans>();
                }

                return mTrans.position;
            }
        }

        //public override void Update()
        //{
        //    base.Update();
        
        //    if (mUpdateTile)
        //    {
        //        List<int> indexList = MaskTile.Mgr.GetNeighborIndex(MaskTile.Mgr.GetMapIndex(playerPos));

        //        foreach (int index in indexList)
        //        {
        //            MaskTile tile = MaskTile.Mgr.Instance.GetTile(index);
        //            if (null == tile)
        //            {
        //                MaskTile.Mgr.Instance.Add(index, new MaskTile());
        //            }
        //        }
        //    }
        //}
    }
}