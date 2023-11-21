using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
[InputControlLayout(stateType = typeof(LogitechDualActionHIDInputReport))]
public class LogitechDualAction : Gamepad
{
    static LogitechDualAction()
    {
        InputSystem.RegisterLayout<LogitechDualAction>(
            null,
            new InputDeviceMatcher()
            .WithInterface("HID")
            .WithManufacturer("Logitech")
            .WithProduct("Logitech Dual Action"));

        InputSystem.RegisterLayout<LogitechDualAction>(
            null,
            new InputDeviceMatcher()
            .WithInterface("HID")
            .WithCapability("vendorId", 0x46D)
            .WithCapability("productId", 0xC216));
    }

    [RuntimeInitializeOnLoadMethod]
    static void Init() { }
}

