using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InverseKinematics : MonoBehaviour
{
    public float scale = 0.1f;
    public int chainLength = 3;

    public Transform target;
    public Transform pole;

    public int iterations = 10;
    public float delta = 0.001f;

    private float completeLength;

    protected Transform[] bones;
    protected Vector3[] positions;
    protected float[] boneLength;

    protected Vector3[] startDirections;
    protected Quaternion[] startRotations;

    protected Quaternion targetRotationStart;
    protected Quaternion rootRotationStart;



    void Start()
    {
        Init();
    }

    void Update()
    { 
        
    }

    void Init()
    {
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        boneLength = new float[chainLength];

        startDirections = new Vector3[chainLength + 1];
        startRotations = new Quaternion[chainLength + 1];


        if(target == null)
        {
            // If no targets are setup, create a new target
            target = new GameObject(gameObject.name + " Target").transform;
            target.position = transform.position;
        }

        targetRotationStart = target.rotation;

        var current = transform;
        completeLength = 0;


        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotations[i] = current.rotation;
            

            if (i == bones.Length - 1)
            {
                startDirections[i] = target.position - current.position;
            }
            else
            {
                startDirections[i] = bones[i + 1].position - current.position;

                boneLength[i] = (bones[i + 1].position - current.position).magnitude;
                completeLength += boneLength[i];


                Debug.Log(boneLength[i] + "  " + completeLength);
            }
            current = current.parent;
        }
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    void ResolveIK()
    {
        if (target == null)
            return;

        if (boneLength.Length != chainLength)
            Init();

        // setting up the rootbone directions
        var rootRotation = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        var rootRotDiff = rootRotation * Quaternion.Inverse(rootRotationStart);


        // Get the position
        for (int i = 0; i < bones.Length; i++)
        {
            positions[i] = bones[i].position;
        }

        if((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
        {
            // target length is greater than the total length

            // calculate the direction
            var direction = (target.position - bones[0].position).normalized;

            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = boneLength[i - 1] * direction + positions[i - 1];
            }
        }
        else
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // back algorithms
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                    {
                        positions[i] = target.position;
                    }
                    else
                    {
                        // setting the position based on the 
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * boneLength[i];
                    }
                }


                // forward algorithm
                for (int i = 1; i < positions.Length-1; i++)
                {
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * boneLength[i-1];
                }


                // Middle joints should move towards a plane
                if (pole != null)
                {
                    for (int i = 1; i < positions.Length-1; i++)
                    {
                        var plane = new Plane((positions[i + 1] - positions[i - 1]), positions[i - 1]);
                        var projectedPole = plane.ClosestPointOnPlane(pole.position);
                        var projectedBone = plane.ClosestPointOnPlane(positions[i]);

                        //calculate the angle
                        var angle = Vector3.SignedAngle((projectedBone - positions[i - 1]), (projectedPole - positions[i - 1]), plane.normal);
                        positions[i] = positions[i - 1] + Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]);
                    }
                }


                //if the last position is closer to the target (closer than the delta), we stop
                if((positions[positions.Length-1] - target.position).sqrMagnitude < delta * delta)
                {
                    break;
                }

                


            }
        }


        // Set the position and rotations
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                bones[i].rotation = target.rotation * Quaternion.Inverse(targetRotationStart) * startRotations[i];
            }
            else
            {
                bones[i].rotation = Quaternion.FromToRotation(startDirections[i], positions[i + 1] - positions[i]) * startRotations[i];
            }


            bones[i].position = positions[i];
        }

    }


    private void OnDrawGizmos()
    {
        var current = transform;
        for (int i = 0; i < chainLength && current!=null && current.parent != null; i++)
        {
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.position, current.parent.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);

            current = current.parent;
        }
    }
}
