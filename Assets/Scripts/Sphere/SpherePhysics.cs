using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereStateMachine))]
public class SpherePhysics : MonoBehaviour
{
    [Tooltip("Movement relative to this object, e.g. a camera")]
    public Transform PlayerInputSpace = default;

    [Range(0f, 100f)]
    public float MaxSpeed = 10f, MaxClimbSpeed = 4f, MaxFallSpeed = 20f;

    [Range(0f, 100f)]
    public float
        MaxAcceleration = 20f,
        MaxAirAcceleration = 6f,
        MaxClimbAcceleration = 40f;

    [Range(0f, 10f)]
    public float JumpHeight = 2f;

    [Min(10f)]
    public float JumpStaminaCost = 50f;

    [Range(0, 3)]
    public int MaxAirJumps = 0;

    [Range(0, 90)]
    public float MaxGroundAngle = 40f, MaxStairsAngle = 50f;

    [Range(90, 170)]
    public float MaxClimbAngle = 140f;

    [Range(0f, 1f), Tooltip("Climbing grip factor. Should be '< 1f' to allow movement on inner corners")]
    public float GripStrengthFactor = 0.9f;

    [Range(0f, 100f), Tooltip("Don't snap the sphere to the ground for greater speeds")]
    public float MaxSnapSpeed = 100f;

    [Min(0f), Tooltip("Don't snap the sphere to the ground if it's too far")]
    public float ProbeDistance = 1f;

    // -1 as initial value matches all layers
    public LayerMask 
	    WhatIsGround = -1,
	    WhatIsStairs = -1,
	    WhatIsClimbable = -1;

    // Sphere could be connected to a moving rigidbody (e.g. platform)
    [HideInInspector]
    public Rigidbody ConnectedBody, PreviousConnectedBody;

    [HideInInspector]
    public Rigidbody Body;

    [HideInInspector]
    public Vector3 Velocity, ConnectionVelocity;

    [HideInInspector]
    public Vector3 ConnectionWorldPosition, ConnectionLocalPosition;

    // Custom axes to take into account custom gravity directions
    [HideInInspector]
    public Vector3 UpAxis, RightAxis, ForwardAxis;

    [HideInInspector]
    public Vector3 Gravity;

    // Collect all collision normals to set the correct contact normal
    [HideInInspector]
    public Vector3 ContactNormal, SteepNormal, ClimbNormal, LastClimbNormal;

    [HideInInspector]
    public Vector3 LastContactNormal, LastSteepNormal, LastConnectionVelocity;

    // Ceilings and overhangs are not steep contacts
    [HideInInspector]
    public int GroundContactCount, SteepContactCount, ClimbContactCount;

    // One step is one execution of FixedUpdate() 
    [HideInInspector]
    public int StepsSinceLastGrounded, StepsSinceLastJump;

    // Number of air jumps made sequentially 
    [HideInInspector]
    public int JumpPhase;

    [HideInInspector]
    public bool WantsToJump, WantsToClimb;

    [HideInInspector]
    public UnityAction OnJump;

    // Turn off climbing if sphere just jumped
    public bool IsClimbing => ClimbContactCount > 0 && StepsSinceLastJump > 2;
    public bool WalkableSurfaceDetected => GroundContactCount > 0;
    public bool SteepDetected => SteepContactCount > 0;

    public float MinGroundDotProduct { get; private set; }
    public float MinStairsDotProduct { get; private set; }
    public float MinClimbDotProduct { get; private set; }

    public SphereStateMachine StateMachine { get; private set; }
    public Stat Stamina { get; private set; }
    public Stat Speed { get; private set; }
    public Stat Agility { get; private set; }

    ISphereState CurrState => StateMachine.CurrState;

    StatsCollectionManager m_Stats;
    InputAction m_JumpAction, m_ClimbAction;
    Vector3 m_MovementInput = Vector3.zero;

    void OnValidate()
    {
        // dot = |A||B|cos(angle)
        // Retrieving the minimum "normal.y" component of a surface using its angle with the up vector
        MinGroundDotProduct = Mathf.Cos(MaxGroundAngle * Mathf.Deg2Rad);
        MinStairsDotProduct = Mathf.Cos(MaxStairsAngle * Mathf.Deg2Rad);
        MinClimbDotProduct = Mathf.Cos(MaxClimbAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        OnValidate();

        Body = GetComponent<Rigidbody>();
        Body.useGravity = false;

        StateMachine = GetComponent<SphereStateMachine>();
    }

    void Start()
    {
        InputManager inputs = FindObjectOfType<InputManager>();
        inputs.SphereInputActions.Player.Move.performed += OnMove;
        m_ClimbAction = inputs.SphereInputActions.Player.Climb;
        m_JumpAction = inputs.SphereInputActions.Player.Jump;

        m_Stats = GetComponentInChildren<StatsCollectionManager>();
        Stamina = m_Stats.GetStat(StatId.STAMINA);
        Speed = m_Stats.GetStat(StatId.SPEED);
        Agility = m_Stats.GetStat(StatId.AGILITY);
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        var value = ctx.ReadValue<Vector2>();
        m_MovementInput.x = value.x;
        m_MovementInput.z = value.y;
    }

    void Update()
    {
        m_MovementInput = Vector3.ClampMagnitude(m_MovementInput, 1f);

        // Retrieve the camera-relative right and forward axes
        // Compute the gravity-relative right and forward axes from the upAxis
        if (PlayerInputSpace)
        {
            RightAxis = ProjectDirectionOnPlane(PlayerInputSpace.right, UpAxis);
            ForwardAxis = ProjectDirectionOnPlane(PlayerInputSpace.forward, UpAxis);
        }
        else
        {
            RightAxis = ProjectDirectionOnPlane(Vector3.right, UpAxis);
            ForwardAxis = ProjectDirectionOnPlane(Vector3.forward, UpAxis);
        }

        WantsToClimb = m_ClimbAction.IsPressed();
        WantsToJump |= m_JumpAction.WasPressedThisFrame();
    }

    void FixedUpdate()
    {
        // Up axis depends on gravity in current position
        Gravity = CustomGravity.GetGravity(Body.position, out UpAxis, canBeZero: false);

        UpdateStatus(); 
        AdjustVelocity();

        if (WantsToJump)
        {
            WantsToJump = false;
            Jump();
        }

        Vector3 velocityDelta = CurrState.GetGravityEffect();
        Velocity += velocityDelta;
        Body.velocity = Velocity;
        ClearState();
    }

    void ClearState()
    {
        LastContactNormal = ContactNormal;
        LastSteepNormal = SteepNormal;
        LastConnectionVelocity = ConnectionVelocity;
        GroundContactCount = SteepContactCount = ClimbContactCount = 0;
        ContactNormal = SteepNormal = ClimbNormal = Vector3.zero;
        ConnectionVelocity = Vector3.zero;
        PreviousConnectedBody = ConnectedBody;
        ConnectedBody = null;
    }

    void UpdateStatus()
    {
        StateMachine.UpdateStateMachine();

        StepsSinceLastGrounded += 1;
        StepsSinceLastJump += 1;
        Velocity = Body.velocity;

        if (CurrState.UpdateGroundedState())
        {
            StepsSinceLastGrounded = 0;
            if (StepsSinceLastJump > 1)
            {
                // Here if grounded after jumping
                JumpPhase = 0;
            }
            if (GroundContactCount > 1)
            {
                // Here if multiple collisions with ground were detected (e.g. a moderate crevasse)
                ContactNormal.Normalize();
            }
        }

        StateMachine.UpdateStateMachine();
        StateMachine.CurrState.OnStay();

        if (!ConnectedBody)
            return;

        // Ignore kinematic or light bodies as connections (should be heavier than the sphere)
        if (ConnectedBody.isKinematic || ConnectedBody.mass >= Body.mass)
            UpdateConnectionState();
    }

    void UpdateConnectionState()
    {
        // Check if sphere remained in contact with the same body
        if (ConnectedBody == PreviousConnectedBody)
        {
            // Derive connection velocity from its position change
            Vector3 connectionMovement =
                ConnectedBody.transform.TransformPoint(ConnectionLocalPosition) -
                ConnectionWorldPosition;
            ConnectionVelocity = connectionMovement / Time.deltaTime;
        }

        // Get the connection point in sphere's local space
        // Makes possible to take into account rotation of the connected body 
        ConnectionWorldPosition = Body.position;
        ConnectionLocalPosition = ConnectedBody.transform.InverseTransformPoint(
            ConnectionWorldPosition
        );
    }

    void AdjustVelocity()
    {
        float acceleration = CurrState.MaxAcceleration * (Agility.GetCurrValue() / 100f);
        float speed = CurrState.MaxSpeed * (Speed.GetCurrValue() / 100f);
        Vector3 xAxis = CurrState.xAxis;
        Vector3 zAxis = CurrState.zAxis;

        // Get axis relative to current surface
        xAxis = ProjectDirectionOnPlane(xAxis, ContactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, ContactNormal);

        // Take into account velocity of the connected body (e.g. a moving platform)
        Vector3 relativeVelocity = Velocity - ConnectionVelocity;

        // Project current relative velocity on X and Z axes
        Vector3 oldVelocity = new Vector3(
            Vector3.Dot(relativeVelocity, xAxis),
            0f,
            Vector3.Dot(relativeVelocity, zAxis)
        );
        Vector3 newVelocity = m_MovementInput * speed;

        // Compute adjustment vector and clamp it by the maximum speed change
        Vector3 adjustment = newVelocity - oldVelocity;
        adjustment.y = 0f;
        adjustment = Vector3.ClampMagnitude(adjustment, acceleration * Time.deltaTime);

        // Adjust velocity by adding differences between new and old speeds along the relative axes
        Velocity += xAxis * adjustment.x + zAxis * adjustment.z;
        Velocity = Vector3.ClampMagnitude(Velocity, MaxFallSpeed);
    }

    void Jump()
    {
        bool allowed = CurrState.UpdateJumpState(out Vector3 jumpDirection, ref JumpPhase)
            && Stamina.CurrValue > 0f;

        if (!allowed)
            return;

        Stamina.CurrValue -= JumpStaminaCost;
	    OnJump?.Invoke();

        StepsSinceLastJump = 0;
        JumpPhase += 1;

        // Compute jump velocity from its desired height as: vel.y = sqrt(-2gh) 
        float jumpSpeed = Mathf.Sqrt(2f * Gravity.magnitude * JumpHeight);

        // Add upward bias to jump direction. Affects only jumps from a not flat ground
        // In wall jumping makes possible to beat gravity and reach positive height
        jumpDirection = (jumpDirection + UpAxis).normalized;

        // Project velocity on surface normal by calculating their dot product
        float alignedSpeed = Vector3.Dot(Velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            // Limiting upward velocity
            // Air jumping in quick succession makes possible to exceed the speed of a single jump
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        Velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision) => EvaluateCollision(collision);
    void OnCollisionStay(Collision collision) => EvaluateCollision(collision);

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(UpAxis, normal);
            if (upDot >= minDot)
            {
                GroundContactCount += 1;      // If here, collision happened with ground
                ContactNormal += normal;      // Accumulate normals in case of multiple ground collisions
                ConnectedBody = collision.rigidbody;  // Last evaluated collision will be the connected body
            }
            else
            {
                // If here, this contact doesn't count as ground, check for both steep and climb
                RegisterSteepContact(normal, upDot, collision);
                RegisterClimbContact(normal, upDot, collision);
            }
        }
    }

    void RegisterSteepContact(Vector3 normal, float upDot, Collision collision)
    {
        // Dot product of perfect vertical wall (or steep) is 0
        if (upDot < -0.01f)
            return;

        SteepContactCount += 1;
        SteepNormal += normal;
        if (GroundContactCount == 0)
        {
            // A ground contact is preferred as the connected body
            // Use the slope contact only if there are no ground contacts
            ConnectedBody = collision.rigidbody;
        }
    }

    void RegisterClimbContact(Vector3 normal, float upDot, Collision collision)
    {
        bool climb = WantsToClimb
            && upDot >= MinClimbDotProduct
            && (WhatIsClimbable & (1 << collision.gameObject.layer)) != 0;

        if (!climb) return;

        ClimbContactCount += 1;
        ClimbNormal += normal;
        LastClimbNormal = normal;
        ConnectedBody = collision.rigidbody;
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    public float GetMinDot(int layer)
    {
        // If ground layer is "Stairs" use a different min dot product 
        bool isStairLayer = (WhatIsStairs & (1 << layer)) == 0;
        return isStairLayer 
	        ? MinGroundDotProduct 
	        : MinStairsDotProduct;
    }

    public void PreventSnapToGround()
    {
        // Used when entering in some vertical acceleration trigger zones
        // Basically simulating a jump state to avoid snapping to the ground
        StepsSinceLastJump = -1;
    }
}
