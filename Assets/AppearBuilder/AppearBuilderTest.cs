using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomCharactor;

namespace AppearBlendShape
{
    public class AppearBuilderTest : MonoBehaviour
    {
        [System.Serializable]
        class ModelInfo
        {
            [SerializeField]
            public GameObject m_boneRoot;

            //public string[] CurrentParts = new string[(int)EPart.Max];

            public AvatarData clothed;
            public AvatarData nude;
        }

        //enum EPart
        //{
        //    Helmet = 0,
        //    Head,
        //    Torso,
        //    Legs,
        //    Hands,
        //    Feet,
        //    Max
        //}

        IEnumerable<string> CurrentParts
        {
            get
            {
                return AvatarData.GetParts(current.clothed, current.nude);
            }
        }

        AvatarData CurrentAvatar
        {
            get
            {
                return current.clothed;
            }
        }

        AvatarData CurrentNudeAvatar
        {
            get
            {
                return current.nude;
            }
        }

        [SerializeField]
        ModelInfo female;
        [SerializeField]
        ModelInfo male;

        ModelInfo current;

        AppearData mAppearData;

        ESex Sex
        {
            get;

            set;
        }

        void Awake()
        {
            CustomCharactor.CustomMetaData.LoadData();

            female.m_boneRoot.transform.parent.gameObject.SetActive(false);
            male.m_boneRoot.transform.parent.gameObject.SetActive(false);

            SetMale();
            //SetFemale();

            SetAppearData(new AppearData());

            female.clothed = new AvatarData();
            female.nude = new AvatarData();
            female.nude.SetFemaleBody();

            male.clothed = new AvatarData();
            male.nude = new AvatarData();
            male.nude.SetMaleBody();
        }

        void SetAppearData(AppearData data)
        {
            mAppearData = data;
        }

        void ChangeCurrent(ModelInfo cur)
        {
            if (null != current)
            {
                current.m_boneRoot.transform.parent.gameObject.SetActive(false);
            }

            current = cur;

            if (null != current)
            {
                current.m_boneRoot.transform.parent.gameObject.SetActive(true);
            }
        }

        void SetMale()
        {
            Sex = ESex.Male;
            ChangeCurrent(male);
        }

        void SetFemale()
        {
            Sex = ESex.Female;
            ChangeCurrent(female);
        }

        void ToggleSex()
        {
            if (ESex.Male == Sex)
            {
                SetFemale();
            }
            else
            {
                SetMale();
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, 500, 500));
            GUILayout.BeginVertical();
            
            DrawHeader();
            DrawHead();
            DrawCloth();
            //DrawBody();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(1000, 0, 300, 800));

            DrawTwist();

            GUILayout.EndArea();
        }

        //void DrawBody()
        //{
        //    if (GUILayout.Button("body"))
        //    {
        //        if (Sex == ESex.Female)
        //        {
        //            CurrentParts[(int)EPart.Helmet] = string.Format("Model/PlayerModel/Female-Helmet");
        //            CurrentParts[(int)EPart.Torso] = string.Format("Model/PlayerModel/Female-torso_0");
        //            CurrentParts[(int)EPart.Legs] = string.Format("Model/PlayerModel/Female-legs_0");
        //            CurrentParts[(int)EPart.Hands] = string.Format("Model/PlayerModel/Female-hands_0");
        //            CurrentParts[(int)EPart.Feet] = string.Format("Model/PlayerModel/Female-feet_0");
        //        }
        //        else
        //        {
        //            CurrentParts[(int)EPart.Helmet] = string.Format("Model/PlayerModel/Male-Helmet");
        //            CurrentParts[(int)EPart.Torso] = string.Format("Model/PlayerModel/Male-torso_0");
        //            CurrentParts[(int)EPart.Legs] = string.Format("Model/PlayerModel/Male-legs_0");
        //            CurrentParts[(int)EPart.Hands] = string.Format("Model/PlayerModel/Male-hands_0");
        //            CurrentParts[(int)EPart.Feet] = string.Format("Model/PlayerModel/Male-feet_0");
        //        }

        //        Apply();
        //    }
        //}

        void DrawCloth()
        {
            GUILayout.BeginVertical();

            for (int i = 1; i <= 10; i++)
            {
                GUILayout.BeginHorizontal();

                if (Sex == ESex.Female)
                {
                    if (GUILayout.Button("helmet" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.HairF, string.Format("Model/PlayerModel/female{0:D2}-Helmet", i));
                    }
                    if (GUILayout.Button("torso" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Torso, string.Format("Model/PlayerModel/female{0:D2}-torso", i));
                    }

                    if (GUILayout.Button("legs" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Legs, string.Format("Model/PlayerModel/female{0:D2}-legs", i));
                    }

                    if (GUILayout.Button("hands" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Hands, string.Format("Model/PlayerModel/female{0:D2}-hands", i));
                    }
                    if (GUILayout.Button("feet" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Feet, string.Format("Model/PlayerModel/female{0:D2}-feet", i));
                    }
                }
                else
                {
                    if (GUILayout.Button("helmet" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.HairF, string.Format("Model/PlayerModel/male{0:D2}-Helmet", i));
                    }
                    if (GUILayout.Button("torso" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Torso, string.Format("Model/PlayerModel/male{0:D2}-torso", i));
                    }

                    if (GUILayout.Button("legs" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Legs, string.Format("Model/PlayerModel/male{0:D2}-legs", i));
                    }

                    if (GUILayout.Button("hands" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Hands, string.Format("Model/PlayerModel/male{0:D2}-hands", i));
                    }
                    if (GUILayout.Button("feet" + i))
                    {
                        CurrentAvatar.SetPart(AvatarData.ESlot.Feet, string.Format("Model/PlayerModel/male{0:D2}-feet", i));
                    }
                }

                //Apply();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        void DrawHead()
        {
            GUILayout.BeginHorizontal();

            CustomMetaData customMetaData;
            if (Sex == ESex.Male)
            {
                customMetaData = CustomMetaData.InstanceMale;    
            }
            else
            {
                customMetaData = CustomMetaData.InstanceFemale;
            }

            for (int i = 0; i < customMetaData.GetHeadCount(); i++)
            {
                if (GUILayout.Button("head"+(i+1)))
                {
                    CurrentNudeAvatar.SetPart(AvatarData.ESlot.Head, customMetaData.GetHead(i).modelPath);
                    Apply();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(Sex.ToString()))
            {
                ToggleSex();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawTwist()
        {
            if (null == mAppearData)
            {
                return;
            }

            GUILayout.BeginVertical();

            for (int i = 0; i < (int)EMorphItem.Max; i++)
            {
                if (i == (int)EMorphItem.LegBelly
                    || i == (int)EMorphItem.LegWaist
                    || i == (int)EMorphItem.Foot
                    || i == (int)EMorphItem.Hand
                    || i == (int)EMorphItem.TorsoUpperLeg)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                EMorphItem eItem = (EMorphItem)i;
                GUILayout.Label(eItem.ToString());

                float weight = mAppearData.GetWeight(eItem);

                float newWeight = GUILayout.HorizontalSlider(weight, -1f, 1f);

                if (!Mathf.Approximately(weight, newWeight))
                {
                    mAppearData.SetWeight(eItem, newWeight);

                    Apply();
                }

                GUILayout.EndHorizontal();
            }

            DrawBuild();

            GUILayout.EndVertical();
        }

        void DrawBuild()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("build"))
            {
                Apply();
            }

            if (GUILayout.Button("random"))
            {
                mAppearData.RandomMorphWeight();

                Apply();
            }

            GUILayout.EndHorizontal();
        }

        void Apply()
        {
            AppearBuilder.Build(current.m_boneRoot, mAppearData, CurrentParts);
        }
    }
}