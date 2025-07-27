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
To discern which button has been activated by the phone app, there is also a ```SwitchNo``` property in th e phone app, which is the pin number that the activated button is connected to on the Arduino device. This can be one of:
```cs
16
18
20
```
The Sketch uses a set of mutually exclusive booleans to discern which button is the active one _(this code could be improved)_:
```cpp
bool waiting4switch16 = false;
bool waiting4switch18 = false;
bool waiting4switch20 = false;
```

State changes in teh sketch are actioned by sending specific chars to it over Serial Bluetooth.  It then returns events and acknowledgements as specific chars.

## Messages

As stated, these are sent as chars. The following is the state changes from received chars by the phone app:

```cs
public void OnCharCmdReceived(char c)
{
    c= char.ToUpper(c);
    switch (State)
    {
        case DeviceState.NotConnected when c == 'C':
            State= DeviceState.Idle; 
            break;
        case DeviceState.Idle when c == 'K':
            State = DeviceState.Ready;
            break;
        case DeviceState.Ready when c == 'A':
            State = DeviceState.Pressed;
            SwitchNo = 16;
            break;
        case DeviceState.Pressed when c == 'B':
            State = DeviceState.Released;
            break;
        case DeviceState.Ready when c == 'C':
            State = DeviceState.Pressed;
            SwitchNo = 18;
            break;
        case DeviceState.Pressed when c == 'D':
            State = DeviceState.Released;
            break;
        case DeviceState.Ready when c == 'E':
            State = DeviceState.Pressed;
            SwitchNo = 20;
            break;
        case DeviceState.Pressed when c == 'F':
            State = DeviceState.Released;
            break;
        case DeviceState.Released when c == 'I':
            State = DeviceState.Idle;
            break;
        default:
            _state = DeviceState.NotConnected;
            break;
    }
}
```
This is called by the serial reception method:
```cs
    void OnCharReceived(char c)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            bool isAlphanumeric = char.IsLetterOrDigit(c);

            if (isAlphanumeric)
            {
                AppViewModel.OnCharCmdReceived(c);
            }
        });
    }
```
Note that ```OnCharCmdReceived()``` is called in the MainThread context which means it can cause UI updates.

## Bluetooth

