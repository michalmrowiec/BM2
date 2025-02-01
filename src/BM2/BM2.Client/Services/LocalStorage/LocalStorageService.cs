using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BM2.Client.Services.LocalStorage
{
    public interface ILocalStorageService
    {
        Task<T> GetItemAsync<T>(string key);
        Task RemoveItemAsync(string key);
        Task SetItemAsync<T>(string key, T value);
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonConvert.SerializeObject(value));
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

            if (json == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
