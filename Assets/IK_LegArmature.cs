using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_LegArmature : InverseKinematicArm
{
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

    protected bool isGrounded; //Contains if the foot of the armature is grounded or not

    #endregion

    #region Leg Armature Initalisation

    protected override void Init()
    {
        base.Init();
        isGrounded = false;
    }

    #endregion

    #region Update Armature

    protected override void LateUpdate()
    {
        base.LateUpdate();
        UpdateTargetPosition();
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
        }
        else
        {
            //Set that the foot is grounded
            isGrounded = true;
        }
    }

    public void CheckMovement()
    {
        //Cast a ray from the root of the armature to the floor ahead
        if (Physics.Raycast(bones[0].position, new Vector3(2, -3, 0).normalized, out var hit, completeLength, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
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

                //Set that the foot is no longer grounded
                isGrounded = false;
            }
        }
    }

    #endregion

    #region Leg Getters

    public bool IsGrounded()
    {
        return isGrounded;
    }

    #endregion

    #region Gizmos

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        //Set base of the armature as staring pos
        Transform startPos = transform;

        //For every node in the chain
        for (int i = 0; i < chainLength - 1; i++)
        {
            //update the starting position to be the parent of the current start position to get the root node
            startPos = startPos.parent;
        }
        //Draw a line from the root node to the end of the complete raycast
        Gizmos.DrawLine(startPos.position, startPos.position + new Vector3(2, -3, 0).normalized * completeLength);
    }

    #endregion
}
