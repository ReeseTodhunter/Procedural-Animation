using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This base script was initally made using the DitzelGames Inverse Kinematics in Unity video found here:
//https://www.youtube.com/watch?v=qqOAzn05fvk&t=1558s
//Github repo: https://github.com/ditzel/SimpleIK
//This script has been rewritten and further adapted personally

public class IK_Armature : MonoBehaviour
{
    #region Armature Base Variables

    [SerializeField]
    protected int chainLength = 3; //Length of the armature

    [SerializeField]
    protected GameObject bone; //Bone for visuals
    [SerializeField]
    protected float boneThickness = 0.5f;

    [SerializeField]
    protected Transform target; //Position the armature end will try to reach
    [SerializeField]
    protected Transform pole; //Position of the pole the armature will attempt to bend towards

    [SerializeField]
    protected int iterations = 10; //How many times to iterate the armature's calculations
    [SerializeField]
    protected float delta = 1.0f; //Minimum desired calculated distance from the target

    protected List<GameObject> visualBones; //A list of all of the visual bones

    protected float[] bonesLength; //List of each individual bone length
    protected float completeLength; //How long in total the armature is
    protected Transform[] bones; //Reference to each bone's position
    protected Vector3[] positions; //All of the bones actual positions for calculations before being applied

    #endregion

    #region Armature Initialisation
    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        //Initalize array
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        bonesLength = new float[chainLength];

        visualBones = new List<GameObject>();

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

    protected virtual void LateUpdate()
    {
        InverseKinematics();
        UpdateVisuals();
    }

    #endregion

    #region Armature Visual

    private void UpdateVisuals()
    {
        //If there is a bone to use
        if (bone != null)
        {
            //Set the starting current transform
            var current = this.transform;

            //For each node on the armature with a parent
            for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
            {
                //If there is not a bone in the list for the current node
                if (visualBones.Count - 1 < i)
                {
                    //Instantiate a new bone and add it to the list
                    visualBones.Add(Instantiate(bone, current));
                }
                //Position the current bone inbetween the current node and it's parent node
                visualBones[i].transform.position = current.position + ((current.parent.position - current.position) / 2);
                //Rotate the current bone to the rotation towards the parent node
                visualBones[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position);
                //Scale the bone based on the selected bone thickness and the distance to the current node's parent
                visualBones[i].transform.localScale = new Vector3(boneThickness, Vector3.Distance(current.parent.position, current.position), boneThickness);
                //Update the current node to the parent node for next loop
                current = current.parent;
            }
        }
    }

    #endregion

    #region Calculate Armature Joint Positioning

    private void InverseKinematics()
    {
        //Check that a target point is set
        if (target == null) return;

        //If not initalised properly reinitalise
        if (bonesLength.Length != chainLength) Init();

        //Get all bones positions
        for (int i = 0; i < bones.Length; i++) positions[i] = bones[i].position;

        //Calculate new bone positions
        //If the target position is further away than the armature can reach move bones to the closest point they can reach
        if ((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
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

    #region Gizmos
    protected virtual void OnDrawGizmos()
    {
        Matrix4x4 baseMatrix = Gizmos.matrix;
        var current = this.transform;
        for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
        {
            //for each node in the armature draw a wire box between them as connectors
            float scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Gizmos.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }

        Gizmos.color = Color.white;
        Gizmos.matrix = baseMatrix;
    }

    #endregion
}
