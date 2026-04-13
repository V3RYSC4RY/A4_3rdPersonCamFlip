using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThirdPersonCamFlip
{
    internal static class UiInputFocus
    {
        public static bool IsTextInputActive()
        {
            if (IsSelectedTextInput())
                return true;

            if (IsAnyTmpInputFocused())
                return true;

            return IsAnyLegacyInputFocused();
        }

        private static bool IsSelectedTextInput()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected == null)
                return false;

            return selected.GetComponent<TMP_InputField>() != null || selected.GetComponent<InputField>() != null;
        }

        private static bool IsAnyTmpInputFocused()
        {
            TMP_InputField[] inputs = Object.FindObjectsOfType<TMP_InputField>();
            foreach (TMP_InputField input in inputs)
            {
                if (input != null && input.isFocused)
                    return true;
            }

            return false;
        }

        private static bool IsAnyLegacyInputFocused()
        {
            InputField[] inputs = Object.FindObjectsOfType<InputField>();
            foreach (InputField input in inputs)
            {
                if (input != null && input.isFocused)
                    return true;
            }

            return false;
        }
    }
}
