using UnityEngine;
using System.Collections;

public enum ItemType { DEFAULT, SWORD, HAND };

public class Item : MonoBehaviour {
    // The item type
    public ItemType itemType;
    // The default icon
    public Sprite spriteNeutral;
    // The clicked icon
    public Sprite spriteHighlighted;
    // Stack size
    public int maxSize;

    /// <summary>
    /// Uses the itme based on the item type
    /// </summary>
    public void Use( ) {
        switch( itemType ) {
            case ItemType.SWORD:
                Debug.Log( "Sword" );
                break;
            case ItemType.HAND:
                Debug.Log( "Hand" );
                break;
        }
    }
}
