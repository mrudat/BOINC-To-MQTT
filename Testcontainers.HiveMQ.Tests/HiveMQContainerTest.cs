// Ignore Spelling: HiveMQ Testcontainers Initialize

using Testcontainers.Tests;
using Xunit.Abstractions;

namespace Testcontainers.HiveMQ.Tests;

public class HiveMQContainerTest(ITestOutputHelper testOutputHelper) : AbstractMqttContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper)
{
}
