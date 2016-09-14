using UnityEngine;
using System.Collections;
using System;

[RequireComponent( typeof( Rigidbody ) )]
[RequireComponent( typeof( CapsuleCollider ) )]
public class Agent : MonoBehaviour {
    [Serializable]
    public class MovementSettings {
        public float forwardSpeed = 8.0f;
        public float backwardSpeed = 4.0f;
        public float strafeSpeed = 4.0f;
        public float runMultiplier = 2.0f;
        public KeyCode runKey = KeyCode.LeftShift;
        public float jumpForce = 30f;
        public AnimationCurve slopeCurveModifier = new AnimationCurve( new Keyframe( -90.0f, 1.0f ), new Keyframe( 0.0f, 1.0f ), new Keyframe( 90.0f, 0.0f ) );
        [HideInInspector]
        public float currentTargetSpeed = 8.0f;

        private bool m_Running;

        public void UpdateDesiredTargetSpeed( Vector2 input ) {
            if( input == Vector2.zero )
                return;
            if( input.x > 0 || input.x < 0 )
                currentTargetSpeed = strafeSpeed;
            if( input.y < 0 )
                currentTargetSpeed = backwardSpeed;
            if( input.y > 0 )
                currentTargetSpeed = forwardSpeed;

            if( Input.GetKey( runKey ) ) {
                currentTargetSpeed *= runMultiplier;
                m_Running = true;
            } else {
                m_Running = false;
            }
        }

        public bool Running { get { return m_Running; } }
    }

    [Serializable]
    public class AdvancedSettings {
        public float groundCheckDistance = 0.01f;
        public float stickToGroundHelperDistance = 0.5f;
        public float slowDownRate = 20f;
        public bool airControl;
        [Tooltip( "Set it to 0.1 or more to aviod sticking to walls" )]
        public float shellOffset;
    }

    public MovementSettings movementSettings = new MovementSettings( );
    public AdvancedSettings advancedSettings = new AdvancedSettings( );

    protected Rigidbody m_Rigidbody;
    protected CapsuleCollider m_Capsule;
    protected float m_YRotation;
    protected Vector3 m_GroundContactNormal;
    protected bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;

    public Vector3 Velocity { get { return m_Rigidbody.velocity; } }
    public bool Grounded { get { return m_IsGrounded; } }
    public bool Jumping { get { return m_Jumping; } }
    public bool Running { get { return movementSettings.Running; } }

    // Use this for initialization
    protected void Start( ) {
        m_Rigidbody = GetComponent<Rigidbody>( );
        m_Capsule = GetComponent<CapsuleCollider>( );
    }

    // Update is called once per frame
    protected void Update( ) {
        if( Input.GetButtonDown( "Jump" ) && !m_Jump ) { m_Jump = true; }
    }

    private void FixedUpdate( ) {
        GroundCheck( );
        Vector2 input = GetInput( );

        if( ( Mathf.Abs( input.x ) > float.Epsilon || Mathf.Abs( input.y ) > float.Epsilon ) && ( advancedSettings.airControl || m_IsGrounded ) ) {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = DesiredMove( );
            desiredMove = Vector3.ProjectOnPlane( desiredMove, m_GroundContactNormal ).normalized;

            desiredMove.x = desiredMove.x * movementSettings.currentTargetSpeed;
            desiredMove.z = desiredMove.z * movementSettings.currentTargetSpeed;
            desiredMove.y = desiredMove.y * movementSettings.currentTargetSpeed;
            if( m_Rigidbody.velocity.sqrMagnitude <
                ( movementSettings.currentTargetSpeed * movementSettings.currentTargetSpeed ) ) {
                m_Rigidbody.AddForce( desiredMove * SlopeMultiplier( ), ForceMode.Impulse );
            }
        }

        if( m_IsGrounded ) {
            m_Rigidbody.drag = 5f;

            if( m_Jump ) {
                m_Rigidbody.drag = 0f;
                m_Rigidbody.velocity = new Vector3( m_Rigidbody.velocity.x, 0f, m_Rigidbody.velocity.z );
                m_Rigidbody.AddForce( new Vector3( 0f, movementSettings.jumpForce, 0f ), ForceMode.Impulse );
                m_Jumping = true;
            }

            if( !m_Jumping && Mathf.Abs( input.x ) < float.Epsilon && Mathf.Abs( input.y ) < float.Epsilon
                && m_Rigidbody.velocity.magnitude < 1f ) {
                m_Rigidbody.Sleep( );
            }
        } else {
            m_Rigidbody.drag = 0f;
            if( m_PreviouslyGrounded && !m_Jumping ) {
                StickToGroundHelper( );
            }
        }
        m_Jump = false;
    }

    private float SlopeMultiplier( ) {
        float angle = Vector3.Angle( m_GroundContactNormal, Vector3.up );
        return movementSettings.slopeCurveModifier.Evaluate( angle );
    }

    private void StickToGroundHelper( ) {
        RaycastHit hitInfo;
        if( Physics.SphereCast( transform.position, m_Capsule.radius * ( 1.0f - advancedSettings.shellOffset ),
                                Vector3.down,
                                out hitInfo,
                                ( ( m_Capsule.height / 2f ) - m_Capsule.radius ) + advancedSettings.stickToGroundHelperDistance,
                                ~0, QueryTriggerInteraction.Ignore ) ) {
            if( Mathf.Abs( Vector3.Angle( hitInfo.normal, Vector3.up ) ) < 85f ) {
                m_Rigidbody.velocity = Vector3.ProjectOnPlane( m_Rigidbody.velocity, hitInfo.normal );
            }
        }
    }

    protected virtual Vector2 GetInput( ) {
        Vector2 input = new Vector2 {
            x = 0,
            y = 0
        };
        movementSettings.UpdateDesiredTargetSpeed( input );
        return input;
    }

    //
    protected virtual Vector3 DesiredMove( ) {
        return Vector3.zero;
    }

    private void GroundCheck( ) {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if( Physics.SphereCast( transform.position, m_Capsule.radius * ( 1.0f - advancedSettings.shellOffset ),
            Vector3.down,
            out hitInfo,
            ( ( m_Capsule.height / 2f ) - m_Capsule.radius ) + advancedSettings.groundCheckDistance,
            ~0, QueryTriggerInteraction.Ignore ) ) {
            m_IsGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
        } else {
            m_IsGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
        if( !m_PreviouslyGrounded && m_IsGrounded && m_Jumping ) { m_Jumping = false; }
    }
}
