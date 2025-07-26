
using Android.Bluetooth;
using Android.Content;
using System.IO;
using System.Threading.Tasks;

namespace PhoneBtSwitchesApp.Platforms.Android;

public class BluetoothRecvr
{
    private BluetoothSocket _socket;
    private Stream _inputStream;


    public bool IsConnected => _socket?.IsConnected ?? false;

    public async Task<bool> ConnectAsync(string deviceName, Action<char> onCharReceived)
    {
        var adapter = BluetoothAdapter.DefaultAdapter;
        foreach (var x in adapter.BondedDevices)
        {
            System.Diagnostics.Debug.WriteLine(x.Name);
        }
        var prefix = deviceName.Length >= 10 ? deviceName.Substring(0, 10) : deviceName;
        var device = adapter.BondedDevices
            .FirstOrDefault(d => d.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (device == null) return false;

        var uuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"); // SPP UUID
        device.CreateBond(); // May help prompt pairing UI on some devices
        _socket = device.CreateRfcommSocketToServiceRecord(uuid);
        //await _socket.ConnectAsync();

        ///////////////////
        var connectTask = _socket.ConnectAsync();
        var timeoutTask = Task.Delay(10000); // 5 seconds

        if (await Task.WhenAny(connectTask, timeoutTask) != connectTask)
        {
            System.Diagnostics.Debug.WriteLine("Bluetooth connection timed out.");
            try
            {
                _socket.Close();
            }
            catch (Exception) { }
            return false;
        }
        ///////////////////

        if (_socket == null || !_socket.IsConnected)
        {
            System.Diagnostics.Debug.WriteLine("Failed to create or connect Bluetooth socket.");
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
        return true;
    }

    public void Disconnect()
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
        }
    }

    public async Task SendCharAsync(char c)
    {
        try
        {
            if (_socket != null && _socket.IsConnected)
            {
                var outputStream = _socket.OutputStream;
                byte[] buffer = new byte[] { (byte)c };
                await outputStream.WriteAsync(buffer, 0, buffer.Length);
                await outputStream.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
        }
    }
    public async Task SendStringAsync(string message)
    {
        try
        {
            await SendAsync(message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SendStringAsync error: {ex.Message}");
        }
    }

    public async Task SendIntAsync(int value)
    {
        try
        {
            await SendAsync(value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SendIntAsync error: {ex.Message}");
        }
    }


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



}

