using Fun_Funding.Application.IService;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.ExternalServices.BackgroundWorkerService
{
    public class WorkerService : BackgroundService
    {

        private readonly ILogger<WorkerService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WorkerService(IServiceScopeFactory serviceScopeFactory, ILogger<WorkerService> logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var validateFundingTask = RunValidationLoop(stoppingToken, TimeSpan.FromHours(6), ValidateFundingStatus);
                    //await Task.WhenAll(validateFundingTask);
                }
                catch (Exception ex)
                {
                    // Log any exceptions to prevent the loop from breaking.
                    _logger.LogError(ex, "An error occurred while validating funding status.");
                }

                // Wait 10 seconds between each iteration.
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task RunValidationLoop(CancellationToken stoppingToken, TimeSpan delay, Func<Task> action)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing {MethodName}.", action.Method.Name);
                }

                // Wait the specified amount of time between iterations.
                await Task.Delay(delay, stoppingToken);
            }
        }

        public async Task TestHello()
        {
            _logger.LogInformation("Hello World at: {time}", DateTimeOffset.Now);
        }

        public async Task ValidateFundingStatus()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var backgroundService = scope.ServiceProvider.GetRequiredService<IBackgroundProcessService>();

                // Run both tasks concurrently
                var updateFundingStatusTask = backgroundService.UpdateFundingStatus();
                var updateProjectMilestoneStatusTask = backgroundService.UpdateProjectMilestoneStatus();

                // Await both tasks to complete
                await Task.WhenAll(updateFundingStatusTask, updateProjectMilestoneStatusTask);
            }
        }
    }
}
