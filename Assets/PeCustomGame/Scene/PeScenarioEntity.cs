using UnityEngine;
using System.Collections;
using Pathea;

namespace PeCustom
{
    public class PeScenarioEntity : MonoBehaviour
    {
        
        public SpawnPoint spawnPoint;
        public PeEntity entity
        {
            get
            {
                if (m_Entity == null)
                {
                    m_Entity = gameObject.GetComponent<PeEntity>();
                }

                return m_Entity;
            }
        }
        private PeEntity m_Entity = null;

        void Update ()
        {
            if (entity == null)
                return;

            spawnPoint.entityPos = entity.peTrans.position;
        }
    }
}
