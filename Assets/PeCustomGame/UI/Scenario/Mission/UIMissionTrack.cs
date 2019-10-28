using UnityEngine;
using System.Collections;

public class UIMissionTrack : MonoBehaviour
{
    [SerializeField]  UIMissionTrackWnd missionUI;
    public UIMissionTrackWndInterpreter missionInterpreter;

    public bool isShow { get { return missionUI.isShow; } }

    public void Show()
    {
        missionUI.Show();
    }

    public void Hide()
    {
        missionUI.Hide();
    }

    public void ChangeWindowShowState()
    {
        missionUI.ChangeWindowShowState();
    }

    public void Init()
    {
        missionInterpreter.Init();
    }

    public void Close()
    {
        missionInterpreter.Close();
    }

    void OnDestroy()
    {
        //missionInterpreter.Close();
    }

    public UIBaseWnd GetBaseWnd()
    {
        return missionUI;
    }
}
