using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Collections;

public class AnimatorCopy
{
    [MenuItem("Assets/Animator/Copy")]
    public static void UpdateAnimator()
    {
        foreach (Object obj in Selection.objects)
        {
            UpdateAnimator(obj);
        }
    }

    static AnimatorStateMachine GetAnimatorStateMachine(AnimatorStateMachine machine, string name)
    {
        for (int i = 0; i < machine.stateMachines.Length; i++)
        {
            if(machine.stateMachines[i].stateMachine.name.ToLower() == name.ToLower())
            {
                return machine.stateMachines[i].stateMachine;
            }
        }

        return null;
    }

    static AnimatorState GetAnimatorState(AnimatorStateMachine machine, string name)
    {
        for (int i = 0; i < machine.states.Length; i++)
        {
            if(machine.states[i].state.name.ToLower() == name.ToLower())
            {
                return machine.states[i].state;
            }
        }

        return null;
    }

    static void UpdateAnimator(Object obj)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        path = path.Remove(path.IndexOf("_fix"), ("_fix").Length);

        AnimatorController srcController = AssetDatabase.LoadAssetAtPath(path, typeof(AnimatorController)) as AnimatorController;
        AnimatorController dstController = obj as AnimatorController;

        AnimatorStateMachine srcMachine = GetAnimatorStateMachine(srcController.layers[0].stateMachine, "attack");
        AnimatorStateMachine dstMachine = GetAnimatorStateMachine(dstController.layers[0].stateMachine, "attack");

        if (srcMachine != null && dstMachine != null)
        {
            for (int i = 0; i < srcMachine.states.Length; i++)
            {
                if (srcMachine.states[i].state.motion == null)
                    continue;

                //Debug.LogError(srcMachine.states[i].state.name);
                AnimatorState state = GetAnimatorState(dstMachine, srcMachine.states[i].state.name);
                if (state != null)
                {
                    EditorUtility.CopySerialized(srcMachine.states[i].state, state);
                    state.AddStateMachineBehaviour<PEAnimatorRunning>().running = "Attacking";
                }
            }
        }
    }
}
