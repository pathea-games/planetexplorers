using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class AutoArchiveRunner:MonoBehaviour
    {
        const float AutoSaveInterval = 60*5f;

        public static AutoArchiveRunner instance;

		public static void Init(){
			if (null == instance)
			{
				instance = new GameObject("AutoArchiveRunner").AddComponent<AutoArchiveRunner>();
			}
		}

        public static void Start()
        {
            instance.StartAutoSave();
        }
        public static void Stop()
        {
            instance.StopAutoSave();
		}
		public static void DestroySelf()
		{
            Object.Destroy(instance.gameObject);
            instance = null;
		}

        void StartAutoSave()
        {
            StartCoroutine(AutoSave());
        }

        void StopAutoSave()
        {
            StopAllCoroutines();
        }

        IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(AutoSaveInterval);

                ArchiveMgr.Instance.Save(ArchiveMgr.ESave.Auto1);

                yield return new WaitForSeconds(AutoSaveInterval);

                ArchiveMgr.Instance.Save(ArchiveMgr.ESave.Auto2);

                yield return new WaitForSeconds(AutoSaveInterval);

                ArchiveMgr.Instance.Save(ArchiveMgr.ESave.Auto3);
            }
        }

        void OnApplicationQuit()
        {
			if(PeFlowMgr.Instance.curScene == PeFlowMgr.EPeScene.GameScene&&RandomDungenMgrData.InDungeon&&PeGameMgr.IsSingleAdventure)
			{
				RandomDungenMgr.Instance.SaveInDungeon();
				RandomDungenMgr.Instance.DestroyDungeon();
			}
			QuitSave ();
        }

        public static void QuitSave()
        {
            if (instance == null)
            {
                return;
            }

            ArchiveMgr.Instance.QuitSave();
        }
    }
}