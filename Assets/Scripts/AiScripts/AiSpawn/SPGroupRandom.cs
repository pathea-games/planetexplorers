using UnityEngine;
using System.Collections;

public class SPGroupRandom : SPGroup
{
    public int id;
    public int minCount;
    public int maxCount;
    public float radius;
    public float rejectRadius;
	public bool  ignoreTerrain;

    AIResource res;

    bool IsValid(Vector3 position)
    {
        AiObject[] aiObjects = transform.GetComponentsInChildren<AiObject>();
        foreach (AiObject ite in aiObjects)
        {
            if (AiUtil.SqrMagnitudeH(ite.position - position) < rejectRadius * rejectRadius)
            {
                return false;
            }

        }

        return true;
    }

	Vector3 GetCorrectPosition()
	{
		if(!ignoreTerrain)
		{
			for (int i = 0; i < 5; i++)
			{
				Vector3 position = AiUtil.GetRandomPosition(
                    transform.position, 0.0f, radius, 200.0f, AiUtil.groundedLayer, 5);
				
				if (IsValid(position))
					return position;
			}
		}
		else
		{
			for (int i = 0; i < 5; i++)
			{
				Vector3 position = transform.position + Random.insideUnitSphere * radius;
				
				if (IsValid(position))
					return position;
			}
		}

		return Vector3.zero;
	}

    public override IEnumerator SpawnGroup()
    {
        if (res == null) yield break;

        int count = Random.Range(minCount, maxCount);

        for (int i = 0; i < count; i++)
        {
			Vector3 position = GetCorrectPosition();

            if (position == Vector3.zero)
                continue;

            Instantiate(id, position, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        yield break;
    }

    void Awake()
    {
        res = AIResource.Find(id);
    }
}
