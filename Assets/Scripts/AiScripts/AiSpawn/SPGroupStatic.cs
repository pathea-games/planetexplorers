using UnityEngine;
using System.Collections;

public class SPGroupStatic : SPGroup 
{
    public override IEnumerator SpawnGroup()
	{
		foreach (Transform ite in transform) 
		{
			if(ite.tag != "AIPoint")
				continue;

            while (!AiUtil.CheckCorrectPosition(ite.position, AiUtil.groundedLayer))
            {
				yield return new WaitForSeconds(0.5f);
            }

			int res = System.Convert.ToInt32(ite.name);
			Instantiate(res, ite.position, ite.rotation);

			yield return new WaitForSeconds(0.1f);
		}

        yield return new WaitForSeconds(0.5f);
		yield break;
	}

    void OnDrawGizmos()
    {
        foreach (Transform tr in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(tr.position, 0.5f);
        }
    }
}
