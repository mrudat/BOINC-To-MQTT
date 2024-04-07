using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace BOINCWorker.Tests;

public class ReadOldThrottleSettingTests
{
    private readonly BOINCWorkerOptions options = new()
    {
        DataPath = "C:\\Program Data\\BOINC",
        BinaryPath = "C:\\Program Files\\BOINC"
    };

    [Theory]
    [InlineData(100)]
    [InlineData(10)]
    public async Task TestReadValue(double expected)
    {
        var logger = new Mock<ILogger<ReadOldThrottleSetting>>();

        var filesystem = new MockFileSystem();

        filesystem.AddFile("C:\\Program Data\\BOINC\\global_prefs_override.xml", new MockFileData($"<global_preferences><cpu_usage_limit>{expected}</cpu_usage_limit></global_preferences>"));

        var readOldThrottleSetting = new ReadOldThrottleSetting(logger.Object, new OptionsWrapper(options), filesystem);

        var actual = await readOldThrottleSetting.FetchAsync();

        actual.Should().Be(expected);
    }

    [Fact]
    public async Task TestFileNotFound()
    {
        var logger = new Mock<ILogger<ReadOldThrottleSetting>>();

        var filesystem = new MockFileSystem();

        filesystem.AddDirectory("C:\\Program Data\\BOINC");

        var readOldThrottleSetting = new ReadOldThrottleSetting(logger.Object, new OptionsWrapper(options), filesystem);

        Func<Task> act = async () => await readOldThrottleSetting.FetchAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task TestValueMissing()
    {
        var logger = new Mock<ILogger<ReadOldThrottleSetting>>();

        var filesystem = new MockFileSystem();

        filesystem.AddFile("C:\\Program Data\\BOINC\\global_prefs_override.xml", new MockFileData($"<global_preferences/>"));

        var readOldThrottleSetting = new ReadOldThrottleSetting(logger.Object, new OptionsWrapper(options), filesystem);

        Func<Task> act = async () => await readOldThrottleSetting.FetchAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
