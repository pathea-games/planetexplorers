using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntroRunner : MonoBehaviour
{
    MovieTexture mMovie;

    void Start()
    {
        mMovie = GetComponent<RawImage>().texture as MovieTexture;
		if (mMovie != null) {
			mMovie.Stop ();
			mMovie.Play ();

			AudioSource sound = gameObject.AddComponent<AudioSource>();
			sound.clip = mMovie.audioClip;
			sound.Stop();
			sound.Play();
		}
    }

    void Update()
    {
		if (mMovie != null && !mMovie.isPlaying)
        {
            EndMovie();
        }

        if (Input.GetKeyDown(KeyCode.Escape)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0)) //lz-2016.08.16  功能 #3111片头动画在现有按键跳过基础上增加鼠标左键跳过
        {
            EndMovie();
        }
    }

    void EndMovie()
    {
        if (movieEnd != null)
        {
            movieEnd();
        }
        enabled = false;
    }

    public delegate void MovieEnd();
    public static MovieEnd movieEnd;
}