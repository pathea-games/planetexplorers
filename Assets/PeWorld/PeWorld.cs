using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    //public class PeWorld : ISerializable
    //{
    //    const int VERSION_0000 = 0;
    //    const int CURRENT_VERSION = VERSION_0000;

    //    const string ArchiveKeyWorld = "ArchiveKeyWorld";

    //    static PeWorld instance;

    //    PeWorld()
    //    {
    //        ArchiveManager.Instance.Register(this);
    //    }

    //    #region public interface

    //    public static PeWorld Instance
    //    {
    //        get
    //        {
    //            if (null == instance)
    //            {
    //                instance = new PeWorld();
    //            }

    //            return instance;
    //        }
    //    }

    //    public bool Load()
    //    {
    //        byte[] data = ArchiveManager.Instance.CurArchive.GetData(ArchiveKeyWorld);
    //        if (null != data)
    //        {
    //            Import(data);
    //            return true;
    //        }
    //        else
    //        {
    //            GameTime.Timer.Day = GameTime.StoryModeStartDay;
    //            GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
    //            return false;
    //        }
    //    }

    //    #endregion

    //    #region ISerializable
    //    void ISerializable.Serialize(Archive archive)
    //    {
    //        archive.AddData(ArchiveKeyWorld, Export());
    //    }
    //    #endregion

    //    #region import export
    //    byte[] Export()
    //    {
    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            using (BinaryWriter w = new BinaryWriter(ms))
    //            {
    //                w.Write((int)CURRENT_VERSION);

    //                w.Write((float)GameTime.Timer.Day);
    //                w.Write((float)GameTime.Timer.ElapseSpeed);

    //                return ms.ToArray();
    //            }
    //        }
    //    }

    //    void Import(byte[] buffer)
    //    {
    //        using (MemoryStream ms = new MemoryStream(buffer, false))
    //        {
    //            using (BinaryReader r = new BinaryReader(ms))
    //            {
    //                int version = r.ReadInt32();
    //                if (version > CURRENT_VERSION)
    //                {
    //                    Debug.LogError("error version:"+version);
    //                }

    //                GameTime.Timer.Day = r.ReadSingle();
    //                GameTime.Timer.ElapseSpeed = r.ReadSingle();
    //            }
    //        }
    //    }
    //    #endregion
    //}
}