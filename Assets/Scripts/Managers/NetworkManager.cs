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
    [SerializeField] private TitleView titleView; // todo: 이벤트 콜백 처리 하기

    private NetworkRunner runner;

    private const int LOBBY_SCENE_INDEX = 0;
    private const int GAME_SCENE_INDEX = 1;



    #region Unity Life Cycle
    private void Start()
    {
        titleView.OnConnectClicked.AddListener(() => Connect().Forget());
    }
    #endregion

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

        // 게임 시작
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
            titleView.UpdateStatusText("Entering...");
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
    #region INetworkRunnerCallbacks
    /// <summary>
    /// 플레이어 (중도) 입장
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="player"></param>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // 게임 씬이면 GameManager에게 스폰 요청
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentSceneIndex == GAME_SCENE_INDEX)
            {           
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SpawnPlayer(runner, player);
                }
            }
        }
    }
    // Scene 이동 시 호출됨
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        titleView.SetVisible(currentSceneIndex == LOBBY_SCENE_INDEX); // Title 씬에서만 Title UI 활성화
        if (runner.IsServer && currentSceneIndex == GAME_SCENE_INDEX)
        {
            if (GameManager.Instance)
            {
                foreach (var player in runner.ActivePlayers)
                {
                    GameManager.Instance.SpawnPlayer(runner, player);
                }
            }
        }

    }

    /// <summary>
    /// 플레이어 퇴장
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DespawnPlayer(runner, player);
        }
    }

    /// <summary>
    /// 서버 종료
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="shutdownReason"></param>
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        titleView.SetButtonInteractable(true);
        titleView.UpdateStatusText($"Disconnected: {shutdownReason}");

        if (this.runner != null) Destroy(this.runner);
        runner = null;
    }


        
    #region Unused
    // 사용하지 않는 인터페이스 구현부 (빈 상태 유지)
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
    #endregion
    #endregion
}