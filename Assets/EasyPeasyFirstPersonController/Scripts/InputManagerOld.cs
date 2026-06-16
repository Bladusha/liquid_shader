namespace EasyPeasyFirstPersonController
{
    using UnityEngine;

    public class InputManagerOld : MonoBehaviour, IInputManager
    {
        public Vector2 moveInput => new Vector2(InputSystemCompat.GetAxis("Horizontal"), InputSystemCompat.GetAxis("Vertical"));
        public Vector2 lookInput => new Vector2(InputSystemCompat.GetAxis("Mouse X"), InputSystemCompat.GetAxis("Mouse Y"));
        public bool jump => InputSystemCompat.GetKey(KeyCode.Space);
        public bool sprint => InputSystemCompat.GetKey(KeyCode.LeftShift);
        public bool crouch => InputSystemCompat.GetKey(KeyCode.LeftControl);
        public bool slide => InputSystemCompat.GetKey(KeyCode.LeftControl);
    }
}
