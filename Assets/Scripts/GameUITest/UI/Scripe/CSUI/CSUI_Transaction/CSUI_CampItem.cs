using UnityEngine;
using System.Collections;

public class CSUI_CampItem : MonoBehaviour {




	[SerializeField] UILabel mCampNameLb;
	[SerializeField] GameObject ChoseBg;

	public delegate void CampChose(object sender);
	public event CampChose e_ItemOnClick = null;

	public enum Typecamp
	{
		Earth,
		Mars
	}
	
	Typecamp mCamp;
	public Typecamp Camp
	{
		set
		{
			mCamp = value;
		}
		get 
		{
			return mCamp ;
		}
	}
	// Use this for initialization

	void Awake()
	{
		ChoseBg.SetActive(false);
	}

	void Start () 
	{
	
	}

	public void SetCampName(string Name)
	{
		if(Name.Length<15)
		{
			mCampNameLb.text = Name;
		}
		else
		{

		}

		//Name.Length
	}

	void OnTooltip (bool show)
	{

	}

    public string GetCampName()
    {
        return mCampNameLb.text;
    }

	public void SetChoeBg(bool Show)
	{
		ChoseBg.SetActive(Show);
	}

	void OnCampChose()
	{
		if(e_ItemOnClick != null)
		{
			e_ItemOnClick(this);
		}
	}

	// Update is called once per frame
	void Update () 
	{
	
	}
}
