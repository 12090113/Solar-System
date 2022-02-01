using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour {

    public float distanceThreshold = 1000;
    List<Transform> physicsObjects;
    Ship ship;
    PlayerController player;
    Camera playerCamera;
    OrbitDebugDisplay orbitLines;

    public event System.Action PostFloatingOriginUpdate;

    void Awake () {
        var ship = FindObjectOfType<Ship> ();
        var player = FindObjectOfType<PlayerController> ();
        var bodies = FindObjectsOfType<CelestialBody> ();

        physicsObjects = new List<Transform> ();
        physicsObjects.Add (ship.transform);
        physicsObjects.Add (player.transform);
        foreach (var c in bodies) {
            physicsObjects.Add (c.transform);
        }

        playerCamera = Camera.main;

        orbitLines = FindObjectOfType<OrbitDebugDisplay>();
    }

    void LateUpdate () {
        UpdateFloatingOrigin ();
        PostFloatingOriginUpdate?.Invoke();
    }

    void UpdateFloatingOrigin () {
        Vector3 originOffset = playerCamera.transform.position;
        float dstFromOrigin = originOffset.magnitude;

        if (dstFromOrigin > distanceThreshold) {
            foreach (Transform t in physicsObjects) {
                t.position -= originOffset;
            }
            orbitLines.Move(-originOffset);
        }
    }

}