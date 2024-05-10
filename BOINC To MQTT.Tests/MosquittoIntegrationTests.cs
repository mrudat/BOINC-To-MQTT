// Ignore Spelling: Mosquitto

using Meziantou.Extensions.Logging.Xunit;
using Testcontainers.Mosquitto;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

public class MosquittoIntegrationTests(ITestOutputHelper testOutputHelper) : AbstractIntegrationTests<MosquittoBuilder, MosquittoContainer>(testOutputHelper, XUnitLogger.CreateLogger<MosquittoIntegrationTests>(testOutputHelper))
{

}
