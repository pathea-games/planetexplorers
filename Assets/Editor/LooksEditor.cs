//
//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System;
//
//[CustomEditor(typeof(LooksAPI))]
//
//public class LooksEditor : Editor
//{
//    public static int tabInt;
//    public static LooksAPI mLooksObject;
//    static GameObject mCam = null;
//    static string mSelectName = "";
//
//    static string[] mSex = { "男","女"};
//    int mSelectSex = 0;
//    static string mName = "";
//    static string mPath = "";
//    static string[] mBody = new string[8];
//    static object mOldTarget = null;
//    static bool mControlButton = false;
//    static Vector3 mSelectData = Vector3.zero;
//    // Initialize all components and default values
//    public void Awake()
//    {
//        mSelectName = target.name;
//        mCam = GameObject.Find("Main Camera");
//        if (mCam != null)
//        {
//            if (mCam.transform.childCount > 0)
//            {
//                for (int i = 0; i < mCam.transform.childCount;i++ )
//                {
//                    LooksAPI fTemp = mCam.transform.GetChild(i).GetComponent<LooksAPI>();
//                    if (fTemp == null)
//                    {
//                        BulidComponents(mCam.transform.GetChild(i));
//                    }
//                }
//            }
//        }
//    }
//
//    void BulidComponents(Transform pT)
//    {
//            for (int i = 0; i < pT.childCount; i++)
//            {
//                LooksAPI fLAPI = pT.GetChild(i).GetComponent<LooksAPI>();
//                if (fLAPI == null)
//                {
//                    pT.GetChild(i).gameObject.AddComponent<LooksAPI>();
//                }
//                BulidComponents(pT.GetChild(i));
//            }
//            LooksAPI fLAPI2 =  pT.gameObject.GetComponent<LooksAPI>();
//            if (fLAPI2 == null)
//            {
//                pT.gameObject.AddComponent<LooksAPI>();
//            }
//    }
//    public override void OnInspectorGUI()
//    {
//        EditorGUILayout.Separator();
//        EditorGUILayout.Separator();
//        EditorGUILayout.Separator();
//        EditorGUILayout.Separator();
//        EditorGUILayout.Separator();
//        EditorGUIUtility.labelWidth = 70;
//		  EditorGUIUtility.fieldWidth = 80;
//
//        if (mSelectName == "Main Camera")
//        {
//            SelectCam();
//        }else
//        {
//            SelectObjectType();
//        }
//
//
//        mOldTarget = target;
//    }
//    public string[] selStrings = new string[] { "Min", "Center", "Max"};
//
//
//    Vector3 GetControlData(string pType)
//    {
//        Vector3 fTempData = Vector3.zero;
//        switch (pType)
//        {
//            case "Move":
//                fTempData = Selection.activeGameObject.transform.localPosition;
//                break;
//            case "Scale":
//                fTempData = Selection.activeGameObject.transform.localScale;
//                break;
//            case "Rotate":
//                fTempData = Selection.activeGameObject.transform.localEulerAngles;
//                break;
//        }
//
//        return fTempData;
//    }
//
//    void SelectObjectType()
//    {
//        for (int i = 0; i < AppearanceData.mFaceElements.Length; i++)
//        {
//            for (int j = 2; j < AppearanceData.mFaceElements[i].Length; j++)
//            {
//                if (target.name == AppearanceData.mFaceElements[i][j])
//                {
//                    if (Tools.current.ToString() == AppearanceData.mFaceElements[i][1])
//                    {
//                        if (mOldTarget != target)
//                        {
//                            mControlButton = false;
//                        }
//
//                        mControlButton = EditorGUILayout.Toggle("数据设定",mControlButton);
//                        EditorGUILayout.BeginHorizontal();
//                        int fSelectTemp = GUILayout.Toolbar(LooksAPI.mFaceData[i].Select, selStrings);
//                        if (LooksAPI.mFaceData[i].Center == Vector3.zero)
//                        {
//                            LooksAPI.mFaceData[i].Center = GetControlData(AppearanceData.mFaceElements[i][1]);
//                            mSelectData = LooksAPI.mFaceData[i].Center;
//                        }
//                        if (LooksAPI.mFaceData[i].Select != fSelectTemp)
//                        {
//                            mSelectData = LooksAPI.mFaceData[i].Center;
//                            LooksAPI.mFaceData[i].Select = fSelectTemp;
//                            mControlButton = false;
//                        }
//                        if (mControlButton)
//                        {
//                            switch (LooksAPI.mFaceData[i].Select)
//                            {
//                                case 0:
//                                    LooksAPI.mFaceData[i].Min = GetControlData(AppearanceData.mFaceElements[i][1]);
//                                    break;
//                                case 1:
//                                    LooksAPI.mFaceData[i].Center = GetControlData(AppearanceData.mFaceElements[i][1]);
//                                    mSelectData = LooksAPI.mFaceData[i].Center;
//                                    break;
//                                case 2:
//                                    LooksAPI.mFaceData[i].Max = GetControlData(AppearanceData.mFaceElements[i][1]);
//                                    break;
//                            }
//                        }
//                        EditorGUILayout.EndHorizontal();
//                        EditorGUILayout.BeginVertical();
//                        EditorGUILayout.PrefixLabel(AppearanceData.mFaceElements[i][0]);
//                        LooksAPI.mFaceData[i].Min = EditorGUILayout.Vector3Field("", LooksAPI.mFaceData[i].Min);
//                        LooksAPI.mFaceData[i].Center = EditorGUILayout.Vector3Field("", LooksAPI.mFaceData[i].Center);
//                        LooksAPI.mFaceData[i].Max = EditorGUILayout.Vector3Field("", LooksAPI.mFaceData[i].Max);
//                        EditorGUILayout.EndVertical();
//
//                        if (GUILayout.Button("居中"))
//                        {
//                            switch (AppearanceData.mFaceElements[i][1])
//                            {
//                                case "Move":
//                                    Selection.activeGameObject.transform.localPosition = mSelectData;
//                                    break;
//                                case "Scale":
//                                    Selection.activeGameObject.transform.localScale = mSelectData;
//                                    break;
//                                case "Rotate":
//                                    Selection.activeGameObject.transform.localEulerAngles = mSelectData;
//                                    break;
//                            }
//                        }
//                        if (GUILayout.Button("重置"))
//                        {
//                            LooksAPI.mFaceData[i].Min = Vector3.zero;
//                            LooksAPI.mFaceData[i].Center = Vector3.zero;
//                            LooksAPI.mFaceData[i].Max = Vector3.zero;
//                        }
//                        return;
//                    }
//                }
//            }
//        }
//    }
//    public void SelectCam()
//    {
//                //GUI.DrawTexture();
//
//        mName = EditorGUILayout.TextField("PlayerName",mName);
//        mPath = EditorGUILayout.TextField("PlayerPath", mPath);
//        for (int i = 0; i < mBody.Length;i++ )
//        {
//           mBody[i] = EditorGUILayout.TextField(AppearanceData.mBodyCombineElement[i].mName, mPath);
//        }
//        mSelectSex = GUILayout.Toolbar(mSelectSex, mSex);
//        EditorGUILayout.Separator();
//        EditorGUILayout.Separator();
//        EditorGUILayout.BeginHorizontal();
//        String[] tabOptions = new String[4];
//        tabOptions[0] = "脸部";
//        tabOptions[1] = "头发";
//        tabOptions[2] = "身体";
//        tabOptions[3] = "服饰";
//        tabInt = GUILayout.Toolbar(tabInt, tabOptions);
//        EditorGUILayout.EndHorizontal();
//
//        EditorGUILayout.Separator();
//        EditorGUILayout.Separator();
//
//        switch (tabInt)
//        {
//            case 0:
//                for (int i = 0; i < AppearanceData.mFaceElements.Length; i++)
//                {
//                    EditorGUILayout.BeginVertical();
//                    EditorGUILayout.PrefixLabel(AppearanceData.mFaceElements[i][0]);
//                    LooksAPI.mFaceData[i].Min = EditorGUILayout.Vector3Field("Min", LooksAPI.mFaceData[i].Min);
//                    LooksAPI.mFaceData[i].Center = EditorGUILayout.Vector3Field("Center", LooksAPI.mFaceData[i].Center);
//                    LooksAPI.mFaceData[i].Max = EditorGUILayout.Vector3Field("Max", LooksAPI.mFaceData[i].Max);
//                    EditorGUILayout.EndVertical();
//                }
//                EditorGUILayout.Separator();
//                EditorGUILayout.Separator();
//                EditorGUILayout.BeginHorizontal();
//                if (GUILayout.Button("重置")) 
//                {
//                    LooksAPI.ResetData();
//                };
//                if (GUILayout.Button("保存"))
//                {
//                    LooksAPI.SaveData(mName,mPath,mSex[mSelectSex],mBody);
//                }
//                EditorGUILayout.EndHorizontal();
//                break;
//            case 1:
//
//                break;
//            case 2:
//                for (int i = 0; i < AppearanceData.mBodilyElements.Length; i++)
//                {
//                    EditorGUILayout.BeginVertical();
//                    EditorGUILayout.PrefixLabel(AppearanceData.mBodilyElements[i][0]);
//                    LooksAPI.mFaceData[i].Min = EditorGUILayout.Vector3Field("Min", LooksAPI.mFaceData[i].Min);
//                    LooksAPI.mFaceData[i].Center = EditorGUILayout.Vector3Field("Center", LooksAPI.mFaceData[i].Center);
//                    LooksAPI.mFaceData[i].Max = EditorGUILayout.Vector3Field("Max", LooksAPI.mFaceData[i].Max);
//                    EditorGUILayout.EndVertical();
//                }
//                break;
//            case 3:
//                break;
//        }
//    }
//}
