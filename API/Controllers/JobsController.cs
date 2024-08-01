using API.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly JobManager _jobManager;

    public JobsController(JobManager jobManager)
    {
        _jobManager = jobManager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<JobStatus>> GetJobStatuses()
    {
        return Ok(_jobManager.GetJobStatuses());
    }

    [HttpPost("{jobName}/start")]
    public async Task<IActionResult> StartJob(string jobName)
    {
        await _jobManager.StartJobAsync(jobName);
        return Ok();
    }

    [HttpPost("{jobName}/stop")]
    public async Task<IActionResult> StopJob(string jobName)
    {
        await _jobManager.StopJobAsync(jobName);
        return Ok();
    }
}