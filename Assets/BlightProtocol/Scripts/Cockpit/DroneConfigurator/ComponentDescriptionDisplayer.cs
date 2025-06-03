using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComponentDescriptionDisplayer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI frontInfo, bodyInfo, propInfo;
    [SerializeField] Image frontLock, bodyLock, propLock;

    public void SetText(RocketComponentType type, string text)
    {
        switch (type)
        {
            case RocketComponentType.FRONT:
                frontInfo.text = text;
                break;
            case RocketComponentType.BODY:
                bodyInfo.text = text;
                break;
            case RocketComponentType.PROPULSION:
                propInfo.text = text;
                break;
        }
    }

    public void ShowText(bool show)
    {
        frontInfo.enabled = show;
        bodyInfo.enabled = show;
        propInfo.enabled = show;
    }

    public void ShowLock(RocketComponentType type, bool show)
    {
        switch (type)
        {
            case RocketComponentType.FRONT:
                frontLock.enabled = show;
                break;
            case RocketComponentType.BODY:
                bodyLock.enabled = show;
                break;
            case RocketComponentType.PROPULSION:
                propLock.enabled = show;
                break;
        }
    }
}
