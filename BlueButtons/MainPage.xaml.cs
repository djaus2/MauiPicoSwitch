
#if ANDROID
using PhoneBtSwitchesApp.Platforms.Android;

#endif

namespace PhoneBtSwitchesApp;

using Android.Graphics.Drawables;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.ApplicationModel;
using System.ComponentModel;

public class BluetoothPermissions : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
    {
        ("android.permission.BLUETOOTH_CONNECT", true),
        ("android.permission.BLUETOOTH_SCAN", true),
        ("android.permission.ACCESS_FINE_LOCATION", true)
    };
#endif
}


public partial class MainPage : ContentPage
{
    int count = 0;
    Stack<Color> Colrs = new Stack<Color>();

    AppViewModel AppViewModel { get; } = new AppViewModel();

    public MainPage()//MainPageModel model)
    {
        InitializeComponent();
        Colrs = new Stack<Color>();
        BindingContext = AppViewModel;
        AppViewModel.PropertyChanged += OnPropertyChangedx;
        


    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AppViewModel.State = DeviceState.NotConnected;
    }


    private void OnPropertyChangedx(object? sender, PropertyChangedEventArgs e)
    {

        if (e.PropertyName == nameof(AppViewModel.State))
        {
            switch (AppViewModel.State)
            {
                case DeviceState.NotConnected:
                    ConnectBtn.Text = "Connect";
                    ConnectBtn.IsEnabled = true;
                    SendRBtn.IsEnabled = false;
                    SendSBtn.IsEnabled = false;
                    SendTBtn.IsEnabled = false;
                    DisconnectBtn.IsEnabled = false;
                    break;
                case DeviceState.Idle:
                    ConnectBtn.Text = "Connected";
                    ConnectBtn.IsEnabled = false;
                    SendRBtn.IsEnabled = true;
                    SendSBtn.IsEnabled = true;
                    SendTBtn.IsEnabled = true;
                    DisconnectBtn.IsEnabled = true;
                    break;
                case DeviceState.Ready:
                    ConnectBtn.Text = "Ready: Waiting for Press";
                    ConnectBtn.IsEnabled = false;
                    SendRBtn.IsEnabled = false;
                    SendSBtn.IsEnabled = false;
                    SendTBtn.IsEnabled = false;
                    DisconnectBtn.IsEnabled = false;
                    break;
                case DeviceState.Pressed:
                    ConnectBtn.Text = "Pressed";
                    ConnectBtn.IsEnabled = false;
                    SendRBtn.IsEnabled = false;
                    SendSBtn.IsEnabled = false;
                    SendTBtn.IsEnabled = false;
                    DisconnectBtn.IsEnabled = false;
                    break;
                case DeviceState.Released:
                    ConnectBtn.Text = "Released";
                    ConnectBtn.IsEnabled = false;
                    SendRBtn.IsEnabled = false;
                    SendSBtn.IsEnabled = false;
                    SendTBtn.IsEnabled = false;
                    DisconnectBtn.IsEnabled = false;
                    break;
            }
        }
    }

    void OnCharReceived(char c)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            bool isAlphanumeric = char.IsLetterOrDigit(c);

            if (isAlphanumeric)
            {
                AppViewModel.OnCharCmdReceived(c);
            }
            System.Diagnostics.Debug.WriteLine($"\t\t\t\t\t\tReceived: {c}");
        });
    }


    BluetoothReceiver? receiver;


    private async Task<bool> OnConnectClickedAsync(object sender, EventArgs e)
    {
        var context = Android.App.Application.Context; // Get the application context
        if (AppViewModel.State != DeviceState.NotConnected)
        {
            System.Diagnostics.Debug.WriteLine("\t\t\tAlready connected or in a different state.");
            return true;
        }
#if ANDROID
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            return false;

        var status2 = await Permissions.RequestAsync<BluetoothPermissions>();
        if (status2 != PermissionStatus.Granted)
        {
            System.Diagnostics.Debug.WriteLine("\t\t\tBluetooth permissions not granted.");
            return false;
        }


        receiver = new BluetoothReceiver(Android.App.Application.Context);

        bool res = await receiver.ConnectAsync("PicoW uPPERCASE", OnCharReceived, this);
        if (!res)
        {
            System.Diagnostics.Debug.WriteLine("\t\t\tFailed to connect to Bluetooth device.");
            return false;
        }
        if (receiver != null)
        {
            if (!receiver.IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("\t\t\tBT Device is not connected.");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("\t\t\tConnected to Bluetooth device.");
            AppViewModel.State = DeviceState.Idle;
            return true;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("\t\t\tNull connection to Bluetooth device.");
            return false;
        }
#endif
    }

    private async Task OnSendRClickedAsync(object sender, EventArgs e)
    {
        //Nb: VisualStateManager.GoToState(myButton, myButton.IsEnabled ? "Normal" : "Disabled");
        var tintBehavior = new IconTintColorBehavior
        {
            TintColor = Colors.Gray // Or Colors.Black when enabled
        };
        Button button = (Button)sender;
        Color clr = button.BackgroundColor;
        Colrs.Push(clr);
        button.BackgroundColor = Colors.Gray;

        var name = button.Text;
        char ch = 'R';
        var icon = purpleIcon;
        if (name.Contains("16"))
        {
            ch = 'R';
            icon = purpleIcon;
        }
        else if (name.Contains("18"))
        {
            ch = 'S';
            icon = greenIcon;
        }
        else if (name.Contains("20"))
        {
            ch = 'T';
            icon = blueIcon;
        }
        try
        {
#if ANDROID
            if (receiver != null)
            {
                if (AppViewModel.State != DeviceState.Idle)
                {
                    System.Diagnostics.Debug.WriteLine("\t\t\tNot in idle state.");
                    return;
                }
                System.Diagnostics.Debug.WriteLine("\t\t\t\tConnected to Bluetooth device.");
                await receiver.SendCharAsync(ch); // Set ready for button press
                //Wait for button press
                while (AppViewModel.State < DeviceState.Pressed) //Could be released by here
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
                //Wait for button release
                icon.Behaviors.Clear();
                icon.Behaviors.Add(tintBehavior);
                while (AppViewModel.State != DeviceState.Released)
                {
                    await Task.Delay(100); // Wait for the device to respond
                }
                icon.Behaviors.Clear();
                switch (AppViewModel.SwitchNo)
                {
                    case 16:
                        //Can also insert calll to a method here
                        await Toast.Make("Button 16 released.", ToastDuration.Short, 14).Show();
                        break;
                    case 18:
                        await Toast.Make("Button 18 released.", ToastDuration.Short, 14).Show();
                        break;
                    case 20:
                        await Toast.Make("Button 20 released.", ToastDuration.Short, 14).Show();
                        break;
                }
                var clr2 = Colrs.Pop();
                button.BackgroundColor = clr2;

                await Task.Delay(333);
                AppViewModel.OnCharCmdReceived('I'); // Reset state to Idle
                while (AppViewModel.State != DeviceState.Idle)
                {
                    await Task.Delay(100); // Wait for the device to respond
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("\t\t\t\tFailed to connect to Bluetooth device.");
            }
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"\t\t\t\tError sending character: {ex.Message}");
            // Optionally show a toast or alert
        }
    }

    private async Task<bool> OnDisconnectClicked2(object sender, EventArgs e)
    {
#if ANDROID
        if (receiver == null)
        {
            System.Diagnostics.Debug.WriteLine("\t\t\t\tNo Bluetooth receiver to disconnect.");
            return false;
        }
        bool? res = receiver?.Disconnect();
        if (res == true)
        {
            await Toast.Make("Android device was disconnected.", ToastDuration.Long, 14).Show();
            System.Diagnostics.Debug.WriteLine("\t\t\t\tDisconnected from Bluetooth device.");
            AppViewModel.State = DeviceState.NotConnected;
            return true;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("\t\t\t\tFailed to disconnect from Bluetooth device.");
            return false;
        }
#endif
    }


    //////////////////////////////////////////////////////////
    /// Button press handlers
    //////////////////////////////////////////////////////////
    ///
    private async void OnDisconnectClicked(object sender, EventArgs e)
    {
        if (AppViewModel.State == DeviceState.NotConnected)
        {
            System.Diagnostics.Debug.WriteLine("\tAlready not connected.");
            return;
        }
        try
        {
            bool res = await OnDisconnectClicked2(sender, e);
            System.Diagnostics.Debug.WriteLine("\t====Disconnected====.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"\tUnhandled disconnect exception: {ex.Message}");
            // Optionally show a toast or alert
        }

    }


    private async void OnConnectClicked(object sender, EventArgs e)
    {
        if (AppViewModel.State != DeviceState.NotConnected)
        {
            System.Diagnostics.Debug.WriteLine("\tAlready connected or in a different state.");
            return;
        }
        try
        {
            bool res = await OnConnectClickedAsync(sender, e);
            if (res)
            {
                System.Diagnostics.Debug.WriteLine($"\t====Connected====");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"\tConnection failed.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"\tUnhandled OnConnect exception: {ex.Message}");
            // Optionally show a toast or alert
        }
    }


    private async void OnSendRClicked(object sender, EventArgs e)
    {
        try
        {
            await OnSendRClickedAsync(sender, e);
            System.Diagnostics.Debug.WriteLine(($"\t====Sent R/SorT===="));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"\tUnhandled OnSendAsync exception: {ex.Message}");
            // Optionally show a toast or alert
        }
    }

}