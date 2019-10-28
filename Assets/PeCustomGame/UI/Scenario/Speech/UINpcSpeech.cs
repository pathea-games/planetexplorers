using UnityEngine;

public class UINpcSpeech : MonoBehaviour
{
    // TODO : Npc Speech UI 的总管理
    [SerializeField] UINpcSpeechBox speechUI;
    public UINpcSpeechBoxInterpreter speechInterpreter;

    public bool isShow { get { return speechUI.isShow; } }
    public bool isChoice { get { return speechUI.IsChoice;} }

    public void Show ()
    {
        speechUI.Show();

    }

    public void Hide ()
    {
        speechUI.Hide();
    } 

    public void Init ()
    {
        speechInterpreter.Init();
    }

    public void Close ()
    {
        speechInterpreter.Init();
    }

    void OnDestroy ()
    {

    }
}
