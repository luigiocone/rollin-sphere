using UnityEngine;
using System.Collections.Generic;

public static class CustomGravity {

    static List<GravitySource> sources = new List<GravitySource>();

    public static void Register (GravitySource source) {
        Debug.Assert(
            !sources.Contains(source),
            "Duplicate registration of gravity source", source
        );
        sources.Add(source);
    }

    public static void Unregister (GravitySource source) {
        Debug.Assert(
            sources.Contains(source),
            "Unregistration of unknown gravity source", source
        );
        sources.Remove(source);
    }
    
    public static Vector3 GetGravity (Vector3 position, bool canBeZero = true) {
        // Return a gravity vector given a position in world space
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++) {
            g += sources[i].GetGravity(position);
        }

        if (!canBeZero && g == Vector3.zero)
            g = Physics.gravity;

        return g;
    }

    public static Vector3 GetGravity (Vector3 position, out Vector3 upAxis, bool canBeZero = true) {
        Vector3 g = GetGravity(position, canBeZero);
        upAxis = -g.normalized;
        return g;
    }

    public static Vector3 GetUpAxis (Vector3 position, bool canBeZero = true) {
        Vector3 g = GetGravity(position, canBeZero);
        return -g.normalized;
    }
}
