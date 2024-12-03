using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DisableTheseIfLocalPlayer : NetworkBehaviour
{
    //[Tooltip("List of components to disable if this is the local player.")]
    //[SerializeField] private List<Behaviour> componentsToDisable = new List<Behaviour>();

    [Tooltip("List of GameObjects to disable if this is the local player.")]
    [SerializeField] private List<GameObject> gameObjectsToDisable = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Ensure this only runs on the local player
        if (isLocalPlayer)
        {
            DisableSpecifiedComponents();
        }
    }

    private void DisableSpecifiedComponents()
    {
        // Disable listed components
        //foreach (var component in componentsToDisable)
        //{
        //    if (component != null)
        //    {
        //        component.enabled = false;
        //    }
        //}

        // Disable listed GameObjects
        foreach (var obj in gameObjectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}
