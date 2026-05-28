using System;
using System.Collections.Generic;
using UnityEngine;

public class RelationManager : MonoBehaviour
{
    public static RelationManager Instance;

    [Serializable]
    public class RelationRecord
    {
        public string from;
        public string to;
        public GameObject relationObject;
    }

    public List<RelationRecord> relationRecords = new List<RelationRecord>();

    private Dictionary<string, List<GameObject>> relationsByNode =
        new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        Instance = this;
        BuildDictionary();
        HideAllRelations();
    }

    public void RegisterRelation(string from, string to, GameObject relationObj)
    {
        relationRecords.Add(new RelationRecord
        {
            from = from,
            to = to,
            relationObject = relationObj
        });

        if (relationObj != null)
            relationObj.SetActive(false);
    }

    private void BuildDictionary()
    {
        relationsByNode.Clear();

        foreach (var record in relationRecords)
        {
            if (record == null || record.relationObject == null)
                continue;

            AddRelation(record.from, record.relationObject);
            AddRelation(record.to, record.relationObject);
        }
    }

    private void AddRelation(string nodeId, GameObject obj)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return;

        if (!relationsByNode.ContainsKey(nodeId))
            relationsByNode[nodeId] = new List<GameObject>();

        relationsByNode[nodeId].Add(obj);
    }

    public void ShowRelationsForNode(string nodeId)
    {
        HideAllRelations();

        if (!relationsByNode.ContainsKey(nodeId))
        {
            Debug.Log("No relations found for node: " + nodeId);
            return;
        }

        foreach (var rel in relationsByNode[nodeId])
        {
            if (rel == null)
                continue;

            rel.SetActive(true);

            RelationVisual visual = rel.GetComponent<RelationVisual>();
            if (visual != null)
                visual.ShowForNode(nodeId);
        }

        Debug.Log("Showing relations for: " + nodeId);
    }

    public void HideAllRelations()
    {
        foreach (var record in relationRecords)
        {
            if (record == null || record.relationObject == null)
                continue;

            RelationVisual visual = record.relationObject.GetComponent<RelationVisual>();
            if (visual != null)
            {
                visual.ResetToRelationColor();
                visual.HideLabels();
            }

            RelationFlowAnimator flow = record.relationObject.GetComponent<RelationFlowAnimator>();
            if (flow != null)
                flow.StopFlow();

            record.relationObject.SetActive(false);
        }
    }

    public void ShowOnlyRelation(GameObject selectedRelation)
    {
        HideAllRelations();

        if (selectedRelation == null)
            return;

        selectedRelation.SetActive(true);

        RelationVisual visual = selectedRelation.GetComponent<RelationVisual>();
        if (visual != null)
            visual.ShowAsSelectedRelation();

        RelationFlowAnimator flow = selectedRelation.GetComponent<RelationFlowAnimator>();
        if (flow != null)
            flow.StartFlow();

        Debug.Log("Showing only selected relation: " + selectedRelation.name);
    }
}