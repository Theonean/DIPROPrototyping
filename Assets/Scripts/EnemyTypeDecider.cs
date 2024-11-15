using UnityEngine;

public class EnemyTypeDecider : MonoBehaviour
{
    //This Class decides what type of enemy the gameobject should represent
    public GameObject enemyRegularModel;
    public GameObject enemyFastModel;

    public bool enemyType = false;
    void Start()
    {
        //Get the followplayer script
        FollowPlayer followPlayer = GetComponent<FollowPlayer>();

        enemyType = Random.value > 0.5f;

        //regular enemy
        if (enemyType)
        {
            enemyRegularModel.SetActive(true);
            enemyFastModel.SetActive(false);
            followPlayer.SetMoveSpeed(4f);
        }
        //Fast enemy
        else
        {
            enemyFastModel.SetActive(true);
            enemyRegularModel.SetActive(false);
            followPlayer.SetMoveSpeed(8f);
        }

    }
}
