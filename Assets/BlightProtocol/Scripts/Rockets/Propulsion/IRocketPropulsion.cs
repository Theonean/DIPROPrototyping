using System.Collections;
using UnityEngine;

public interface IRocketPropulsion
{
    public IEnumerator FlyToPosition(Vector3 position);
}
