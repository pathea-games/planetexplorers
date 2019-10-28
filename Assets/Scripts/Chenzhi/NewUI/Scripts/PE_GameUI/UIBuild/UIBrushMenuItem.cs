using UnityEngine;
using System.Collections;

public class UIBrushMenuItem : MonoBehaviour 
{
	public enum BrushType
	{
		pointAdd,
		pointRemove,
		boxAdd,
		boxRemove,
		diagonalXPos,
		diagonalXNeg,
		diagonalZPos,
		diagonalZNeg,
		SelectAll,
		SelectDetail
	}

	public BrushType m_Type;


	public delegate void ClickEvent (BrushType type);
	//public event ClickEvent onBtnClick;

	public GameObject Target;
	public string ClickFunctionName;



	private BoxCollider mBoxClollider;
	//private bool mRaycastGUI = false;

	static int s_Counter;

	public static bool MouseOnHover { get { return s_Counter != 0; }}

	bool rayCast = false;
	void Start () 
	{
		mBoxClollider = this.gameObject.GetComponent<BoxCollider>();
	}

	void Update() 
	{
		if (mBoxClollider == null || UICamera.currentCamera == null)
		{
			return;
		}

		if (!rayCast)
			s_Counter ++;
		
		Ray ray = UICamera.mainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		RaycastHit rch;

		rayCast = mBoxClollider.Raycast(ray, out rch, 1000);

		if (!rayCast)
			s_Counter--;
	}
	
	void OnDisable ()
	{
		if (rayCast)
		{
			s_Counter --;
			rayCast = false;
		}
	}

	void OnClick()
	{
		if (Target != null && ClickFunctionName != "")
		{
			Target.SendMessage(ClickFunctionName, m_Type);
		}

	}


	

}
