using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_MedicalHistoryItem : MonoBehaviour
{

    public UILabel npcNameL;
    public UILabel diseaseNameL;
    public UILabel treatNameL;
    public UILabel needTreatTimes;


    public List<ItemIdCount> medicineList;
    public UISprite medicineIcon;
    public UILabel medicineNum;//药品数量
    public GameObject root;

    public void SetInfo(string _npcName, string _diseaseName, string _treatName,int _treatTime, string _icon,int _medicineCount)
    {
        npcNameL.text = "Name:" + _npcName;
        diseaseNameL.text = "Disease:" + _diseaseName;
        treatNameL.text = "TreatmentPlan:" + _treatName;
        needTreatTimes.text = "Visits:"+_treatTime.ToString();
        medicineIcon.spriteName = _icon;
        medicineNum.text = "x"+_medicineCount.ToString();
        medicineIcon.MakePixelPerfect();
        root.SetActive(true);
    }

    public void ClearInfo()
    {
        npcNameL.text = "";
        diseaseNameL.text = "";
        treatNameL.text = "";
        needTreatTimes.text = "";
        medicineIcon.spriteName = "Null";
        medicineNum.text = "";
        medicineIcon.MakePixelPerfect();
        root.SetActive(false);

    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
