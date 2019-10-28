using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;

public class GameCreditsPanelCtrl : UIStaticWnd
{
    [SerializeField]
    private float m_StartWaitTime;
    [SerializeField]
    private float m_LogoSpeed;
    [SerializeField]
    private float m_ContentSpeed;
    [SerializeField]
    private UILabel m_ContentLabel;
    [SerializeField]
    private Transform m_ContentParent;
    [SerializeField]
    private GameObject m_OptionParent;
    [SerializeField]
    private N_ImageButton m_MainMenuBtn;
    [SerializeField]
    private GameObject m_ProfessionItemPrefab;
    [SerializeField]
    private GameObject m_OneNameItemPrefab;
    [SerializeField]
    private Vector2 m_Padding;
    [SerializeField]
    private bool m_UseXmlModel = true;
    [SerializeField]
    private BoxCollider m_BgBoxCollider;
    [SerializeField]
    private bool m_LoadKickstarterBackers=false;
    [SerializeField]
    private Transform m_TopPos;
    [SerializeField]
    private Transform m_ContentStartPos;
    [SerializeField]
    private Transform m_LogoStartPos;
    [SerializeField]
    private Transform m_LogoTrans;
    [SerializeField]
    private int m_MaxNameWidth = 200;
    [SerializeField]
    private int m_ProfessionItemMaxCapacity = 50;
    [SerializeField]
    private int m_MaxCol=4;

    private bool m_IsPlayCredites;
    private Vector3 m_TempVector;
    private ManyPeopleItem m_EndManyPeopleItem;
    private AudioController m_CurAudioCtrl;
    private List<int> m_BgMusicIDs;
    private int m_CurBgMusicIndex = 0;
    
    

    #region mono methods

    void Start()
    {
        this.Init();
        this.Show();
    }

    void Update()
    {
        if (this.m_IsPlayCredites)
        {
            this.m_TempVector.y=Time.deltaTime*this.m_ContentSpeed;
            this.m_ContentParent.transform.localPosition += this.m_TempVector;
            if (this.m_EndManyPeopleItem.transform.position.y> m_TopPos.transform.position.y)
            {
                this.m_ContentParent.transform.position = this.m_ContentStartPos.position;
            }
        }

        //lz-2016.11.06 声音后面改为异步加载了，所以判断有长度的时候再判读是否播放完
        if (null == m_CurAudioCtrl || (m_CurAudioCtrl.length > 0 && !m_CurAudioCtrl.isPlaying))
        {
            NextBgMusic();
        }
    }

    #endregion

    #region override methods
    public override void Show()
    {
        base.Show();
        this.StartCoroutine(this.ScrollContentInterator());
    }

    protected override void OnHide()
    {
        base.OnHide();
        StopBgMusic();
    }

    #endregion

    #region private methods

    void Init()
    {
        this.m_IsPlayCredites = false;
        this.m_TempVector = Vector3.zero;
        this.m_LogoTrans.position = this.m_LogoStartPos.position;
        this.TrySaveKickstarterBackersToXml();
        this.m_OptionParent.SetActive(false);
        this.m_ContentParent.position = this.m_ContentStartPos.position;
        this.m_CurAudioCtrl = null;
        UIEventListener.Get(this.m_MainMenuBtn.gameObject).onClick += (go) => this.OnMainMenuEvent();
        UIEventListener.Get(this.m_BgBoxCollider.gameObject).onClick+=(go)=>this.ShowMenu();
        this.FillContent();
        this.AdaptiveLogo();
        InitBgMusicList();
        this.PlayBgMusic(this.m_CurBgMusicIndex);
    }

    void FillContent()
    {
        if (this.m_UseXmlModel)
        {
            this.XmlModeFillContent();
        }
        else
        {
            this.TxtModeFillContent();
        }
    }

    void InitBgMusicList()
    {
        m_BgMusicIDs = new List<int>();
        m_BgMusicIDs.Add(1920);
		m_BgMusicIDs.Add(4505);
        m_BgMusicIDs.Add(4506);
        m_CurBgMusicIndex = 0;
    }

    void PlayBgMusic(int index)
    {
		if (null == this.m_CurAudioCtrl&& index<m_BgMusicIDs.Count&& index>=0)
		{
			this.m_CurAudioCtrl = AudioManager.instance.Create(transform.position, m_BgMusicIDs[index], null, false, false);
        }
		if(null != this.m_CurAudioCtrl && !this.m_CurAudioCtrl.isPlaying)
		{
			this.m_CurAudioCtrl.PlayAudio();
		}
    }

    void StopBgMusic()
    {
        if (null != this.m_CurAudioCtrl)
        {
            this.m_CurAudioCtrl.Delete();
            this.m_CurAudioCtrl = null;
        }
    }

    void NextBgMusic()
    {
        if (null!=m_BgMusicIDs&&m_BgMusicIDs.Count > 0)
        {
            if (m_CurBgMusicIndex < (m_BgMusicIDs.Count - 1))
                m_CurBgMusicIndex++;
            else
                m_CurBgMusicIndex = 0;
            StopBgMusic();
            PlayBgMusic(m_CurBgMusicIndex);
        }
    }

    void AdaptiveLogo()
    {
        float aspectRatio = m_LogoTrans.localScale.x / m_LogoTrans.localScale.y;
        m_LogoTrans.localScale = new Vector3(aspectRatio * Screen.height / 6, Screen.height / 6, 1);
    }

    void ContentToCenter()
    {
        float curX=this.m_ContentParent.transform.localPosition.x;
        Bounds contentBounds=NGUIMath.CalculateRelativeWidgetBounds(this.m_ContentParent.transform);
        this.m_ContentParent.transform.localPosition += new Vector3((curX-contentBounds.size.x)* 0.5f, 0, 0);
    }

    //lz-2016.06.13 这种方法可以使可以更便捷的调整每一块，每一条上下左右的间距，和对齐问题
    void XmlModeFillContent()
    {
        //lz-2016.08.02 这里不能用全路径Load
        TextAsset textAsset = Resources.Load("Credits/CreditsXml", typeof(TextAsset)) as TextAsset;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(textAsset.text);

        //root
        XmlNode rootNode=xmlDoc.SelectSingleNode("Root");

        //title
        XmlElement titleNode = (XmlElement)rootNode.SelectSingleNode("Title");
        ManyPeopleItem titleItem=GetNewParticipantItem();
        titleItem.UpdateNames(titleNode.GetAttribute("Name"));
        //Participant
        XmlNode participantNode = rootNode.SelectSingleNode("Participant");
        for (int i = 0; i < participantNode.ChildNodes.Count; i++)
		{
			XmlElement professionNode=(XmlElement)participantNode.ChildNodes[i];

            
            //lz-2016.08.15 Kickstarter Backers人员较多，所以结构不一样
            if (professionNode.GetAttribute("Name").Equals("Kickstarter Backers")
			    ||(professionNode.GetAttribute("Name").Equals("Other Contributors")))
            {
                List<string> kBNames = new List<string>();
                for (int k = 0; k < professionNode.ChildNodes.Count; k++)
                {
                    XmlElement peopleNode = (XmlElement)professionNode.ChildNodes[k];
                    string tempStr = string.Empty;
                    for (int aIndex = 0; aIndex < this.m_MaxCol; aIndex++)
                    {
                        tempStr = peopleNode.GetAttribute("Name" + aIndex);
                        if (string.IsNullOrEmpty(tempStr))
                            continue;
                        kBNames.Add(tempStr);
                    }
                }
                List<string> shotNames=new List<string> ();
                List<string> longNames=new List<string> ();
                kBNames.ForEach(name => { if (name.Length <= this.m_MaxNameWidth)shotNames.Add(name); else longNames.Add(name); });
                shotNames = shotNames.OrderBy(a => a.Length).ToList();
                //lz-2016.08.16 如果名字短的不是整行数，就把不够一行的那个几个元素放到长名字里面去
                if (shotNames.Count % this.m_MaxCol != 0&&longNames.Count>0)
                {
                    int startIndex = shotNames.Count - (shotNames.Count % this.m_MaxCol);
                    longNames.AddRange(shotNames.GetRange(startIndex,shotNames.Count-startIndex));
                    shotNames.RemoveRange(startIndex, shotNames.Count - startIndex);
                }
                longNames = longNames.OrderBy(a => a.Length).ToList();
                if (shotNames.Count > 0)
                {
                    int professionCount=0;
                    ProfessionItem professionItem=null;
                    List<ManyPeopleName> manyPeopleNameList=new List<ManyPeopleName> ();
                    string professionName=professionNode.GetAttribute("Name");
                    for (int j = 0; j < shotNames.Count; j+=this.m_MaxCol)
                    {
                        professionCount++;
                        int count=j+this.m_MaxCol<shotNames.Count?this.m_MaxCol:shotNames.Count-j;
                        string[] names=shotNames.GetRange(j,count).ToArray();
                        manyPeopleNameList.Add(new ManyPeopleName(names));
                        if (professionCount % this.m_ProfessionItemMaxCapacity == 0 || j + this.m_MaxCol >= shotNames.Count)
                        {
                            professionItem = this.GetNewProfessionItem();
                            professionItem.UpdateInfo(new ProfessionInfo(professionName, manyPeopleNameList),false);
                            manyPeopleNameList.Clear();
                            professionName="";
                        }
                    }
                }
                if (longNames.Count > 0)
                {
                    ProfessionItem professionItem = null;
                    List<ManyPeopleName> namelist = new List<ManyPeopleName>();
                    int professionCount = 0;
                    string professionName=shotNames.Count>0?"":professionNode.GetAttribute("Name");
                    for (int m = 0; m < longNames.Count; m+=2)
                    {
                        professionCount++;
                        namelist.Add(new ManyPeopleName(longNames[m], m + 1 < longNames.Count ? longNames[m + 1] : ""));
                        if (professionCount % this.m_ProfessionItemMaxCapacity == 0 || m + 2 >= longNames.Count)
                        {
                            professionItem = this.GetNewProfessionItem();
                            professionItem.UpdateInfo(new ProfessionInfo(professionName,namelist));
                            namelist.Clear();
                        }
                    }
                }
            }
            else
            {
                ProfessionItem professionItem = this.GetNewProfessionItem();
                List<ManyPeopleName> namelist = new List<ManyPeopleName>();
                for (int j = 0; j < professionNode.ChildNodes.Count; j++)
                {
                    XmlElement peopleNode = (XmlElement)professionNode.ChildNodes[j];
                    namelist.Add(new ManyPeopleName(peopleNode.GetAttribute("EnglishName"), peopleNode.GetAttribute("ChineseName")));
                }
                professionItem.UpdateInfo(new ProfessionInfo(professionNode.GetAttribute("Name"), namelist));
            }
            
		}
        //End
        XmlElement endNode = (XmlElement)rootNode.SelectSingleNode("End");
        this.m_EndManyPeopleItem = GetNewParticipantItem();
        this.m_EndManyPeopleItem.UpdateNames(endNode.GetAttribute("Name"));
    }

    void RepositionVertical()
    {
        int count = this.m_ContentParent.childCount;
        Bounds tempBounds;
        Vector3 preEndPos = Vector3.zero;
        Transform curTrans;
        Vector3 padingY=new Vector3(0,this.m_Padding.y,0);
        ProfessionItem tempProfessionItem=null;
        float fontHeight = 31;
        for (int i = 0; i < count; i++)
        {
            curTrans=this.m_ContentParent.GetChild(i);
            tempProfessionItem = curTrans.GetComponent<ProfessionItem>();
            curTrans.localPosition= preEndPos-padingY;
            //lz-2016.08.11 因为KickstarterBackers这一项的内容太多，所以从数据上分成了几项，做了分段处理，但是位置应该连起来
            if (null!=tempProfessionItem&&tempProfessionItem.TitleIsNullOrEmpty)
                curTrans.localPosition = preEndPos - new Vector3(0, (tempProfessionItem.CellHeight - fontHeight)*2, 0);
            else
                curTrans.localPosition = preEndPos - padingY;    
            tempBounds = NGUIMath.CalculateRelativeWidgetBounds(curTrans);
            preEndPos.y = curTrans.localPosition.y- tempBounds.size.y;
        }
    }

    //lz-2016.06.13 这种方法把所有中英文字符加在一个UILabel上面，会因为中英文字体的间距和空格的间距问题不便调整
    void TxtModeFillContent()
    {
        TextAsset textAsset = Resources.Load("Credits/Credits", typeof(TextAsset)) as TextAsset;
        if (textAsset != null)
        {
            this.m_ContentLabel.text = textAsset.text;
        }
    }

    ManyPeopleItem GetNewParticipantItem()
    {
        GameObject go=GameObject.Instantiate(this.m_OneNameItemPrefab)as GameObject;
        go.transform.parent = this.m_ContentParent.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go.GetComponent<ManyPeopleItem>();
    }

    ProfessionItem GetNewProfessionItem()
    {
        GameObject go = GameObject.Instantiate(this.m_ProfessionItemPrefab) as GameObject;
        go.transform.parent = this.m_ContentParent.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go.GetComponent<ProfessionItem>();
    }

    IEnumerator ScrollContentInterator()
    {
        float startTime=Time.realtimeSinceStartup;
           
        while (Time.realtimeSinceStartup - startTime<this.m_StartWaitTime)
        {
            yield return null;
        }
        this.RepositionVertical();
        this.m_IsPlayCredites = true;

        Vector3 newPos = Vector3.zero;
        this.m_LogoTrans.localPosition = new Vector3(this.m_LogoTrans.localPosition.x, this.m_LogoTrans.localPosition.y, 0);
        while (this.m_LogoTrans.localPosition.y<0)
	    {
            newPos.y=Time.deltaTime * this.m_LogoSpeed;
            this.m_LogoTrans.localPosition += newPos;
            yield return null;
	    }
    }

    void OnMainMenuEvent()
    {
        OnHide();
        PeSceneCtrl.Instance.GotoMainMenuScene();
    }

    void ShowMenu()
    {
        this.m_OptionParent.SetActive(true);
    }

    //lz-2016.08.10 这是个保存KickstarterBackers名单到CreditsXml的工具
    void TrySaveKickstarterBackersToXml()
    {
        if (Application.isEditor&&this.m_LoadKickstarterBackers)
        {
            TextAsset textAsset = Resources.Load("Credits/CreditsXml", typeof(TextAsset)) as TextAsset;
            if (null == textAsset) return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(textAsset.text);
            XmlNodeList professionList= xmlDoc.SelectNodes("Root/Participant/Profession");
            XmlNode kickstarterBackersNode = null;
            for (int i = 0; i < professionList.Count; i++)
            {
                if (professionList[i].Attributes["Name"].Value == "Kickstarter Backers")
                {
                    kickstarterBackersNode = professionList[i];
                    break;
                }
            }
            if (null != kickstarterBackersNode)
            {
                TextAsset kbText = Resources.Load("Credits/KickstarterBackers", typeof(TextAsset)) as TextAsset;
                if(string.IsNullOrEmpty(kbText.text))return;
                string[] lineStrs = kbText.text.Split(new string[]{"\r\n"},System.StringSplitOptions.None);
                if (lineStrs.Length <= 0) return;
                XmlNode nameNode = kickstarterBackersNode.LastChild;
                for (int i = 0; i < lineStrs.Length; i += this.m_MaxCol)
                {
                    XmlNode newNode=nameNode.CloneNode(true);
                    for (int aIndex = 0; aIndex <this.m_MaxCol; aIndex++)
                    {
                        newNode.Attributes["Name" + aIndex].Value = aIndex + i < lineStrs.Length ? lineStrs[aIndex + i] : "";
                    }
                    kickstarterBackersNode.AppendChild(newNode);
                }
            }
            string newXmlPath = Application.dataPath + @"/Resources/Credits/NewCreditsXml.xml";
            try
            {
                xmlDoc.Save(newXmlPath);
                Debug.Log("Save NewCreditsXml Succeed! \nPath:" + newXmlPath);
            }
            catch (System.Exception e)
            {
                Debug.Log("Save Failed! Error:"+e.Message);
            }
        }
    }

    #endregion
}

//lz-2016.06.13 人员分组用的数据结构
public class ProfessionInfo 
{
    private string m_ProfessionName;
    public string ProfessionName { get { return m_ProfessionName; } }
    private List<ManyPeopleName> m_ManyPeopleList;
    public List<ManyPeopleName> ManyPeopleList { get { return m_ManyPeopleList; } }

    public ProfessionInfo(string professionName, List<ManyPeopleName> manyPeopleList)
    {
        this.m_ProfessionName = professionName;
        this.m_ManyPeopleList = manyPeopleList;
    }
}


//lz-2016.08.15 这个名字结构用来显示KickstarterBackers名单，因为这个名单人很多，用多列来显示
public class ManyPeopleName
{
    private List<string> m_NameList = new List<string>();
    public List<string> NameList { get{return m_NameList;}}

    public ManyPeopleName(params string[] names)
    {
        if (names.Length > 0)
        {
            this.m_NameList.AddRange(names);
        }
    }
}
