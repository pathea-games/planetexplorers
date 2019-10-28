using UnityEngine;
using System.Collections;

public class PETransparent : MonoBehaviour
{
    public delegate void OnAlphaChangeCompleted(AiAlpha alpha, float dstAlpha);
    static float Accuracy = 0.05f;

    Renderer render;
    Shader[] originalShaders;
    Shader transparent;

    bool isDormant;
    //float targetAlpha;

    public float CurrentAlphaValue
    {
        get
        {
            if (render == null)
                return 0.0f;
            else
                return render.material.color.a;
        }
    }

    public void Fadein(float fadeTime)
    {
        if (render == null || transparent == null)
            return;

        for (int i = 0; i < render.materials.Length; i++)
        {
            Color oldColor = render.materials[i].color;
            render.materials[i].shader = transparent;
            render.materials[i].color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.0f);
        }

        ChangeAlphaToValue(1.0f, fadeTime);
    }

    public void Fadeout(float fadeTime)
    {
        if (render == null || transparent == null)
            return;

        if(render != null)
            ChangeAlphaToValue(0.0f, fadeTime);
    }

    public void ChangeAlphaToValue(float dstAlpha, float time = 2.0f)
    {
        if (render == null)
            return;

        if (Mathf.Abs(render.material.color.a - dstAlpha) < 0.1f)
            return;

        //ActivateTransparent(true);
        //targetAlpha = dstAlpha;
        StopAllCoroutines();
        StartCoroutine(AlphaUpdate(dstAlpha, time));
    }

    void ActivateTransparent(bool isTransparent)
    {
        if (isTransparent)
        {
            if (isDormant)
            {
                isDormant = false;

                for (int i = 0; i < render.materials.Length; i++)
                {
                    render.materials[i].shader = transparent;
                }
            }
        }
        else
        {
            if (!isDormant)
            {
                isDormant = true;

                for (int i = 0; i < render.materials.Length; i++)
                {
                    render.materials[i].shader = originalShaders[i];
                }
            }
        }
    }

    IEnumerator AlphaUpdate(float dstAlpha, float time = 2.0f)
    {
        ActivateTransparent(true);

        float startTime = Time.time;
        float startAlpha = render.material.color.a;
        float alphaValue = -1000.0f;

        while (render != null && Mathf.Abs(alphaValue - dstAlpha) > Accuracy)
        {
            alphaValue = Mathf.Lerp(startAlpha, dstAlpha, (Time.time - startTime) / time);
            for (int i = 0; i < render.materials.Length; i++)
            {
                Color oldColor = render.materials[i].color;
                render.materials[i].color = new Color(oldColor.r, oldColor.g, oldColor.b, alphaValue);
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (Mathf.Abs(dstAlpha - 1.0f) <= Accuracy &&
            Mathf.Abs(render.material.color.a - 1.0f) <= Accuracy)
        {
            ActivateTransparent(false);
        }
    }

    void Awake()
    {
        render = transform.GetComponentInChildren<Renderer>();

        if (render == null)
            return;

        isDormant = true;

        originalShaders = new Shader[render.materials.Length];

        for (int i = 0; i < render.materials.Length; i++)
        {
            originalShaders[i] = render.materials[i].shader;
        }

        transparent = Shader.Find("Transparent/Bumped Diffuse");
    }

}
