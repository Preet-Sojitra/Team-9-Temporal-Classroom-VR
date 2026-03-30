using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapBlueToothKey : MonoBehaviour
{
    private static readonly int key_count = 15;
    private readonly string[] js_buttons = new string[key_count];
    public GameObject jsKeyText;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < js_buttons.Length; i++)
        {
            js_buttons[i] = string.Format("js{0}", i);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.anyKey)
        {
            //Debug.Log("Bluetooth Key detected");
            TextMeshPro tm = jsKeyText.GetComponent<TextMeshPro>();
            tm.text = "Bluetooth Key:\n";
            for (int i = 0; i < js_buttons.Length; i++)
            {
                if (Input.GetButton(js_buttons[i]))
                {
                    tm.text += string.Format("joystick button {0}\n", js_buttons[i]); ;
                }


            }
            if (Input.GetButton("Submit"))
            {
                tm.text += string.Format("Submit \n"); ;
            }
            if (Input.GetButton("Cancel"))
            {
                tm.text += string.Format("Cancel \n"); ;
            }

            if (Input.GetButton("Jump"))
            {
                tm.text += string.Format("Jump \n"); ;
            }

            if (Input.GetAxis("Joystick Axis 1") != 0)
            {
                tm.text += string.Format("Joystick Axis 1: {0}\n", Input.GetAxis("Joystick Axis 1")); ;
            }

            if (Input.GetAxis("Joystick Axis 2") != 0)
            {
                tm.text += string.Format("Joystick Axis 2: {0}\n", Input.GetAxis("Joystick Axis 2")); ;
            }
        }

    }
}



// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class MapBlueToothKey : MonoBehaviour
// {
//     private static readonly int key_count = 15;
//     private readonly string[] js_names = new string[key_count];
//     public GameObject jsKeyText;
//     private TextMeshPro tm;

//     void Start()
//     {
//         tm = jsKeyText.GetComponent<TextMeshPro>();

//         // Initialize the names to match your Input Manager setup (js0, js1, etc.)
//         for (int i = 0; i < js_names.Length; i++)
//         {
//             js_names[i] = string.Format("js{0}", i);
//         }
//     }

//     void Update()
//     {
//         // Reset text each frame
//         tm.text = "Bluetooth Key Status:\n";

//         // Check every 'js' mapping defined in your Input Manager
//         for (int i = 0; i < js_names.Length; i++)
//         {
//             string currentName = js_names[i];

//             // 1. Check if it's being pressed as a Button
//             if (Input.GetButton(currentName))
//             {
//                 tm.text += string.Format("Button Pressed: {0}\n", currentName);
//             }

//             // 2. Check if it's being moved as an Axis (Joystick movement)
//             float axisValue = Input.GetAxis(currentName);
//             if (Mathf.Abs(axisValue) > 0.1f) // 0.1f threshold to ignore minor stick drift
//             {
//                 tm.text += string.Format("Axis {0} Value: {1:F2}\n", currentName, axisValue);
//             }
//         }

//         // Standard Unity mappings
//         if (Input.GetButton("Submit")) { tm.text += "Standard: Submit\n"; }
//         if (Input.GetButton("Cancel")) { tm.text += "Standard: Cancel\n"; }
//         if (Input.GetButton("Jump")) { tm.text += "Standard: Jump\n"; }
//     }
// }