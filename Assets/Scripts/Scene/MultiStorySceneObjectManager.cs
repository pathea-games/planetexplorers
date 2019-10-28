using System.Collections.Generic;
using UnityEngine;

namespace Pathea
{

    /// <summary>
    /// lz-2018.01.29 用来管理多人故事模式非主场景的场景状态
    /// </summary>
    public class MultiStorySceneObjectManager : MonoBehaviour
    {
        public class NumberOfScnene
        {
            public List<int> playerIDs;
            public SceneBasicObjAgent _obj;
            public Light _directionLight;

            public NumberOfScnene(SingleGameStory.StoryScene scene, SceneBasicObjAgent obj)
            {
                playerIDs = new List<int>(2);
                _obj = obj;

                switch (scene)
                {
                    case SingleGameStory.StoryScene.PajaShip:
                        PajaShipMgr ship = obj.Go.GetComponent<PajaShipMgr>();
                        if (ship != null) _directionLight = ship.directionLight;
                        break;
                    case SingleGameStory.StoryScene.DienShip0:
                    case SingleGameStory.StoryScene.DienShip1:
                    case SingleGameStory.StoryScene.DienShip2:
                    case SingleGameStory.StoryScene.DienShip3:
                    case SingleGameStory.StoryScene.DienShip4:
                    case SingleGameStory.StoryScene.DienShip5:
                        DienManager ship2 = obj.Go.GetComponent<DienManager>();
                        if (ship2 != null) _directionLight = ship2.directionLight;
                        break;
                }

                RefreshState();
            }

            public void RefreshState()
            {
                //没人在里面就关闭GameObject
                if (playerIDs.Count == 0)
                {
                    _obj.Go.SetActive(false);
                }
                else
                {
                    //有人在里面就开启GameObject
                    _obj.Go.SetActive(true);
                    //本玩家在里面就开启平行光
                    if (_directionLight) _directionLight.enabled = playerIDs.Contains(PlayerNetwork.mainPlayerId);
                }
            }
        }
        public static MultiStorySceneObjectManager instance { get { return _instance; } }

        static MultiStorySceneObjectManager _instance;

        Dictionary<int, NumberOfScnene> _objects;       //key == StoryScene , value == object

        private void Awake()
        {
            if (PeGameMgr.IsMultiStory)
            {
                _instance = this;
                LoadMultiStoryScenePrafeb();
            }
            else enabled = false;
        }


        void LoadMultiStoryScenePrafeb()
        {
            if (PeGameMgr.IsMultiStory)
            {
                _objects = new Dictionary<int, NumberOfScnene>();

                var scene = SingleGameStory.StoryScene.DienShip0;

                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/DienShip", "", new Vector3(14798f, 3f, 8344f), Quaternion.identity, Vector3.one)));


                scene = SingleGameStory.StoryScene.DienShip1;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/DienShip", "", new Vector3(16545.25f, 3.93f, 10645.7f), Quaternion.identity, Vector3.one)));


                scene = SingleGameStory.StoryScene.DienShip2;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/DienShip", "", new Vector3(2876f, 265.6f, 9750.3f), Quaternion.identity, Vector3.one)));


                scene = SingleGameStory.StoryScene.DienShip3;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/DienShip", "", new Vector3(13765.5f, 75.7f, 15242.7f), Quaternion.identity, Vector3.one)));

                scene = SingleGameStory.StoryScene.DienShip4;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/DienShip", "", new Vector3(12547.7f, 523.7f, 13485.5f), Quaternion.identity, Vector3.one)));

                scene = SingleGameStory.StoryScene.DienShip5;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/DienShip", "", new Vector3(7750.4f, 349.7f, 14712.8f), Quaternion.identity, Vector3.one)));


                scene = SingleGameStory.StoryScene.L1Ship;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                new SceneBasicObjAgent("Prefab/Other/old_scene_boatinside", "", new Vector3(9661f, 88.8f, 12758f), Quaternion.identity, Vector3.one)));

                Quaternion q = Quaternion.identity;
                q.eulerAngles = new Vector3(352, 55, 0);

                scene = SingleGameStory.StoryScene.PajaShip;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/paja_port_shipinside", "", new Vector3(1471f, 101.3f - 500f, 7928.3f), q, Vector3.one)));

                q.eulerAngles = new Vector3(0, 180, 0);
                scene = SingleGameStory.StoryScene.LaunchCenter;
                _objects.Add((int)scene, new NumberOfScnene(scene,
                    new SceneBasicObjAgent("Prefab/Other/paja_launch_center", "", new Vector3(1713, 140 - 500, 10402), q, Vector3.one)));
            }
        }

        public void RequestChangeScene(int playerID, int sceneID)
        {
            if (PeGameMgr.IsMultiStory && _objects != null)
            {
                foreach (var item in _objects)
                {
                    if (item.Value.playerIDs.Contains(playerID))
                    {
                        item.Value.playerIDs.Remove(playerID);
                        item.Value.RefreshState();
                    }
                    if (item.Key == sceneID)
                    {
                        if (!item.Value.playerIDs.Contains(playerID))
                        {
                            item.Value.playerIDs.Add(playerID);
                            item.Value.RefreshState();
                        }
                    }
                }
            }
        }
    }
}