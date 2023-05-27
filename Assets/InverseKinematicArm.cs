using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InverseKinematicArm : MonoBehaviour
{
    #region Armature Base Variables

    [SerializeField]
    private int chainLength = 3; //Length of the armature

    [SerializeField]
    private Transform target; //Position the armature end will try to reach
    [SerializeField]
    private Transform pole; //Position of the pole the armature will attempt to bend towards

    [SerializeField]
    private int iterations = 10; //How many times to iterate the armature's calculations
    [SerializeField]
    private float delta = 1.0f; //Minimum desired calculated distance from the target

    protected float[] bonesLength; //List of each individual bone length
    protected float completeLength; //How long in total the armature is
    protected Transform[] bones; //Reference to each bone's position
    protected Vector3[] positions; //All of the bones actual positions for calculations before being applied

    #endregion

    #region Armature Target Movement Variables

    [SerializeField]
    private float stepTime = 1.0f; //Time taken to complete a step

    [SerializeField]
    private float strideLength = 5.0f; //Total length each step should go

    [SerializeField]
    private float stepHeight = 5.0f; //Amount to raise end of armature during steps

    protected Vector3 previousTargetPosition; //Position of the last target
    protected Vector3 midPointPosition; //Stores the middle position of target movement
    protected Vector3 newTargetPosition; //Position of the new target

    protected Vector3 targetMovement; //Stores amount to move Target by

    protected float horizontalElapsedTime; //Total time passed moving armature target horizontally
    protected float verticalElapsedTime; //Total time passed moving armature target vertically

    protected float horizontalPercentageComplete; //Percentage horizontal movement update is done
    protected float verticalPercentageComplete; //Percentage vertical movement update is done

    #endregion

    #region Armature Initialisation
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
        Transform currentTransform = transform;
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = currentTransform;

            //if not the last bone in the armature add to the bone length + complete length
            if (i != bones.Length - 1)
            {
                bonesLength[i] = (bones[i + 1].position - currentTransform.position).magnitude;
                completeLength += bonesLength[i];
            }

            //Continue up the armature to the root
            currentTransform = currentTransform.parent;
        }
    }
    #endregion

    #region Update Armature

    private void LateUpdate()
    {
        InverseKinematics();
        UpdateTargetPosition();
    }

    #endregion

    #region Calculate Armature Joint Positioning

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

        //Move to bend towards the pole
        if (pole != null)
        {
            //for every position after the end point on the armature
            for (int i = 1; i < positions.Length - 1; i++)
            {
                //Create a plane for working out how to place each bone to bend towards the pole
                Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);

                //Get the positions of both the pole and the current bone on the plane
                Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
                Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);

                //Get the angle around the centre of the plane to move the bone so that it is nearest the pole position
                float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);

                //Move the current bone by the angle given to the position nearest the pole
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        //Set bone positions
        for (int i = 0; i < positions.Length; i++) bones[i].position = positions[i];
    }

    #endregion

    #region Calculate Target Motion

    private void UpdateTargetPosition()
    {
        //If the current target position is not at the new Target position
        if (target.position != newTargetPosition)
        {
            //Update the total elapsed time moving
            horizontalElapsedTime += Time.deltaTime;
            verticalElapsedTime += Time.deltaTime;

            //Calculate how far through the movement the target should be
            horizontalPercentageComplete = horizontalElapsedTime / stepTime;
            verticalPercentageComplete = verticalElapsedTime / (stepTime / 2);

            //Update the horizontal movement position
            targetMovement = Vector3.Lerp(previousTargetPosition, newTargetPosition, horizontalPercentageComplete);

            //If in the first half of the movement raise the target's position
            if (horizontalPercentageComplete < 0.5)
            {
                targetMovement.y = Vector3.Lerp(previousTargetPosition, midPointPosition, verticalPercentageComplete).y;
            }
            //If in the second half of the movement lower the targets position
            else
            {
                targetMovement.y = Vector3.Lerp(midPointPosition, newTargetPosition, verticalPercentageComplete - 1).y;
            }

            //Update the targets position;
            target.position = targetMovement;
            //Return without checking for movement if currently moving
            return;
        }
        CheckMovement();
    }

    private void CheckMovement()
    {
        //Cast a ray from the root of the armature to the floor ahead
        if (Physics.Raycast(bones[0].position, new Vector3(1, -3, 0).normalized, out var hit, completeLength, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
            //If the distance from the ray hit point to the current target position is greater than the set stride length update the new target position
            if (Vector3.Distance(hit.point, target.position) >= strideLength)
            {
                //Get the previous targeted position
                previousTargetPosition = target.position;
                //Get the middle point of the previous target and the new target and add the step height to this
                midPointPosition = (hit.point - previousTargetPosition) / 2 + previousTargetPosition;
                midPointPosition.y += stepHeight;
                
                //===========================================================
                //Insert obstacle detection here to avoid kicking an obstacle
                //===========================================================

                //Update the new targeted position to where the ray hit the ground
                newTargetPosition = hit.point;
                
                //Reset the total elapsed movement time
                horizontalElapsedTime = 0.0f;
                verticalElapsedTime = 0.0f;
            }
        }
    }

    #endregion

    #region Gizmos
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

    #endregion
}