using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace BM2.Infrastructure.Services;

public class BM2SieveProcessor(IOptions<SieveOptions> options) : SieveProcessor(options)
{
    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        return mapper;
    }
}