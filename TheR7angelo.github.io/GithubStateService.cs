using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io;

public class GithubStateService
{
    private List<GithubRepositoryInformationDto> _repositories = [];

    public event Action? OnDataChanged;

    public List<GithubRepositoryInformationDto> Repositories
    {
        get => _repositories;
        set
        {
            _repositories = value;
            OnDataChanged?.Invoke();
        }
    }

    public bool HasData
        => _repositories.Count > 0;
}