# MauiPicoSwitch

## Apps

### MauiPicoSwitch
  - A Maui Android Phone App
  - Connects to the bluetoothswitch sketch over Bluetooth
  - Can then enable one of 3 physical switches on the Pico, at a time.
  - When enabled, the button pressed and released events are received from the sketch
  - Each individual button event can then call an in-app method.
    - As coded here, a Toast pops up.
   - Can disconnect from the device
### BluetoothPicoSwitch Sketch
  - Runs on Rpi Pico W
    - Configured as an Arduino device.
  - Is connected to by the phone app using Bluetooth, and is then in Idle mode.
  - Phone can then enable one of 3 physical switches on the Pico, at a time.
    - It is then in Ready mode.
  - The pressed and then released events are sent to the phone
  - It then returns to Idle mode

## App States

Both apps are state machines. The states are

```cs
    public enum DeviceState
    {
        NotConnected,
        Idle,
        Ready,
        Pressed,
        Released
    }
```
To discern which button has been activated in the phone app, there is also a ```SwitchNo`` property which is the pin number that the activated button is connected to onbthe Arduino device.
```cs
```

