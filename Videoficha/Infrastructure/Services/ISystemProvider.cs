using System.Threading.Tasks;
using Videoficha.Models;

namespace Videoficha.Infrastructure.Services
{
    public interface ISystemProvider
    {
        Task<SystemSpec> GetSystemInfoAsync();
    }
}
