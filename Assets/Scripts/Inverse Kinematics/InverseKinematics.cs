using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InverseKinematics : MonoBehaviour
{
    public float scale = 0.1f;
    public int chainLength = 3;

    public Transform target;

    public Transform[] bones;
    public Vector3[] positions;
    public float[] boneLength;

    public int iterations = 10;
    public float delta = 0.001f;

    private float completeLength;

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


        var current = transform;
        completeLength = 0;


        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;

            

            if (i == bones.Length - 1)
            {

            }
            else
            {
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


                //if the last position is closer to the target (closer than the delta), we stop
                if((positions[positions.Length-1] - target.position).sqrMagnitude < delta * delta)
                {
                    break;
                }

            }
        }


        // Set the position
        for (int i = 0; i < positions.Length; i++)
        {
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
