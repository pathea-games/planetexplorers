using UnityEngine;
using System.Collections;

namespace Railway
{
    public interface IPassenger
    {
        void GetOn(string pose);
        void GetOff(Vector3 pos);
        void UpdateTrans(Transform trans);
    }

    public class RailwaySeat : MonoBehaviour
    {
        [SerializeField] string[] m_Pose;
        IPassenger mPassenger;

        public IPassenger passenger
        {
            get
            {
                return mPassenger;
            }
        }

        public bool SetPassenger(IPassenger passenger)
        {
            if (null == passenger || mPassenger != null)
            {
                return false;
            }

            mPassenger = passenger;

			mPassenger.GetOn(m_Pose[Random.Range(0, m_Pose.Length - 1)]);
            return true;
        }

        public bool ResetPassenger(Vector3 pos)
        {
            if (mPassenger == null)
            {
                return false;
            }

            mPassenger.GetOff(pos);
            mPassenger = null;
            return true;
        }

        void Update()
        {
			if (mPassenger != null && !mPassenger.Equals(null))
            {
                mPassenger.UpdateTrans(transform);
            }
        }
    }
}