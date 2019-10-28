using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class TutorMgr : MonoBehaviour {
    static GameObject tutorObj = null;

    static public void Load()
    {
        if (TutorLoaded())
        {
            return;
        }

        GameObject obj = Resources.Load("CreationSystemTutor", typeof(GameObject)) as GameObject;
        tutorObj = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
    }

    

    static public bool TutorLoaded()
    {
        return null != tutorObj;
    }

    static public void Destroy()
    {
        if (!TutorLoaded())
        {
            return;
        }

        GameObject.Destroy(tutorObj);
        tutorObj = null;
    }

    //public GameObject root;
    public UIGrid tutorialGrid;
    public UIPanel stepPanel;
    public UILabel currentStepLabel;
    public UITexture tutorTexture;

    public TextAsset tutorialTextData;
    TutorData tutorData;

    public GameObject tutorialProtoType;
    public GameObject stepProtoType;

    int currentLessonIndex;
    int currentStepIndex;

	void Awake ()
    {
        //instance = this;

        currentLessonIndex = 0;
        currentStepIndex = 0;

        VCEditor.OnCloseComing += OnVcEditorClose;
	}

    void Start()
    {
        //CreateTemplate();
        LoadData();

        UpdateUI();

		InitUI();
    }

    void OnDestroy()
    {
        VCEditor.OnCloseComing -= OnVcEditorClose;
    }

    void OnVcEditorClose()
    {
        TutorMgr.Destroy();
    }

    TutorItem CreateItem(Transform parent)
    {
        GameObject item = GameObject.Instantiate(tutorialProtoType) as GameObject;
        item.transform.parent = parent;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        item.SetActive(true);

        return item.GetComponentInChildren<TutorItem>();
    }

    void DeleteAllChildren(Transform trans)
    {
        while (0 != trans.childCount)
        {
            NGUITools.Destroy(trans.GetChild(0).gameObject);
        }
    }

    void UpdateTutorial()
    {
        DeleteAllChildren(tutorialGrid.transform);
        
        for (int i = 0; i < tutorData.lessons.Count; i++)
        {
            TutorLessonData lesson = tutorData.lessons[i];

            TutorItem item = CreateItem(tutorialGrid.transform);
            item.SetText(lesson.lessonName);
            item.itemId = i;

            UIButtonMessage btnMsg = item.GetComponent<UIButtonMessage>();
            btnMsg.functionName = "OnLessonClicked";
            btnMsg.target = gameObject;

            if (i == currentLessonIndex)
            {
                item.Checked();
            }
        }

        tutorialGrid.repositionNow = true;
    }

    TutorStepItem CreateStepItem(Transform parent)
    {
        GameObject item = GameObject.Instantiate(stepProtoType) as GameObject;
        item.transform.parent = parent;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        item.SetActive(true);

        return item.GetComponent<TutorStepItem>();
    }

    void UpdateStep()
    {
        DeleteAllChildren(stepPanel.transform);
        float clipSizeY = stepPanel.clipRange.w;

        float y = clipSizeY/2;
        float selectedY = 0f;
        for(int i = 0; i < tutorData.lessons[currentLessonIndex].steps.Count; i++)
        {
            TutorStepData step = tutorData.lessons[currentLessonIndex].steps[i];

            TutorStepItem item = CreateStepItem(stepPanel.transform);
            
            item.Text = step.name.ToLocalizationString() + "\n" + step.description.ToLocalizationString();
            item.itemId = i;

            UIButtonMessage btnMsg = item.GetComponent<UIButtonMessage>();
            btnMsg.functionName = "OnStepClicked";
            btnMsg.target = gameObject;

            y -= item.Size.y/2;
            item.transform.localPosition = new Vector3(0f, y, 0f);

            if (i == currentStepIndex)
            {
                currentStepLabel.text = "STEP:" + (i + 1) + "/" + tutorData.lessons[currentLessonIndex].steps.Count;
                item.Selected();
                selectedY = y;
            }

            y -= (item.Size.y/2 + 10);
        }

        UIDraggablePanel dragPanel = stepPanel.GetComponent<UIDraggablePanel>();
        dragPanel.ResetPosition();
        float relativeY = selectedY / (y + clipSizeY / 2 + 10);
        //dragPanel.SetDragAmount(0f, (84 - selectedY - 85) / (84-y-10-170), false);
        dragPanel.SetDragAmount(0f, relativeY, false);
    }

    void UpdateStepImage()
    {
        string file = tutorData.lessons[currentLessonIndex].steps[currentStepIndex].imageFileName;
        if (string.IsNullOrEmpty(file))
        {
            tutorTexture.enabled = false ;
            return;
        }
        else
        {
			file = "TutorTexture/" + file;
			if(SystemSettingData.Instance.IsChinese)
				file += "_cn";
            //Debug.Log(file);
            Texture2D tex = Resources.Load(file) as Texture2D;
            if (null == tex)
            {
                tutorTexture.enabled = false;
                return;
            }
            else
            {
                tutorTexture.enabled = true;
                tutorTexture.mainTexture = tex;
                //tutorTexture.MakePixelPerfect();
            }
        }

        if (IsFullScreenTex())
        {
            tutorTexture.transform.localPosition = Vector3.zero;
        }
        else
        {
            tutorTexture.transform.localPosition = new Vector3(0f, -133f, 0f);
        }

        tutorTexture.MakePixelPerfect();

    }

	void InitUI()
	{
		string file = "TutorTexture/1_1";
		if(SystemSettingData.Instance.IsChinese)
			file += "_cn";
		Texture2D tex = Resources.Load(file) as Texture2D;
		if (null == tex)
		{
			tutorTexture.enabled = false;
			return;
		}
		else
		{
			tutorTexture.enabled = true;
			tutorTexture.mainTexture = tex;
			//tutorTexture.MakePixelPerfect();
		}
	}


    void UpdateUI()
    {
        UpdateTutorial();
        UpdateStep();
        UpdateStepImage();
    }

    void OnClose()
    {
        TutorMgr.Destroy();
    }

    void OnStepPre()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
        }
        else
        {
            if (currentLessonIndex > 0)
            {
                currentLessonIndex--;
                currentStepIndex = tutorData.lessons[currentLessonIndex].steps.Count - 1;
            }
        }

        UpdateUI();
    }

    void OnStepNext()
    {
        if (currentStepIndex < tutorData.lessons[currentLessonIndex].steps.Count - 1)
        {
            currentStepIndex++;
        }
        else
        {
            if (currentLessonIndex < tutorData.lessons.Count - 1)
            {
                currentLessonIndex++;
                currentStepIndex = 0;
            }
        }

        UpdateUI();
    }

    void OnStepClicked(GameObject arg)
    {
        TutorStepItem step = arg.GetComponent<TutorStepItem>();

        currentStepIndex = step.itemId;

        UpdateStep();

        UpdateStepImage();
    }

    void OnLessonClicked()
    {
        TutorItem tutorItem = UICheckbox.current.GetComponent<TutorItem>();
		if (tutorItem == null) {
			Debug.LogError("[TutorMgr]Error:TutorItem not found");
			return;
		}

        currentLessonIndex = tutorItem.itemId;
        currentStepIndex = 0;

        UpdateStep();
        UpdateStepImage();
    }

    bool IsFullScreenTex()
    {
        if (currentLessonIndex != 0 || currentStepIndex != 0)
        {
            return false;
        }

        return true;
    }

    void OnTextureClicked()
    {
        if (!IsFullScreenTex())
        {
            return;
        }

        Vector3 pos = tutorTexture.transform.localPosition;
        pos.y = -133f;

        tutorTexture.transform.localPosition = pos;
        tutorTexture.transform.localScale = new Vector3(676f, 380f, 1f);
    }

    void LoadData()
    {
        using (MemoryStream ms = new MemoryStream(tutorialTextData.bytes))
        {
            if (null != ms)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TutorData));

                tutorData = serializer.Deserialize(ms) as TutorData;

                //Debug.Log(tutorData.lessons.Count);
            }
        }    
    }

    void CreateTemplate()
    {
        TutorData tutorDataTmp = new TutorData();
        
        for(int i = 0; i < 7; i++)
        {
            TutorLessonData lesson = new TutorLessonData(i);
            for(int j = 0; j < 3; j++)
            {
                lesson.steps.Add(new TutorStepData(i, j));
            }

            tutorDataTmp.lessons.Add(lesson);
        }
        
        using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/Tutorials.xml", FileMode.Create, FileAccess.Write))
        {
            if (null != fs)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TutorData));

                serializer.Serialize(fs, tutorDataTmp);
            }
        }
    }
}

[System.Serializable]
public class TutorStepData
{
    [XmlAttribute()]
    public string name;
    public string description;
    [XmlAttribute()]
    public string imageFileName;

    public TutorStepData(int i, int j)
    {
        name = "Step:" + (j+1);
        description = "description excample.";
        imageFileName = ""+(i+1)+"_"+(j+1);
    }
    public TutorStepData(){}
}

[System.Serializable]
public class TutorLessonData
{
    [XmlAttribute()]
    public string lessonName;

    public List<TutorStepData> steps;
    public TutorLessonData(int i)
    {
        lessonName = "lesson:" + (i+1);
        steps = new List<TutorStepData>(3);
    }
    public TutorLessonData(){}
}

[System.Serializable]
public class TutorData
{
    public List<TutorLessonData> lessons;
    public TutorData()
    {
        lessons = new List<TutorLessonData>(3);
    }
}