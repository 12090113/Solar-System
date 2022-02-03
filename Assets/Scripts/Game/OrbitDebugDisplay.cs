using UnityEngine;

//[ExecuteInEditMode]
public class OrbitDebugDisplay : MonoBehaviour {

    public int numSteps = 1000;
    public float timeStep = 0.1f;
    public bool usePhysicsTimeStep;

    public bool relativeToBody;
    public CelestialBody centralBody;
    public float width = 100;
    public bool useThickLines;
    public bool drawLines = false;
    public float updateInterval = 100f;

    public float time = 1000000000f;
    private Vector3[][] points;
    private int refFrameIndex = 0;

    void Start () {
        
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            drawLines = !drawLines;
            if (!drawLines)
            {
                HideOrbits();
                time = updateInterval + 1;
            }
        }
        if (drawLines)
        {
            time += Time.deltaTime;
            if (time > updateInterval)
            {
                DrawOrbits(true);
                time = 0f;
            }
            else DrawOrbits(false);
        }
    }

    private void FixedUpdate()
    {
        CelestialBody[] bodies = FindObjectsOfType<CelestialBody>();
        Move(bodies[refFrameIndex].Position - points[refFrameIndex][0]);
    }

    void DrawOrbits (bool updateLines) {
        CelestialBody[] bodies = FindObjectsOfType<CelestialBody> ();
        var virtualBodies = new VirtualBody[bodies.Length];
        var drawPoints = new Vector3[bodies.Length][];
        int referenceFrameIndex = 0;
        Vector3 referenceBodyInitialPosition = Vector3.zero;

        
        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++) {
            virtualBodies[i] = new VirtualBody(bodies[i]);
            virtualBodies[i].velocity = bodies[i].Rigidbody.velocity;
            drawPoints[i] = new Vector3[numSteps];

            if (bodies[i] == centralBody && relativeToBody) {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
                refFrameIndex = i;
            }
        }

        if (updateLines) {
            // Simulate
            for (int step = 0; step < numSteps; step++) {
                Vector3 referenceBodyPosition = (relativeToBody) ? virtualBodies[referenceFrameIndex].position : Vector3.zero;
                // Update velocities
                for (int i = 0; i < virtualBodies.Length; i++) {
                    virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep;
                }
                // Update positions
                for (int i = 0; i < virtualBodies.Length; i++) {
                    Vector3 newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                    virtualBodies[i].position = newPos;
                    if (relativeToBody) {
                        var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                        newPos -= referenceFrameOffset;
                    }
                    if (relativeToBody && i == referenceFrameIndex) {
                        newPos = referenceBodyInitialPosition;
                    }

                    drawPoints[i][step] = newPos;
                }
            }
            points = drawPoints;
        } else
        {
            drawPoints = points;
        }

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++) {
            var pathColour = new Color(1, 0, 0); //bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer> ().sharedMaterial.color;
            //if (bodies[bodyIndex].bodyName.Equals("Ship"))
            //    pathColour = new Color(0, 0, 1);

            if (useThickLines) {
                var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
                lineRenderer.enabled = true;
                lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                lineRenderer.SetPositions (drawPoints[bodyIndex]);
                lineRenderer.startColor = pathColour;
                lineRenderer.endColor = pathColour;
                lineRenderer.widthMultiplier = width;
            } else {
                for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++) {
                    Debug.DrawLine (drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
                }

                // Hide renderer
                var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
                if (lineRenderer) {
                    lineRenderer.enabled = false;
                }
            }

        }
    }

    Vector3 CalculateAcceleration (int i, VirtualBody[] virtualBodies) {
        Vector3 acceleration = Vector3.zero;
        for (int j = 0; j < virtualBodies.Length; j++) {
            if (i == j) {
                continue;
            }
            Vector3 forceDir = (virtualBodies[j].position - virtualBodies[i].position).normalized;
            float sqrDst = (virtualBodies[j].position - virtualBodies[i].position).sqrMagnitude;
            acceleration += forceDir * Universe.gravitationalConstant * virtualBodies[j].mass / sqrDst;
        }
        return acceleration;
    }

    public void Move(Vector3 move)
    {
        if (!drawLines) return;
        for (int step = 0; step < points.Length; step++)
        {
            for (int i = 0; i < points[0].Length; i++)
            {
                points[step][i] += move;
            }
        }
    }

    void HideOrbits () {
        CelestialBody[] bodies = FindObjectsOfType<CelestialBody> ();

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++) {
            var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
            lineRenderer.positionCount = 0;
        }
    }

    void OnValidate () {
        if (usePhysicsTimeStep) {
            timeStep = Universe.physicsTimeStep;
        }
    }

    class VirtualBody {
        public Vector3 position;
        public Vector3 velocity;
        public float mass;

        public VirtualBody (CelestialBody body) {
            position = body.transform.position;
            velocity = body.initialVelocity;
            mass = body.mass;
        }
    }
}