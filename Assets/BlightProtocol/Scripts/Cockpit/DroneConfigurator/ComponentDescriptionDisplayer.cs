using TMPro;
using UnityEngine;

public class ComponentDescriptionDisplayer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI frontInfo, bodyInfo, propInfo;

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
}
