using System;
using System.Collections.Generic;
using Microsoft.Win32;
using TfsGource.Configuration;
using TfsGource.Utils;

namespace TfsConverter
{
	public class Configuration
	{
		private static string _username;
		private static string _tfsUrl;
	    private static bool _fullscreen;

		public static string Username
		{
			get { return _username; }
			set
			{
				_username = value;
				RegistryAccess.WriteRegistryValue(AppSettings.UserRegistryPath, "Username", value);
			}
		}

		public static string TfsUrl
		{
			get { return _tfsUrl; }
			set
			{
				_tfsUrl = value;
				RegistryAccess.WriteRegistryValue(AppSettings.UserRegistryPath, "TfsUrl", value);
			}
		}

        public static bool Fullscreen
        {
            get { return _fullscreen; }
            set
            {
                _fullscreen = value;
                RegistryAccess.WriteRegistryValue(AppSettings.UserRegistryPath, "FullScreen", value.ToString());
            }
        }

		static Configuration()
		{
			Username = RegistryAccess.GetRegistryValue(AppSettings.UserRegistryPath, "Username", "username");
			TfsUrl = RegistryAccess.GetRegistryValue(AppSettings.UserRegistryPath, "TfsUrl", @"https://tfs.server.com.au:8081/tfs");
		    Fullscreen = Boolean.Parse(RegistryAccess.GetRegistryValue(AppSettings.UserRegistryPath, "FullScreen", "true"));
		}
	}
}
