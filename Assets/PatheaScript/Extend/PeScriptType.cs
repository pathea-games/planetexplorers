using System.Collections.Generic;
using System.Xml;

namespace PatheaScriptExt
{
    public enum EStatus
    {
        Any,
        HitPoint,
        Stamina,
        Oxygen,
        Max
    }

    public static class PeType
    {
        public static List<Pathea.PeEntity> GetPlayer(int playerId)
        {
            List<Pathea.PeEntity> list = new List<Pathea.PeEntity>(1);

            if (0 == playerId)
            {
                list.Add(GetMainPlayer());
            }
            else
            {
                throw new System.Exception("not found player type.");
            }

            return list;
        }

        public static Pathea.PeEntity GetMainPlayer()
        {
            return Pathea.PeCreature.Instance.mainPlayer;
        }

        public static PatheaScript.VarRef GetPlayerId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "player", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static PatheaScript.VarRef GetItemId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "item", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static PatheaScript.VarRef GetMonsterId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "monster", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static PatheaScript.VarRef GetNpcId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "npc", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static PatheaScript.VarRef GetEffectId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "effect", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static PatheaScript.VarRef GetLocationId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "location", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static PatheaScript.VarRef GetUIId(XmlNode xmlNode, PatheaScript.Trigger trigger)
        {
            return PatheaScript.Util.GetVarRefOrValue(xmlNode, "ui", PatheaScript.VarValue.EType.Int, trigger);
        }

        public static EStatus GetStatusType(XmlNode xmlNode)
        {
            int id = PatheaScript.Util.GetInt(xmlNode, "stat");
            switch (id)
            {
                case -1:
                    return EStatus.Any;
                case 1:
                    return EStatus.HitPoint;
                case 2:
                    return EStatus.Stamina;
                case 3:
                    return EStatus.Oxygen;
                default:
                    return EStatus.Max;
            }
        }
    }
}