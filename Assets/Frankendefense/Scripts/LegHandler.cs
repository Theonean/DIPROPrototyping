using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHandler : MonoBehaviour
{
    /*
    IDEA CORNER:
        maybe scale up leg when it's flying away so it has more effect or range
        maybe add rightclick on leg when the leg is on floor to make a leg manually return to player
    */

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
    float m_SpinAttackDuration = 3f; //Duration of the spin attack
    float m_SpinAttackSpeed = 2000000f; //rotation Speed of the spin attack
    float m_LegFlySpeed = 25f; //Speed of the leg when flying away.
    Vector3 m_LegOriginalScale; //Original scale of the leg.
    float m_ScaleMultiplierToFly = 1.5f; //Scale multiplier which slowly acts until the leg has reached the target position.
    Camera m_Camera;
    PlayerCore core;
    void Awake()
    {
        //Find parent "core" object and subscribe to the return event
        core = FindObjectOfType<PlayerCore>();
        core.returnLegs.AddListener(() =>
        {
            isSpinning = false;
            m_LegState = LegState.RETURNING;
        });

        m_Camera = Camera.main;


        m_DistanceToCore = transform.position - core.transform.position;
        m_InitialRotation = transform.rotation;
        m_LegOriginalScale = transform.localScale;

        //Generate mouseTarget object
        m_mouseTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(m_mouseTarget.GetComponent<SphereCollider>());
        m_mouseTarget.GetComponent<MeshRenderer>().material.color = Color.red;
        m_mouseTarget.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        m_mouseTarget.SetActive(false);
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = m_Camera.ScreenPointToRay(mousePosition);

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
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, 0.1f * Time.deltaTime * m_LegFlySpeed);
                //Lerp scale by how close leg is to final position
                transform.localScale = Vector3.Lerp(transform.localScale, m_LegOriginalScale * m_ScaleMultiplierToFly, 0.1f * Time.deltaTime * m_LegFlySpeed);

                //Check for arrival at target position
                if (Vector3.Distance(transform.position, m_TargetPosition) < 0.1f)
                {
                    m_LegState = LegState.DETACHED;
                }
                break;
            case LegState.DETACHED:
                break;
            case LegState.RETURNING:
                //Move the leg back to the player core
                transform.position = Vector3.MoveTowards(transform.position, core.transform.position + m_DistanceToCore, 0.1f * Time.deltaTime * m_LegFlySpeed * 1.5f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_InitialRotation, 1f * Time.deltaTime);
                transform.localScale = Vector3.MoveTowards(transform.localScale, m_LegOriginalScale, 0.1f * Time.deltaTime * m_LegFlySpeed * 1.5f);

                if (Vector3.Distance(transform.position, core.transform.position + m_DistanceToCore) < 0.01f)
                {
                    m_LegState = LegState.ATTACHED;
                    transform.SetParent(core.transform);
                }
                break;

        }
    }

    public void LegClicked()
    {
        if (m_LegState == LegState.ATTACHED)
        {
            //Leg starts waiting for mouse release to fly away and starts highlighting current mouse position, rightclick to cancel.
            m_LegState = LegState.CLICKED;
            m_mouseTarget.SetActive(true);

            //Colour mesh blue for selected
            GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }

    public void LegReleased()
    {
        if (m_LegState == LegState.CLICKED)
        {
            //Recolor to gray
            GetComponent<MeshRenderer>().material.color = Color.gray;

            //If a "safe" position is clicked while waiting for next click, start flying to the mouse position.
            m_LegState = LegState.FLYING;
            m_mouseTarget.SetActive(false);

            //Raycast to find the position to fly to
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
            m_TargetPosition = new Vector3(hit.point.x, gameObject.transform.position.y, hit.point.z); //Keep leg height

            //Reparent leg to be on the same level as the player so it doesn't move with it
            transform.SetParent(null);
        }
    }

    private void OnMouseDown()
    {

        switch (m_LegState)
        {
            case LegState.FLYING:
            case LegState.DETACHED:
                //If clicked when lying on the floor and not alread spinning, do a spin attack
                if (!isSpinning) { StartCoroutine(SpinningCoroutine()); }
                break;
        }
    }

    private IEnumerator SpinningCoroutine()
    {
        isSpinning = true;
        //Spin the leg around y axis for a duration with designated speed, slowly ramp up at start and down at end with animationcurve
        float timer = 0;
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        while (timer < m_SpinAttackDuration && isSpinning)
        {
            timer += Time.deltaTime;
            float t = timer / m_SpinAttackDuration;
            float speed = m_SpinAttackSpeed * curve.Evaluate(t);
            transform.RotateAround(transform.position, Vector3.forward, speed * Time.deltaTime);
            //Debug.Log("Leg " + gameObject.name + " is spinning at speed " + speed * Time.deltaTime);
            yield return null;
        }
        isSpinning = false;
    }

    public bool isAttacking()
    {
        return isSpinning || m_LegState == LegState.FLYING || m_LegState == LegState.RETURNING;
    }
}
