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
        }

        SetMeshColoursToRegion();
    }


    public void SetMeshColoursToRegion()
    {
        foreach (Obstacle obstacle in obstacles)
        {
            // Calculate the color based on the obstacle's position along the path
            float zPosition = obstacle.transform.position.z;
            Color regionColor = ProceduralTileGenerator.Instance.GetColorForPosition(zPosition);

            // Use MaterialPropertyBlock to set color without affecting shared materials
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            if (obstacle.meshRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                skinnedMeshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", regionColor);

                Color shadowColor1 = skinnedMeshRenderer.material.GetColor("_1st_ShadeColor");
                Color shadowColor2 = skinnedMeshRenderer.material.GetColor("_2nd_ShadeColor");

                propBlock.SetColor("_1st_ShadeColor", regionColor * shadowColor1);
                propBlock.SetColor("_2nd_ShadeColor", regionColor * shadowColor2);

                skinnedMeshRenderer.SetPropertyBlock(propBlock);


            }
        }
    }


}
