// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class CharacterMovement : MonoBehaviour
// {
//     CharacterController charCntrl;
//     [Tooltip("The speed at which the character will move.")]
//     public float speed = 5f;
//     [Tooltip("The camera representing where the character is looking.")]
//     public GameObject cameraObj;
//     [Tooltip("Should be checked if using the Bluetooth Controller to move. If using keyboard, leave this unchecked.")]
//     public bool joyStickMode;

//     // Start is called before the first frame update
//     void Start()
//     {
//         charCntrl = GetComponent<CharacterController>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         //Get horizontal and Vertical movements
//         float horComp = Input.GetAxis("Horizontal");
//         float vertComp = Input.GetAxis("Vertical");

//         if (joyStickMode)
//         {
//             horComp = Input.GetAxis("Vertical");
//             vertComp = Input.GetAxis("Horizontal") * -1;
//         }

//         Vector3 moveVect = Vector3.zero;

//         //Get look Direction
//         Vector3 cameraLook = cameraObj.transform.forward;
//         cameraLook.y = 0f;
//         cameraLook = cameraLook.normalized;

//         Vector3 forwardVect = cameraLook;
//         Vector3 rightVect = Vector3.Cross(forwardVect, Vector3.up).normalized * -1;

//         moveVect += rightVect * horComp;
//         moveVect += forwardVect * vertComp;

//         moveVect *= speed;


//         charCntrl.SimpleMove(moveVect);


//     }
// }


using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    public float speed = 2f;
    public float gravity = -9.81f;
    public Transform cameraObj; // assign Main Camera here

    private CharacterController controller;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        // Movement relative to PLAYER, not camera
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Prevent head tilt affecting movement
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * inputZ + right * inputX) * speed;

        // Manual gravity (REQUIRED)
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}