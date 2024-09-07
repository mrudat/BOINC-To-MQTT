// <copyright file="HomeAssistantContainer.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace Testcontainers.HomeAssistant;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Containers;

public class HomeAssistantContainer(
    HomeAssistantConfiguration configuration) : DockerContainer(configuration), IAsyncDisposable
{
    private string? authCode = null;

    private DateTimeOffset expiresAt;

    private bool isMqttConfigured = false;

    private bool isOnboarded = false;

    private string? refreshToken = null;

    private ushort? rpcPort = null;

    private Uri? rpcUri = null;

    private HttpClient? webUiHttpClient;

    private ushort? webUiPort = null;

    private Uri? webUIUri = null;

    /// <summary>
    /// Gets the TCP port for RPC interaction with Home Assistant.
    /// </summary>
    public ushort RpcPort => this.rpcPort ?? throw new InvalidOperationException("Container is not running");

    /// <summary>
    /// Gets the <see cref="Uri"/> for RPC interaction with Home Assistant.
    /// </summary>
    public Uri RpcUri => this.rpcUri ?? throw new InvalidOperationException("Container is not running");

    /// <summary>
    /// Gets a HttpClient configured to talk to the Web UI.
    /// </summary>
    public HttpClient WebUiHttpClient => this.webUiHttpClient ?? throw new InvalidOperationException("Container has not been started");

    /// <summary>
    /// Gets the TCP port for user interaction with Home Assistant.
    /// </summary>
    public ushort WebUIPort => this.webUiPort ?? throw new InvalidOperationException("Container is not running");

    /// <summary>
    /// Gets the <see cref="Uri"/> for user interaction with Home Assistant.
    /// </summary>
    public Uri WebUIUri => this.webUIUri ?? throw new InvalidOperationException("Container is not running");

    public Task<ExecResult> CheckConfigAsync(CancellationToken cancellationToken = default)
    {
        return this.ExecAsync(["python", "-m", "homeassistant", "--script", "check_config", "--config", "/config"], cancellationToken);
    }

    public Task ConfigureMqttAsync(IMqttContainer mqttContainer, string? userName = null, CancellationToken cancellationToken = default)
    {
        return this.ConfigureMqttAsync(mqttContainer.GetNetworkMqttUri(userName), cancellationToken);
    }

    public async Task ConfigureMqttAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        if (!this.isOnboarded)
        {
            await this.PerformOnBoardingAsync(cancellationToken).ConfigureAwait(false);
        }

        if (this.isMqttConfigured)
        {
            return;
        }

        if (this.expiresAt <= DateTimeOffset.UtcNow)
        {
            await this.RefreshBearerTokenAsync(cancellationToken).ConfigureAwait(false);
        }

        using var flowDocument = await this.JsonPostAsync(
            "/api/config/config_entries/flow", "{ \"handler\": \"mqtt\" }", cancellationToken).ConfigureAwait(false);

        var flowIdentifier = flowDocument.RootElement.GetProperty("flow_id").GetString();

        var userInfo = uri.UserInfo;

        var parts = userInfo.Split(':');

        var (userName, password) = parts.Length switch
        {
            1 => (parts[0], string.Empty),
            2 => (parts[0], parts[1]),
            _ => throw new ArgumentException($"Don't know how to parse {userInfo}", nameof(uri)),
        };

        var payload = new MqttConfigurationFlowData()
        {
            Broker = uri.Host,
            Port = uri.Port,
            UserName = userName,
            Password = password,
        }.SerializeToJsonUtf8Bytes();

        using var document = await this.JsonPostAsync(
            $"/api/config/config_entries/flow/{flowIdentifier}",
            payload,
            cancellationToken).ConfigureAwait(false);

        this.isMqttConfigured = true;
    }

    /// <inheritdoc/>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        this.WebUiHttpClient.Dispose();
        await this.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public async Task GetBearerTokenAsync(CancellationToken cancellationToken = default)
    {
        var userResponse = await this.WebUiHttpClient.PostAsync(
            "/auth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["client_id"] = this.WebUIUri.ToString(),
                ["grant_type"] = "authorization_code",
                ["code"] = this.authCode!,
            }),
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();

        // TODO make this an actual object?
        var document = JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));

        var token_type = document.RootElement.GetProperty("token_type").GetString();

        if (token_type?.Equals("Bearer") != true)
        {
            throw new NotImplementedException($"Don't know how to handle a token type of {token_type ?? "null"}");
        }

        var accessToken = document.RootElement.GetProperty("access_token").GetString()!;

        this.refreshToken = document.RootElement.GetProperty("refresh_token").GetString()!;

        var expiresIn = TimeSpan.FromSeconds(document.RootElement.GetProperty("expires_in").GetInt32());

        expiresIn /= 2;

        this.expiresAt = DateTimeOffset.UtcNow.Add(expiresIn);

        // TODO validate token?

        this.WebUiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task PerformOnBoardingAsync(CancellationToken cancellationToken = default)
    {
        if (this.isOnboarded)
        {
            return;
        }

        await this.OnBoardingCreateUser(cancellationToken).ConfigureAwait(false);

        await this.GetBearerTokenAsync(cancellationToken).ConfigureAwait(false);

        await this.OnBoardingIntegrationAsync(cancellationToken).ConfigureAwait(false);

        await this.OnBoardingCoreConfigAsync(cancellationToken).ConfigureAwait(false);

        await this.OnBoardingRemainingTasks(cancellationToken).ConfigureAwait(false);

        this.isOnboarded = true;
    }

    public async Task RefreshBearerTokenAsync(CancellationToken cancellationToken = default)
    {
        var userResponse = await this.WebUiHttpClient.PostAsync(
            "/auth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["client_id"] = this.WebUIUri.ToString(),
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = this.refreshToken!,
            }),
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();

        // TODO make this an actual object?
        var document = JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));

        var token_type = document.RootElement.GetProperty("token_type").GetString();

        if (token_type?.Equals("Bearer") != true)
        {
            throw new NotImplementedException($"Don't know how to handle a token type of {token_type ?? "null"}");
        }

        var accessToken = document.RootElement.GetProperty("access_token").GetString()!;

        var expiresIn = TimeSpan.FromSeconds(document.RootElement.GetProperty("expires_in").GetInt32());

        expiresIn /= 2;

        this.expiresAt = DateTimeOffset.UtcNow.Add(expiresIn);

        // TODO validate token?

        this.WebUiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken ct = default)
    {
        await base.StartAsync(ct).ConfigureAwait(false);

        this.webUiPort = this.GetMappedPublicPort(HomeAssistantBuilder.WebUIPort);
        this.rpcPort = this.GetMappedPublicPort(HomeAssistantBuilder.RpcPort);

        this.webUIUri = new UriBuilder(Uri.UriSchemeHttp, this.Hostname, this.WebUIPort).Uri;
        this.rpcUri = new UriBuilder(Uri.UriSchemeHttp, this.Hostname, this.RpcPort).Uri;

        this.webUiHttpClient = new() { BaseAddress = this.WebUIUri };
    }

    public async Task<string> TemplateAsync(string template, CancellationToken cancellationToken = default)
    {
        if (!this.isOnboarded)
        {
            await this.PerformOnBoardingAsync(cancellationToken).ConfigureAwait(false);
        }

        if (this.expiresAt <= DateTimeOffset.UtcNow)
        {
            await this.RefreshBearerTokenAsync(cancellationToken).ConfigureAwait(false);
        }

        var requestDocument = new TemplateRequestData()
        {
            Template = template,
        }.SerializeToJsonUtf8Bytes();

        return await this.PostAsync("/api/template", requestDocument, cancellationToken).ConfigureAwait(false);
    }

    private async Task<JsonDocument> JsonGetAsync(string path, CancellationToken cancellationToken = default)
    {
        var userResponse = await this.WebUiHttpClient.GetAsync(
            path,
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();

        return JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));
    }

    private async Task<JsonDocument> JsonPostAsync(string path, string request, CancellationToken cancellationToken = default)
    {
        var userResponse = await this.WebUiHttpClient.PostAsync(
            path,
            new StringContent(request, Encoding.UTF8, "application/json"),
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();

        return JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));
    }

    private async Task<JsonDocument> JsonPostAsync(string path, byte[] request, CancellationToken cancellationToken = default)
    {
        var requestContent = new ByteArrayContent(request);
        requestContent.Headers.Add("Content-Type", "application/json");

        var userResponse = await this.WebUiHttpClient.PostAsync(
            path,
            requestContent,
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();

        return JsonDocument.Parse(await userResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));
    }

    private Task OnBoardingCoreConfigAsync(CancellationToken cancellationToken)
    {
        var payload = new OnBoardingCoreConfigurationData()
        {
            Language = configuration.Language!,
            TimeZone = configuration.TimeZone!,
            Currency = configuration.Currency!,
            Country = configuration.Country!,
        }.SerializeToJsonUtf8Bytes();

        return this.VoidPostAsync(
            "/api/onboarding/core_config",
            payload,
            cancellationToken);
    }

    private async Task OnBoardingCreateUser(CancellationToken cancellationToken)
    {
        var payload = new OnBoardingOwnerData()
        {
            ClientIdentifier = this.WebUIUri,
            Name = configuration.OwnerDisplayName!,
            UserName = configuration.OwnerUserName!,
            Password = configuration.OwnerPassword!,
            Language = configuration.Language!,
        }.SerializeToJsonUtf8Bytes();

        using var document = await this.JsonPostAsync("/api/onboarding/users", payload, cancellationToken).ConfigureAwait(false);

        this.authCode = document.RootElement.GetProperty("auth_code").GetString();
    }

    private Task OnBoardingIntegrationAsync(CancellationToken cancellationToken)
    {
        var payload = new OnBoardingIntegrationData()
        {
            ClientIdentifier = this.WebUIUri,
            RedirectUri = this.WebUIUri,
        }.SerializeToJsonUtf8Bytes();

        return this.VoidPostAsync(
            "/api/onboarding/integration",
            payload,
            cancellationToken);
    }

    private async Task OnBoardingRemainingTasks(CancellationToken cancellationToken)
    {
        while (true)
        {
            using var document = await this.JsonGetAsync("/api/onboarding", cancellationToken).ConfigureAwait(false);

            var step = document.RootElement.EnumerateArray()
                .Where(item => !item.GetProperty("done").GetBoolean())
                .Select(item => item.GetProperty("step").GetString())
                .FirstOrDefault(step => step != null && step.All(char.IsAsciiLetterOrDigit));

            if (step == null)
            {
                break;
            }

            await this.VoidPostAsync($"/api/onboarding/{step}", "{}", cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<string> PostAsync(string path, byte[] request, CancellationToken cancellationToken = default)
    {
        var requestContent = new ByteArrayContent(request);
        requestContent.Headers.Add("Content-Type", "application/json");

        var userResponse = await this.WebUiHttpClient.PostAsync(
            path,
            requestContent,
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();

        return await userResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task VoidPostAsync(string path, string request, CancellationToken cancellationToken = default)
    {
        var userResponse = await this.WebUiHttpClient.PostAsync(
            path,
            new StringContent(request, Encoding.UTF8, "application/json"),
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();
    }

    private async Task VoidPostAsync(string path, byte[] request, CancellationToken cancellationToken = default)
    {
        var requestContent = new ByteArrayContent(request);
        requestContent.Headers.Add("Content-Type", "application/json");

        var userResponse = await this.WebUiHttpClient.PostAsync(
            path,
            requestContent,
            cancellationToken)
        .ConfigureAwait(false);

        userResponse.EnsureSuccessStatusCode();
    }
}
