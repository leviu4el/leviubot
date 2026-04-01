using Newtonsoft.Json;
using System.Net;

namespace api;
public class Api
{
    public static async Task<Resource<T>> GetResultAsync<T>(string link)
    {
        using var client = new HttpClient();
        var endpoint = new Uri($"http://localhost:8080/api{link}");
        //var endpoint = new Uri($"://leviu.west-city.ts.net/api/{link}");
        string json;

        try
        {
            var response = await client.GetAsync(endpoint);
            json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ExpectationFailed) return HandleError<T>(json);

                return new Resource<T>.Error($"Status: {(int)response.StatusCode} ({response.StatusCode})\tMessage: {json}");
            }

            var result = JsonConvert.DeserializeObject<T>(json);
            if (result == null) return new Resource<T>.Error("Deserialization returned null");

            return new Resource<T>.Success(result);
        }
        catch(HttpRequestException e)
        {
            return new Resource<T>.ServerError("Connection lost");
        }
        catch (JsonReaderException)
        {
            return new Resource<T>.Error("Parsing JSON Error");
        }
        catch (JsonException e)
        {
            return new Resource<T>.Error($"JSON Error: {e.Message}");
        }
        catch (Exception e)
        {
            return new Resource<T>.Error($"Unknown Error: {e.Message}");
        }
    }

    private static Resource<T> HandleError<T>(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("error", out var errorElement))
            return new Resource<T>.ServerError("Invalid server error");

        return new Resource<T>.ServerError(error: errorElement.GetString() ?? "");
    }
}
