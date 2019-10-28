using UnityEngine;
using System.Collections;

public class UINpcDialog : MonoBehaviour
{
    [SerializeField] UINpcDialogWnd dialogUI;
    public UINpcDialogWndInterpreter dialogInterpreter;

    public bool isShow { get { return dialogUI.isShow; } }

    public bool allowShow = true;

    public void Show ()
    {
        if (allowShow)
            dialogUI.Show();
    }

    public void Hide ()
    {
        dialogUI.Hide();
    }

    public void Init ()
    {
        dialogInterpreter.Init();
    }

    public void Close ()
    {
        dialogInterpreter.Close();
    }

    void OnDestroy ()
    {

    }

}
