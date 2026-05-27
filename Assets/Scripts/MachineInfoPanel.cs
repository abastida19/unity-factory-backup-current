using UnityEngine;
using TMPro;

public class MachineInfoPanel : MonoBehaviour
{
    public static MachineInfoPanel Instance;

    public GameObject panel;
    public TMP_Text infoText;

    void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(GeneratedComponentTag_PrefabTest tag)
    {
        if (panel == null || infoText == null || tag == null)
            return;

        infoText.text =
            "<b>MACHINE DETAILS</b>\n\n" +
            "<b>Name:</b> " + Safe(tag.machine_name) + "\n" +
            "<b>ID:</b> " + Safe(tag.id) + "\n" +
            "<b>Type:</b> " + Safe(tag.type) + "\n" +
            "<b>Process step:</b> " + Safe(tag.process_step);

        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private string Safe(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Not specified";

        return value.Replace("_", " ");
    }
}
