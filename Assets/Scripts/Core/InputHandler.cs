using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class InputHandler : MonoBehaviour
    {
        private Vector2 m_inputVector;
        public Vector2 InputVector => m_inputVector;
        public bool InputDetected => m_inputVector != Vector2.zero;



        public bool InteractClicked { get; set; }
        public bool InteractedReleased { get; set; }


        private void Update()
        {
            GetMovement();
            GetInteraction();
        }


        private void GetMovement()
        {
            m_inputVector.x = Input.GetAxisRaw("Horizontal");
            m_inputVector.y = Input.GetAxisRaw("Vertical");



        }

        private void GetInteraction()
        {
            InteractClicked = Input.GetKeyDown(KeyCode.E);
            InteractedReleased = Input.GetKeyUp(KeyCode.E);
        }
    }
}