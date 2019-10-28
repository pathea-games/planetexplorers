using ItemAsset;
using UnityEngine;


public class ItemScript_State : ItemScript
{
    protected int mSubState;

    protected virtual void SetState(int state)
    {
        ApplyState(state);
    }

    protected virtual void ApplyState(int state)
    {
        mSubState = state;
    }
}