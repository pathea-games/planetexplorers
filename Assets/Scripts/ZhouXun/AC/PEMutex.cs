using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

public class PEMutex : MonoBehaviour
{
	private static Mutex mutex = null;
	private static string s_MutexStr = "Planet Explorers Instance";
	
	public static bool Errored = false;
	
	void Awake ()
	{
		DontDestroyOnLoad(this);
	}
	
	// Use this for initialization
	void Start ()
	{
		mutex = null;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Time.frameCount == 100 )
		{
			try
			{
				mutex = new Mutex (true, s_MutexStr);
				if ( mutex != null )
					Debug.Log("MUTEX Created.");
				else
					Debug.LogWarning("Unable to create MUTEX");
			}
			catch (Exception)
			{
				Debug.LogWarning("Unable to create MUTEX");
			}
		}
	}
	
	void OnDestroy()
	{
		try
		{
			if ( mutex != null )
			{
				mutex.Close();
				mutex = null;
				Debug.Log("MUTEX Deleted.");
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Unable to close MUTEX");
		}

		if (!Application.isEditor)
		{
			//brutal force quit
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}
	}
}
