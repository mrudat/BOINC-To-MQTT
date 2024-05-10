// Ignore Spelling: Mosquitto Testcontainers Initialize

using Testcontainers.Tests;
using Xunit.Abstractions;

namespace Testcontainers.Mosquitto.Tests;

public class MosquittoContainerTest(ITestOutputHelper testOutputHelper) : AbstractMqttContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper)
{
}
