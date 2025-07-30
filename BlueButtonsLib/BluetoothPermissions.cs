using Microsoft.Maui.ApplicationModel;

namespace BlueButtonsLib
{
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
}
