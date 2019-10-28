using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using PETools;
using Pathea;

namespace Behave.Runtime.Action
{
    [BehaveAction(typeof(BTThreaten), "Threaten")]
    public class BTThreaten : BTNormal
    {
        public class Data
        {
            [BehaveAttribute]
            public float probability = 0.0f;
            [BehaveAttribute]
            public string[] anims = new string[0];
            [BehaveAttribute]
            public float[] times = new float[0];

            public float m_StartTime = 0.0f;
            public int index = 0;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_Data.anims.Length <= 0 || m_Data.anims.Length != m_Data.times.Length)
                return BehaveResult.Failure;

            if (Random.value > m_Data.probability)
                return BehaveResult.Failure;

            m_Data.m_StartTime = Time.time;
            m_Data.index = Random.Range(0, m_Data.anims.Length);
            SetBool(m_Data.anims[m_Data.index], true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.times[m_Data.index])
                return BehaveResult.Success;
            else
                return BehaveResult.Running;
        }
    }
}