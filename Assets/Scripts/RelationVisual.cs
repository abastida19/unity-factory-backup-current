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
    public TextMesh typeLabel;
    public string relationType;
    public float relationWeight;

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

    public void HideLabels()
    {
        if (startLabel != null)
            startLabel.gameObject.SetActive(false);

        if (endLabel != null)
            endLabel.gameObject.SetActive(false);

        if (typeLabel != null)
            typeLabel.gameObject.SetActive(false);
    }

    public void ShowDirectionLabels()
    {
        if (startLabel != null)
        {
            startLabel.text = "FROM:\n" + CleanName(from);
            startLabel.gameObject.SetActive(true);
        }

        if (endLabel != null)
        {
            endLabel.text = "TO:\n" + CleanName(to);
            endLabel.gameObject.SetActive(true);
        }
    }

    private string CleanName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        return value
            .Replace("_station", "")
            .Replace("_farm", "")
            .Replace("_skid", "")
            .Replace("_plant", "")
            .Replace("_module", "")
            .Replace("_", " ");
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

        if (blackMaterial == null)
            BuildMaterials();

        if (segA != null)
            segA.sharedMaterial = blackMaterial;

        ShowDirectionLabels();
        ShowTypeLabel();
    }

    public void ShowTypeLabel()
    {
        if (typeLabel == null)
            return;

        typeLabel.text = GetRelationTypeDisplayText();
        typeLabel.gameObject.SetActive(true);
    }

    private string GetRelationTypeDisplayText()
    {
        string readableType = GetReadableRelationType(relationType);

        if (relationType == "keep_out")
        {
            return readableType + "\nSeparation required";
        }

        if (relationType == "material_flow")
        {
            return readableType + "\nWeight " + relationWeight + "/10";
        }

        return readableType + "\nWeight " + relationWeight + "/10";
    }

    private string GetReadableRelationType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "RELATION";

        switch (value)
        {
            case "material_flow":
                return "MATERIAL FLOW";

            case "keep_out":
                return "KEEP OUT";

            case "distance":
                return "DISTANCE CONSTRAINT";

            case "adjacency_preferred":
                return "ADJACENCY PREFERRED";

            case "same_line":
                return "SAME LINE";

            case "order_along_axis":
                return "ORDER ALONG AXIS";

            default:
                return value.Replace("_", " ").ToUpper();
        }
    }
}