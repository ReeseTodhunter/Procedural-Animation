using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InverseKinematicArm : MonoBehaviour
{
    [SerializeField]
    private int chainLength = 3; //Length of the armature

    [SerializeField]
    private Transform target; //Position the armature end will try to reach

    [SerializeField]
    private int iterations = 10; //How many times to iterate the armature's calculations
    [SerializeField]
    private float delta = 1.0f; //Minimum desired calculated distance from the target

    protected float[] bonesLength;
    protected float completeLength;
    protected Transform[] bones;
    protected Vector3[] positions;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        //Initalize array
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        bonesLength = new float[chainLength];

        completeLength = 0;

        //Initalize array data
        var current = transform;
        for (var i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;

            //if not the last bone in the armature add to the bone length + complete length
            if (i != bones.Length - 1)
            {
                bonesLength[i] = (bones[i + 1].position - current.position).magnitude;
                completeLength += bonesLength[i];
            }

            current = current.parent;
        }
    }

    private void LateUpdate()
    {
        InverseKinematics();
    }

    private void InverseKinematics()
    {
        //Check that a target point is set
        if (target == null) return;

        //If not initalised properly reinitalise
        if(bonesLength.Length != chainLength) Init();

        //Get all bones positions
        for (int i = 0; i < bones.Length; i++) positions[i] = bones[i].position;

        //Calculate new bone positions
        //If the target position is further away than the armature can reach move bones to the closest point they can reach
        if((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
        {
            Vector3 direction = (target.position - positions[0]).normalized;
            //Set position of all bones after the root bone
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
            }
        }
        else
        {
            //Iterate over the bone placement based on the number of set iterations
            for (int iter = 0; iter < iterations; iter++)
            {
                //Move bones from the end of the armature into positions to touch the target position
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    //Set the end of the armature to the target position
                    if (i == positions.Length - 1) positions[i] = target.position;

                    //Set the bones to be within the range of their lengths
                    else positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLength[i];
                }

                //Return bones to the correct lengths calculating positions from the root bone instead this time
                for (int i = 1; i < positions.Length; i++)
                {
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];
                }

                //check if the end of the armature is within the minimum distance
                if ((positions[positions.Length - 1] - target.position).sqrMagnitude < delta * delta) break;
            }
        }

        //Set bone positions
        for (int i = 0; i < positions.Length; i++) bones[i].position = positions[i];
    }

    private void OnDrawGizmos()
    {
        var current = this.transform;
        for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
        {
            //for each node in the armature draw a wire box between them as connectors
            float scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.red;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
    }
}
