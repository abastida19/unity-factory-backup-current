using System;
using UnityEngine;

public class MachineClickManager : MonoBehaviour
{
    public Camera cam;
    public float clickDistance = 500f;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, clickDistance);

        if (hits == null || hits.Length == 0)
        {
            ClearSelection();
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // 1. FIRST PRIORITY: invisible click proxies
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.name.StartsWith("ClickProxy_"))
                continue;

            GeneratedComponentTag_PrefabTest tag =
                hit.collider.GetComponent<GeneratedComponentTag_PrefabTest>();

            if (tag == null)
                continue;

            SelectMachine(tag);
            return;
        }

        // 2. SECOND PRIORITY: relation lines
        foreach (RaycastHit hit in hits)
        {
            RelationInfo relationInfo =
                hit.collider.GetComponentInParent<RelationInfo>();

            if (relationInfo == null)
                continue;

            Debug.Log(relationInfo.GetDisplayText());

            if (RelationManager.Instance != null)
                RelationManager.Instance.ShowOnlyRelation(relationInfo.gameObject);

            if (RelationInfoPanel.Instance != null)
                RelationInfoPanel.Instance.Show(relationInfo);

            if (MachineInfoPanel.Instance != null)
                MachineInfoPanel.Instance.Hide();
            else
                Debug.LogWarning("RelationInfoPanel.Instance is null. Is the Canvas in the scene?");

            return;
        }

        // 3. THIRD PRIORITY: normal machine colliders
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponentInParent<RelationInfo>() != null)
                continue;

            GeneratedComponentTag_PrefabTest tag =
                hit.collider.GetComponentInParent<GeneratedComponentTag_PrefabTest>();

            if (tag == null)
                tag = hit.collider.GetComponentInChildren<GeneratedComponentTag_PrefabTest>();

            if (tag == null)
                continue;

            SelectMachine(tag);
            return;
        }

        ClearSelection();
    }

    private void SelectMachine(GeneratedComponentTag_PrefabTest tag)
    {
        if (tag == null)
            return;

        Debug.Log("Clicked machine: " + tag.id);

        if (RelationInfoPanel.Instance != null)
            RelationInfoPanel.Instance.Hide();

        if (MachineInfoPanel.Instance != null)
            MachineInfoPanel.Instance.Show(tag);

        if (RelationManager.Instance != null)
            RelationManager.Instance.ShowRelationsForNode(tag.id);
        else
            Debug.LogWarning("RelationManager.Instance is null.");
    }

    private void ClearSelection()
    {
        if (RelationInfoPanel.Instance != null)
            RelationInfoPanel.Instance.Hide();

        if (RelationManager.Instance != null)
            RelationManager.Instance.HideAllRelations();

        if (MachineInfoPanel.Instance != null)
            MachineInfoPanel.Instance.Hide();
    }
}