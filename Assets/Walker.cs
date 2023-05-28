using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField]
    List<IK_LegArmature> legs = null; //A list of all the walkers legs

    public legPair[] legPairs;

    public float walkSpeed = 5;

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
        currentPosition = transform.position;
        legPairs = new legPair[legs.Count / 2];
        for (int i = 0, p = 0; i < legPairs.Length - 1; i++, p += 2)
        {
            legPairs[i].legOne = legs[p];
            if (legs[p + 1] != null)
            {
                legPairs[i].legTwo = legs[p + 1];
            }
            else
            {
                legPairs[i].legTwo = legs[p];
            }
            
        }
    }

    private void MoveWalker()
    {
        currentVelocity = -(currentPosition - transform.position) / Time.deltaTime;
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
        for(int i = 0; i < legs.Count; i += 2)
        {
            if (legs[i + 1] != null)
            {
                if (legs[i].IsGrounded() && legs[i + 1].IsGrounded())
                {
                    legs[i].CheckMovement(currentVelocity);
                }
                if (legs[i].IsGrounded() && legs[i + 1].IsGrounded())
                {
                    legs[i + 1].CheckMovement(currentVelocity);
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
