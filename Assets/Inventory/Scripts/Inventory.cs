using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour {

    /// <summary>
    /// Holds the information about how the slots in the inventory should be layed out
    /// </summary>
    [Serializable]
    public struct SlotLayout {
        public int slots, rows;
        public float slotPaddingLeft, slotPaddingTop;
        public float slotSize;
        public GameObject slotPrefab;
    }
    public SlotLayout slotLayout;

    /// <summary>
    /// Holds the information about how large the inventory should be
    /// </summary>
    [Serializable]
    private struct InventoryInfo {
        public RectTransform inventoryRect;
        public float inventoryWidth, inventoryHeight;
        public List<GameObject> allSlots;
    }
    private InventoryInfo inventoryInfo;

    // Two varibles to use when we are moving items between slots
    private static Slot from, to;
    // The icon that shows up when we grab an item to move
    public GameObject iconPrefab;
    // The object we use to track where the grabbed icon goes
    private static GameObject hoverObject;
    // Used to easy place an item so we don't click the object we are holding
    private float hoverYOffset;

    private static GameObject clicked;

    // The ui element that changes the count
    public GameObject selectStackSize;
    // The text field that holds the split amount
    public Text stackText;
    // The amount of items we want to remove from the stack
    private int splitAmount;
    // Holds of the max amount of movable items
    private int maxStackCount;
    // The temp var that hold the items while moving
    private static Slot moveSlot;

    private static Inventory instance;
    public static Inventory Instance {
        get {
            if( instance == null )
                instance = GameObject.FindObjectOfType<Inventory>( );

            return Inventory.instance;
        }
    }


    // The canvas the inventory is using
    public Canvas canvas;
    // The event system the inventory is using
    public EventSystem eventSystem;

    // How many empty slots are left
    private static int emptySlots;
    /// <summary>
    /// Get the amout of how many empty slots are left
    /// </summary>
    public static int EmptySlots {
        get { return emptySlots; }
        set { emptySlots = value; }
    }

    private static CanvasGroup canvasGroup;
    private bool fadingIn;
    private bool fadingOut;
    public float fadeTime;

    public static bool Opened {
        get { return canvasGroup.alpha == 1; }
    }

    void Start( ) {
        canvasGroup = transform.parent.GetComponent<CanvasGroup>( );
        CreateInventoryLayout( );
        moveSlot = GameObject.Find( "MovingSlot" ).GetComponent<Slot>( );
    }

    void Update( ) {

        // If we left click
        if( Input.GetMouseButtonUp( 0 ) ) {
            // And its a valid place
            if( !eventSystem.IsPointerOverGameObject( -1 ) && from != null ) {
                // Reset things
                from.GetComponent<Image>( ).color = Color.white;
                from.ClearSlot( );
                Destroy( GameObject.Find( "Hover" ) );
                to = null;
                from = null;
                emptySlots++;
            }
        }

        // If the hoverObject is set to something
        if( hoverObject != null ) {
            // Move it around with the mouse
            Vector2 position = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle( canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out position );
            position.Set( position.x, position.y - hoverYOffset );
            hoverObject.transform.position = canvas.transform.TransformPoint( position );
        }

        // Inventory fade in/out
        if( Input.GetKeyDown( KeyCode.B ) ) {
            if( Opened ) {
                StartCoroutine( "FadeOut" );
                PutItemBack( );
            } else
                StartCoroutine( "FadeIn" );
        }
    }

    /// <summary>
    /// Creates the Layout of the inventory
    /// </summary>
    private void CreateInventoryLayout( ) {
        // Init the varibles
        inventoryInfo.allSlots = new List<GameObject>( );
        EmptySlots = slotLayout.slots;
        hoverYOffset = slotLayout.slotSize * 0.01f;

        // Calc the width and height of inv
        inventoryInfo.inventoryWidth = ( slotLayout.slots / slotLayout.rows ) * ( slotLayout.slotSize + slotLayout.slotPaddingLeft );
        inventoryInfo.inventoryHeight = slotLayout.rows * ( slotLayout.slotSize + slotLayout.slotPaddingTop );
        // Get the transform
        inventoryInfo.inventoryRect = GetComponent<RectTransform>( );
        // Resize the inv
        inventoryInfo.inventoryRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, inventoryInfo.inventoryWidth + slotLayout.slotPaddingLeft );
        inventoryInfo.inventoryRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, inventoryInfo.inventoryHeight + slotLayout.slotPaddingTop );
        // Calc the amount of slots and cols 
        int cols = slotLayout.slots / slotLayout.rows;

        // Make a new slot for each row and column
        for( int y = 0; y < slotLayout.rows; ++y )
            for( int x = 0; x < cols; ++x ) {
                // Make the slot we are adding
                GameObject newSlot = ( GameObject )Instantiate( slotLayout.slotPrefab );
                // Get the rect for the slot
                RectTransform slotRect = newSlot.GetComponent<RectTransform>( );
                // Rename the slot
                newSlot.name = "Slot";
                // Set it to a child of the inventory
                newSlot.transform.SetParent( this.transform.parent );
                // Change the position 
                slotRect.localPosition = inventoryInfo.inventoryRect.localPosition + new Vector3( slotLayout.slotPaddingLeft * ( x + 1 ) + ( slotLayout.slotSize * x ), -slotLayout.slotPaddingTop * ( y + 1 ) - ( slotLayout.slotSize * y ) );
                slotRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, slotLayout.slotSize * canvas.scaleFactor );
                slotRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, slotLayout.slotSize * canvas.scaleFactor );

                newSlot.transform.SetParent( this.transform );
                // Add it
                inventoryInfo.allSlots.Add( newSlot );
            }
    }

    /// <summary>
    /// Add Item to the inventory
    /// </summary>
    /// <param name="item">Add we are going to add</param>
    /// <returns>True if we are successful, false if not</returns>
    public bool AddItem( Item item ) {
        // If we can't stack the item
        if( item.maxSize == 1 )
            // Find a new slot for it
            return PlaceEmpty( item );
        // If we can stack it
        else {
            // Loop through all the slot objects
            foreach( GameObject slot in inventoryInfo.allSlots ) {
                // Create a temp slot based on the current looped object
                Slot tmp = slot.GetComponent<Slot>( );
                // If it's stack has space
                if( !tmp.IsEmpty )
                    // And there are the same type
                    if( tmp.CurrentItem.itemType == item.itemType && tmp.CanStack ) {
                        // Add the item
                        tmp.AddItem( item );
                        // Return to leave the search
                        return true;
                    }
            }
            // If we couldn't find an empty stack, start a new one if the inventory has space
            if( EmptySlots > 0 )
                PlaceEmpty( item );
        }
        // Return false because we didn't find anything
        return false;
    }

    /// <summary>
    /// Place the item in first empty slot
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>True if we find an empty slot to add to, else false</returns>
    private bool PlaceEmpty( Item item ) {
        // If there is space in the inventory
        if( EmptySlots > 0 )
            // Loop through all the slots
            foreach( GameObject slot in inventoryInfo.allSlots ) {
                // Get the slot for the current object
                Slot tmp = slot.GetComponent<Slot>( );
                // If the slot is full
                if( tmp.IsEmpty ) {
                    // Add a new slot
                    tmp.AddItem( item );
                    // Tick down the empty slot count
                    EmptySlots--;
                    // And exit
                    return true;
                }
            }
        // Didn't find an empty slot
        return false;
    }

    /// <summary>
    /// Move an item to a different location
    /// </summary>
    /// <param name="clicked">Item stack we clicked</param>
    public void MoveItem( GameObject clicked ) {
        Inventory.clicked = clicked;

        if( !moveSlot.IsEmpty ) {
            Slot tmp = clicked.GetComponent<Slot>( );
            if( tmp.IsEmpty ) {
                tmp.AddItems( moveSlot.Items );
                moveSlot.Items.Clear( );
                Destroy( GameObject.Find( "Hover" ) );
            } else if( !tmp.IsEmpty && moveSlot.CurrentItem.itemType == tmp.CurrentItem.itemType && tmp.CanStack ) {
                MergeStack( moveSlot, tmp );
            }
            // If the from is null check if we can make the clicked object it
        } else if( from == null && Opened && !( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) ) {
            // If it has something in it
            if( !clicked.GetComponent<Slot>( ).IsEmpty ) {
                // Set the from value to the clicked object
                from = clicked.GetComponent<Slot>( );
                // Grey out the clicked object
                from.GetComponent<Image>( ).color = Color.gray;
                CreateHoverObject( );
            }
            // If the from is valid, set the to
        } else if( to == null && !( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) ) {
            to = clicked.GetComponent<Slot>( );
            Destroy( GameObject.Find( "Hover" ) );
        }

        // Finally if we are valid
        if( to != null && from != null ) {
            // Move the items around
            Stack<Item> tmpTo = new Stack<Item>( to.Items );
            to.AddItems( from.Items );
            if( tmpTo.Count == 0 )
                from.ClearSlot( );
            else
                from.AddItems( tmpTo );
            // Reset everything
            from.GetComponent<Image>( ).color = Color.white;
            to = null;
            from = null;
            Destroy( GameObject.Find( "Hover" ) );
        }
    }

    /// <summary>
    /// Creates the hover icon
    /// </summary>
    private void CreateHoverObject( ) {
        // Create the hover object
        hoverObject = ( GameObject )Instantiate( iconPrefab );
        hoverObject.GetComponent<Image>( ).sprite = clicked.GetComponent<Image>( ).sprite;
        hoverObject.name = "Hover";
        // Get a few values
        RectTransform hoverTransform = hoverObject.GetComponent<RectTransform>( );
        RectTransform clickedTransform = clicked.GetComponent<RectTransform>( );
        // Set the positions
        hoverTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, clickedTransform.sizeDelta.x );
        hoverTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, clickedTransform.sizeDelta.y );
        hoverObject.transform.SetParent( GameObject.Find( "Canvas" ).transform, true );
        hoverObject.transform.localScale = clicked.gameObject.transform.localScale;

        hoverObject.transform.GetChild( 0 ).GetComponent<Text>( ).text = moveSlot.Items.Count > 1 ? moveSlot.Items.Count.ToString( ) : string.Empty;
    }

    /// <summary>
    /// Puts the item back when the inventory is closed
    /// </summary>
    private void PutItemBack( ) {
        if( from != null ) {
            Destroy( GameObject.Find( "Hover" ) );
            from.GetComponent<Image>( ).color = Color.white;
            from = null;
        }
    }

    public void SetStackInfo( int maxStackCount ) {
        selectStackSize.SetActive( true );
        splitAmount = 0;
        this.maxStackCount = maxStackCount;
        stackText.text = splitAmount.ToString( );
    }

    public void SplitStack( ) {
        selectStackSize.SetActive( false );
        if( splitAmount == maxStackCount )
            MoveItem( clicked );
        else if( splitAmount > 0 ) {
            moveSlot.Items = clicked.GetComponent<Slot>( ).RemoveItems( splitAmount );
            CreateHoverObject( );
        }
    }

    public void MergeStack( Slot source, Slot dest ) {
        int max = dest.CurrentItem.maxSize - dest.Items.Count;
        int count = source.Items.Count < max ? source.Items.Count : max;

        for( int i = 0; i < count; ++i ) {
            dest.AddItem( source.RemoveItem( ) );
        }

        if( source.Items.Count == 0 ) {
            source.ClearSlot( );
            Destroy( GameObject.Find( "Hover" ) );
        }
    }

    public void ChangeStackText( int i ) {
        splitAmount += i;

        if( splitAmount < 0 )
            splitAmount = 0;
        if( splitAmount > maxStackCount )
            splitAmount = maxStackCount;

        stackText.text = splitAmount.ToString( );
    }

    /// <summary>
    /// Fade out function for the inventory
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOut( ) {
        if( !fadingOut ) {
            fadingOut = true;
            fadingIn = false;
            StopCoroutine( "FadeIn" );

            float startAlpha = canvasGroup.alpha;
            float rate = 1.0f / fadeTime;
            float fadeProgress = 0.0f;

            while( fadeProgress < 1.0f ) {
                canvasGroup.alpha = Mathf.Lerp( startAlpha, 0.0f, fadeProgress );
                fadeProgress += rate * Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0.0f;
            fadingOut = false;
        }
    }

    /// <summary>
    /// Fade in function for the inventory
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeIn( ) {
        if( !fadingIn ) {
            fadingIn = true;
            fadingOut = false;
            StopCoroutine( "FadeOut" );

            float startAlpha = canvasGroup.alpha;
            float rate = 1.0f / fadeTime;
            float fadeProgress = 0.0f;

            while( fadeProgress < 1.0f ) {
                canvasGroup.alpha = Mathf.Lerp( startAlpha, 1.0f, fadeProgress );
                fadeProgress += rate * Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1.0f;
            fadingIn = false;
        }
    }
}
