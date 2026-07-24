using System;
using System.Collections.Generic;
using System.Text;

namespace cloudflare_dns_container.Models
{
    internal class CloudflareDnsSettings
    {
        public string ApiToken { get; set; }
        public int UpdateInterval { get; set; }
        public string DnsZone { get; set; }
        public string[] Records { get; set; }

        public CloudflareDnsSettings()
        {
            ApiToken = string.Empty;
            UpdateInterval = 120;
            DnsZone = string.Empty;
            Records = Array.Empty<string>();
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(ApiToken)) return false;
            if (string.IsNullOrWhiteSpace(DnsZone)) return false;
            if (Records.Length == 0) return false;
            return true;
        }
    }
}
