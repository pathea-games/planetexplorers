using UnityEngine;
using System.Collections;

public class RailwayRunner : MonoBehaviour
{
    static bool useGameTime = false;
    static double lastGameTime;

    static float deltaTime()
    {
        if (!useGameTime)
        {
            return Time.deltaTime * GameTime.NormalTimeSpeed;
        }
        else
        {
            float delta = (float)(GameTime.Timer.Second - lastGameTime);
            lastGameTime = GameTime.Timer.Second;
            return delta;
        }
    }

    public static void SetTime(bool value)
    {
        useGameTime = value;

        if (useGameTime)
        {
            lastGameTime = GameTime.Timer.Second;
        }
    }

    void Update()
    {
        Railway.Manager.Instance.UpdateTrain(deltaTime());
    }
}
