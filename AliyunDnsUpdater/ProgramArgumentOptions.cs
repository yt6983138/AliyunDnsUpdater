using CommandLine;

namespace AliyunDnsUpdater;
public class ProgramArgumentOptions
{
	[Option('d', "describeDomain", Required = false, HelpText = "Get record infos for domain.")]
	public string? DescribeDomain { get; set; }
}
