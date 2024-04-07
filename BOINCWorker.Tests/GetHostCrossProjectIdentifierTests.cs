using CommonStuff;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using FluentAssertions;

namespace BOINCWorker.Tests;

public class GetHostCrossProjectIdentifierTests
{
    private readonly BOINCWorkerOptions options = new()
    {
        DataPath = "C:\\Program Data\\BOINC",
        BinaryPath = "C:\\Program Files\\BOINC"
    };

    [Theory]
    [InlineData("123456")]
    [InlineData("abcdef")]
    public async Task TestReadValue(string expected)
    {
        var filesystem = new MockFileSystem();

        filesystem.AddFile("C:\\Program Data\\BOINC\\client_state.xml", new MockFileData($"<client_state><host_info><host_cpid>{expected}</host_cpid></host_info></client_state>"));

        var getHostCrossProjectIdentifier = new GetHostCrossProjectIdentifier(new OptionsWrapper(options), filesystem);

        var actual = await getHostCrossProjectIdentifier.GetHostCrossProjectIdentifierAsync();

        actual.Should().Be(expected);
    }

    [Fact]
    public async Task TestFileNotFound()
    {
        var filesystem = new MockFileSystem();

        var getHostCrossProjectIdentifier = new GetHostCrossProjectIdentifier(new OptionsWrapper(options), filesystem);

        Func<Task> act = async () => await getHostCrossProjectIdentifier.GetHostCrossProjectIdentifierAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task TestValueMissing()
    {
        var filesystem = new MockFileSystem();

        filesystem.AddFile("C:\\Program Data\\BOINC\\client_state.xml", new MockFileData("<client_state/>"));

        var getHostCrossProjectIdentifier = new GetHostCrossProjectIdentifier(new OptionsWrapper(options), filesystem);

        Func<Task> act = async () => await getHostCrossProjectIdentifier.GetHostCrossProjectIdentifierAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
