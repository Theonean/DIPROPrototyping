using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UIPlayerRocketLoader : MonoBehaviour
{

    public Rocket Rocket1; //northwest
    public Rocket Rocket2; //Northeast
    public Rocket Rocket3; //Southeast
    public Rocket Rocket4; //Southwest

    private RocketAimController playerAimController;

    void Start()
    {
        playerAimController = PlayerCore.Instance.GetComponentInChildren<RocketAimController>();

        CopyRocketComponents(Rocket1, playerAimController.Rocket1);
        CopyRocketComponents(Rocket2, playerAimController.Rocket2);
        CopyRocketComponents(Rocket3, playerAimController.Rocket3);
        CopyRocketComponents(Rocket4, playerAimController.Rocket4);

    }

    private void CopyRocketComponents(Rocket source, Rocket target)
    {
        Logger.Log($"Copying components from {source.name} to {target.name}", LogLevel.INFO, LogType.ROCKETS);
        GameObject selectedFront = playerAimController.rocketFronts
            .Where(x => x.GetComponent<ACRocketComponent>().GetType() == source.frontComponent.GetType())
            .FirstOrDefault();
        target.SetFront(selectedFront);

        GameObject selectedBody = playerAimController.rocketBodies
            .Where(x => x.GetComponent<ACRocketComponent>().GetType() == source.bodyComponent.GetType())
            .FirstOrDefault();
        target.SetBody(selectedBody);

        GameObject selectedPropulsion = playerAimController.rocketPropulsions
            .Where(x => x.GetComponent<ACRocketComponent>().GetType() == source.propulsionComponent.GetType())
            .FirstOrDefault();
        target.SetPropulsion(selectedPropulsion);
    }
}
