using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Pathea
{
    class SwapSpace
    {
        DirectoryInfo mDirInfo;

        public void Init(string dir = null)
        {
            if (string.IsNullOrEmpty(dir))
            {
                string tempPath = Path.GetTempPath();
                Debug.Log("<color=aqua>temp path:"+ tempPath+"</color>");
                dir = Path.Combine(tempPath, "planet_explorers_swap");
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);

                if (dirInfo.Exists)
                {
                    FileSystemInfo[] fileSystemInfos = dirInfo.GetFileSystemInfos();
                    if (fileSystemInfos == null || fileSystemInfos.Length == 0)
                    {
                        mDirInfo = dirInfo;
                        return;
                    }
                    else
                    {
                        Directory.Delete(dir, true);
                    }
                }

                mDirInfo = Directory.CreateDirectory(dir);
            }
            catch (System.Exception ex)
            {
				GameLog.HandleIOException(ex);
            }
        }

        public bool CopyTo(string dirDst, System.Action<FileInfo> action = null)
        {
            try
            {
                CopyDir(mDirInfo, new DirectoryInfo(dirDst), action);
                return true;
            }
            catch (System.Exception ex)
            {
				GameLog.HandleIOException(ex);
                return false;
            }
        }

        public delegate bool NeedCopy(FileInfo fileInfo);

        public bool CopyFrom(string dirSrc, NeedCopy needCopy = null)
        {
            if (!Directory.Exists(dirSrc))
            {
                return false;
            }

            try
            {
                CopyDir(new DirectoryInfo(dirSrc), mDirInfo, null, needCopy);
                return true;
            }
            catch (System.Exception ex)
            {
				GameLog.HandleIOException(ex);
                return false;
            }
        }

        static void CopyDir(DirectoryInfo dirSrc, DirectoryInfo dirDst, System.Action<FileInfo> action = null, NeedCopy needCopy = null)
        {
            //Profiler.BeginSample("sync:" + dirSrc.FullName + "->" + dirDst.FullName);
			FileInfo[] fileInfosSrc = dirSrc.GetFiles();
			DirectoryInfo[] subDirsSrc = dirSrc.GetDirectories();

            if ((fileInfosSrc == null || fileInfosSrc.Length == 0) && (subDirsSrc == null || subDirsSrc.Length == 0))
                return;

            if (!dirDst.Exists){
                dirDst.Create();
            }

            foreach (FileInfo fileInfoSrc in fileInfosSrc)
            {
                Profiler.BeginSample("copy:" + fileInfoSrc.FullName);

                if (needCopy == null || needCopy(fileInfoSrc))
                {
                    FileInfo newFileInfo = fileInfoSrc.CopyTo(Path.Combine(dirDst.FullName, fileInfoSrc.Name), true);

                    if (action != null)
                    {
                        action(newFileInfo);
                    }
                }

                Profiler.EndSample();
            }

            foreach (DirectoryInfo subDirSrc in subDirsSrc)
            {
                CopyDir(subDirSrc, new DirectoryInfo(Path.Combine(dirDst.FullName, subDirSrc.Name)), action, needCopy);
            }

            //Profiler.EndSample();
        }
    }
}