using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameobject;

    //konwersja gameobject preafab na entity prefab
    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                playerPrefabEntity = GetEntity(authoring.playerPrefabGameobject, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity playerPrefabEntity;
}