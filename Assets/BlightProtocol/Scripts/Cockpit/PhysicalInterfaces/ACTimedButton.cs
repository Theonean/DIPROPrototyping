// ACTimedButton.cs - Pure abstract base class
using UnityEngine;
using System.Collections;

public abstract class ACTimedButton : ACButton
{
    public float chargeTime = 1f;
    public bool isCharging = false;
    public Vector2 sweetSpotMinMax = new Vector2(0.6f, 0.8f);

    public override void OnStartInteract()
    {
        if (isCharging)
        {
            bool success = CheckSweetSpot();
            HandleChargeResult(success ? 1 : 2);
            isCharging = false;
        }
        else
        {
            StartCoroutine(ChargeRoutine());
            isCharging = true;
        }
    }

    private IEnumerator ChargeRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < chargeTime)
        {
            float progress = elapsedTime / chargeTime;
            UpdateChargeProgress(progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        UpdateChargeProgress(1f);
        HandleChargeResult(0);
        isCharging = false;
    }

    protected virtual bool CheckSweetSpot()
    {
        bool inSweetSpot = GetCurrentProgress() > sweetSpotMinMax.x && GetCurrentProgress() < sweetSpotMinMax.y;
        return inSweetSpot;
    }
    protected abstract void UpdateChargeProgress(float progress);
    protected abstract void HandleChargeResult(int result);
    protected abstract float GetCurrentProgress();
}