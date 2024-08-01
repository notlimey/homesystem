using API.Classes.Job.Jobs;
using API.Services.Job;

namespace API.Extensions;


public static partial class JobsExtension
{
    public static IServiceCollection InitializeJobs(this IServiceCollection services)
    {
        services.AddSingleton<IJob, WeatherUpdateJob>();
        return services;
    }
}