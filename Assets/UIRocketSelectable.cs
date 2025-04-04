using UnityEngine;

public class UIRocketSelectable : MonoBehaviour
{
    private UISelectedRocketManager parentManager;
    private bool isSelected = false;
    private void Start()
    {
        parentManager = GetComponentInParent<UISelectedRocketManager>();
    }

    private void Update()
    {
        if(!isSelected) return;

        if (Input.GetMouseButtonUp(0))
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.material.color = Color.white;
        }
    }
    private void OnMouseEnter()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.material.color = Color.yellow;
    }
    private void OnMouseExit()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.material.color = Color.white;
    }
    void OnMouseDown()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.material.color = Color.green;
        parentManager.SetSelectedRocket(GetComponent<Rocket>());
    }
}
