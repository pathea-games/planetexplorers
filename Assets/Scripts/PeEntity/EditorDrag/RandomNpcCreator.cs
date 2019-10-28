using UnityEngine;
using System.Collections;

using Pathea;

public class RandomNpcCreator : MonoBehaviour
{
    [SerializeField]
    int m_randomTemplateId = 1;

	void Start ()
    {
        int id = WorldInfoMgr.Instance.FetchNonRecordAutoId();

        Create(id, m_randomTemplateId);

        Destroy(gameObject);
    }

    void Create(int id, int randomTemplateId)
    {
		/*PeEntity entity = */PeEntityCreator.Instance.CreateRandomNpc(randomTemplateId, id, transform.position, Quaternion.identity, Vector3.one);
    }
}
