using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushChest : EventUnit
{
    private bool activated = false;
    public override void Interact(RoomScript room)
    {
        if (!activated)
        {
            room.ActivateRoom(RoomTypes.Combat);
            activated = true;
        }
    }

    public override void OnMouseDown()
    {
    }

    public override void OnPlayerMove()
    {
    }
}
