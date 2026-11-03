using System.Net.Http.Json;
using System.Text.Json;

namespace CloneDevOpsTemplate.Models;

public class CloneResult
{
    private readonly List<string> _errors = [];

    public IReadOnlyList<string> Errors
    {
        get
        {
            lock (_errors)
            {
                return [.. _errors];
            }
        }
    }

    public bool Succeeded => Errors.Count == 0;

    public string? ErrorMessage => Succeeded ? null : string.Join(" ", Errors);

    // Thread-safe, because repositories are cloned in parallel.
    public void AddError(string error)
    {
        lock (_errors)
        {
            _errors.Add(error);
        }
    }

    public async Task<bool> CheckResponseAsync(HttpResponseMessage? response, string action)
    {
        if (response?.IsSuccessStatusCode == true)
        {
            return true;
        }

        AddError($"{action} failed ({await DescribeFailureAsync(response)}).");
        return false;
    }

    public static async Task<string> DescribeFailureAsync(HttpResponseMessage? response)
    {
        if (response is null)
        {
            return "no response";
        }

        var status = $"{(int)response.StatusCode} {response.ReasonPhrase}".TrimEnd();
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return $"{status}: {error.Message}";
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // The body is not an Azure DevOps error payload; the status code is enough.
        }

        return status;
    }

    public static CloneResult FromError(string? error)
    {
        var result = new CloneResult();
        if (!string.IsNullOrWhiteSpace(error))
        {
            result.AddError(error);
        }
        return result;
    }

    public static CloneResult Combine(params IEnumerable<CloneResult> results)
    {
        var combined = new CloneResult();
        foreach (var error in results.SelectMany(r => r.Errors))
        {
            combined.AddError(error);
        }
        return combined;
    }
}
