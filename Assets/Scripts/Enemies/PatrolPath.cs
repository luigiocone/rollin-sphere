using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    [SerializeField, Tooltip("Enemy that will be assigned to this path")]
    EnemyController Enemy;

    [SerializeField, Tooltip("The nodes making up the path")]
    List<Transform> PathNodes;

    [SerializeField, Range(0.01f, 5f), Tooltip("Minimum distance with current destination")]
    float minDistance;

    public int Count => PathNodes.Count;

    public int CurrPathNodeIndex 
    { 
	    get => node;
        set
        {
            if (!IsIndexInvalid(value))
                node = value;
        }
    }

    public Vector3 Destination
    {
        get
        {
            if (lastFrameUpdate != Time.frameCount)
            {
                this.UpdateDestination();
                lastFrameUpdate = Time.frameCount;
            }
            return PathNodes[CurrPathNodeIndex].position;
        }
    }

    int node = 0;
    int lastFrameUpdate = -1;

    void Awake()
    {
        if (Count == 0 || Enemy == null)
        {
            this.enabled = false;
            return;
        }

        Enemy.PatrolPath = this;
        GoToClosestNode(Enemy.transform.position);
    }

    bool IsIndexInvalid(int index)
    {
        return index < 0
            || index >= PathNodes.Count
            || PathNodes[index] == null;
    }

    void UpdateDestination()
    {
        Vector3 position = Enemy.transform.position;
        Vector3 destination = PathNodes[node].position;
        float distance = (position - destination).magnitude;
        if (distance <= minDistance)
            node = (node + 1) % Count;
    }

    public void GoToClosestNode(Vector3 origin)
    {
        float minDistance = Mathf.NegativeInfinity;
        for (int i = 0; i < Count; i++)
        {
            float distance = (PathNodes[i].position - origin).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                node = i;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < PathNodes.Count; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= PathNodes.Count)
            {
                nextIndex -= PathNodes.Count;
            }

            Gizmos.DrawLine(PathNodes[i].position, PathNodes[nextIndex].position);
            Gizmos.DrawSphere(PathNodes[i].position, 0.1f);
        }
    }
}

