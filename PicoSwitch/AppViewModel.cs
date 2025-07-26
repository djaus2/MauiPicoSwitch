using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBtSwitchesApp
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private string _msg;
        public string Msg
        {
            get => _msg;
            set
            {
                if (_msg != value)
                {
                    _msg = value;
                    OnPropertyChanged(nameof(Msg));
                }
            }
        }

        private DeviceState _state = DeviceState.NotConnected;
        public DeviceState State { get => _state; set { _state = value; OnPropertyChanged(nameof(State)); } }

        public int SwitchNo { get => _switchNo; set { _switchNo = value; OnPropertyChanged(nameof(SwitchNo)); } }

        private int _switchNo = 16;

        public void OnCharCmdReceived(char c)
        {
            c= char.ToUpper(c);
            Msg += c.ToString();
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
                    Msg = "";
                    break;
                default:
                    _state = DeviceState.NotConnected;
                    Msg = "";
                    break;
                    // Add logging or UI updates as needed
            }

            System.Diagnostics.Debug.WriteLine($"Device sent: {c}, new current state: {_state}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public enum DeviceState
    {
        NotConnected,
        Idle,
        Ready,
        Pressed,
        Released
    }






}
