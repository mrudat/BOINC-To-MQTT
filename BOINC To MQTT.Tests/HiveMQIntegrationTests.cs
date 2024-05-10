using Meziantou.Extensions.Logging.Xunit;
using Testcontainers.HiveMQ;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

public class HiveMQIntegrationTests(ITestOutputHelper testOutputHelper) : AbstractIntegrationTests<HiveMQBuilder, HiveMQContainer>(testOutputHelper, XUnitLogger.CreateLogger<HiveMQIntegrationTests>(testOutputHelper))
{

}

