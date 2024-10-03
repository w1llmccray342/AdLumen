using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class Death2 : MonoBehaviour
{
    public Vector2 startPosition;
    public Vector2 velocity;
    public float rotation;
    public float angVelocity;
    public bool isTouchingGround = false;
    public int timeTilDeath = 3;
    private int lastWholeSecond = 0;
    // True until the player hits the ground.
    public bool isInSpawnState = true;
    // Start is called before the first frame update
    public void Start()
    {
        // Create SW here: 
        CloneState();
    }

    // Update is called once per frame
   public void Update()
    {
        float timerRef = Mathf.Floor(Time.time);
        print("Current Time with Floor to Int is..." + Mathf.FloorToInt(Time.time));

        // If we see a whole number in Time.time we want to store that as the lastSecond
        if(timerRef > lastWholeSecond + 1)
        {
            lastWholeSecond += 1;
            print("This was my last whole second: " + lastWholeSecond );
            timeTilDeath--;
        }

        if (!isTouchingGround && !(isInSpawnState)) // Expand this if we are in a state that we want the player to be falling. E.g. if we have a drop off somewhere in the game that has the player falling for more than 3 seconds we need to keep this updated.
        {

            if (GetExpiredState() == true) // && Some check to see if the player is 
            {
                GetPreservedState();
                timeTilDeath = 3;
                // We should keep the same movement vector even though it's kind of cool that we don't. If you respawn at a differnet rate of speed
                // You're basically just making portals and doing the thing where you loop through until it cannons you out.
                //lets also preserve our rotation state.

            }
        }

    }

    // As we exit collision.
    public void OnCollisionExit2D()
    {
        // If we're falling and it's not predefined then we're not touching the ground; otherwise we 
        isTouchingGround = false;
        print("I have left my collision zone!");
        // Some function to determine whether the player has ellapsed the allowed TTL


    }

    //Method returns time in seconds.
    //Returns time in seconds
    public bool GetExpiredState()
    {
        if (timeTilDeath == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        isTouchingGround = true;
        Get2DCollisionOptions();
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        Get2DCollisionOptions();
    }

    public void GetPreservedState()
    {
        transform.position = startPosition;
        gameObject.GetComponent<Rigidbody2D>().velocity = velocity;
        gameObject.GetComponent<Rigidbody2D>().rotation = rotation;
        gameObject.GetComponent<Rigidbody2D>().angularVelocity = angVelocity;
    }

    public void CloneState()
    {
        startPosition = transform.position;
        velocity = gameObject.GetComponent<Rigidbody2D>().velocity;
        rotation = gameObject.GetComponent<Rigidbody2D>().rotation;
        angVelocity = gameObject.GetComponent<Rigidbody2D>().angularVelocity;
    }

    public void Get2DCollisionOptions()
    {
        isInSpawnState = false;
        timeTilDeath = 3;
    }

}