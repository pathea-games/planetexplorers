using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;

namespace AiAsset
{
    public class DamageMsgArg
    {
        public AiObject attacker;
        public GameObject hurter;
        public float damage;

        public DamageMsgArg(AiObject attacker, GameObject hurter, float damage)
        {
            this.attacker = attacker;
            this.hurter = hurter;
            this.damage = damage;
        }
    }

    public class AiFunction
    {
        public static void Log(string str)
        {
            UnityEngine.Debug.Log(str);
        }

        public static bool Match(AttackSkill skill, string skillName)
        {
            return skill.name == skillName;
        }
    }

    [System.Serializable]
    public class AiPrefab : IComparable<AiPrefab>
    {
        public LifeHabit habit = LifeHabit.Day;
        public Nature nature = Nature.Land;
        public string prefabName;
        public float radius;
        public float height;
        public float rate;

        public int CompareTo(AiPrefab other)
        {
            return rate.CompareTo(other.rate);
        }
    }

    [System.Serializable]
    public class ActionArgument : IComparable<ActionArgument>
    {
        public bool  enable = false;
        [HideInInspector]
        public bool  canDamage = true;
        public float probability = 0.0f;
        public float coolTime = 0.0f;

        public int CompareTo(ActionArgument other)
        {
            return probability.CompareTo(other.probability);
        }
    }

    [System.Serializable]
    public class DefaultActionArgument : ActionArgument
    {
        public int    skill = 0;
        public string anim = "";
        public float  rangeMin = 0.0f;
        public float  rangeMax = 0.0f;


        public DefaultActionArgument() { }
    }

    public class AttackSkill : IComparable<AttackSkill>
    {
        public string name;
        public int id;
        public float probability;

        public int CompareTo(AttackSkill other)
        {
            return probability.CompareTo(other.probability);
        }
    }

    public struct SkillData
    {
        public int id;
        public float probability;
    }

    public enum Direction
    {
        X_Axis = 0,
        Y_Axis = 1,
        Z_Axis = 2
    }

    [System.Serializable]
    public class CapsuleCollision
    {
        public Vector3 center = Vector3.zero;
        public float radius = 0.0f;
        public float height = 0.0f;
        public Direction direction = Direction.Y_Axis;
    }
}
