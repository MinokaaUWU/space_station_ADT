using Content.Shared.ADT.Surgery.Components;

namespace Content.Server.ADT.Surgery;

public sealed partial class SurgerySystem
{
    private static readonly TimeSpan OpenWoundCheckInterval = TimeSpan.FromSeconds(3);
    private const float OpenWoundBleedAmount = 1f;

    private TimeSpan _nextOpenWoundCheck;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTiming.CurTime < _nextOpenWoundCheck)
            return;

        _nextOpenWoundCheck = _gameTiming.CurTime + OpenWoundCheckInterval;

        var query = EntityQueryEnumerator<OperatedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!IsWoundOpen(uid, comp))
                continue;

            _bloodstream.TryModifyBleedAmount(uid, OpenWoundBleedAmount);
        }
    }

    private bool IsWoundOpen(EntityUid patient, OperatedComponent comp)
    {
        if (string.IsNullOrEmpty(comp.CurrentNode) || !TryGetGraph(comp, out var graph))
            return false;

        if (graph.StartNodes.Contains(comp.CurrentNode))
            return false;

        var node = GetNode(graph, comp.CurrentNode);
        if (node == null)
            return false;

        foreach (var edge in GetAllEdges(node))
        {
            if (EdgeConditionsMet(patient, edge))
                return true;
        }

        return false;
    }
}
