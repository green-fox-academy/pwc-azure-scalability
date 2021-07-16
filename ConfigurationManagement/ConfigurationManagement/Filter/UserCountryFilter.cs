using Microsoft.FeatureManagement;
using System.Threading.Tasks;

namespace ConfigurationManagement.Filter
{
    [FilterAlias("UserCountry")]
    public class UserCountryFilter : IFeatureFilter
    {
        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            var countryWhereFetureIsOn = context.Parameters["Country"];
            return Task.FromResult(GetUserCountry() == countryWhereFetureIsOn);
        }

        private string GetUserCountry()
        {
            return "Hungary";
        }
    }
}
