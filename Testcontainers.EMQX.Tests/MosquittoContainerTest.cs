// Ignore Spelling: EMQX Testcontainers Initialize

using Testcontainers.Tests;
using Xunit.Abstractions;

namespace Testcontainers.EMQX.Tests;

public class EmqxContainerTest(ITestOutputHelper testOutputHelper) : AbstractMqttContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper)
{
}
