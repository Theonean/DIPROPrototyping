using UnityEngine;

public class EnemyTypeDecider : MonoBehaviour
{
    //This Class decides what type of enemy the gameobject should represent
    public GameObject enemyRegularModel;
    public GameObject enemyFastModel;
    void Start()
    {
        //Get the followplayer script
        FollowPlayer followPlayer = GetComponent<FollowPlayer>();

        bool random = Random.value > 0.5f;

        //regular enemy
        if (random)
        {
            enemyRegularModel.SetActive(true);
            enemyFastModel.SetActive(false);
            followPlayer.m_MoveSpeed = 4f;
        }
        //Fast enemy
        else
        {
            enemyFastModel.SetActive(true);
            enemyRegularModel.SetActive(false);
            followPlayer.m_MoveSpeed = 6f;
        }

    }
}
