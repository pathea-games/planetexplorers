using UnityEngine;
using System.Collections;

public class AiAlpha : MonoBehaviour
{
    public delegate void OnAlphaChangeCompleted(AiAlpha alpha, float dstAlpha);
    static float Accuracy = 0.05f;

    SkinnedMeshRenderer render;
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

    public void Fadein(float delayTime, float fadeTime)
    {
        if (render == null || transparent == null)
            return;

        for (int i = 0; i < render.materials.Length; i++)
        {
            Color oldColor = render.materials[i].color;
            render.materials[i].shader = transparent;
            render.materials[i].color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.0f);
        }

        ChangeAlphaToValue(1.0f, delayTime, fadeTime);
    }

    public void ChangeAlphaToValue(float dstAlpha, float delayTime = 0.0f, 
        float time = 2.0f, OnAlphaChangeCompleted alphaChange = null)
    {
        if (render == null) 
            return;

        if (Mathf.Abs(render.material.color.a - dstAlpha) < 0.1f)
            return;

        //ActivateTransparent(true);
        //targetAlpha = dstAlpha;
        StopAllCoroutines();
        StartCoroutine(AlphaUpdate(dstAlpha, delayTime, time, alphaChange));
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

    IEnumerator AlphaUpdate(float dstAlpha, float delayTime = 0.0f, float time = 2.0f, OnAlphaChangeCompleted alphaChange = null)
    {
        yield return new WaitForSeconds(delayTime);

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

        if (alphaChange != null)
            alphaChange(this, dstAlpha);

        if (Mathf.Abs(dstAlpha - 1.0f) <= Accuracy &&
            Mathf.Abs(render.material.color.a - 1.0f) <= Accuracy)
        {
            ActivateTransparent(false);
        }
    }

    void Awake()
    {
        if (transform.parent != null)
            render = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
        else
            render = transform.GetComponentInChildren<SkinnedMeshRenderer>();

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
