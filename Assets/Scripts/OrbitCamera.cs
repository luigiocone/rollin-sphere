using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour {

    [SerializeField, Tooltip("What to focus on")]
    Transform Focus = default;

    [SerializeField, Range(1f, 20f), Tooltip("Distance from the focus")]
    float Distance = 5f;

    [SerializeField, Min(0f), Tooltip(
        "Max distance between ideal and current focus point. " + 
        "Camera will move faster only when that distance is greater than this value"
    )]
    float FocusRadius = 1f;

    [SerializeField, Range(0f, 1f), Tooltip(
        "The lower this value, the slower will be the centering of the camera on the focus"
    )]
    float FocusCentering = 0.5f;

    [SerializeField, Range(1f, 360f), Tooltip("Sensitivity of manual and automatic rotation (degrees per second)")]
    float RotationSpeed = 90f;

    [SerializeField, Range(-89f, 89f)]
    float MinVerticalAngle = -45f, MaxVerticalAngle = 45f;

    [SerializeField, Min(0f), Tooltip("After how many seconds the camera return to the back of the focus")]
    float AlignDelay = 5f;

    [SerializeField, Range(0f, 90f), Tooltip("Camera alignment at full speed if angle delta is greater than this value")]
    float AlignSmoothRange = 45f;

    [SerializeField, Min(0f), Tooltip("How fast camera adjust its up vector (degrees per second)")]
    float UpAlignmentSpeed = 360f;

    [SerializeField, Tooltip("What should be considered as camera obstruction")]
    LayerMask ObstructionMask = -1;

    Camera m_RegularCamera;
    InputManager m_InputManager;
    Vector2 m_Input;
    Vector3 m_FocusPoint, m_PreviousFocusPoint;
    float m_LastManualRotationTime;
    Quaternion m_GravityAlignment = Quaternion.identity;
    Quaternion m_OrbitRotation;

    // Camera orientation can be described with two orbit angles (vertical (x) and horizontal (y) orientation)
    Vector2 m_OrbitAngles = new(45f, 0f);

    // BoxCast requires a 3D vector with the half extends of a box (x, y, z = width, height, depth)
    Vector3 CameraHalfExtends {
        get {
            Vector3 halfExtends;
            halfExtends.y =
                m_RegularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * m_RegularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * m_RegularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    void OnValidate () {
        if (MaxVerticalAngle < MinVerticalAngle)
            MaxVerticalAngle = MinVerticalAngle;
    }

    void Awake () {
        m_RegularCamera = GetComponentInChildren<Camera>();
        m_FocusPoint = Focus.position;
        transform.localRotation = m_OrbitRotation = Quaternion.Euler(m_OrbitAngles);
    }

    void Start()
    {
        m_InputManager = FindObjectOfType<InputManager>();
        m_InputManager.SphereInputActions.Player.Look.performed += OnLook;
        m_InputManager.SphereInputActions.Player.Look.canceled += OnLook;
    }

    void OnDestroy()
    {
        m_InputManager.SphereInputActions.Player.Look.performed -= OnLook;
        m_InputManager.SphereInputActions.Player.Look.canceled -= OnLook;
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        var vec = ctx.ReadValue<Vector2>();
        m_Input = new(-vec.y, vec.x);
    }

    void LateUpdate () {
        // Camera's position is updated in LateUpdate() in case anything moves the focus in any Update()
        UpdateGravityAlignment();
        UpdateFocusPoint();

        // Rotation methods returns true only if the angles have changed
        if (ManualRotation() || AutomaticRotation()) {
            ConstrainAngles();
            m_OrbitRotation = Quaternion.Euler(m_OrbitAngles);
        }
        Quaternion lookRotation = m_GravityAlignment * m_OrbitRotation;

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = m_FocusPoint - lookDirection * Distance;

        Vector3 rectOffset = lookDirection * m_RegularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = Focus.position;                     // Cast from the ideal focus point
        Vector3 castLine = rectPosition - castFrom;            // Cast to the near plane box position
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        // If something was hit, pull camera in front of the obstruction
        if (Physics.BoxCast(
            castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
            lookRotation, castDistance, ObstructionMask,
            QueryTriggerInteraction.Ignore
        )) {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }
        
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateGravityAlignment () {
        Vector3 fromUp = m_GravityAlignment * Vector3.up;
        Vector3 toUp = CustomGravity.GetUpAxis(m_FocusPoint);
        
        // abs(dot) can be slightly greater than 1 due to precision limitations 
        // clamp it since arccos(x) accepts only x in [-1, 1]
        float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
        // Get angle between up vectors and max allowed angle rotation for this frame
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;        
        float maxAngle = UpAlignmentSpeed * Time.deltaTime;

        // Rotate gravityAlignement towards the new up vector
        Quaternion newAlignment = Quaternion.FromToRotation(fromUp, toUp) * m_GravityAlignment;

        // If "angle" is sufficiently small there is no need for interpolation
        if (angle <= maxAngle) {
            m_GravityAlignment = newAlignment;
        }
        else {
            m_GravityAlignment = Quaternion.SlerpUnclamped(
                m_GravityAlignment, newAlignment, maxAngle / angle
            );
        }
    }

    void UpdateFocusPoint () {
        Vector3 targetPoint = Focus.position;   // Ideal focus point
        if (FocusRadius == 0f) {                // Rigid camera movement 
            m_FocusPoint = targetPoint;
            return;
        }

        m_PreviousFocusPoint = m_FocusPoint;        // Current focus point
        float distance = Vector3.Distance(targetPoint, m_FocusPoint);
        float t = 1f;
        if (distance > 0.01f && FocusCentering > 0f) {
            // Compute an exponential interpolator
            t = Mathf.Pow(1f - FocusCentering, Time.deltaTime);
        }
        if (distance > FocusRadius) {
            // Choose the minimum interpolator
            t = Mathf.Min(t, FocusRadius / distance);
        }
        // Pull the focus point towards the target
        m_FocusPoint = Vector3.Lerp(targetPoint, m_FocusPoint, t);
    }

    bool ManualRotation () {
        m_Input *= GameSettings.CameraSensitivity;

        // Considering the input only if greater than an epsilon
        const float e = 0.001f;
        if (m_Input.x < -e || m_Input.x > e || m_Input.y < -e || m_Input.y > e) {
            m_OrbitAngles += RotationSpeed * Time.deltaTime * m_Input;
            m_LastManualRotationTime = Time.deltaTime;
            return true;
        }
        return false;
    }

    bool AutomaticRotation () {
        // Check if "alignDelay" seconds have passed
        if (Time.deltaTime - m_LastManualRotationTime < AlignDelay)
            return false;

        // Compute the focus movement to later align the camera
        Vector3 alignedDelta =
            Quaternion.Inverse(m_GravityAlignment) *     // The inverse represents the opposite rotation
            (m_FocusPoint - m_PreviousFocusPoint);
        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

        // Check if the movement was significant (square magnitude is more efficient)
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f) {
            return false;
        }

        Vector2 normalizedMovement = movement / Mathf.Sqrt(movementDeltaSqr);       // same as ".normalized()", more efficient
        float headingAngle = GetAngle(normalizedMovement);  
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(m_OrbitAngles.y, headingAngle));  // Difference between current and desired angle
        float rotationChange =
            RotationSpeed * Mathf.Min(Time.deltaTime, movementDeltaSqr);
        if (deltaAbs < AlignSmoothRange) {
            // Rotate slowly if the angle is not greater than a threshold
            rotationChange *= deltaAbs / AlignSmoothRange;
        }
        else if (180f - deltaAbs < AlignSmoothRange) {
            // Previous case covers if focus moves away from the camera, here if it moves toward
            rotationChange *= (180f - deltaAbs) / AlignSmoothRange;
        }
        m_OrbitAngles.y =
            Mathf.MoveTowardsAngle(m_OrbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    void ConstrainAngles () {
        // Vertical orbit clamping
        m_OrbitAngles.x =
            Mathf.Clamp(m_OrbitAngles.x, MinVerticalAngle, MaxVerticalAngle);

        // Horizontal orbit has no limits, but ensure that the angle stays inside [0, 360] 
        if (m_OrbitAngles.y < 0f) {
            m_OrbitAngles.y += 360f;
        }
        else if (m_OrbitAngles.y >= 360f) {
            m_OrbitAngles.y -= 360f;
        }
    }

    static float GetAngle (Vector2 direction) {
        // Convert a 2D direction to an angle
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }
}
