using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class BoneLineGenerator : MonoBehaviour
{
    public int length;

    private LineRenderer line;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        line.positionCount = length;
    }

    // Update is called once per frame
    void Update()
    {
        var current = transform;
        for (int i = 0; i < length && current != null && current.parent != null; i++)
        {
            line.SetPosition(i, current.position);
            current = current.parent;
        }
    }
}
