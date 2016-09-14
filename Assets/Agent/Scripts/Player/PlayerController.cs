using UnityEngine;
using System.Collections;

public class PlayerController : Agent {

    public Camera cam;
    public MouseLook mouseLook = new MouseLook( );
    public bool canLook = false;

    // Use this for initialization
    new void Start( ) {
        mouseLook.Init( transform, cam.transform );
        base.Start( );
    }

    // Update is called once per frame
    new void Update( ) {
        RotateView( );
        base.Update( );
    }

    protected override Vector2 GetInput( ) {
        Vector2 input = new Vector2 {
            x = Input.GetAxisRaw( "Horizontal" ),
            y = Input.GetAxisRaw( "Vertical" )
        };
        movementSettings.UpdateDesiredTargetSpeed( input );
        return input;
    }

    protected override Vector3 DesiredMove( ) {
        return cam.transform.forward * GetInput( ).y + cam.transform.right * GetInput( ).x;
    }

    private void RotateView( ) {
        //avoids the mouse looking if the game is effectively paused
        if( Mathf.Abs( Time.timeScale ) < float.Epsilon )
            return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        if(canLook) mouseLook.LookRotation( transform, cam.transform );

        if( m_IsGrounded || advancedSettings.airControl ) {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis( transform.eulerAngles.y - oldYRotation, Vector3.up );
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }
    }
}
