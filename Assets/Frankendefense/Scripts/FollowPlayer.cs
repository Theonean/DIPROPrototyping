using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
    MISSNOMER CLASS BUT IM NOT RENAMING IT AS THIS IS A PROTOTYPE

    Enemies now target the control zone and die when touching it.
*/

public class FollowPlayer : MonoBehaviour
{
    GameObject m_ControlZone;
    public float m_MoveSpeed = 20f;
    bool m_IsMoving = true;
    public AnimationCurve knockbackCurve;
    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        m_ControlZone = GameObject.Find("ControlZone");
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsMoving && m_ControlZone != null)
        {
            agent.SetDestination(m_ControlZone.transform.position);
        }
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

    public void SetMoveSpeed(float speed)
    {
        m_MoveSpeed = speed;
        //set speed on nav mesh agent
        agent.speed = speed;
    }
}
