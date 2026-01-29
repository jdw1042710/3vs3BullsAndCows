using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

public class NetworkController_UniTask : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private TextMeshProUGUI connectText;
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_InputField playerNameInputField;

    private NetworkRunner runner;

    private void Start()
    {
        connectText.text = "Ready to Connect";

        // UniTask 스타일의 버튼 이벤트 등록 (선택 사항)
        // 기존처럼 Inspector에서 연결해도 되지만, 코드에서 제어하면 더 명확합니다.
        // .Forget()은 "이 비동기 작업의 결과를 기다리지 않는다"고 명시하는 것입니다.
        connectButton.onClick.AddListener(() => Connect().Forget());
        connectButton.enabled = true;
    }

    // async void 대신 async UniTaskVoid를 사용합니다.
    // UniTaskVoid는 Unity 이벤트(버튼 클릭 등)에서 실행되는 비동기 함수에 최적화되어 있습니다.
    public async UniTaskVoid Connect()
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        if (runner.IsRunning) return;

        connectButton.enabled = false;
        connectText.text = "Connecting...";

        // Fusion의 StartGame은 표준 C# Task<T>를 반환합니다.
        // .AsUniTask()를 붙여서 UniTask로 변환하여 처리하면 Unity 라이프사이클과 더 잘 맞습니다.
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "test",
            PlayerCount = 6,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>() ?? runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),

            // Build Settings에 씬이 등록되어 있어야 합니다.
            Scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Main.unity"))
        }).AsUniTask();

        if (result.Ok)
        {
            connectText.text = "Success to Start Game";
            Debug.Log("Game Started Successfully");
        }
        else
        {
            // 연결 실패 시 처리
            connectText.text = $"Fail: {result.ShutdownReason}";
            connectButton.enabled = true;
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    // --- INetworkRunnerCallbacks (변경 없음) ---
    // 콜백 메서드들은 Fusion 내부에서 호출되므로 UniTask로 변경하지 않습니다.

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            connectText.text = "I Joined the Session!";
            Debug.Log($"Local Player Joined! Name Intent: {playerNameInputField.text}");
        }
        else
        {
            connectText.text = $"Player {player.PlayerId} Joined";
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        connectText.text = $"Disconnected: {shutdownReason}";
        connectButton.enabled = true;

        if (this.runner != null) Destroy(this.runner);
        runner = null;
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
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}