using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    MISSNOMER CLASS BUT IM NOT RENAMING IT AS THIS IS A PROTOTYPE

    Enemies now target the control zone and die when touching it.
*/

public class FollowPlayer : MonoBehaviour
{
    GameObject m_ControlZone;
    [SerializeField]
    float m_MoveSpeed = 20f;
    bool m_IsMoving = true;
    public AnimationCurve knockbackCurve;

    // Start is called before the first frame update
    void Start()
    {
        m_ControlZone = GameObject.Find("ControlZone");
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsMoving)
        {
            transform.LookAt(m_ControlZone.transform);

            //If enemy is seen by camera.main or close to zone move normal speed otherwise move 5x speed
            if (IsVisibleToCamera() || Vector3.Distance(transform.position, m_ControlZone.transform.position) < 35f)
            {
                transform.position += transform.forward * m_MoveSpeed * Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward * (m_MoveSpeed * 5f) * Time.deltaTime;
            }
        }


        //transform.position = Vector3.MoveTowards(transform.position, m_ControlZone.transform.position, m_MoveSpeed * Time.deltaTime);
    }
    bool IsVisibleToCamera()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, GetComponentInChildren<Collider>().bounds);
    }

    public IEnumerator ApplyKnockback(Vector3 direction, float knockback)
    {
        m_IsMoving = false;
        float timer = 0f;
        float knockbackTime = 0.5f;
        while (timer < knockbackTime)
        {
            transform.position += direction * knockback * Time.deltaTime * knockbackCurve.Evaluate(timer / knockbackTime);
            timer += Time.deltaTime;
            yield return null;
        }
        m_IsMoving = true;
    }
}
