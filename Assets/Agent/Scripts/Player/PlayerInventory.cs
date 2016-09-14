using UnityEngine;
using System.Collections;

public class PlayerInventory : MonoBehaviour {

    // The player inventory
    public Inventory inventory;

    // Use this for initialization
    void Start( ) {
    }

    // Update is called once per frame
    void Update( ) {

    }

    public void OnTriggerEnter( Collider other ) {
        if( other.tag == "Item" ) {
            inventory.AddItem( other.GetComponent<Item>( ) );
        }
    }
}
