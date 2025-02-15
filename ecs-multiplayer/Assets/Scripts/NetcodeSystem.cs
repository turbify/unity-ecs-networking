using TMPro;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetcodeSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField _addressField;
    [SerializeField] private TMP_InputField _portField;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _serverButton;

    private ushort Port => ushort.Parse(_portField.text);
    private string Address => _addressField.text;

    private void OnEnable()
    {
        _connectButton.onClick.AddListener(OnButtonConnect);
        _hostButton.onClick.AddListener(OnButtonHost);
        _serverButton.onClick.AddListener(OnButtonServer);
    }

    private void OnDisable()
    {
        _connectButton.onClick.RemoveAllListeners();
        _hostButton.onClick.RemoveAllListeners();
        _serverButton.onClick.RemoveAllListeners();
    }

    private void Awake()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if(arg == "-launch-as-server")
            {
                DestroyLocalSimulationWorld();
                SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

                StartServer();
            }
        }
    }

    private void OnButtonConnect()
    {
        DestroyLocalSimulationWorld();
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

        StartClient();
    }

    private void OnButtonHost()
    {
        DestroyLocalSimulationWorld();
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

        StartServer();

        StartClient();
    }

    private void OnButtonServer()
    {
        DestroyLocalSimulationWorld();
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

        StartServer();
    }

    

    private void StartServer()
    {
        World serverWorld = ClientServerBootstrap.CreateServerWorld("Magus Server World");

        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
        {
            using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }
    }

    private void StartClient()
    {
        World clientWorld = ClientServerBootstrap.CreateClientWorld("Magus Client World");

        if(World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        {
            using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        }
    }

    private void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }
}
