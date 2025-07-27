# MauiPicoSwitch

## Apps

### MauiPicoSwitch
  - A Maui Android 
  - Connects to the bluetoothswitch sketch over Bluetooth
  - Can then enable one of 3 physical switches on the Pico, at a time.
  - When enabled, the button pressed and released events are received from the sketch
  - Each individual button event can then call an in-app method.
    - As coded here, a Toast pops up.
   - Can disconnect from the device
### BluetoothPicoSwitch Sketch
  - Runs on Pico W 
  - Is connected to by the phone app using Bluetooth, and is then in Idle mode.
  - Phone can then enable one of 3 physical switches on the Pico, at a time.
    - It is then in Ready mode.
  - The pressed and then released events are sent to the phone
  - It then returns to Idle mode

