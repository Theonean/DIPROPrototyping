using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class Button : ACInteractable
{
    public UnityEvent<Button> OnPressed;
    public UnityEvent<Button> OnReleased;
    public bool isToggle = false;
    public bool isPressed = false;
    private Renderer _renderer;
    private Color defaultColor;
    [SerializeField] private Color pressedColor = Color.blue;
    private MaterialPropertyBlock _propertyBlock; // Cached property block

    protected void Awake()
    {
        base.Start();
        _renderer = GetComponentInChildren<Renderer>();
        defaultColor = _renderer.material.GetColor("_Color");

        // Initialize once and reuse
        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetColor("_Color", defaultColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public override void OnStartInteract()
    {
        base.OnStartInteract();
        if (isToggle)
        {
            SetPressed(!isPressed);
        }
        else
        {
            OnPressed.Invoke(this);
        }
    }

    public void SetPressed(bool pressed)
    {
        if (pressed)
        {
            isPressed = true;
            _propertyBlock.SetColor("_Color", pressedColor);
            _renderer.SetPropertyBlock(_propertyBlock);
            OnPressed.Invoke(this);
        }
        else
        {
            isPressed = false;
            _propertyBlock.SetColor("_Color", defaultColor);
            _renderer.SetPropertyBlock(_propertyBlock);
            OnReleased.Invoke(this);
        }
    }
}
