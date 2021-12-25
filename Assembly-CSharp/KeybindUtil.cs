using System;
using System.Collections.Generic;
using InControl;

namespace Modding
{
    /// <summary>
    /// Utils for interacting with InControl keybindings.
    /// </summary>
    public static class KeybindUtil
    {
        /// <summary>
        /// Gets a <c>KeyOrMouseBinding</c> from a player action.
        /// </summary>
        /// <param name="action">The player action</param>
        /// <returns></returns>
        public static Patches.InputHandler.KeyOrMouseBinding GetKeyOrMouseBinding(this PlayerAction action)
        {
            foreach (var src in action.Bindings)
            {
                Patches.InputHandler.KeyOrMouseBinding ret = default;
                if (src is KeyBindingSource kbs && kbs.Control.IncludeCount == 1)
                {
                    ret = new Patches.InputHandler.KeyOrMouseBinding(
                        kbs.Control.GetInclude(0)
                    );
                }
                else if (src is MouseBindingSource mbs)
                {
                    ret = new Patches.InputHandler.KeyOrMouseBinding(mbs.Control);
                }
                if (!Patches.InputHandler.KeyOrMouseBinding.IsNone(ret))
                {
                    return ret;
                }
            }
            return default;
        }

        /// <summary>
        /// Adds a binding to the player action based on a <c>KeyOrMouseBinding</c>.
        /// </summary>
        /// <param name="action">The player action</param>
        /// <param name="binding">The binding</param>
        public static void AddKeyOrMouseBinding(this PlayerAction action, Patches.InputHandler.KeyOrMouseBinding binding)
        {
            if (binding.Key != Key.None)
            {
                action.AddBinding(new KeyBindingSource(new KeyCombo(binding.Key)));
            }
            else if (binding.Mouse != Mouse.None)
            {
                action.AddBinding(new MouseBindingSource(binding.Mouse));
            }
        }

        /// <summary>
        /// Parses a key or mouse binding from a string.
        /// </summary>
        /// <param name="src">The source string</param>
        /// <returns></returns>
        public static Patches.InputHandler.KeyOrMouseBinding? ParseBinding(string src)
        {
            try
            {
                Key key = (Key)Enum.Parse(typeof(Key), src);
                return new Patches.InputHandler.KeyOrMouseBinding(key);
            }
            catch(Exception e1)
            {
                try
                {
                    Mouse mouse = (Mouse)Enum.Parse(typeof(Mouse), src);
                    return new Patches.InputHandler.KeyOrMouseBinding(mouse);
                }
                catch (Exception e2)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a controller button binding for a player action.
        /// </summary>
        /// <param name="ac">The player action.</param>
        /// <returns></returns>
        public static InputControlType GetControllerButtonBinding(this PlayerAction ac)
        {
            foreach (var src in ac.Bindings)
            {
                if (src is DeviceBindingSource dsrc)
                {
                    return dsrc.Control;
                }
            }
            return InputControlType.None;
        }

        
        /// <summary>
        /// Adds a controller button binding to the player action based on a <c>InputControlType</c>.
        /// </summary>
        /// <param name="action">The player action</param>
        /// <param name="binding">The binding</param>
        public static void AddInputControlType(this PlayerAction action, InputControlType binding)
        {
            if (binding != InputControlType.None)
            {
                action.AddBinding(new DeviceBindingSource(binding));
            }
        }

        /// <summary>
        /// Parses a InputControlType binding from a string.
        /// </summary>
        /// <param name="src">The source string</param>
        /// <returns></returns>
        public static InputControlType? ParseInputControlTypeBinding(string src)
        {
            try
            {
                InputControlType key =(InputControlType) Enum.Parse(typeof(InputControlType), src);
                return key;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}