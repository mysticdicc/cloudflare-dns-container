using CloudFlare.Client;
using CloudFlare.Client.Api.Zones;
using CloudFlare.Client.Api.Zones.DnsRecord;
using CloudFlare.Client.Enumerators;
using cloudflare_dns_container.Models;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace cloudflare_dns_container
{
    internal class Worker(ILogger<Worker> logger, IOptionsMonitor<CloudflareDnsSettings> config) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IOptionsMonitor<CloudflareDnsSettings> _config = config;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_config.CurrentValue.IsValid()) await UpdateDnsRecordsAsync();
                    else _logger.LogError("One or more configuration values is invalid.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                _logger.LogInformation($"Service action finished entering sleep for {_config.CurrentValue.UpdateInterval} seconds.");
                await Task.Delay((_config.CurrentValue.UpdateInterval * 1000), stoppingToken);
            }
        }

        private async Task UpdateDnsRecordsAsync()
        {
            using var client = new HttpClient();
            var ip = await client.GetStringAsync("https://api.ipify.org/");
            if (string.IsNullOrEmpty(ip)) return;
            _logger.LogInformation($"Current IP address is: {ip}");

            using var cfClient = new CloudFlareClient(_config.CurrentValue.ApiToken);
            var cfZone = await TryGetDnsZoneAsync(cfClient);
            if (null == cfZone) return;

            var cfRecords = await GetDnsRecordsAsync(cfClient, cfZone);
            if (cfRecords.Count == 0) return;

            foreach (var record in _config.CurrentValue.Records)
            {
                var dnsName = $"{record}.{_config.CurrentValue.DnsZone}";
                var cfRecord = cfRecords.Where(x => x.Name == dnsName).FirstOrDefault();

                if (null == cfRecord)
                {
                    _logger.LogInformation($"No record was found for {dnsName} but it is specified in configuration.");
                    continue;
                }

                if (cfRecord.Content == ip)
                {
                    _logger.LogInformation($"{dnsName} already matches current IP address.");
                    continue;
                }

                var newRecord = new ModifiedDnsRecord
                {
                    Type = DnsRecordType.A,
                    Name = dnsName,
                    Content = ip
                };

                await cfClient.Zones.DnsRecords.UpdateAsync(cfZone.Id, cfRecord.Id, newRecord);
                _logger.LogInformation($"{dnsName} updated to {ip}");
            }
        }

        private async Task<Zone?> TryGetDnsZoneAsync(CloudFlareClient client)
        {
            _logger.LogInformation("Fetching available DNS zones from CloudFlare.");
            var zones = await client.Zones.GetAsync();

            if (!zones.Success)
            {
                _logger.LogError("Failed to fetch DNS zones.");
                return null;
            }
            else if (zones.Result.Count == 0)
            {
                _logger.LogInformation("No DNS zones found but no failures occurred.");
                return null;
            }

            foreach (var zoneResult in zones.Result)
            {
                _logger.LogInformation($"DNS Zone: {zoneResult.Id} : {zoneResult.Name}");
            }

            var zone = zones.Result.Where(x => x.Name == _config.CurrentValue.DnsZone).FirstOrDefault();
            if (null == zone)
            {
                _logger.LogError($"No DNS zone found with name: {_config.CurrentValue.DnsZone}");
                return null;
            }

            return zone;
        }

        private async Task<IReadOnlyList<DnsRecord>> GetDnsRecordsAsync(CloudFlareClient client, Zone zone)
        {
            var records = await client.Zones.DnsRecords.GetAsync(zone.Id);
            if (records == null || records.Result.Count == 0)
            {
                _logger.LogInformation($"No DNS records found in zone ID: {zone.Id}");
                return [];
            }

            return records.Result;
        }
    }
}
