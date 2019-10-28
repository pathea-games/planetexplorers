using UnityEngine;
using System.Collections.Generic;

public class FadeInOut : MonoBehaviour
{
    Dictionary<Renderer, Material[]> rendererDic = new Dictionary<Renderer, Material[]>(1);

    float currentAlpha = 1.0f;
    float targetAlpha;
    bool isDormant = true;

    public bool ChangeAlphaToValue(float dstAlpha)
    {
        if (Mathf.Abs(currentAlpha - dstAlpha) < 0.1f)
        {
            isDormant = true;
            return true;
        }

        isDormant = false;
        targetAlpha = dstAlpha;

        return false;
    }

    void Update()
    {
        if (isDormant)
        {
            return;
        }

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * 5.0f);

        foreach (KeyValuePair<Renderer, Material[]> iter in rendererDic)
        {
            if (null == iter.Key)
            {
                continue;
            }

            for (int i = 0; i < iter.Key.materials.Length; i++)
            {
                if (iter.Value.Length <= i)
                {
                    Debug.LogError(gameObject.name + "'s fadeinout error.");
                    break;
                }

                if (null == iter.Value[i])
                {
                    continue;
                }

                Color oldColor = Color.white;
                if (iter.Value[i].HasProperty("_Color"))
                {
                    oldColor = iter.Value[i].color;
                }

                oldColor.a = oldColor.a * currentAlpha;
                iter.Key.materials[i].color = oldColor;
            }
        }
    }

    bool InitRenderer()
    {
        Renderer[] rendererArray = transform.GetComponentsInChildren<Renderer>();
        if (rendererArray.Length <= 0)
        {
            Debug.LogError("no render to fade in or out");

            return false;
        }

        Shader transparent = Shader.Find("Transparent/Bumped Diffuse");

        foreach (Renderer renderer in rendererArray)
        {
            if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
            {
                rendererDic.Add(renderer, renderer.materials);
                int matLength = renderer.materials.Length;
                Material[] matArray = new Material[matLength];

                for(int i = 0; i < matLength; i++)
                {
                    matArray[i] = new Material(renderer.materials[i]);

                    matArray[i].shader = transparent;
                }

                renderer.materials = matArray;
            }
        }

        return true;
    }

    void RestoreRenderer()
    {
        foreach (KeyValuePair<Renderer, Material[]> iter in rendererDic)
        {
            iter.Key.materials = iter.Value;
        }
    }

    void Awake()
    {
        InitRenderer();
    }

    void OnDestroy()
    {
        RestoreRenderer();
    }
}
