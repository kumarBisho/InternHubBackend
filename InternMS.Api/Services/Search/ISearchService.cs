using InternMS.Api.DTOs.Search;

namespace InternMS.Api.Services.Search
{
    public interface ISearchService
    {
        Task<PaginatedSearchResultsDto<TaskSearchResultDto>> SearchTasksAsync(TaskSearchRequestDto request);
        Task<PaginatedSearchResultsDto<ProjectSearchResultDto>> SearchProjectsAsync(ProjectSearchRequestDto request);
        Task<PaginatedSearchResultsDto<UserSearchResultDto>> SearchUsersAsync(UserSearchRequestDto request);
        Task<GlobalSearchResultDto> GlobalSearchAsync(GlobalSearchRequestDto request);
    }
}
