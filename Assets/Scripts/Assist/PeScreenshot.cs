using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace Pathea
{
    public class PeScreenshot : MonoBehaviour
    {
        void Update()
        {
			if (PeInput.Get(PeInput.LogicFunction.ScreenCapture))
            {
                CaptureToFile();
            }
        }

        static void CaptureToFile()
        {
            Texture2D tex = GetTex();

            if (null == tex)
            {
                return;
            }

            SaveTex(tex);
        }

        static void SaveTex(Texture2D tex)
        {
			try{
	            string filePath = Application.dataPath + "/ScreenCapture";
	            if (!Directory.Exists(filePath))
	            {
	                Directory.CreateDirectory(filePath);
	            }

	            string fileName = filePath + "/PE_"
	                + DateTime.Now.ToShortDateString().Replace(" ", "").Replace("/", ".") + "_"
	                + DateTime.Now.ToShortTimeString().Replace(":", ".").Replace(" ", "") + "."
	                + DateTime.Now.Second.ToString()
	                + ".png";
	            File.WriteAllBytes(fileName, tex.EncodeToPNG());
			}catch(Exception e){
				Debug.Log("Failed to save screen capture! "+e.ToString());
			}
        }

        public static Texture2D GetTex()
        {
            Camera camera = Camera.main;

            camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 32);
            Texture2D image = PhotoStudio.RTImage(camera);
            camera.targetTexture = null;
            return image;
        }
    }
}