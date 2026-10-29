using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class ExteriorDoor : Interactible
{


    public UnityEvent BuildingEntranceInteract;



    public override void Interact()
    {

        BuildingEntranceInteract.Invoke();

        //Debug.Log("Turning down Alpha");


    }



    

}