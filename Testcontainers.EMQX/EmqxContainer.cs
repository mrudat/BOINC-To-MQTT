// <copyright file="EmqxContainer.cs" company="Martin Rudat">
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

namespace Testcontainers.EMQX;

using System.Security.Cryptography.X509Certificates;
using DotNet.Testcontainers.Containers;

public class EmqxContainer(EmqxConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttTlsContainer, IMqttWebSocketsContainer, IMqttWebSocketsTlsContainer
{
    public ushort DashboardPort => this.GetMappedPublicPort(EmqxBuilder.DashboardPort);

    /// <inheritdoc/>
    ushort IMqttContainer.MqttPort => this.GetMappedPublicPort(EmqxBuilder.MqttPort);

    /// <inheritdoc/>
    ushort IMqttTlsContainer.MqttTlsPort => this.GetMappedPublicPort(EmqxBuilder.MqttTlsPort);

    /// <inheritdoc/>
    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => this.GetMappedPublicPort(EmqxBuilder.MqttWebSocketsPort);

    /// <inheritdoc/>
    ushort IMqttWebSocketsTlsContainer.MqttWebSocketsTlsPort => this.GetMappedPublicPort(EmqxBuilder.MqttWebSocketsTlsPort);

    public Uri GetDashboardUri() => new UriBuilder(Uri.UriSchemeHttp, this.Hostname, this.DashboardPort).Uri;

    /// <inheritdoc/>
    Uri IMqttTlsContainer.GetMqttTlsUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtts, this.Hostname, (this as IMqttTlsContainer).MqttTlsPort), userName).Uri;

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetMqttUri(string? userName) => (this as IMqttTlsContainer).GetMqttTlsUri(userName);

    /// <inheritdoc/>
    Uri IMqttContainer.GetMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, this.Hostname, (this as IMqttContainer).MqttPort), userName).Uri;

    /// <inheritdoc/>
    Uri IMqttTlsContainer.GetNetworkMqttTlsUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtts, configuration.NetworkAliases.First(), EmqxBuilder.MqttTlsPort), userName).Uri;

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => (this as IMqttTlsContainer).GetNetworkMqttTlsUri(userName);

    /// <inheritdoc/>
    Uri IMqttContainer.GetNetworkMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, configuration.NetworkAliases.First(), EmqxBuilder.MqttPort), userName).Uri;

    /// <inheritdoc/>
    Uri IMqttWebSocketsTlsContainer.GetNetworkWebSocketsTlsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWss, configuration.NetworkAliases.First(), EmqxBuilder.MqttWebSocketsTlsPort, "mqtt"), userName).Uri;

    /// <inheritdoc/>
    Uri IMqttWebSocketsContainer.GetNetworkWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, configuration.NetworkAliases.First(), EmqxBuilder.MqttWebSocketsPort, "mqtt"), userName).Uri;

    /// <inheritdoc/>
    Task<X509Certificate2> IGetServerCertificate.GetServerCertificateAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    Uri IMqttWebSocketsTlsContainer.GetWebSocketsTlsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWss, this.Hostname, (this as IMqttWebSocketsTlsContainer).MqttWebSocketsTlsPort, "mqtt"), userName).Uri;

    /// <inheritdoc/>
    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, this.Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort, "mqtt"), userName).Uri;

    private static UriBuilder SetCredentials(UriBuilder uriBuilder, string? userName)
    {
        uriBuilder.UserName = Uri.EscapeDataString(userName ?? EmqxBuilder.DefaultUserName);
        return uriBuilder;
    }
}
