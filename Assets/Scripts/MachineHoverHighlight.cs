using System;
using UnityEngine;

public class MachineHoverHighlight : MonoBehaviour
{
    public Camera cam;
    public float hoverDistance = 500f;

    private GameObject currentTarget;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, hoverDistance);

        if (hits == null || hits.Length == 0)
        {
            ClearOutline();
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        GameObject target = null;

        // Prefer ClickProxy objects
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.name.StartsWith("ClickProxy_"))
                continue;

            ClickProxyTarget proxyTarget = hit.collider.GetComponent<ClickProxyTarget>();

            if (proxyTarget != null && proxyTarget.target != null)
            {
                target = proxyTarget.target;
                break;
            }
        }

        // Fallback: real machine object
        if (target == null)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.GetComponentInParent<RelationInfo>() != null)
                    continue;

                GeneratedComponentTag_PrefabTest tag =
                    hit.collider.GetComponentInParent<GeneratedComponentTag_PrefabTest>();

                if (tag == null)
                    continue;

                target = tag.gameObject;
                break;
            }
        }

        if (target == null)
        {
            ClearOutline();
            return;
        }

        ShowOutline(target);
    }

    private void ShowOutline(GameObject target)
    {
        if (target == null)
        {
            ClearOutline();
            return;
        }

        if (currentTarget == target)
            return;

        ClearOutline();

        currentTarget = target;

        MeshRenderer[] renderers = currentTarget.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.GetComponent<TextMesh>() != null)
                continue;

            SimpleMeshOutline outline = renderer.gameObject.GetComponent<SimpleMeshOutline>();

            if (outline == null)
                outline = renderer.gameObject.AddComponent<SimpleMeshOutline>();

            outline.enabled = true;
        }
    }

    private void ClearOutline()
    {
        if (currentTarget == null)
            return;

        SimpleMeshOutline[] outlines =
            currentTarget.GetComponentsInChildren<SimpleMeshOutline>(true);

        foreach (SimpleMeshOutline outline in outlines)
        {
            if (outline != null)
                outline.enabled = false;
        }

        currentTarget = null;
    }
}