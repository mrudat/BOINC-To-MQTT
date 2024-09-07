// <copyright file="AsyncTaskCompletionSourceTests.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Tests;

using FluentAssertions;

public class AsyncTaskCompletionSourceTests
{
    [Fact]
    public async Task TestAsyncTaskCompletionSource()
    {
        var atcs = new AsyncTaskCompletionSource<int>();
        await atcs.SetResult(1);
        var result = await atcs.Task;
        result.Should().Be(1);
    }

    [Fact]
    public async Task TestVoidAsyncTaskCompletionSource()
    {
        var atcs = new AsyncTaskCompletionSource();
        await atcs.SetResult();
        await atcs.Task;
        true.Should().BeTrue();
    }
}
