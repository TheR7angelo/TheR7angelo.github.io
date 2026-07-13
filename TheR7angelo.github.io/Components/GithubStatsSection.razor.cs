using MudBlazor;

namespace TheR7angelo.github.io.Components;

public partial class GithubStatsSection(GithubStateService stateService) : IDisposable
{
    private List<ChartSeries<double>> _chartData = [];
    private readonly ChartOptions _chartOptions = new();

    private string[] _chartLabels = [];
    private List<LanguageStatDto> _topLanguages = [];

    protected override void OnInitialized()
    {
        stateService.OnDataChanged += HandleDataChanged;

        if (stateService.HasData)
        {
            CalculerStatsGlobales();
        }
    }

    private void HandleDataChanged()
    {
        CalculerStatsGlobales();
        InvokeAsync(StateHasChanged);
    }

    private void CalculerStatsGlobales()
    {
        var repos = stateService.Repositories;
        if (repos.Count is 0) return;

        var aggregatedLanguages = repos
            .SelectMany(repo => repo.LanguagesBadges)
            .Where(badge => !string.IsNullOrEmpty(badge.Text))
            .GroupBy(badge => badge.Text)
            .Select(group => new
            {
                LanguageName = group.Key!,
                TotalBytes = group.Sum(l => l.Bytes ?? 0),
                group.FirstOrDefault()?.Color
            })
            .Where(l => l.TotalBytes > 0)
            .OrderByDescending(l => l.TotalBytes)
            .ToList();

        double grandTotalBytes = aggregatedLanguages.Sum(l => l.TotalBytes);
        if (grandTotalBytes <= 0) return;

        var localTopLanguages = new List<LanguageStatDto>();
        var labels = new List<string>();
        var newChartSeriesValues = new List<double>();
        var customPalette = new List<string>();

        foreach (var lang in aggregatedLanguages)
        {
            var percentage = Math.Round(lang.TotalBytes / grandTotalBytes * 100, 2);

            if (percentage <= 0) continue;

            newChartSeriesValues.Add(percentage);
            labels.Add($"{lang.LanguageName} ({percentage}%)");

            customPalette.Add(!string.IsNullOrEmpty(lang.Color) ? lang.Color : "#24292e");

            localTopLanguages.Add(new LanguageStatDto(
                Name: lang.LanguageName,
                Url: $"https://cdn.jsdelivr.net/gh/devicons/devicon/icons/{lang.LanguageName.ToLower()}/{lang.LanguageName.ToLower()}-original.svg",
                Percentage: percentage
            ));
        }

        _chartOptions.ChartPalette = customPalette.ToArray();


        _chartData.Clear();
        _chartData.Add(new ChartSeries<double> { Data = newChartSeriesValues.ToArray() });

        _chartLabels = labels.ToArray();
        _topLanguages = localTopLanguages.Take(3).ToList();
    }

    public void Dispose()
    {
        stateService.OnDataChanged -= HandleDataChanged;
        GC.SuppressFinalize(this);
    }

    public record LanguageStatDto(string Name, string Url, double Percentage);
}