using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ShopKeeperScript : EventUnit, IInteractable
{
    [SerializeField] private protected ShopScript shop;
    [SerializeField] private protected BaseUnitInfo info;
    private protected bool shopInstantiated = false;
    public override void OnPlayerMove()
    {
        //check if the player is within 1 tile
    }
    public override void OnMouseDown()
    {
        Interact(CurrentRoom);
    }
    public override void Interact(RoomScript room)
    {
        if (!shopInstantiated)
        {
            shopInstantiated = true;
            shop = Instantiate(shop, UIManagerScript.Canvas.transform);
            shop.transform.localPosition = new Vector3(-1920, 0);
        }
        shop.Open();
    }
}
