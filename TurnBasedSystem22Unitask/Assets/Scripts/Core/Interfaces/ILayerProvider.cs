using UnityEngine;

public interface ILayerProvider
{
    LayerMask EnemyMask { get; }
    LayerMask OptimisedGroupMask { get; }
    LayerMask DefaultMask { get; }
    LayerMask UnaddedObjectMask { get; }
    int EnemyLayer { get; }
}
