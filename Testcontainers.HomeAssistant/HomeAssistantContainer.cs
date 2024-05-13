// Ignore Spelling: RPC homeassistant API onboarding username MQTT

using DotNet.Testcontainers.Containers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Testcontainers.HomeAssistant;

public class HomeAssistantContainer(
    HomeAssistantConfiguration configuration) : DockerContainer(configuration), IAsyncDisposable
{
    /// <summary>
    /// The TCP port for user interaction with Home Assistant.
    /// </summary>
    public ushort WebUIPort => webUiPort ?? throw new InvalidOperationException("Container is not running");

    /// <summary>
    /// The TCP port for RPC interaction with Home Assistant.
    /// </summary>
    public ushort RpcPort => rpcPort ?? throw new InvalidOperationException("Container is not running");

    /// <summary>
    /// The <see cref="Uri"/> for user interaction with Home Assistant.
    /// </summary>
    public Uri WebUIUri => webUIUri ?? throw new InvalidOperationException("Container is not running");

    /// <summary>
    /// The <see cref="Uri"/> for RPC interaction with Home Assistant.
    /// </summary>
    public Uri RpcUri => rpcUri ?? throw new InvalidOperationException("Container is not running");

    private ushort? webUiPort = null;

    private ushort? rpcPort = null;

    private Uri? webUIUri = null;

    private Uri? rpcUri = null;

    private HttpClient? webUiHttpClient;

    private HttpClient WebUiHttpClient => webUiHttpClient ?? throw new InvalidOperationException("Container has not been started");

    private bool IsOnboarded = false;

    private string? AuthCode = null;

    private string? AccessToken = null;

    private string? RefreshToken = null;

    private bool IsMqttConfigured = false;

    public async override Task StartAsync(CancellationToken ct = default)
    {
        await base.StartAsync(ct);

        webUiPort = GetMappedPublicPort(HomeAssistantBuilder.WebUIPort);
        rpcPort = GetMappedPublicPort(HomeAssistantBuilder.RpcPort);

        webUIUri = new UriBuilder(Uri.UriSchemeHttp, Hostname, WebUIPort).Uri;
        rpcUri = new UriBuilder(Uri.UriSchemeHttp, Hostname, RpcPort).Uri;

        webUiHttpClient = new() { BaseAddress = WebUIUri };
    }

    public async Task<ExecResult> CheckConfig(CancellationToken cancellationToken = default)
    {
        return await ExecAsync(["python", "-m", "homeassistant", "--script", "check_config", "--config", "/config"], cancellationToken);
    }

    public async Task PerformOnBoardingAsync(CancellationToken cancellationToken = default)
    {
        if (IsOnboarded)
            return;

        // create new user.
        {
            using var document = await PostAsync(
                "/api/onboarding/users",
                $$"""
                {
                    "client_id": "{{WebUIUri}}",
                    "name": "{{configuration.OwnerDisplayName}}",
                    "username": "{{configuration.OwnerUserName}}",
                    "password": "{{configuration.OwnerPassword}}",
                    "language": "{{configuration.Language}}"
                }
                """,
                cancellationToken
                );

            AuthCode = document.RootElement.GetProperty("auth_code").GetString();
        }

        await GetBearerToken(cancellationToken);

        {
            using var document = await PostAsync(
                "/api/onboarding/integration",
                $$"""
                {
                    "client_id": "{{WebUIUri}}",
                    "redirect_uri": "{{WebUIUri}}"
                }
                """,
                cancellationToken
                );
        }

        {
            // TODO country.
            using var document = await PostAsync(
                "/api/onboarding/core_config",
                $$"""
                {
                    "language": "{{configuration.Language}}",
                    "time_zone": "{{configuration.TimeZone}}",
                    "currency": "{{configuration.Currency}}"
                }
                """,
                cancellationToken
                );
        }


        bool done = false;

        while (!done)
        {
            using var document = await GetAsync("/api/onboarding", cancellationToken);

            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.GetProperty("done").GetBoolean())
                    continue;
                var step = item.GetProperty("step").GetString();
                if (step == null || !step.All(Char.IsAsciiLetterOrDigit))
                    continue;
                using var document2 = await PostAsync($"/api/onboarding/{step}", "{}", cancellationToken);
                goto again;
            }

            done = true;
again:;
        }

        IsOnboarded = true;
    }

    public async Task GetBearerToken(CancellationToken cancellationToken = default)
    {
        var userResponse = await WebUiHttpClient.PostAsync(
            "/auth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["client_id"] = WebUIUri.ToString(),
                ["grant_type"] = "authorization_code",
                ["code"] = AuthCode!
            }),
            cancellationToken
        );

        userResponse.EnsureSuccessStatusCode();

        // TODO make this an actual object?
        var document = JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken));

        var token_type = document.RootElement.GetProperty("token_type").GetString();

        if (token_type?.Equals("Bearer") != true)
            throw new NotImplementedException($"Don't know how to handle a token type of {token_type ?? "null"}");

        AccessToken = document.RootElement.GetProperty("access_token").GetString()!;

        RefreshToken = document.RootElement.GetProperty("refresh_token").GetString()!;

        var expiresIn = TimeSpan.FromSeconds(document.RootElement.GetProperty("expires_in").GetInt32());

        // TODO refresh in expiresIn/2?

        // TODO validate token?

        WebUiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    }

    public async Task RefreshBearerToken(CancellationToken cancellationToken = default)
    {
        var userResponse = await WebUiHttpClient.PostAsync(
            "/auth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["client_id"] = WebUIUri.ToString(),
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = RefreshToken!
            }),
            cancellationToken
        );

        userResponse.EnsureSuccessStatusCode();

        // TODO make this an actual object?
        var document = JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken));

        var token_type = document.RootElement.GetProperty("token_type").GetString();

        if (token_type?.Equals("Bearer") != true)
            throw new NotImplementedException($"Don't know how to handle a token type of {token_type ?? "null"}");

        AccessToken = document.RootElement.GetProperty("access_token").GetString()!;

        var expiresIn = TimeSpan.FromSeconds(document.RootElement.GetProperty("expires_in").GetInt32());

        // TODO refresh in expiresIn/2?

        // TODO validate token?

        WebUiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    }

    private async Task<JsonDocument> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        var userResponse = await WebUiHttpClient.GetAsync(
            path,
            cancellationToken
        );

        userResponse.EnsureSuccessStatusCode();

        return JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken));
    }

    private async Task<JsonDocument> PostAsync(string path, string request, CancellationToken cancellationToken = default)
    {
        var userResponse = await WebUiHttpClient.PostAsync(
            path,
            new StringContent(request, Encoding.UTF8, "application/json"),
            cancellationToken
        );

        userResponse.EnsureSuccessStatusCode();

        return JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken));
    }

    public Task ConfigureMqttAsync(IMqttContainer mqttContainer, string? userName = null, CancellationToken cancellationToken = default)
    {
        return ConfigureMqttAsync(mqttContainer.GetNetworkMqttUri(userName), cancellationToken);
    }

    public async Task ConfigureMqttAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        if (!IsOnboarded)
            await PerformOnBoardingAsync(cancellationToken);

        if (IsMqttConfigured)
            return;

        using var flowDocument = await PostAsync(
            "/api/config/config_entries/flow", "{ \"handler\": \"mqtt\" }", cancellationToken);

        var flowIdentifier = flowDocument.RootElement.GetProperty("flow_id").GetString();

        var userInfo = uri.UserInfo;

        var parts = userInfo.Split(':');

        var (userName, password) = parts.Length switch
        {
            1 => (parts[0], ""),
            2 => (parts[0], parts[1]),
            _ => throw new ArgumentException($"Don't know how to parse {userInfo}", nameof(uri)),
        };

        using var document = await PostAsync(
            $"/api/config/config_entries/flow/{flowIdentifier}",
            $$"""
            {
                "broker": "{{uri.Host}}",
                "port": {{uri.Port}},
                "username": "{{userName}}",
                "password": "{{password}}"
            }
            """,
            cancellationToken);

        IsMqttConfigured = true;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        WebUiHttpClient.Dispose();
        await DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
