using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPeriscopeButton : ACInteractable
{
    private Periscope periscope;

    protected override void Start()
    {
        base.Start();
        periscope = Periscope.Instance;
    }

    public override void OnStartInteract()
    {
        base.OnStartInteract();
        periscope.EndActive();
    }
}
