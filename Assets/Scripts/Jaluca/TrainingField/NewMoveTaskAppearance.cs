using UnityEngine;
using System.Collections;

namespace TrainingScene
{
    public class NewMoveTaskAppearance : MonoBehaviour
    {

        public Transform orgterrain;
        public float fadeTime;
        public bool produce;
        public bool destroy;
        public float part1time;
        public float minwidth;
        public float reduceSpeed;
        static Vector3 fadeCenter = new Vector3(14.8f, 1.53f, 11f);
        Material mat;
        float ctime = 0f;
        float progress;

        void Start()
        {
            mat = transform.GetComponent<MeshRenderer>().material;
            mat.SetTexture(0, HoloCameraControl.Instance.textureBase);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            //Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>().Add(12, 1);
        }


        void FixedUpdate()
        {
            if (produce)
            {
                ctime += Time.deltaTime;
                //FadeHoloTerrain();

                if (ctime >= fadeTime)
                {
                    ctime = fadeTime;
                    mat.SetFloat("_Scale", 1f);
                    produce = false;
                }
            }
            else if (destroy)
            {
                ctime -= Time.deltaTime;
                //FadeHoloTerrain();
                if (ctime <= 0f)
                {
                    ctime = 0f;
                    destroy = false;
                }
            }
        }
      

        void LateUpdate()
        {
            for (int i = 0, j = 0; i < Block45Man.self.transform.childCount && j < 2; i++)
            {
                GameObject t = Block45Man.self.transform.GetChild(i).gameObject;
                if (t.activeSelf && t.name == "b45Chnk_8_0_12_0")
                {
                    j++;
                    if (t.name == "b45Chnk_8_0_12_0")
                    {
                        orgterrain = t.transform;
                        Debug.Log(t.transform.position);
                    }
                    GetComponent<MeshFilter>().mesh = orgterrain.GetComponent<MeshFilter>().mesh;
                    NewMoveTask.Instance.ChangeRenderTarget(orgterrain.GetComponent<MeshRenderer>());
                }
            }
        }

        void FadeHoloTerrain()
        {
            progress = Mathf.Clamp(ctime / fadeTime, 0f, 1f);
            if (progress == 0f)
                orgterrain.localScale = Vector3.zero;
            else if (progress < part1time)
                orgterrain.localScale = new Vector3(minwidth, Mathf.Min(1f, progress / part1time), minwidth);
            else
                orgterrain.localScale = new Vector3(Mathf.Clamp((progress - part1time) / (1 - part1time), minwidth, 1f), 1f, Mathf.Clamp((progress - part1time) / (1 - part1time), minwidth, 1f));
            orgterrain.position = new Vector3(fadeCenter.x * (1f - orgterrain.localScale.x), fadeCenter.y * (1f - orgterrain.localScale.y), fadeCenter.z * (1f - orgterrain.localScale.z));
            mat.SetFloat("_Scale", 1f / progress);
        }
    }
}
