
    using Android.Bluetooth;
    using Android.Content;
    using System.IO;
    using System.Threading.Tasks;

    namespace PicoBluetoothApp.Platforms;

    public class BluetoothReceiver
    {
        private BluetoothSocket _socket;
        private Stream _inputStream;

        public async Task ConnectAsync(string deviceName, Action<char> onCharReceived)
        {
            var adapter = BluetoothAdapter.DefaultAdapter;
            var device = adapter.BondedDevices.FirstOrDefault(d => d.Name == deviceName);
            if (device == null) return;

            var uuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"); // SPP UUID
            _socket = device.CreateRfcommSocketToServiceRecord(uuid);
            await _socket.ConnectAsync();

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
        }
    }

