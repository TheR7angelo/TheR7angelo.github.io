using System.Collections.ObjectModel;
using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Service.Interface.Services;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Components;

public partial class GithubRepoSection(IGithubService githubService, ILogger<GithubRepoSection> logger) : IDisposable
{
    private ObservableCollection<GithubRepositoryInformationDto> GithubRepositoryInformationDtos { get; } = [];
    private bool IsLoading { get; set; } = true;
    private bool IsRateLimited { get; set; }
    private string? ResetTime { get; set; }

    private int SecondsLeft { get; set; }
    private string RemainingTimeText
    {
        get
        {
            var time = TimeSpan.FromSeconds(SecondsLeft);

            return time.TotalMinutes >= 1
                ? $"{(int)time.TotalMinutes} min {time.Seconds} sec"
                : $"{time.Seconds} seconds";
        }
    }

    private CancellationTokenSource? _autoReloadCts;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await GetAllGithubRepository();
    }

    private async Task GetAllGithubRepository()
    {
        try
        {
            CancelAutoReload();

            IsLoading = true;
            IsRateLimited = false;
            ResetTime = null;
            StateHasChanged();

            var resultGithubRepositories = await githubService.GetAllGithubRepository();
            if (!resultGithubRepositories.IsSuccess)
            {
                logger.LogError("Failed to retrieve GitHub repositories");
                if (resultGithubRepositories.ErrorCode is not ErrorCode.GithubRateLimited) return;

                IsRateLimited = true;
                ExtractResetTime(resultGithubRepositories.InternalMessage);
                StartCountdown();
                return;
            }

            var result = await githubService.GetAllGithubRepositoryInformation(resultGithubRepositories.Value!);
            if (result.IsSuccess)
            {
                GithubRepositoryInformationDtos.Clear();
                foreach (var githubRepositoryInformationDto in result.Value!.OrderBy(s => s.Name))
                {
                    GithubRepositoryInformationDtos.Add(githubRepositoryInformationDto);
                }
            }
            else if (result.ErrorCode is ErrorCode.GithubRateLimited)
            {
                IsRateLimited = true;
                ExtractResetTime(result.InternalMessage);
                StartCountdown();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while loading GitHub repositories");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void ExtractResetTime(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage)) return;
        var match = System.Text.RegularExpressions.Regex.Match(errorMessage, @"\b\d{2}:\d{2}\b");
        if (!match.Success) return;

        if (TimeSpan.TryParse(match.Value, out var parsedTime))
        {
            var updatedTime = parsedTime.Add(TimeSpan.FromMinutes(1));
            ResetTime = updatedTime.ToString(@"hh\:mm");
        }
        else
        {
            ResetTime = match.Value;
        }
    }

    private void StartCountdown()
    {
        if (string.IsNullOrEmpty(ResetTime) || !TimeSpan.TryParse(ResetTime, out var targetTime)) return;

        var now = DateTime.Now.TimeOfDay;
        var difference = targetTime - now;

        if (difference.TotalSeconds <= 0)
        {
            difference = difference.Add(TimeSpan.FromDays(1));
        }

        SecondsLeft = (int)difference.TotalSeconds;

        _autoReloadCts = new CancellationTokenSource();
        var token = _autoReloadCts.Token;

        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            try
            {
                while (SecondsLeft > 0 && await timer.WaitForNextTickAsync(token))
                {
                    SecondsLeft--;
                    await InvokeAsync(StateHasChanged);
                }

                if (SecondsLeft == 0)
                {
                    logger.LogInformation("Rate limit cooldown finished. Automatically reloading repositories...");
                    await InvokeAsync(async () => await GetAllGithubRepository());
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Auto-reload countdown cancelled");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the auto-reload countdown");
            }
        }, token);
    }

    private void CancelAutoReload()
    {
        if (_autoReloadCts is null) return;
        _autoReloadCts.Cancel();
        _autoReloadCts.Dispose();
        _autoReloadCts = null;
    }

    public void Dispose()
    {
        CancelAutoReload();
        GC.SuppressFinalize(this);
    }
}