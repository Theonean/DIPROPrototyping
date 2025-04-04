using UnityEngine;

public class RadarPulseButton : ACTimedButton
{
    public float chargeSuccededModifier = 1.5f;
    public float chargeFailedModifier = 0.8f;

    

    public override void OnChargeSucceeded()
    {
        base.OnChargeSucceeded();
        Radar.Instance.Pulse(chargeSuccededModifier);
    }

    public override void OnChargeFailed()
    {
        base.OnChargeFailed();
        Radar.Instance.Pulse(chargeFailedModifier);
    }

    public override void OnChargeTimeElapsed() {
        base.OnChargeTimeElapsed();
        Radar.Instance.Pulse(1f);
    }
}
