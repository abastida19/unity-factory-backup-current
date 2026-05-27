using System;
using UnityEngine;

public class RelationHoverHighlight : MonoBehaviour
{
    public Camera cam;
    public float hoverDistance = 500f;

    private RelationVisual currentVisual;

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
            ClearHover();
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RelationVisual hoveredVisual = null;

        foreach (RaycastHit hit in hits)
        {
            RelationInfo info = hit.collider.GetComponentInParent<RelationInfo>();

            if (info == null)
                continue;

            hoveredVisual = info.GetComponent<RelationVisual>();
            break;
        }

        if (hoveredVisual == null)
        {
            ClearHover();
            return;
        }

        if (currentVisual == hoveredVisual)
            return;

        ClearHover();

        currentVisual = hoveredVisual;
        currentVisual.SetHover(true);
    }

    private void ClearHover()
    {
        if (currentVisual != null)
            currentVisual.SetHover(false);

        currentVisual = null;
    }
}
