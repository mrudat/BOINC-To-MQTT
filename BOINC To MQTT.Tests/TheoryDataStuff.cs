// Ignore Spelling: BOINC MQTT TCP

namespace BOINC_To_MQTT.Tests;

public static class TheoryDataStuff
{

    public static TheoryData<string> Implementing<TInterface>(Type[] classes)
    {
        var theoryData = new TheoryData<string>();

        foreach (var clazz in classes)
        {
            if (clazz.IsAssignableTo(typeof(TInterface)))
                theoryData.Add(clazz.Name);
        }
        return theoryData;
    }
}
