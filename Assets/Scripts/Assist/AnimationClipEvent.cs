using UnityEngine;
using System.Collections;

public class AnimationClipEvent : MonoBehaviour
{
    void AnimationSound(int id)
    {
        AudioManager.instance.Create(transform.position, id);
    }
}
