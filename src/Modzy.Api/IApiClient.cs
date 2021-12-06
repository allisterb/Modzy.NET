namespace Modzy;
interface IApiClient
{
    Task<T> RestHttpGetAsync<T>(string query);
}

