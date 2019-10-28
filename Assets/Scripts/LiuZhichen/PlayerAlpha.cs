using UnityEngine;
using System.Collections;


public class PlayerAlpha : MonoBehaviour
{
    protected float targetAlpha;
    protected GameObject gameObj;
    public bool isDormant;
    protected SkinnedMeshRenderer render;
    protected Shader[] originalShaders;
    protected Shader transparent;

    public virtual void Start()
    {
        render = null;
        isDormant = true;
        gameObj = this.gameObject;

        render = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

        originalShaders = new Shader[render.materials.Length];

        for (int i = 0; i < render.materials.Length; i++)
        {
            originalShaders[i] = render.materials[i].shader;

        }

        transparent = Shader.Find("Transparent/Bumped Diffuse");
    }

    public float GetCurrentAlpha()
    {
        if (render == null)
            return 0.0f;
        return render.material.color.a;
    }

    public void setTargetAlpha(float _targetAlpha)
    {
        if (Mathf.Abs(_targetAlpha - targetAlpha) < PETools.PEMath.Epsilon)
            return;

        if (isDormant == true)
        {
            for (int i = 0; i < render.materials.Length; i++)
            {
                render.materials[i].shader = transparent;
            }
        }

        isDormant = false;
        targetAlpha = _targetAlpha;
    }

    protected virtual void UpdateRenderAlpha()
    {
        if (isDormant == true)
            return;

        float currentAlpha = render.material.color.a;
        if (Mathf.Abs(currentAlpha - targetAlpha) <= 0.01f)
        {
            currentAlpha = targetAlpha;
        }
        else if (currentAlpha < targetAlpha)
        {
            currentAlpha += 0.01f;
        }
        else
        {
            currentAlpha -= 0.01f;
        }

        for (int i = 0; i < render.materials.Length; i++)
        {
            Color oldColor = render.materials[i].color;
            render.materials[i].color = new Color(oldColor.r, oldColor.g, oldColor.b, currentAlpha);
        }

        if (Mathf.Abs(currentAlpha - 1.0f) <= 0.0001f)
        {
            for (int i = 0; i < render.materials.Length; i++)
            {
                //render.materials[i].shader = Shader.Find("Bumped Diffuse");

                render.materials[i].shader = originalShaders[i];
            }
            isDormant = true;
        }

    }

    public virtual void Update()
    {
        UpdateRenderAlpha();
    }
}
