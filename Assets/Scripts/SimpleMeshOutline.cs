using UnityEngine;

[DisallowMultipleComponent]
public class SimpleMeshOutline : MonoBehaviour
{
    public Color outlineColor = new Color(1f, 0.55f, 0f, 0.35f);
    public float outlineScale = 1.008f;

    private GameObject outlineObject;

    void OnEnable()
    {
        CreateOutline();
    }

    void OnDisable()
    {
        RemoveOutline();
    }

    private void CreateOutline()
    {
        RemoveOutline();

        MeshFilter sourceMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = GetComponent<MeshRenderer>();

        if (sourceMeshFilter == null || sourceRenderer == null)
            return;

        GameObject outline = new GameObject("MeshOutline");
        outline.transform.SetParent(transform, false);
        outline.transform.localPosition = Vector3.zero;
        outline.transform.localRotation = Quaternion.identity;
        outline.transform.localScale = Vector3.one * outlineScale;

        MeshFilter mf = outline.AddComponent<MeshFilter>();
        mf.sharedMesh = sourceMeshFilter.sharedMesh;

        MeshRenderer mr = outline.AddComponent<MeshRenderer>();
        mr.sharedMaterial = CreateOutlineMaterial(outlineColor);

        outlineObject = outline;
    }

    private void RemoveOutline()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
            outlineObject = null;
        }
    }

    private Material CreateOutlineMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        mat.color = color;

        // Make it transparent/subtle
        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 1f);

        if (mat.HasProperty("_Blend"))
            mat.SetFloat("_Blend", 0f);

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        mat.renderQueue = 3000;

        return mat;
    }
}