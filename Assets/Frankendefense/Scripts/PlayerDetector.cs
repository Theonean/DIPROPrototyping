using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDetector : MonoBehaviour
{
    public UnityEvent OnPlayerDetected;
    public UnityEvent OnPlayerLost;

    public Material m_MaterialBlue;
    public Material m_MaterialGreen;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Player Detected");
            OnPlayerDetected.Invoke();
            //Change Material to Green
            GetComponent<MeshRenderer>().material = m_MaterialGreen;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Player Lost");
            OnPlayerLost.Invoke();
            //Change Material to Blue
            GetComponent<MeshRenderer>().material = m_MaterialBlue;
        }
    } 
}
