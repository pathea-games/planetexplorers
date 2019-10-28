using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TrainingScene
{
    public class HoloCameraControl : MonoBehaviour
    {
        private static HoloCameraControl s_instance;
        public static HoloCameraControl Instance { get { return s_instance; } }
        Camera holocmr;
        Camera maincmr;
        [HideInInspector]
        public RenderTexture textureBase;
        //		[HideInInspector]public GlowEffect glowEffect;
        public List<GameObject> renderObjs1 = new List<GameObject>();
        public List<MeshRenderer> renderObjs2 = new List<MeshRenderer>();
        public List<MeshRenderer> renderObjs3 = new List<MeshRenderer>();
        void Awake()
        {
            s_instance = this;
        }
        void Start()
        {
            holocmr = (Object.Instantiate(Resources.Load("TrainingHoloCamera")) as GameObject).GetComponent<Camera>();
            maincmr = Camera.main;
            Transform t = holocmr.transform;
            t.parent = maincmr.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            holocmr.targetTexture = new RenderTexture(Screen.width, Screen.height, 16);
            textureBase = holocmr.targetTexture;
            //			glowEffect = holocmr.GetComponent<GlowEffect>();
        }
        void Update()
        {
            holocmr.fieldOfView = maincmr.fieldOfView;
            if (0 < renderObjs1.Count)
            {
                foreach (GameObject i in renderObjs1)
                    i.SetActive(true);
                holocmr.Render();
                foreach (GameObject i in renderObjs1)
                    i.SetActive(false);
            }
            else if (0 < renderObjs2.Count)
            {
                foreach (MeshRenderer i in renderObjs2)
                    i.enabled = true;
                if (0 < renderObjs3.Count)
                    foreach (MeshRenderer i in renderObjs3)
                        i.enabled = true;
                holocmr.Render();
                foreach (MeshRenderer i in renderObjs2)
                    i.enabled = false;
                if (0 < renderObjs3.Count)
                    foreach (MeshRenderer i in renderObjs3)
                        i.enabled = false;
            }
        }
    }
}
