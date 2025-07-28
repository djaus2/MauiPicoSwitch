# MauiPicoSwitch

An Android phone app that connects to a Rpi Pico W in Arduino mode, over Classic Serial Bluetooth where 3 switches can trigger actions on the phone.  Bluetooth pairing by the phone between the phone and Pico device is handle programmatically for all paired/unpaired contexts.

> Todo: Extract Non UI code into separate Maui Class project so can be published on NuGet. Also improve Sketch code (no functionality change).

## Apps

### BlueButtons
  - A Maui Android _(only)_ Phone App
  - Connects to the PicoSwitch sketch over Bluetooth
  - Can then enable one of 3 physical switches on the Pico, at a time.
  - When enabled, the button pressed and released events are received from the sketch
  - Each individual button event can then call an in-app method.
    - As coded here, a Toast pops up.
   - Can disconnect from the device
### PicoButtons Sketch
  - Runs on a Rpi Pico W
    - Configured as an Arduino device.
      - Using the [earlephilhower//arduino-pico](https://github.com/earlephilhower/arduino-pico) BSP.
    - Could be simply modified for other Android devices
  - Is connected to by the phone app using Bluetooth, and is then in Idle mode.
  - Phone can then enable one of 3 physical switches on the Pico, at a time.
    - It is then in Ready mode.
    - Its corresponding LED is activated
    - The pressed event is sent to the phone when button is pressed.
      - The LED flashes Off/On
    - The released event is sent when the button is released.
      - LED is turned off.
  - It then returns to Idle mode

## App States

Both apps are state machines. The states are

```cs
    public enum DeviceState
    {
        NotConnected, // All phone app buttons disabled except Connect
        Idle,         // All 3 app Activate Buttons plus Disconnect enabled
        Ready,        // All phone app buttons diabled
        Pressed,      // No change
        Released      // No changed  ... as this is only momentary (Transitions to Idle)
    }
```
To discern which button has been activated by the phone app, there is also a ```SwitchNo``` property in th e phone app, which is ...  pin number that the activated button is connected to on the Arduino device. This can be one of:
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

## Activation by Pico

> Phone App waits for DeviceState.Pressed state (eevent). Then can call a method etc depending upon which button was activated:

```cs
    private async Task OnSendRClickedAsync(object sender, EventArgs e)
    {
          ...
          ...
         // Wait until past Ready state
          // Could already be released by here, hence < not ==
          // i.e. Could pressed or released here.
          while (AppViewModel.State < DeviceState.Pressed) 
          {
              await Task.Delay(100); // Wait for the device to respond
          }
          switch (AppViewModel.SwitchNo)
          {
              case 16:
                  //Can insert a call to method here
                  await Toast.Make("Button 16 pressed.", ToastDuration.Short, 14).Show();
                  break;
              case 18:
                  await Toast.Make("Button 18 pressed.", ToastDuration.Short, 14).Show();
                  break;
              case 20:
                  await Toast.Make("Button 20 pressed.", ToastDuration.Short, 14).Show();
                  break;
          }
```
> There is a similar switch code for the released event but it would be the pressed event that would normally be used to trigger in phone actions;

## Bluetooth

The suite uses the Bluetooth Classic  ```Bluetooth Serial Profile (SPP)``` for communication between the devices. It passes messages as single characters as in the OnCharCmdReceived() method above which shows the messages sent from the Pico and how they are interpretted on the phone. The phone and Pico need to be paired and connected, and this is initiated by the phone app. 
- When connecting, the phone app determines all paired devices and then looks for the string associated with Picos Bluetooth profile (actually a 10 character substring of it)
  - If found it then connects.
  - If this fails you have the option to delete this entry.
    - Either way you return to the app where eyou can initiate the connection again. 
- If the Pico string is not found in the list of paired devices, then you can initiate a new pairing.
  - When and if the Pico is found, you will get an OS popup dialog with a code.
  - You accept this regardless of the pin _(which will be different for each new pairing)_ and the devices are paired.
  - It then attempts to connect.
> Note that if the Sketch has had a new deployment of its code, then the existing pairing for it WILL fail. In that case you take the option of deleting as above, and restart the connection, where it will activate the pairing as covered here.

---

| First Header | Second Header |
| ------------ | ------------- |
| Content Cell | Content Cell |
| Content Cell | Content Cell |

| --- | --- |
| ![PicoSwitches](https://github.com/user-attachments/assets/e4baf986-6061-4334-aa9b-0140f6f3b78d) | ![PicoSwitches](https://github.com/user-attachments/assets/e4baf986-6061-4334-aa9b-0140f6f3b78d)  |
| (https://github.com/user-attachments/assets/c05d2b6c-5275-4d80-86b8-aee01d7c8c96) | (https://github.com/user-attachments/assets/8e089625-9472-46ad-9842-f24444111360)|

> In the video above , the phone app is connected using Bluetooth to the sketch as below. Push Buttons are on the right and corresponding LEDS are on the left. The switch/button on the Pico's GPIO pin 16 is activated; its corresponding LED is actiavted. When pressed the LED flashes but stays on until the button is released. This is then repeated for the buttons on GPIO pins 18 and 20. Note the corresponding icons for each button animate the pressing as well.





