using Lab3.Models;
using System.Threading.Tasks;

namespace Lab3.Interfaces
{
    public interface ILcgParamsProvider
    {
        long Modulus { get; }
        Task<LcgParams> GetLcgParamsAcync();
    }
}
