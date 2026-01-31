using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("View Reference")]
    [SerializeField] private TitleView titleView;

    [Header("Network Settings")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private NetworkRunner runner;
    private int titleSceneIndex;

    private void Start()
    {
        titleSceneIndex = SceneManager.GetSceneByName("Boot").buildIndex;

        titleView.OnConnectClicked.AddListener(() => Connect().Forget());
    }

    /// <summary>
    /// 방 접속 로직
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid Connect()
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        if (runner.IsRunning) return;

        titleView.SetButtonInteractable(false);
        titleView.UpdateStatusText("Connecting...");

        // Fusion의 StartGame은 표준 C# Task<T>를 반환합니다.
        // .AsUniTask()를 붙여서 UniTask로 변환하여 처리하면 Unity 라이프사이클과 더 잘 맞습니다.
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "test",
            PlayerCount = 6,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>() ?? runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            Scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Main.unity")) // 연결 성공시 이동할 씬
        }).AsUniTask();

        if (result.Ok)
        {
            titleView.UpdateStatusText("Success to Start Game");
            Debug.Log("Game Started Successfully");
        }
        else
        {
            // 연결 실패 시 처리
            titleView.SetButtonInteractable(true);
            titleView.UpdateStatusText($"Fail: {result.ShutdownReason}");
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    // --- INetworkRunnerCallbacks (변경 없음) ---
    // 콜백 메서드들은 Fusion 내부에서 호출되므로 UniTask로 변경하지 않습니다.

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            titleView.UpdateStatusText("Joined the Session!");
            Debug.Log($"Local Player Joined! Name Intent: {titleView.PlayerName}");
        }
        else
        {
            titleView.UpdateStatusText($"Player {player.PlayerId} Joined");
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        titleView.SetButtonInteractable(true);
        titleView.UpdateStatusText($"Disconnected: {shutdownReason}");

        if (this.runner != null) Destroy(this.runner);
        runner = null;
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        titleView.SetVisible(currentSceneIndex == titleSceneIndex); // Title 씬에서만 Title UI 활성화
    }

    // 사용하지 않는 인터페이스 구현부 (빈 상태 유지)
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}