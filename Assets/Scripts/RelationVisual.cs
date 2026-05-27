using UnityEngine;

public class RelationVisual : MonoBehaviour
{
    public string from;
    public string to;

    public Renderer segA;
    public Renderer segB;
    public Renderer segC;

    public TextMesh startLabel;
    public TextMesh endLabel;

    [SerializeField] private Color relationColor = Color.white;

    private Material relationMaterial;
    private Material blackMaterial;
    private Material hoverMaterial;
    private bool isHovering = false;

    public void Setup(string fromNode, string toNode, Color color)
    {
        from = fromNode;
        to = toNode;
        relationColor = color;

        BuildMaterials();
        ResetToRelationColor();
    }

    private void Awake()
    {
        BuildMaterials();
    }

    private void BuildMaterials()
    {
        relationMaterial = CreateMaterial(relationColor);
        blackMaterial = CreateMaterial(Color.black);
        Color hoverColor = Color.Lerp(relationColor, Color.white, 0.45f);
        hoverMaterial = CreateMaterial(hoverColor);
    }

    public void ShowForNode(string selectedNode)
    {
        BuildMaterials();
        ResetToRelationColor();

        // If the clicked node is the FROM side, black segment is Seg_A
        if (selectedNode == from && segA != null)
            segA.sharedMaterial = blackMaterial;

        // If the clicked node is the TO side, black segment is Seg_C
        if (selectedNode == to && segC != null)
            segC.sharedMaterial = blackMaterial;
    }

    public void ResetToRelationColor()
    {
        if (relationMaterial == null)
            BuildMaterials();

        if (segA != null) segA.sharedMaterial = relationMaterial;
        if (segB != null) segB.sharedMaterial = relationMaterial;
        if (segC != null) segC.sharedMaterial = relationMaterial;
    }

    public void SetHover(bool hover)
    {
        if (isHovering == hover)
            return;

        isHovering = hover;

        if (hover)
            ApplyHoverColor();
        else
            ResetToRelationColor();
    }

    private void ApplyHoverColor()
    {
        if (hoverMaterial == null)
            return;

        if (segA != null) segA.sharedMaterial = hoverMaterial;
        if (segB != null) segB.sharedMaterial = hoverMaterial;
        if (segC != null) segC.sharedMaterial = hoverMaterial;
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
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
        return mat;
    }

    public void ShowAsSelectedRelation()
    {
        ResetToRelationColor();

        if (segA != null)
            segA.sharedMaterial = blackMaterial;
    }
}