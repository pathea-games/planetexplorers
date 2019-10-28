using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class FlagViewCmpt : ViewCmpt
    {
        DragItemLogicFlag mLogic;

        public DragItemLogicFlag FlagLogic
        {
            get
            {
                if (null == mLogic)
                    mLogic = GetComponent<DragItemLogicFlag>();

                return mLogic;
            }
        }

        public override bool hasView
        {
            get
            {
                return null == mLogic ? false : null != mLogic.FlagEntity;
            }
        }


        /// <summary>
        /// 中心 Transform 对象. 子类实现时应当保证 View 层存在时返回有效的 transform 对象, 否则返回 null
        /// </summary>
        public override Transform centerTransform
        {
            get
            {
                if (!hasView)
                    return null;
                else
                    return FlagLogic.transform;
            }
        }
    }
}