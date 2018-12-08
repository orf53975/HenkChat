namespace HenkChat
{
    class Settings
    {
        public static object GetIp() { return Windows.Storage.ApplicationData.Current.LocalSettings.Values["Ip"]; }
        public static object GetUsername() { return Windows.Storage.ApplicationData.Current.LocalSettings.Values["Name"]; }

        public static void SetIp(string Value) => Windows.Storage.ApplicationData.Current.LocalSettings.Values["Ip"] = Value;
        public static void SetUsername(string Value) => Windows.Storage.ApplicationData.Current.LocalSettings.Values["Name"] = Value;
    }
}
