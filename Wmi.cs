using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace Ox.BizTalk.TrackedMessageExtractor
{
	/// <summary>
	/// WMI searcher for BizTalk config
	/// </summary>
	internal static class BizTalkWmiSearcher
	{
		private const string MGMT_ROOT = @"root\MicrosoftBizTalkServer";

		public static void PopulateWmiSettings(Settings settings)
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(MGMT_ROOT, "SELECT * FROM MSBTS_GroupSetting");

			foreach (ManagementObject group in searcher.Get())
			{
				if (group != null)
				{
					group.Get();
					settings.BizTalkMgmtDb = group.GetMgmtValueSafe<string>("MgmtDbName");
					settings.BizTalkMgmtHost = group.GetMgmtValueSafe<string>("MgmtDbServerName");
					settings.BizTalkDTADb = group.GetMgmtValueSafe<string>("TrackingDBName");
					settings.BizTalkDTAHost = group.GetMgmtValueSafe<string>("TrackingDBServerName");
				}
			}
		}

		private static T GetMgmtValueSafe<T>(this ManagementObject mgmtObj, string key)
		{
			T result = default(T);
			try
			{
				if (mgmtObj != null)
				{
					result = (T)mgmtObj[key];
				}
			}
			catch { }
			return result;
		}
	}
}
