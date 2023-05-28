using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField]
    List<IK_LegArmature> legs = null; //A list of all the walkers legs

    public List<legPair> legPairs;

    public float walkSpeed = 5.0f;
    public float rotateSpeed = 3.0f;

    private Vector3 currentPosition;
    private Vector3 currentVelocity;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        MoveWalker();
        UpdateLegs();
    }

    private void Init()
    {
        //Setup the starting current position
        currentPosition = transform.position;

        //Create the list of legPairs
        legPairs = new List<legPair>();

        //Run a loop through all of the leg armatures in the list of legs
        for (int i = 0, p = 0; i < legs.Count; i += 2, p++)
        {
            //if the leg has a pairing leg available
            if (i + 1 <= legs.Count - 1)
            {
                //Create a new legPair with both leg armatures and add it to the list of leg pairs
                legPairs.Add(new legPair{ legOne = legs[i], legTwo = legs[i + 1] });
            }
            //if there is an uneven amount of legs add the remaining leg to the list of leg pairs
            else
            {
                legPairs.Add(new legPair { legOne = legs[i], legTwo = legs[i] });
            }
        }
    }

    private void MoveWalker()
    {
        //Work out the current velocity of the walker based on it's previous position
        currentVelocity = (transform.position - currentPosition) / Time.deltaTime;
        //Update the walker's current position
        currentPosition = transform.position;

        //Allow player to control walker with arrow keys / WASD
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            transform.position += transform.right * Input.GetAxisRaw("Horizontal") * walkSpeed * Time.deltaTime;
            transform.position += transform.forward * Input.GetAxisRaw("Vertical") * walkSpeed * Time.deltaTime;
        }
    }

    private void UpdateLegs()
    {
        //For all of the pairs of legs
        foreach (legPair pair in legPairs)
        {
            //Check that both legs are grounded before lifting a leg
            if (pair.legOne.IsGrounded() && pair.legTwo.IsGrounded())
            {
                //Update both legs raycasts
                pair.legOne.CheckMovement(currentVelocity);
                pair.legTwo.CheckMovement(currentVelocity);

                //If leg One is further from the target than leg two
                if (pair.legOne.DistanceToTarget() > pair.legTwo.DistanceToTarget())
                {
                    //Update leg one's target positioning
                    pair.legOne.UpdateTarget();
                }
                else
                {
                    //Otherwise update leg two's target positioning
                    pair.legTwo.UpdateTarget();
                }
            }
        }
    }

    public struct legPair
    {
        public IK_LegArmature legOne;
        public IK_LegArmature legTwo;
    }
}
