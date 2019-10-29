using UnityEngine;
using System.Collections;

public class EDRunner : MonoBehaviour 
{
	MovieTexture mMovie;
	// Use this for initialization
	void Start()
	{
		mMovie = GetComponent<UITexture>().mainTexture as MovieTexture;
		if (mMovie != null) 
		{
			mMovie.Stop ();
			mMovie.Play ();
			
			AudioSource sound = gameObject.AddComponent<AudioSource>();
			sound.clip = mMovie.audioClip;
			sound.Stop();
			sound.Play();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(PeInput.Get(PeInput.LogicFunction.OptionsUI) || !mMovie.isPlaying)
		{
            Application.LoadLevel("GameCredits");
        }
	}
}
