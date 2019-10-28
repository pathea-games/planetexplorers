//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//namespace Pathea
//{
//    public abstract class Detector
//    {
//        protected Detector()
//        {
//            Radius = 10f;
//            Layer = ~0;
//        }

//        public Vector3 pos
//        {
//            get;
//            set;
//        }

//        public float Radius
//        {
//            get;
//            set;
//        }

//        public int Layer
//        {
//            get;
//            set;
//        }

//        public abstract void Query(List<Detectable> list);
//    }

//    public class DetectorSight : Detector
//    {
//        public override void Query(List<Detectable> list)
//        {
//            if (null == list)
//            {
//                return;
//            }

//            Collider[] colliders = Physics.OverlapSphere(pos, Radius, Layer);

//            foreach (Collider collider in colliders)
//            {
//                Detectable d = PETools.PEUtil.GetComponent<Detectable>(collider.gameObject);
//                if (d != null && !list.Contains(d))
//                {
//                    list.Add(d);
//                }
//            }
//        }
//    }

//    public class DetectorMonsterSight : DetectorSight
//    {
//        public DetectorMonsterSight()
//        {
//            Radius = 128f;
//            Layer = 1 << Pathea.Layer.Player;
//        }
//    }

//    //public class DetectorHearing : Detector
//    //{
//    //    float mRange;

//    //    public DetectorHearing(float range)
//    //    {
//    //        mRange = range;
//    //    }

//    //    public override void Query(List<Detectable> list)
//    //    {
//    //    }
//    //}
//}