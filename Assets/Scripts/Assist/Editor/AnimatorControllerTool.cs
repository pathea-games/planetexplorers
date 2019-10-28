using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;

public class AnimatorControllerTool
{
    [MenuItem("Assets/AnimatorController/Rebuild")]
    static void RebuildAnimatorController()
    {
        int length = Selection.objects.Length;
        for (int i = 0; i < length; i++)
        {
            AnimatorController controller = Selection.objects[i] as AnimatorController;
            if (controller != null)
                RebuildController(controller);
        }
    }

    static void RebuildController(AnimatorController controller)
    {
        AnimatorStateMachine layer = controller.layers[0].stateMachine;
        for (int i = 0; i < layer.stateMachines.Length; i++)
        {
            AnimatorStateMachine stateMachine = layer.stateMachines[i].stateMachine;
            if(stateMachine.name.Equals("Locomotion"))
            {
                for (int j = 0; j < stateMachine.states.Length; j++)
                {
                    AnimatorState state = stateMachine.states[j].state;
                    if (state.name.Equals("Forward"))
                    {
                        BlendTree blend = state.motion as BlendTree;
                        if(blend != null)
                        {
                        }
                    }
                }
            }
        }
    }
}
