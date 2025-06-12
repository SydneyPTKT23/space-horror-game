using UnityEngine;

namespace SLC.SpaceHorror.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;

        private void Awake()
        {
            if (inputReader != null)
                inputReader.Initialize();
        }

        private void OnEnable()
        {
            inputReader?.EnablePlayerInput();
        }

        private void OnDisable()
        {
            inputReader?.DisablePlayerInput();
        }

        private void OnDestroy()
        {
            inputReader?.ResetValues();
        }
    }
}