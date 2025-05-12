using UnityEngine;

[System.Serializable]
public class GameplaySnapshotData
{
    public int rocketsFired { get; private set; } = 0;
    private int successfullRocketShots = 0;
    private bool rocketHitEnemyInFlight = false;
    public int killsTotal { get; private set; } = 0;
    public int killsFront { get; private set; } = 0;
    public int killsBody { get; private set; } = 0;
    public int killsPropulsion { get; private set; } = 0;
    public float accuracy { get; private set; } = 0;

    public GameplaySnapshotData(Rocket rocket)
    {
        rocket.OnRocketStateChange.AddListener(RocketChangedState);
        rocket.frontComponent.OnKilledEnemy.AddListener(KilledEnemies);
        rocket.bodyComponent.OnKilledEnemy.AddListener(KilledEnemies);
        rocket.propulsionComponent.OnKilledEnemy.AddListener(KilledEnemies);
    }

    public void KilledEnemies(RocketComponentType componentType, int numKills)
    {
        killsTotal += numKills;
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                killsPropulsion += numKills;
                break;
            case RocketComponentType.BODY:
                killsBody += numKills;
                break;
            case RocketComponentType.FRONT:
                killsFront += numKills;
                if (!rocketHitEnemyInFlight)
                {
                    rocketHitEnemyInFlight = true;
                    successfullRocketShots++;
                    accuracy = successfullRocketShots / rocketsFired;
                }
                break;
        }
    }

    private void RocketChangedState(RocketState newState)
    {
        if(newState == RocketState.FLYING)
        {
            rocketsFired++;
            rocketHitEnemyInFlight = false;
        }
    }
}

namespace GameplayAnalysis
{
    public class GameplayAnalyzer : MonoBehaviour
    {
        /* Data to save 
         * Rockets fired
         * Kills_Total
         * Kills_Front
         * Kills_Body
         * Accuracy_Front
         * Accuracy_Body
         * 
         */

        public float saveInterval = 30f;
        public Rocket[] rocketsToTrack = new Rocket[4];
        public GameplaySnapshotData[] rocketData;
        private string savePath;

        private void Start()
        {
            savePath = Application.persistentDataPath;
            rocketData = new GameplaySnapshotData[rocketsToTrack.Length];
            for (int i = 0; i < rocketsToTrack.Length; i++)
            {
                rocketData[i] = new GameplaySnapshotData(rocketsToTrack[i]);
            }
        }
    }
}