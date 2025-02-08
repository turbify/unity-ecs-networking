using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct GoInGameClientSystem : ISystem
{
    private float _retryTimer; // Timer for retry logic
    private GameConnectionState _connectionState;
    public enum GameConnectionState
    {
        NotConnected,
        Connected,
    }

    public void OnCreate(ref SystemState state)
    {
        // Require NetworkId and GhostCollection to be present
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<GhostCollection>();

        // Initialize the retry timer
        _retryTimer = 0;
        _connectionState = GameConnectionState.NotConnected;
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if(_connectionState == GameConnectionState.NotConnected)
        {
            // Check if the GhostCollection singleton is present and initialized
            if (!SystemAPI.HasSingleton<GhostCollection>())
            {
                Debug.Log("GhostCollection is not ready. Skipping connection attempt.");
                return;
            }

            // Increment the retry timer
            _retryTimer += SystemAPI.Time.DeltaTime;

            // Retry connection every 3 seconds if not already connected
            if (_retryTimer < 3.0f)
            {
                return; // Wait until the retry timer reaches 3 seconds
            }

            // Reset the retry timer
            _retryTimer = 0;
            Debug.Log("Trying to create entity...");
        }

        // Create an EntityCommandBuffer to record entity changes
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        // Iterate over all entities with NetworkId but without NetworkStreamInGame
        foreach ((
            RefRO<NetworkId> networkId,
            Entity entity)
            in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            // Mark the entity as in-game
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);
            Debug.Log("Setting Client as InGame");
            _connectionState = GameConnectionState.Connected;

            // Create an RPC entity to request going in-game
            Entity rpcEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(rpcEntity, new GoInGameRequestRpc());
            entityCommandBuffer.AddComponent(rpcEntity, new SendRpcCommandRequest());
        }

        // Play back the EntityCommandBuffer and dispose of it
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}

public struct GoInGameRequestRpc : IRpcCommand
{
    // You can add fields here if needed
}