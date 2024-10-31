using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObstaclePattern : MonoBehaviour
{
    Obstacle[] obstacles;
    // Start is called before the first frame update
    void Awake()
    {
        obstacles = GetComponentsInChildren<Obstacle>();
    }

    public void MovePattern(Vector3 position)
    {
        transform.position = position;
        foreach (Obstacle obstacle in obstacles)
        {
            obstacle.gameObject.SetActive(true);
            obstacle.RandomizeBlendWeights();
            obstacle.UpdateColor();
        }
    }


    public void SetMeshColoursToRegion()
    {
        foreach (Obstacle obstacle in obstacles)
        {
            obstacle.UpdateColor();
        }
    }


}
