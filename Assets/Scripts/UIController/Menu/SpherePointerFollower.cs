using UnityEngine;
using UnityEngine.InputSystem;

public class SpherePointerFollower : MonoBehaviour
{
    [SerializeField, Range(0.5f, 3f)]
    float RotationFactor = 1f;

    [SerializeField]
	Rect AllowedArea = new(-8f, -4.25f, 16f, 8.5f);

    [SerializeField] Transform StartPosition;

    readonly Vector3 k_RotationPlaneNormal = -Vector3.forward;
    readonly float k_SphereScaleAt768px = 25f;

    float m_BallRadius;
    TrailRenderer m_TrailRenderer;

    void Awake()
    {
        //Cursor.visible = false;

	    m_BallRadius = GetComponent<SphereCollider>().radius;
        m_TrailRenderer = GetComponent<TrailRenderer>();
        m_TrailRenderer.enabled = false;
        RescaleSphere();

        if(StartPosition)
        {
            var start = StartPosition.position;
            this.transform.position = start;
            Mouse.current.WarpCursorPosition(Camera.main.WorldToScreenPoint(start));
        }
    }

    void RescaleSphere()
    { 
        Vector2 screen = new(Screen.width, Screen.height);
        float scale = k_SphereScaleAt768px / 768f * screen.x;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        m_TrailRenderer.enabled = GameSettings.SphereTrail; // TODO Events?

        Vector3 newPosition = NextSpherePosition();

        // Check the distance covered by the ball
        Vector3 movement = newPosition - transform.position;
        float distance = movement.magnitude;
        bool isMoving = distance >= 0.001f;
        if (!isMoving) 
	        return;

        // Check if the ball movement does not cross the rect area, else adjust it
        if (!AllowedArea.Contains(newPosition))
        {
            newPosition.x =
				Mathf.Clamp(newPosition.x, AllowedArea.xMin, AllowedArea.xMax);
			newPosition.y =
				Mathf.Clamp(newPosition.y, AllowedArea.yMin, AllowedArea.yMax);

            movement = newPosition - transform.position;
            distance = movement.magnitude;
        }
        transform.position = newPosition;
        RotateByDistance(movement, distance);
    }

    Vector3 NextSpherePosition()
    { 
        Vector2 pointer = 
	        Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return new Vector3(pointer.x, pointer.y, transform.position.z);
    }

    void RotateByDistance(Vector3 movement, float distance)
    { 
        // Compute the rotation angle using the covered distance and the ball radius
        float angle = distance * RotationFactor * (180f / Mathf.PI) / m_BallRadius;

        Vector3 rotationAxis = Vector3.Cross(k_RotationPlaneNormal, movement).normalized;
        Quaternion rotation = transform.localRotation;
        rotation = Quaternion.Euler(rotationAxis * angle) * rotation;
        transform.localRotation = rotation;
    }

    void OnDrawGizmos()
    {
        // Draw allowed moving area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(AllowedArea.center, AllowedArea.size);
    }
}

