using System.Text.Json;

namespace AliyunDnsUpdater;
public class Config
{
	public bool DaemonOn { get; set; } = false;
	public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(10);

	public string AccessKeyId { get; set; } = "";
	public string AccessKeySecret { get; set; } = "";
	public string RegionId { get; set; } = "cn-shenzhen";
	public string Domain { get; set; } = "";
	public List<RecordUpdateInfo> RecordsToUpdate { get; set; } = new();

	public static Config Load(string path)
	{
		Config? result = null;
		try
		{
			string json = File.Exists(path) ? File.ReadAllText(path) : "";
			result = JsonSerializer.Deserialize<Config>(json);
		}
		catch { }
		if (result is null)
		{
			result = new();
			File.WriteAllText(path, JsonSerializer.Serialize(result));
		}
		return result;
	}
}
