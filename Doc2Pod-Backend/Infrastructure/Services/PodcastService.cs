using Application.Features.Podcasts.Commands.GeneratePodcast;
using Application.Features.Podcasts.DTOs.Requests;
using Application.Features.Podcasts.DTOs.Responses;
using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services
{
    public class PodcastService : IPodcastService
    {
        private readonly IPythonRagService _ragService;
        private readonly IServiceProvider _serviceProvider;

        public PodcastService(IPythonRagService ragService, IServiceProvider serviceProvider)
        {
            _ragService = ragService;
            _serviceProvider = serviceProvider;
        }

        public async Task ProcessPodcastGenerationAsync(Guid podcastId, string filePath, GeneratePodcastCommand command)
        {
            await UpdateStatus(podcastId, PodcastStatus.Processing);

            var cancellationToken = CancellationToken.None;

            try
            {
                var request = new GeneratePodcastRequest
                {
                    FileKey = filePath,
                    Mode = (int)command.Mode,
                    Topic = command.Topic,
                    StartPage = command.StartPage,
                    EndPage = command.EndPage
                };

                var taskId = await _ragService.StartGenerationAsync(request);

                PodcastGenerationStatusResponse status = null;
                bool isDone = false;

                int maxRetries = 2000;

                while (!isDone && maxRetries > 0)
                {
                    maxRetries--;
                    await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

                    try
                    {
                        status = await _ragService.GetStatusAsync(taskId);

                        if (status.Status == "DONE")
                        {
                            await SaveResult(podcastId, status.ScriptPath, status.AudioPath);
                            isDone = true;
                        }
                        else if (status.Status == "ERROR")
                        {
                            string userMessage = "Podcast generation failed.";

                            await UpdateStatus(
                                podcastId,
                                PodcastStatus.Failed,
                                userMessage
                            );
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Podcast {podcastId}] Transient error checking status: {ex.Message}. Retrying in 15s...");
                    }
                }
                if (!isDone)
                {
                    await UpdateStatus(podcastId, PodcastStatus.TimedOut, "The generation process exceeded the maximum time limit (8 hours).");
                }
            }
            catch (Exception ex)
            {
                await UpdateStatus(podcastId, PodcastStatus.Failed, $"Unexpected error: {ex.Message}");
            }
        }

        private async Task UpdateStatus(Guid id, PodcastStatus status, string error = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var podcast = await uow.Podcast.GetByIdAsync(id);

            if (podcast != null)
            {
                podcast.UpdateStatus(status, error);
                await uow.CompleteAsync();
            }
        }

        private async Task SaveResult(Guid id, string script, string audio)
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var podcast = await uow.Podcast.GetByIdAsync(id);
            if (podcast != null)
            {
                podcast.SetPaths(script, audio);
                podcast.UpdateStatus(PodcastStatus.Completed);
                await uow.CompleteAsync();
            }
        }
    }
}
