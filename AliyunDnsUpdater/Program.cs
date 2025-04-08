using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using CommandLine;
using Dumpify;
using System.Text.Json;

namespace AliyunDnsUpdater;
public class Program
{
	public static Task Main(string[] args) => new Program(args).MainAsync();

	public IClientProfile Profile { get; }
	public IAcsClient Client { get; }
	public HttpClient HttpClient { get; } = new();

	public ProgramArgumentOptions Options { get; private set; } = null!;
	public Config Config { get; }

	public Program(string[] args)
	{
		this.Config = Config.Load("./Config.json");
		this.Profile = DefaultProfile.GetProfile(
			this.Config.RegionId,
			this.Config.AccessKeyId,
			this.Config.AccessKeySecret
		);
		this.Client = new DefaultAcsClient(this.Profile);
		Parser.Default.ParseArguments<ProgramArgumentOptions>(args)
			.WithParsed(options => this.Options = options);
	}

	public async Task MainAsync()
	{
		if (this.Options.DescribeDomain is not null)
		{
			DescribeDomainRecordsResponse describeResponse = this.GetDomainInfo(this.Options.DescribeDomain, null, null);
			describeResponse.DomainRecords.Dump();
			this.Log($"Hint: Update info template(s):");
			foreach (DescribeDomainRecordsResponse.Record? item in describeResponse.DomainRecords)
			{
				this.Log(JsonSerializer.Serialize(
					new RecordUpdateInfo(
						item.RecordId, item.RR, item.Type, item.TTL, item.Priority, "https://api.ipify.org")).ToString());
			}
			return;
		}
		do
		{
			try
			{
				DescribeDomainRecordsResponse domainInfo = this.GetDomainInfo(this.Config.Domain, null, null);
				foreach (RecordUpdateInfo item in this.Config.RecordsToUpdate)
				{
					string ip = await this.HttpClient.GetStringAsync(item.IPSource);
					if (domainInfo.DomainRecords.Any(x => x.Value == ip))
					{
						this.Log($"No need to update {item} with {ip}");
						continue;
					}
					this.Log($"Updating {item} with {ip}");
					this.UpdateDomainRecord(item.RecordId, item.SubdomainName, item.RecordType, ip, item.TTL, item.Priority);
				}

				this.Log("Waiting for next run...");
			}
			catch (Exception ex)
			{
				this.Log($"Error: {ex}");
			}
			await Task.Delay(this.Config.DaemonOn ? this.Config.Interval : default);
		}
		while (this.Config.DaemonOn);
	}

	private void Log(string message)
	{
		Console.WriteLine($"[{DateTime.Now}] {message}");
	}
	public DescribeDomainRecordsResponse GetDomainInfo(string domainName, string? subdomainKeyword, string? recordTypeKeyword)
	{
		DescribeDomainRecordsRequest describeRequest = new()
		{
			DomainName = domainName,
			RRKeyWord = subdomainKeyword,
			TypeKeyWord = recordTypeKeyword
		};
		return this.Client.GetAcsResponse(describeRequest);
	}
	public UpdateDomainRecordResponse UpdateDomainRecord(string recordId, string subdomain, string type, string value, long? ttl, long? priority)
	{
		UpdateDomainRecordRequest request = new()
		{
			RecordId = recordId,
			RR = subdomain,
			Type = type,
			Value = value,
			TTL = ttl,
			Priority = priority
		};
		return this.Client.GetAcsResponse(request);
	}
}
