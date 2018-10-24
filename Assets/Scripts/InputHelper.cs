using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Helper for the unity input system that doesn't require the use of magic strings.
    /// </summary>
    static public class InputHelper
    {

        #region Static methods

        static public float GetHorizontalAxis()
        {
            return Input.GetAxis("Horizontal");
        }

        static public float GetVerticalAxis()
        {
            return Input.GetAxis("Vertical");
        }

        static public bool GetJumpButtonDown()
        {
            return Input.GetButtonDown("Jump");
        }

        static public bool GetJumpButtonHeldDown()
        {
            return Input.GetButton("Jump");
        }

        static public bool GetJumpButtonUp()
        {
            return Input.GetButtonUp("Jump");
        }

        #endregion // Static methods

    }
}
