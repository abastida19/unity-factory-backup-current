using UnityEngine;
using TMPro;

public class RelationInfoPanel : MonoBehaviour
{
    public static RelationInfoPanel Instance;

    public GameObject panel;
    public TMP_Text infoText;

    void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(RelationInfo info)
    {
        if (panel == null || infoText == null || info == null)
            return;

        infoText.text = info.GetDisplayText();
        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}