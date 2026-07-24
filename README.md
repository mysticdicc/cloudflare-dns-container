# CloudFlare DDNS Updater
Cloudflare provides free DNS services for small users which makes it ideal for home lab setups, along with its ongoing impressive uptime with rare outages making it a very stable and approachable platform. To support my home lab I created this simple container that will:
1. Connect to Cloudflare via API.
2. Update any specified DNS records with your current public IP address.

Thus creating your own software based DDNS service for your house without needing to spend any extra money on external services, for those with routers that do not support DDNS features (not only low grade router but often enterprise level equipment will not support this
as there is small use case for it in the mediume to large enterprise space).

## Getting Started
### Get your API Token from CloudFlare
1. Login to the CloudFlare dashboard (dash.cloudflare.com).
2. Open the "Account API tokens" section.
3. Create Token.
4. Edit Policy > Specify Domains > Select the domain you want to update.
5. Grant "Edit" on "DNS" permission.
6. Preview Token.
7. Create Token.
8. Copy token somewhere or leave the tab open until later.

### Setting up the Container (Linux)
1. Create a directory to store your configuration file:
```
mkdir ~/ddns
```
3. Create the configuration file:
```
nano ~/ddns/appsettings.json
```
4. Paste this into the configuration file, replacing any CAPS variables with the relevent details for your DNS zones. Do not remove any existing quotes.
```
{
  "CloudflareDnsSettings": {
    "ApiToken": "API_TOKEN",
    "UpdateInterval": 120,
    "DnsZone":  "EXAMPLE.COM",
    "Records": [
      "WWW"
    ]
  }
}
```
If you wish to add multiple DNS records to be updated you can do so by updating the "Records" section:
```
"Records": [
  "domain1",
  "domain2"
]
```
5. Save your configuration file.
6. Use the following command to run the docker container:
```
docker run -d \
--name=cloudflare-ddns \
-v /home/username/ddns/appsettings.json:/app/appsettings.json \
--restart unless-stopped \
ghcr.io/mysticdicc/cloudflare-ddns:latest
```
You can check the docker container logs to check its progress and any issues.
