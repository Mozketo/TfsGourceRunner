using Microsoft.Win32;

namespace TfsGource.Utils
{
	public class RegistryAccess
	{
		public static string GetRegistryValue(string path, string key, string defaultValue)
		{
			RegistryKey item = Registry.CurrentUser.OpenSubKey(path, false);
			if (item == null)
			{
				return defaultValue;
			}
			var value = (string)item.GetValue(key);
			if (value == null)
			{
				value = defaultValue;
			}
			return value;
		}

		public static void WriteRegistryValue(string path, string key, string value)
		{
			RegistryKey item = Registry.CurrentUser.OpenSubKey(path, true);
			if (item == null)
			{
				item = Registry.CurrentUser.CreateSubKey(path);
			}
			item.SetValue(key, value, RegistryValueKind.String);
		}
	}
}
