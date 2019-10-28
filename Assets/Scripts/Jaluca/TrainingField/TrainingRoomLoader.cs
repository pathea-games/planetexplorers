using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	class TrainingRoomLoader
	{
		public static void LoadTrainingRoom()
		{
			Object.Instantiate(Resources.Load("TrainingRoomLight"));
			Object.Instantiate(Resources.Load("TrainingRoomExitSign"));
			Object.Instantiate(Resources.Load("TrainingRoomLifts"));
			Object.Instantiate(Resources.Load("EpsilonIndi"));
			Object.Instantiate(Resources.Load("TrainingManager_New"));
            Object.Instantiate(Resources.Load("Prefab/Audio/bg_music_tutorial"));
			Object.Instantiate(Resources.Load<GameObject>("Prefab/Mission/MissionManager"));
            GameObject trainingRoom = Object.Instantiate(Resources.Load<GameObject>("scene_TrainingRoom"));
            Quaternion q = Quaternion.identity;
            q.eulerAngles = new Vector3(0, -90, 0);
            trainingRoom.transform.rotation = q;
            trainingRoom.transform.position = new Vector3(12f, 1.5f, 12f);

            LoadPathfinding(trainingRoom);
		}

        static void LoadPathfinding(GameObject obj)
        {
            if (obj == null) return;

            if(AstarPath.active != null)
            {
                if (AstarPath.active.transform.parent != null)
                    GameObject.Destroy(AstarPath.active.transform.parent.gameObject);
                else
                    GameObject.Destroy(AstarPath.active.gameObject);
            }

            GameObject.Instantiate(Resources.Load("Prefab/Pathfinder_Tutorial"));

            if (AstarPath.active != null)
            {
                for (int i = 0; i < AstarPath.active.graphs.Length; i++)
                {
                    Pathfinding.LayerGridGraph graph = AstarPath.active.graphs[i] as Pathfinding.LayerGridGraph;
                    if (graph != null)
                    {
                        graph.center = new Vector3(12f, 1.5f, 12f);
                        //graph.rotation = new Vector3(0.0f, -90.0f, 0.0f);
                    }
                }

                AstarPath.active.Scan();
            }
        }
	}
}
