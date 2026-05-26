using UnityEngine;

public class RelationInfo : MonoBehaviour
{
    public string from;
    public string to;
    public string relation;
    public float weight;
    public string note;

    public string GetDisplayText()
    {
        string safeNote = string.IsNullOrWhiteSpace(note) ? "No additional note." : note;

        return
            "<b>RELATION DETAILS</b>\n\n" +
            "<b>From:</b> " + from + "\n" +
            "<b>To:</b> " + to + "\n" +
            "<b>Relation:</b> " + relation + "\n" +
            "<b>Weight:</b> " + weight + "/10\n\n" +
            "<b>Note:</b>\n" + safeNote;
    }
}