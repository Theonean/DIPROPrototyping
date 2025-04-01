using UnityEngine;

public class ShrapnelShotFront : ACRocketFront
{
    public GameObject shrapnelPrefab;
    public int shrapnelCount = 10;
    public float shrapnelSpreadAngle = 30f;
    public override void ActivateAbility(Collider collider)
    {
        Vector3 shrapnelDirection = parentRocket.shootingDirection.normalized;
        float angleStep = shrapnelSpreadAngle / (shrapnelCount - 1);
        for (int i = 0; i < shrapnelCount; i++)
        {
            float angle = -shrapnelSpreadAngle / 2 + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 newDirection = rotation * shrapnelDirection;

            ShrapnelShot shrapnel = Instantiate(shrapnelPrefab, rocketTransform.position, Quaternion.identity).GetComponent<ShrapnelShot>();
            shrapnel.Activate(newDirection);
        }
    }
}
