using UnityEngine;

public class MachineSelectionOutlineManager : MonoBehaviour
{
    public static MachineSelectionOutlineManager Instance;

    private GameObject selectedTarget;

    void Awake()
    {
        Instance = this;
    }

    public void Select(GameObject target)
    {
        if (target == null)
        {
            ClearSelection();
            return;
        }

        if (selectedTarget == target)
            return;

        ClearSelection();

        selectedTarget = target;

        MeshRenderer[] renderers = selectedTarget.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.GetComponent<TextMesh>() != null)
                continue;

            SelectedMeshOutline outline =
                renderer.gameObject.GetComponent<SelectedMeshOutline>();

            if (outline == null)
                outline = renderer.gameObject.AddComponent<SelectedMeshOutline>();

            outline.enabled = true;
        }
    }

    public void ClearSelection()
    {
        if (selectedTarget == null)
            return;

        SelectedMeshOutline[] outlines =
            selectedTarget.GetComponentsInChildren<SelectedMeshOutline>(true);

        foreach (SelectedMeshOutline outline in outlines)
        {
            if (outline != null)
                outline.enabled = false;
        }

        selectedTarget = null;
    }
}