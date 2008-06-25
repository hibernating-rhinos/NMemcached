namespace NMemcached
{
	public enum CacheOperationResult
	{
		NotStored = 1,
		NotFound = 2,  
		Stored = 3,
		Deleted = 4,
		Exists = 5
	}
}