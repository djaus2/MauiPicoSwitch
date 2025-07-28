using Android.Bluetooth;
using Android.Content;
using System.IO;
using System.Threading.Tasks;
using Android.Util;
using Android.Provider;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PhoneBtSwitchesApp.Platforms.Android;

public class BluetoothReceiver : IDisposable
{
    private BluetoothSocket _socket;
    private Stream _inputStream;
    private readonly Context _context;
    private readonly BondStateReceiver _receiver;
    private Page _page;

    public bool IsConnected => _socket?.IsConnected ?? false;

    public BluetoothReceiver(Context context)
    {
        _context = context;
        _receiver = new BondStateReceiver();
        var filter = new IntentFilter(BluetoothDevice.ActionBondStateChanged);
        _context.RegisterReceiver(_receiver, filter);
    }

    public async Task<bool> ConnectAsync(string deviceName, Action<char> onCharReceived, Page page)
    {
        _page = page;
        var adapter = BluetoothAdapter.DefaultAdapter;
        foreach (var x in adapter.BondedDevices)
        {
            System.Diagnostics.Debug.WriteLine(x.Name);
        }

        //[1] Check if device with (partial) devicename is paired
        var prefix = deviceName.Length >= 10 ? deviceName.Substring(0, 10) : deviceName;
        var device = adapter.BondedDevices
            .FirstOrDefault(d => d.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (device == null)
        {
            //[2] If not found(paired) , scan for device
            var scanner = new BluetoothScanner("PicoW uPPERCASE");
            var context = _context; // Get the application context
            device = await scanner.DiscoverFirstMatchingDeviceAsync(context);

            if (device != null)
            {
                System.Diagnostics.Debug.WriteLine($"Found device: {device.Name} - {device.Address}");
                // Proceed with pairing or connection
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No matching device found.");
                return false;
            }
            // [3] Pair if needed
            if (device.BondState != Bond.Bonded)
            {
                Log.Debug("BluetoothRecvr", $"Pairing with {device.Name}...");
                device.CreateBond();
            }
        }

     

        var uuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"); // SPP UUID
        _socket = device.CreateRfcommSocketToServiceRecord(uuid);

        var connectTask = _socket.ConnectAsync();
        var timeoutTask = Task.Delay(10000);

        bool isNotConnected = false;
        if (await Task.WhenAny(connectTask, timeoutTask) != connectTask)
        {
            System.Diagnostics.Debug.WriteLine("Bluetooth connection timed out.");
            try { _socket.Close(); } catch { }
            isNotConnected =    true;
        }
        if (!isNotConnected)
        {
            if (_socket == null || !_socket.IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("Failed to create or connect Bluetooth socket.");
                isNotConnected = true;
            }
        }

        if(isNotConnected)
        {

            bool result = await _page.DisplayAlert("Confirm", "Paired device not found. OK to remove?", "OK", "Cancel");

            if (result)
            {
                UnpairDevice(device);

                await Toast.Make("FYI:You can now connect again and 'Re-Pair'.", ToastDuration.Long, 14).Show();

                //await _page.DisplayAlert("FYI", "You can now connect again and 'Re-Pair'.", "OK");
            }
            else
            {
                await Toast.Make("FYI:If Android device was redeployed you DO NEED to 'Un-Pair' and 'Re-Pair'.", ToastDuration.Long, 14).Show();

            }
            return false;
        }


        _inputStream = _socket.InputStream;

        _ = Task.Run(() =>
        {
            while (_socket.IsConnected)
            {
                int b = _inputStream.ReadByte();
                if (b != -1)
                    onCharReceived?.Invoke((char)b);
            }
        });
        await Toast.Make("Bluetooth Device Connected OK.", ToastDuration.Long, 14).Show();

        return true;
    }

    public bool Disconnect()
    {
        try
        {
            _inputStream?.Close();
            _inputStream?.Dispose();
            _socket?.Close();
            _socket?.Dispose();
            _socket = null;
            _inputStream = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Disconnect error: {ex.Message}");
            return false;
        }
        return true;
    }

    public async Task SendCharAsync(char c) => await SendAsync(c);
    public async Task SendStringAsync(string message) => await SendAsync(message);
    public async Task SendIntAsync(int value) => await SendAsync(value);

    public async Task SendAsync<T>(T value)
    {
        try
        {
            if (_socket != null && _socket.IsConnected)
            {
                var outputStream = _socket.OutputStream;
                string message = value?.ToString() ?? string.Empty;
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);
                await outputStream.WriteAsync(buffer, 0, buffer.Length);
                await outputStream.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _context.UnregisterReceiver(_receiver);
        Disconnect();
    }

    private class BondStateReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            var bondState = device?.BondState ?? Bond.None;
            Log.Debug("BluetoothRecvr", $"Bond state changed: {bondState}");
        }
    }

    private void UnpairDevice(BluetoothDevice device)
    {
        try
        {
            var method = device.Class.GetMethod("removeBond");
            method?.Invoke(device);
            System.Diagnostics.Debug.WriteLine($"Unpaired device: {device.Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unpair error: {ex.Message}");
        }
    }
}
