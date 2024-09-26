using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHandler : MonoBehaviour
{
    enum LegState
    {
        ATTACHED, // The leg is attached to the player.
        CLICKED, // The leg is clicked on and is waiting for next click to fly away.
        FLYING, // The leg is flying away.
        DETACHED, // The leg is detached from the player and lying on the floor (could be clicked again for attack).
        RETURNING // The leg is returning to the player.
    }

    LegState m_LegState = LegState.ATTACHED;
    private bool isSpinning = false; //If the leg is currently spinning (one form of attack). to see all forms of attack, go to public function isAttacking
    Vector3 m_DistanceToCore; //local position the leg spawned in, used for returning the leg to the player.
    Quaternion m_InitialRotation; //local rotation the leg spawned in, used for returning the leg to the player.
    Vector3 m_TargetPosition; //position the leg will fly to when in FLYING state.
    GameObject m_mouseTarget; //Tracker for the mouse position when the leg is in clicked state to show the player where the leg will fly to.
    float m_InteractionCooldown = 0.5f; //Cooldown for the leg to be interacted with again after being clicked NOT the attack cooldown
    float m_InteractionTimer = 0f; //Timer for the cooldown -> should fix bug where during one click four clicks are registered on a leg 
    float m_SpinAttackDuration = 3f; //Duration of the spin attack
    float m_SpinAttackSpeed = 2000000f; //rotation Speed of the spin attack
    Camera m_Camera;
    PlayerCore core;
    void Awake()
    {
        //Find parent "core" object and subscribe to the return event
        core = FindObjectOfType<PlayerCore>();
        core.returnLegs.AddListener(ReturnToCore);

        m_Camera = Camera.main;
        m_DistanceToCore = transform.position - core.transform.position;
        m_InitialRotation = transform.rotation;

        //Generate mouseTarget object
        m_mouseTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(m_mouseTarget.GetComponent<SphereCollider>());
        m_mouseTarget.GetComponent<MeshRenderer>().material.color = Color.red;
        m_mouseTarget.SetActive(false);
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = m_Camera.ScreenPointToRay(mousePosition);

        if (m_InteractionTimer > 0)
        {
            m_InteractionTimer -= Time.deltaTime;
        }
        //Handle Input ~ MOVE TO HANDLER LATER
        //Leftclick to interact with the leg
        else if (Input.GetMouseButtonDown(0))
        {
            //Check if raycast even hits anything
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Checkk for SELF-HIT
                if (hit.collider.gameObject == gameObject)
                {
                    m_InteractionTimer = m_InteractionCooldown;
                    switch (m_LegState)
                    {
                        case LegState.ATTACHED:
                            //Leg starts waiting for next click to fly away and starts highlighting current mouse position, rightclick to cancel.
                            m_LegState = LegState.CLICKED;
                            m_mouseTarget.SetActive(true);
                            break;
                        case LegState.CLICKED:
                            //If the leg is clicked again while waiting for next click, treat it as a cancel and revert to attached state.
                            m_LegState = LegState.ATTACHED;
                            m_mouseTarget.SetActive(false);
                            break;
                        case LegState.FLYING:
                        case LegState.DETACHED:
                            //If clicked when lying on the floor and not alread spinning, do a spin attack
                            if (!isSpinning) { StartCoroutine(SpinningCoroutine()); }
                            break;
                    }
                }
                //If this leg is waiting for a click and click is somewhere else, start flying to the clicked position.
                else if (m_LegState == LegState.CLICKED) //also don't allow clicking on other legs so they wont stack
                {
                    //If another "Leg" is clicked, return to avoid stacking legs
                    if (hit.collider.gameObject.tag == "Leg")
                    {
                        m_LegState = LegState.ATTACHED;
                        m_mouseTarget.SetActive(false);
                    }
                    else
                    {
                        //If a "safe" position is clicked while waiting for next click, start flying to the mouse position.
                        m_LegState = LegState.FLYING;
                        m_mouseTarget.SetActive(false);
                        m_TargetPosition = new Vector3(hit.point.x, gameObject.transform.position.y, hit.point.z); //Keep leg height
                    }
                }
            }
        }
        //Rightclick to cancel the leg away from clicked state
        else if (m_LegState == LegState.CLICKED && Input.GetMouseButtonDown(1))
        {
            m_LegState = LegState.ATTACHED;
            m_mouseTarget.SetActive(false);
        }

        //Decide on action based on current leg state.
        switch (m_LegState)
        {
            case LegState.ATTACHED:
                //Do Nothing while simply attached to body
                break;
            case LegState.CLICKED:
                //track the mouse for the currently clicked leg with the red sphere
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    m_mouseTarget.transform.position = hit.point;
                }

                break;
            case LegState.FLYING:
                //Update position to move towards target position
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, 0.1f);

                //Check for arrival at target position
                if (Vector3.Distance(transform.position, m_TargetPosition) < 0.1f)
                {
                    m_LegState = LegState.DETACHED;
                    Debug.Log("Leg " + gameObject.name + " has reached target position.");
                }
                break;
            case LegState.DETACHED:
                break;
            case LegState.RETURNING:
                //Move the leg back to the player core
                transform.position = Vector3.MoveTowards(transform.position, core.transform.position + m_DistanceToCore, 0.1f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_InitialRotation, 1f);

                //Draw a debug line between the two positions
                Debug.DrawLine(transform.position, core.transform.position + m_DistanceToCore, Color.green);

                if (Vector3.Distance(transform.position, core.transform.position + m_DistanceToCore) < 0.01f)
                {
                    m_LegState = LegState.ATTACHED;
                    Debug.Log("Leg " + gameObject.name + " has returned to player.");
                }
                break;

        }
    }

    private void ReturnToCore()
    {
        isSpinning = false;
        m_LegState = LegState.RETURNING;
    }

    private IEnumerator SpinningCoroutine()
    {
        isSpinning = true;
        Debug.Log("Leg " + gameObject.name + " is spinning.");
        //Spin the leg around y axis for a duration with designated speed, slowly ramp up at start and down at end with animationcurve
        float timer = 0;
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        while (timer < m_SpinAttackDuration && isSpinning)
        {
            timer += Time.deltaTime;
            float t = timer / m_SpinAttackDuration;
            float speed = m_SpinAttackSpeed * curve.Evaluate(t);
            transform.RotateAround(transform.position, Vector3.forward, speed * Time.deltaTime);
            Debug.Log("Leg " + gameObject.name + " is spinning at speed " + speed * Time.deltaTime);
            yield return null;
        }
        Debug.Log("Leg " + gameObject.name + " has stopped spinning.");
        isSpinning = false;
    }

    public bool isAttacking()
    {
        return isSpinning || m_LegState == LegState.FLYING;
    }
}
