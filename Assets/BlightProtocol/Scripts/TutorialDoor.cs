using System.Collections;
using UnityEngine;

public class TutorialDoor : MonoBehaviour
{
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    [SerializeField] private AnimationCurve doorsAnimationCurve;
    [SerializeField] private float doorSlideTime = 2f;
    [SerializeField] private float closedX = 120f;
    [SerializeField] private float openedX = 240f;

    public void OpenSegmentDoor()
    {
        Vector3 leftStartPos = new Vector3(-closedX, 0, leftDoor.transform.position.z);
        Vector3 leftEndPos = new Vector3(-openedX, 0, leftDoor.transform.position.z);

        Vector3 rightStartPos = new Vector3(closedX, 0, rightDoor.transform.position.z);
        Vector3 rightEndPos = new Vector3(openedX, 0, rightDoor.transform.position.z);

        StartCoroutine(AnimateDoor(leftDoor, leftStartPos, leftEndPos));
        StartCoroutine(AnimateDoor(rightDoor, rightStartPos, rightEndPos));
    }

    public void CloseSegmentDoor()
    {
        Vector3 leftStartPos = new Vector3(-openedX, 0, leftDoor.transform.position.z);
        Vector3 leftEndPos = new Vector3(-closedX, 0, leftDoor.transform.position.z);

        Vector3 rightStartPos = new Vector3(openedX, 0, rightDoor.transform.position.z);
        Vector3 rightEndPos = new Vector3(closedX, 0, rightDoor.transform.position.z);

        StartCoroutine(AnimateDoor(leftDoor, leftStartPos, leftEndPos));
        StartCoroutine(AnimateDoor(rightDoor, rightStartPos, rightEndPos));
    }

    private IEnumerator AnimateDoor(GameObject door, Vector3 startPos, Vector3 endPos)
    {
        float t = 0;
        while (t < doorSlideTime)
        {
            door.transform.position = Vector3.Lerp(startPos, endPos, doorsAnimationCurve.Evaluate(t / doorSlideTime));
            t += Time.deltaTime;
            yield return null;
        }
    }
}
