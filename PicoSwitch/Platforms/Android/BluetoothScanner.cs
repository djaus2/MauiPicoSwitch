using Android.Bluetooth;
using Android.Content;

namespace PhoneBtSwitchesApp.Platforms.Android;
public class BluetoothScanner : BroadcastReceiver
{
    private readonly BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
    private readonly string prefix;
    private BluetoothDevice matchedDevice;
    private readonly TaskCompletionSource<BluetoothDevice> tcs = new();

    public BluetoothScanner(string prefix)
    {
        this.prefix = prefix;
    }

    public async Task<BluetoothDevice> DiscoverFirstMatchingDeviceAsync(Context context)
    {
        var filter = new IntentFilter(BluetoothDevice.ActionFound);
        context.RegisterReceiver(this, filter);

        if (adapter.IsDiscovering)
            adapter.CancelDiscovery();

        bool started = adapter.StartDiscovery();
        if (!started)
        {
            System.Diagnostics.Debug.WriteLine("Discovery failed to start.");
            context.UnregisterReceiver(this);
            return null;
        }

        // Timeout after 15 seconds
        var timeoutTask = Task.Delay(15000);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        adapter.CancelDiscovery();
        context.UnregisterReceiver(this);

        return completedTask == tcs.Task ? matchedDevice : null;
    }

    public override void OnReceive(Context context, Intent intent)
    {
        if (BluetoothDevice.ActionFound.Equals(intent.Action))
        {
            var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            if (device?.Name != null && device.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                matchedDevice = device;
                tcs.TrySetResult(device);
            }
        }
    }



}
