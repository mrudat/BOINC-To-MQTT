using Meziantou.Extensions.Logging.Xunit;
using Testcontainers.EMQX;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

public class EMQXIntegrationTests(ITestOutputHelper testOutputHelper) : AbstractIntegrationTests<EmqxBuilder, EmqxContainer>(testOutputHelper, XUnitLogger.CreateLogger<EMQXIntegrationTests>(testOutputHelper))
{

}

