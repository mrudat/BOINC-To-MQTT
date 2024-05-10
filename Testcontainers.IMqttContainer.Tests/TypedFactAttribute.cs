// Ignore Spelling: MQTT Testcontainers Initialize TLS initialization URI mqtts wss

using Xunit.Sdk;

namespace Testcontainers.Tests;

[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class TypedFactAttribute<TObject, TInterface> : FactAttribute
{
    public TypedFactAttribute()
    {
        var objectType = typeof(TObject);
        var interfaceType = typeof(TInterface);

        if (!objectType.IsAssignableTo(interfaceType))
        {
            Skip = $"{objectType.Name} does not implement {interfaceType.Name}";
        }
    }
}
