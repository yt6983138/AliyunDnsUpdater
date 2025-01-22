namespace AliyunDnsUpdater;
public record RecordUpdateInfo(string RecordId, string SubdomainName, string RecordType, long? TTL, long? Priority, string IPSource);
