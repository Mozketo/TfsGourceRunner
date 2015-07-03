using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TfsGource.Configuration
{
    public class AppSettings
    {
        public static string WindowedScriptFileName
        {
            get { return ConfigurationManager.AppSettings["WindowedScriptFileName"]; }
        }

        public static string FullscreenScriptFileName
        {
            get { return ConfigurationManager.AppSettings["FullscreenScriptFileName"]; }
        }

        public static string UserRegistryPath
        {
            get { return ConfigurationManager.AppSettings["UserRegistryPath"]; }
        }

    	public static DateTime AutoShutdown
    	{
			get
			{
				DateTime shutdown;
				var parsed = DateTime.TryParseExact(ConfigurationManager.AppSettings["AutoShutdownTime"], "HHmm",
				                                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out shutdown);
				if (!parsed)
				{
					shutdown = DateTime.MinValue.AddMinutes(1050);
				}
				return shutdown;
			}
    	}
    }
}
