using Lab3.Models;
using System.Threading.Tasks;

namespace Lab3.Interfaces
{
    public interface IAccountProvider
    {
        Task<Account> GetAccountAcync();
    }
}
