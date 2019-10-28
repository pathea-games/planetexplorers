using UnityEngine;
using System.Collections;

public class UIMissionGoal : MonoBehaviour
{
    [SerializeField] UIMissionGoalWnd missionUI;
    public UIMissionGoalWndInterpreter missionInterpreter;

    public bool isShow { get { return missionUI.isShow; } }

    public void Show ()
    {
        missionUI.Show();
    }

    public void Hide ()
    {
        missionUI.Hide();
    }

    public void ChangeWindowShowState()
    {
        missionUI.ChangeWindowShowState();
    }

    public void Init ()
    {
        missionInterpreter.Init();
    }

    public void Close ()
    {
        missionInterpreter.Close();
    }
}
