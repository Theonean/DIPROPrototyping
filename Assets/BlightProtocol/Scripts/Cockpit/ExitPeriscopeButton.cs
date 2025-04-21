using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPeriscopeButton : ACButton
{
    private Periscope periscope;

    void Start()
    {
        periscope = Periscope.Instance;
    }

    public override void OnStartInteract()
    {
        base.OnStartInteract();
        periscope.EndActive();
    }
}
