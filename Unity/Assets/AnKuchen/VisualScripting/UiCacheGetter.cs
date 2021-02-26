#if ANKUCHEN_VISUALSCRIPTING_SUPPORT
using AnKuchen.Map;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("UiCacheGetter")]
[UnitCategory("AnKuchen")]
public class UiCacheGetterNode : Unit
{
    [DoNotSerialize]
    public ValueInput uiCache { get; private set; }

    [DoNotSerialize]
    public ValueInput path { get; private set; }

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput outValue { get; private set; }

    protected override void Definition()
    {
        uiCache = ValueInput<GameObject>(nameof(uiCache), null);
        path = ValueInput(nameof(path), string.Empty);
        outValue = ValueOutput(nameof(outValue), GetOutput);
        Requirement(uiCache, outValue);
        Requirement(path, outValue);
    }

    private GameObject GetOutput(Flow flow)
    {
        var uiCacheValue = flow.GetValue<GameObject>(uiCache);
        var pathValue = flow.GetValue<string>(path);
        return uiCacheValue.GetComponent<UICache>().Get(pathValue);
    }
}
#endif
