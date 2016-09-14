using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Slot : MonoBehaviour, IPointerClickHandler {

    // Holds the information of the items in the stack
    private Stack<Item> items;
    /// <summary>
    /// Returns the stack of items the slot has
    /// </summary>
    public Stack<Item> Items {
        get { return items; }
        set { items = value; }
    }
    // Shows the count of the stack
    public Text stackText;
    // The icon that shows the default icon
    public Sprite slotEmpty;
    // The icon that shows the highlight icon
    public Sprite slotHighlight;

    /// <summary>
    /// Return true if items is empty
    /// </summary>
    public bool IsEmpty {
        get { return Items.Count == 0; }
    }

    /// <summary>
    /// Returns true if we can add more to the stack
    /// </summary>
    public bool CanStack {
        get { return CurrentItem.maxSize > Items.Count; }
    }

    /// <summary>
    /// Returns the top item in the item stack
    /// </summary>
    public Item CurrentItem {
        get { return Items.Peek( ); }
    }

    // Use this for initialization
    void Start( ) {
        Items = new Stack<Item>( );
        RectTransform slotRect = GetComponent<RectTransform>( );
        RectTransform textRect = stackText.GetComponent<RectTransform>( );
        // Set the min/max size for text
        int textScaleFactor = ( int )( slotRect.sizeDelta.x * 0.6 );
        stackText.resizeTextMaxSize = textScaleFactor;
        stackText.resizeTextMinSize = textScaleFactor;
        // Place the text position
        textRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, slotRect.sizeDelta.x );
        textRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, slotRect.sizeDelta.y );
    }

    /// <summary>
    /// Handles adding a single item
    /// </summary>
    /// <param name="item">The item we are adding</param>
    public void AddItem( Item item ) {
        // Add the item to the stack
        Items.Push( item );
        // If there is more than one item in the stack show the count
        if( Items.Count > 1 ) {
            stackText.text = Items.Count.ToString( );
        }
        // Change the icon we show for the slot
        ChangeSprite( item.spriteNeutral, item.spriteHighlighted );
    }

    /// <summary>
    /// Handles adding multiple items
    /// </summary>
    /// <param name="items">The stack of items to be added</param>
    public void AddItems( Stack<Item> items ) {
        // Change the stack items to the one we passed in
        this.Items = new Stack<Item>( items );
        // Update the text
        stackText.text = items.Count > 1 ? items.Count.ToString( ) : string.Empty;
        // Change the item icons
        ChangeSprite( CurrentItem.spriteNeutral, CurrentItem.spriteHighlighted );
    }

    /// <summary>
    /// Changes the sprite data the slot will use
    /// </summary>
    /// <param name="neutral">Default slot icon</param>
    /// <param name="highLight">Clicked on slot icon</param>
    private void ChangeSprite( Sprite neutral, Sprite highLight ) {
        // Change the default icon
        GetComponent<Image>( ).sprite = neutral;
        // Create the SpriteState
        SpriteState st = new SpriteState( );
        // Set the sprites
        st.highlightedSprite = neutral;
        st.pressedSprite = highLight;
        // Change this button's sprite state to the one we created
        GetComponent<Button>( ).spriteState = st;
    }

    /// <summary>
    /// Uses the item
    /// </summary>
    private void UseItem( ) {
        // If the slot isn't empty
        if( !IsEmpty && Inventory.Opened ) {
            // Pop the top item after and call the use function from that item
            Items.Pop( ).Use( );
            // Update the text
            stackText.text = Items.Count > 1 ? Items.Count.ToString( ) : string.Empty;
            // Check if it was the last one
            if( IsEmpty ) {
                // Change the icons to the defaults
                ChangeSprite( slotEmpty, slotHighlight );
                // Increase empty slot count
                Inventory.EmptySlots++;
            }
        }
    }

    /// <summary>
    /// Deletes the stuff in the slot
    /// </summary>
    public void ClearSlot( ) {
        // Clear the stack
        items.Clear( );
        // Change icon to the defaults
        ChangeSprite( slotEmpty, slotHighlight );
        // Update the text
        stackText.text = string.Empty;
    }

    public Stack<Item> RemoveItems( int amount ) {
        Stack<Item> retVal = new Stack<Item>( );
        for( int i = 0; i < amount; ++i ) 
            retVal.Push( items.Pop( ) );

        stackText.text = items.Count > 1 ? items.Count.ToString( ) : string.Empty;

        return retVal;
    }

    public Item RemoveItem( ) {
        Item retVal;
        retVal = Items.Pop( );
        stackText.text = items.Count > 1 ? items.Count.ToString( ) : string.Empty;

        return retVal;
    }

    /// <summary>
    /// A fucnction to handle clicks other than left click
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick( PointerEventData eventData ) {
        if( eventData.button == PointerEventData.InputButton.Right && !GameObject.Find( "Hover" ) ) {
            UseItem( );
        } else if( eventData.button == PointerEventData.InputButton.Left && ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) && !IsEmpty && !GameObject.Find("Hover")) {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle( Inventory.Instance.canvas.transform as RectTransform, Input.mousePosition, Inventory.Instance.canvas.worldCamera, out position );
            Inventory.Instance.selectStackSize.SetActive( true );
            Inventory.Instance.selectStackSize.transform.position = Inventory.Instance.canvas.transform.TransformPoint( position );
            Inventory.Instance.SetStackInfo( items.Count );
        }
    }
}
