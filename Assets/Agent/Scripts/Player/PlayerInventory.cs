using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerInventory : MonoBehaviour
{

    // The player inventory
    public Inventory inventory;
    private PlayerController playerCon;

    // Use this for initialization
    void Start()
    {
        playerCon = this.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Inventory.Opened)
        {
            playerCon.canLook = false;
            
            playerCon.mouseLook.SetCursorLock(false);
        }
        else
        {
            playerCon.canLook = true;
            playerCon.mouseLook.SetCursorLock(true);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            inventory.AddItem(other.GetComponent<Item>());
        }
    }
}
