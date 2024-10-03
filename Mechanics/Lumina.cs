using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using UnityEditor.SearchService;

public class Lumina : MonoBehaviour
{

    // Dev helpers
    // By default we expect this to be false
    // If it ever switches to true then this mechanic is broken

    public GameObject playerToAvoid;

    public bool isTouchingPlayer = false;
    public bool enableDebug = false;
    public bool isChargingLum = false;
    public bool disableRotate = false;
    public bool inFlight = false;
    public bool onCooldown = false;
    public bool moveToNextStep = false;
    


    // Moddable.
    public float rotateLuminaSpeed = 1.0f;
    public float rotatePlayerSpeed = 1.0f;
    public float accelerationFactor = 1.0f;
    public float luminaCharge = 0.0f;
    public float luminaChargeStep = 1.5f;
    public float massLum;
    public float breakFactor = 2.0f;
    public float maxDeltaY = 0.1f;
    public float maxDeltaX = 0.1f;
    public float minDeltaY;
    public float minDeltaX;
    public float deltaX = 0.0f;
    public float deltaY = 0.0f; 

    public float LumAngVelocity;
    public float force;

    public Vector3 LumVelocity;



    // This never changes. Check to see how long held down. 
    public float maxTimeHeldDown = 3.0f;
    public float restoreRotationSpeed;


    // Enums
    public enum RotationType { Orbital = 1, Binary = 2 }



    // Start is called before the first frame update
    // Call necessary GetComponents here.
    public void Start()
    {

        // Modifiers shouldn't affect most of this.

        // Set min deltaY && deltaX
        minDeltaX = -maxDeltaX;
        minDeltaY = -maxDeltaY; 

        restoreRotationSpeed = rotateLuminaSpeed;
        LumAngVelocity = gameObject.GetComponent<Rigidbody2D>().angularVelocity;
        LumVelocity = gameObject.GetComponent<Rigidbody2D>().velocity;
        playerToAvoid = GameObject.Find("AveryTestObj");
        massLum = gameObject.GetComponent<Rigidbody2D>().mass;
    }

    // Update is called once per frame
    public void Update()
    {
        // If you release space early or you release it after max time the same behavior should happen.
        // Breaking? Oh we're like not supposed to be there lol

        if (Input.GetKeyUp(KeyCode.Space))
        {
            disableRotate = true;
            moveToNextStep = true;
            onCooldown = true;
            isChargingLum = false;
        }

        if(Input.GetKey(KeyCode.Space))
        {
            isChargingLum = true;   
        }

        // Default to this
        // && Use coroutine to manage which rotation we want later. 
        
        
    }

    public void FixedUpdate()
    {
        if (enableDebug)
        {
            EnableDebugLum();
        }

        GetTimeChargedLum();
        RotateAroundLum();
        //BinaryRotationLum();
    }


    // Can collide with other objects like background and enemies but doesn't ever intersect with Avery
    // Doesn't check if colliding after. We need to check that in.
    public void OnCollisionEnter2D(Collision2D collision)
    {
        CheckIfIntersecting(collision);
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        CheckIfIntersecting(collision);
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (!playerToAvoid.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }

    // Function to simply make Lumina gravitate and move around the player character.
    // We get a base position for Lumina and roate her around the player character consistently. 
    // If we are colliding with an object then we want to move to a static state
    // Then we switch back.
    // Though we CAN and should make a rotate class we'll do so outside of this
    // This rotate script is specific to Lumina as others will not follow the same behavior as Lumina acts as a sort of companion.
    public void RotateAroundLum()
    {

        if (!disableRotate)
        {
            GenerateRotation(gameObject, playerToAvoid, (int)RotationType.Orbital, false, rotateLuminaSpeed, rotatePlayerSpeed);
        }
    }


    // If we want Avery and Lumina to rotate around each other for whatever reason we can do it this way. So just call the base rotation on Lumina and then do the same for Avery around Lumina
    public void BinaryRotationLum()
    {
        GenerateRotation(gameObject, playerToAvoid, (int)RotationType.Binary, false, rotateLuminaSpeed, rotatePlayerSpeed);
    }

    public void CheckIfIntersecting(Collision2D collision)
    {
        // If colliding with player to avoid
        if (collision.gameObject.name == playerToAvoid.name)
        {

            if (enableDebug)
            {
                print("Debug: Lumina - Touching Player!! Fix the script involved in this or change a setting in editor to make sure this isn't the case!");
            }

            isTouchingPlayer = true;
        }

        else
        {
            isTouchingPlayer = false;
        }
    }

    // #1: Singular, #2: Binary

    public void GenerateRotation(GameObject objectToRotate, GameObject objectToAvoid, int typeRotation, bool isRotationBackwards, float speedToRotate, float speedToRotateObj2)
    {
        Vector3 vectorToRotate;
        Vector3 objectToAvoidPos = objectToAvoid.transform.position;
        Vector3 objectToRotatePos = objectToRotate.transform.position;

        // Get if the rotation is a backwards rotation
        if (isRotationBackwards)
        {
            vectorToRotate = Vector3.back;
        }

        else
        {
            vectorToRotate = Vector3.forward;
        }

        // Normalize the vector
        vectorToRotate.Normalize();

        // If a Orbital Rotation
        if (typeRotation == (int)RotationType.Orbital)
        {

            objectToRotate.transform.Rotate(vectorToRotate, speedToRotate * Time.deltaTime);
            objectToRotate.transform.RotateAround(objectToAvoidPos, Vector3.forward, speedToRotate * Time.deltaTime);

        }

        // If a Binary Rotation we rotate both objects!
        // Is there a way to change their rotation if we want something as Binary and want the vector around
        else if (typeRotation == (int)RotationType.Binary)
        {
            objectToRotate.transform.Rotate(vectorToRotate, speedToRotate * Time.deltaTime);
            objectToRotate.transform.RotateAround(objectToAvoidPos, vectorToRotate, speedToRotate * Time.deltaTime);

            objectToAvoid.transform.Rotate(vectorToRotate, speedToRotateObj2 * Time.deltaTime);
            objectToAvoid.transform.RotateAround(objectToRotatePos, vectorToRotate, speedToRotateObj2 * Time.deltaTime);
        }


    }

    /// <summary>
    /// Method that lets us slingshot lumina a desired distance.
    /// That speed and force is determined by how long the spacebar is being held.
    /// </summary>
    
    // Currently not using this but we can use force stored here for some quick math.
    public float SlingshotLum(float lumAcceleration, float mass)
    {
        // Max value for force. // Find our acceleration.
        // Force = Mass * Accelleration
        force = mass * lumAcceleration;
        return force;
    }

    // Use a coroutine.
    // Reverse this falloff.
    public void GetTimeChargedLum()
    {
        float timer = 0.0f;

        // Not really a way to check if this is getting updated correctly.
        timer += Time.deltaTime;
        if (timer > 0.0f)
        {
            // Use deltatime here.
            // 


            // Start tracking how long this key is pressed from the frame it is pressed and held down.

            // Was while loop breaking game loop?
           
                // Lumina charge is equal to the exact time elapsed. So use Time.deltaTime to track this.
                // Exact.
                // Once we know how long we've held down the space bar let's add that to our charge time.

                // As we charge lumina we should probably go ahead and start blurring her too to give the illusion that we're going really fast.
                // We can maybe use PPE for this.

               

                // If a second has passed, then we consider this logic and step our acceleration factor. // Conditionally checks if luminaCharge is greater than maxTimeHeldDown if it is we don't increment further.
                if (timer % 1 >= 0 && timer <= 1 && (!(luminaCharge >= maxTimeHeldDown)) && isChargingLum)
                {
                    luminaCharge += timer;
                    rotateLuminaSpeed += (rotateLuminaSpeed * (luminaChargeStep + accelerationFactor) * Time.deltaTime);
                }
        }
            // Well that worked lol
            // If our object doesn't have rotational speed? Launch that fucker.

        if (disableRotate)
        {
            StartCoroutine(LaunchLumina());
        }
            
    }

    // Method to restore original state after we have hit our correct endpoint. // Might need to step down for this instead.
    public void RestoreLuminaVelocity()
    {

    }

    // Break orbital method
    /// <summary>
    /// Way for us to break out of current orbitals
    /// </summary>
    // Keep current velocity.
    // When called, launch the 
    public void BreakOrbit()
    {
      
        // I think what's happening here is we're not conserving the ORIGINAL speed we had
        // So it instead is just giving us a relative force with the impuls
        // We need to add ontop of whatever or angular and normal velocity are.
        // Our vector shifts upwards each frame.
        //gameObject.transform.position += Vector3.right * Time.deltaTime;

        // Simple print statement to check we're working with this function.
        print("We are breaking out of orbit.");
        // Move vector forward if we break orbit.

        ConserveMomentum(gameObject);

    }

    // Need references to the methods.
    // Check if these are running before doing anything
    // Unimplemented but this will be the "brain" behind our follower.
    // It really only has two rules:
    // 1: Never touch the player
    // 2: Stay within bounds.

    // So we need to define some boundary for it.
    // Move this to LumAI (Literally no reason to keep this here with the LuminaOribt script.)
    /// <summary>
    /// Method we should be calling in which Lumina is moving around the player.
    /// Basically, we use this to find the best path for Lumina to take and depending on best path available we call the necessary movement functions.
    /// Simple AI.
    /// </summary>
    public void LuminaAvoid()
    {

    }

    // Return an a string in message array when called.
    // Think about how to do this later, not as necessary.
    public void EnableDebugLum()
    {
        // 
        print("Debug: Lumina - OKAY!");
        // UnityEngine.Debug.LogError("DEBUG: Could not determine rotation type. SEE LUMINA.CS for scripting issues"); (False positive make sure we check that these functions are running BEFORE we flag this.)
        print("At charge time the force of Lumina was... " + force);
        print("Lumina charge with Time += Time.deltaTime" + (luminaCharge + Time.deltaTime));
        print("Lumina Velocity is..." + LumVelocity);
        print("Lumina Charge Time at  terminal execution was at..." + luminaCharge);
        print("Lumina Charge Time at execution was at... " + luminaCharge);
    }

    public void ResetLumina()
    {

        luminaCharge = 0;

        // For however long it takes restore Lumina to her original speed.
        // If we set it back normally then it appears as a broken animation
        // We don't want that
        //  // Instead we'll call lumina to move back to the player after a few seconds. We can also randomize the second count to make it more interesting.
        // We can now orbit again!


        rotateLuminaSpeed -= (rotateLuminaSpeed + restoreRotationSpeed);
        disableRotate = false;
    }


    // We call 
    public IEnumerator LaunchLumina()
    {

        if (isChargingLum && Input.GetKeyUp(KeyCode.Space) == true && luminaCharge <= maxTimeHeldDown || isChargingLum && Input.GetKeyUp(KeyCode.Space) == true && luminaCharge >= maxTimeHeldDown)
        {
           
            disableRotate = true;

        }


        if (moveToNextStep && (onCooldown))
        {
            BreakOrbit();
            inFlight = true;
        }
        // Break the orbit of Lumina


        // Wait two seconds
        yield return new WaitForSeconds(2); // Travel for two seconds
        //ResetLumina(); // Reset speed.

        StopCoroutine(LaunchLumina());
        onCooldown = false;
        // Wait five seconds to get next input.
    }

    // Logic to handle what type of rotation we want can go here
    // Might be unnecessary.
    public IEnumerable RotateLumina()
    {
        yield return new WaitForSeconds(1);
    }

    /// <summary>
    /// Conserve momentum. 
    /// Basically we want a method that transfers our current state over
    /// To the object we want to affect. That means, speed, rotation, everything gets accounted for.
    /// </summary>
    public void ConserveMomentum(GameObject obj)
    {

        Vector2 movement;
        // This is gonna speed the fuck off.
        // Actually didn't but this is still wrong.
        // Whatever position we're facing in.
        // We need to get that and call it correctly.

        // If we want to actually apply force we have to find a way to break orbit.
        // Fuck
        // Breaks orbit but now the "animations" are breaking.
        // Add force both right and up
        // K we're still breaking this but it's better at least :)

        // get direction that object is facing before we do this.

        // Looks very close to right but we need to consider how to keep our angular
        // What if we just zeroed the vector??

        // Get velocity as it changes overtime and shift our position
        // We need to slow this down considerably.
        // 2D velocity isn't working so we'll just use 3d for now. IDK why it works but it does so fuck it we ball.


        // Change in time 


        // This is the correct movement. But we need to slow it down so consider drag
        // Additionally we should look to see if 
        // Try to use impulse 
        
        // Get current velocity + a right vector. Multiply it by fixedDeltaTime and divide by 10.
        movement = ((obj.GetComponent<Rigidbody2D>().velocity + Vector2.right) * Time.fixedDeltaTime);
        movement.Normalize();
        movement = movement / breakFactor;

        print("Movement calculated is at: " + movement);

        print("DEBUG: I saw the last call for breakvector!");
        obj.transform.Translate(movement);
        StartCoroutine(SetBounds(obj));

        // Debug and see why we're getting such an enormous increase
        print("Movement " + movement);
        obj.GetComponent<Rigidbody2D>().angularVelocity = LumAngVelocity;

    }
    // Deal with this later! TODO: Setup movement patterns for Lumina after we Fire her off.
    // She should be able to float around whatever point she lands at.
    /* public float IEnumerator GetLuminaMovement(float luminaCharge)
     {
         yield return GetTimeChargedLum();
     } */

    /// <summary>
    ///  Method to set the boundary that our little ball can travel.
    ///  If it speeds past the change of it's current position to our next position
    ///  We should reset the ball. This sets a clear boundary for where Lumina can and can't go.
    /// </summary>
   
    // Get position of rigidbodies as they are moving.
    public IEnumerator SetBounds(GameObject obj)
    {

        bool isOkayChange;
        Vector2 nextVector;
        Vector2 initVector = obj.GetComponent<Rigidbody2D>().position;


        // Wait a fourth of a second
        yield return new WaitForSeconds(0.25f);
        nextVector = obj.GetComponent<Rigidbody2D>().position;
        print("V1: " + initVector + " V2: " + nextVector);
        Vector2 deltaV = initVector - nextVector;
   

        print("This is my change in Vectors: " + deltaV);
        print("This is my change in X: " + deltaV.x);
        print("This is my change in Y: " + deltaV.y);
        deltaY = deltaV.y;
        deltaX = deltaV.x;

        if(deltaV.y < maxDeltaY && deltaV.y > minDeltaY)
        {
            isOkayChange = true;
        }
        else
        {
            isOkayChange = false;
        }

        // Move position back. SPECIFICALLY for deltaY. (This never turns our sentinel variable to true so we just loop forever.)

       while(!isOkayChange)
        {
            obj.GetComponent<Rigidbody2D>().position = initVector;
            print("Obj was at " + initVector + " object is now at... " + nextVector);
            isOkayChange = true;
            // We should reactivate rotation if Lumina flies off
            disableRotate = false;  
        }

        StopCoroutine(SetBounds(obj));


    }
}
