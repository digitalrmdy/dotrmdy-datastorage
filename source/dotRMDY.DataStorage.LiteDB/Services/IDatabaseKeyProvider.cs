using System.Threading.Tasks;

namespace dotRMDY.DataStorage.LiteDB.Services
{
	public interface IDatabaseKeyProvider
	{
		Task<string> GetDbKey();
	}
}