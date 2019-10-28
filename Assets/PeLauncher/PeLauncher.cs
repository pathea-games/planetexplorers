using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Pathea
{
    public class PeLauncher : MonoBehaviour
    {
        static PeLauncher instance;
        public static PeLauncher Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new GameObject("PeLauncher").AddComponent<PeLauncher>();
                }

                return instance;
            }
        }

        class LaunchInfo
        {
            public LaunchInfo(float time, ILaunchable target)
            {
                mTime = time;
                mTarget = target;
            }

            public float mTime;
            public ILaunchable mTarget;
        }

        List<LaunchInfo> mList = new List<LaunchInfo>(10);
        EndLaunch mEndLaunch = null;

        void UpdateIndicator(float percent, string info)
        {
            Debug.Log(info + ", progress:" + percent);
            UILoadScenceEffect.Instance.SetProgress((int)(100 * percent));
        }
		public bool isLoading = false;
        IEnumerator Load()
        {
            float curTime = 0f;
            float totalTime = 0f;

            mList.ForEach(delegate(LaunchInfo launch)
            {
                totalTime += launch.mTime;
            });

            foreach (LaunchInfo launch in mList)
            {
                launch.mTarget.Launch();

                curTime += launch.mTime;

                UpdateIndicator(curTime / totalTime, launch.mTarget.GetType().ToString());

                yield return 0;
            }
			
			isLoading = false;

            while (true)
            {
                if (null == mEndLaunch || mEndLaunch())
                {
                    break;
                }
                //Debug.Log("wait 1s to end launch");
                yield return new WaitForSeconds(1f);
            }

            endLaunch = null;
            yield return new WaitForSeconds(3f);

            Destroy(gameObject);

            UILoadScenceEffect.Instance.EnableProgress(false);
            UILoadScenceEffect.Instance.BeginScence(null);

            eventor.Dispatch(new LoadFinishedArg());
			FastTravel.bTraveling = false;
        }

        #region public
        public class LoadFinishedArg : PeEvent.EventArg
        {
        }

        PeEvent.Event<LoadFinishedArg> mEventor = new PeEvent.Event<LoadFinishedArg>();

        public PeEvent.Event<LoadFinishedArg> eventor
        {
            get
            {
                return mEventor;
            }
        }

        public delegate bool EndLaunch();

        public EndLaunch endLaunch
        {
            get
            {
                return mEndLaunch;
            }
            set
            {
                mEndLaunch = value;
            }
        }
        public interface ILaunchable
        {
            void Launch();
        }

        public void Add(ILaunchable target, float time = 1f)
        {
            mList.Add(new LaunchInfo(time, target));
        }

        public void StartLoad()
		{
			isLoading = true;
			UILoadScenceEffect.Instance.EnableProgress(true);
            StartCoroutine(Load());
        }
        #endregion
    }
}