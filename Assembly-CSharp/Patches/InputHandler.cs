using InControl;
using MonoMod;
using UnityEngine;

#pragma warning disable 1591
#pragma warning disable CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::InputHandler")]
    public class InputHandler : global::InputHandler
    {
        [MonoModIgnore]
        private bool isTitleScreenScene;

        [MonoModIgnore]
        private bool isMenuScene;

        [MonoModIgnore]
        private bool controllerPressed;

        // Reverted cursor behavior
        [MonoModReplace]
        private void OnGUI()
        {
            Cursor.lockState = CursorLockMode.None;
            if (isTitleScreenScene)
            {
                Cursor.visible = false;
                return;
            }

            if (!isMenuScene)
            {
                ModHooks.Instance.OnCursor();
                return;
            }

            if (controllerPressed)
            {
                Cursor.visible = false;
                return;
            }

            Cursor.visible = true;
        }
        
        public readonly struct KeyOrMouseBinding
        {
            public readonly Key Key;
            public readonly Mouse Mouse;

            public KeyOrMouseBinding(Key key)
            {
                this.Key = key;
                this.Mouse = Mouse.None;
            }

            public KeyOrMouseBinding(Mouse mouse)
            {
                this.Key = Key.None;
                this.Mouse = mouse;
            }

            public static bool IsNone(InputHandler.KeyOrMouseBinding val) => val.Key == Key.None && val.Mouse == Mouse.None;

            public override string ToString() => this.Mouse != Mouse.None ? this.Mouse.ToString() : this.Key.ToString();
        }
    }
}