
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    // 플레이어 프리팹
    private GameObject playerPrefab; 

    // 맵에 존재하는 스폰 포인트들
    private List<SpawnPoint> spawnPoints = new ();

    // 스폰된 캐릭터들을 추적 관리
    private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new ();
    // 캐릭터 팀 추적관리
    private Dictionary<PlayerRef, eTeamType> playerTeams = new ();

    #region Unity Life Cycle
    private void Start()
    {
        playerPrefab = ResourceManager.Instance.LoadAssetSync<GameObject>("Prefabs/Player.prefab");
#if UNITY_EDITOR
        // NetworkObject 컴포넌트 확인
        if (playerPrefab != null && playerPrefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError("Player prefab must have a NetworkObject component!");
        }
#endif
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// 캐릭터 생성 로직
    /// </summary>
    /// <param name="team">속한 팀</param>
    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // 이미 스폰된 플레이어라면 중복 생성 방지
        if (spawnedCharacters.ContainsKey(player) || playerTeams.ContainsKey(player))
        {
            Debug.LogWarning($"Player {player.PlayerId} is already spawned.");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not loaded!");
            return;
        }

        eTeamType team = GetBalancedTeam();

        // 적절한 스폰 위치 계산
        SpawnPoint targetPoint = GetSpawnPointForTeam(team);
        Vector3 spawnPos = targetPoint != null ? targetPoint.GetSpawnPosition() : Vector3.zero;
        Quaternion spawnRot = targetPoint != null ? targetPoint.transform.rotation : Quaternion.identity;

        // 네트워크 객체 생성 (Host만 실행해야 함)
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPos, spawnRot, player);
        spawnedCharacters.Add(player, networkPlayerObject);
        playerTeams.Add(player, team);
        Debug.Log($"[GameManager] Spawned Player {player.PlayerId} for {team}");
    }

    /// <summary>
    /// 캐릭터 삭제
    /// </summary>
    public void DespawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            spawnedCharacters.Remove(player);
            Debug.Log($"[GameManager] Despawned Player {player.PlayerId}");
        }

        if (playerTeams.ContainsKey(player))
        {
            playerTeams.Remove(player);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 팀에 따른 스폰 포인트 찾기
    /// </summary>
    /// <returns>스폰 포인트</returns>
    private SpawnPoint GetSpawnPointForTeam(eTeamType team)
    {
        if (spawnPoints.Count == 0) return null;

        // 해당 팀의 포인트만 필터링
        List<SpawnPoint> teamPoints = spawnPoints.Where(p => p.team == team).ToList();

        if (teamPoints.Count == 0)
        {
            // 팀 포인트가 없으면 전체 중에서라도 랜덤 선택 (Fallback)
            return spawnPoints[Random.Range(0, spawnPoints.Count)];
        }

        // 랜덤 선택
        return teamPoints[Random.Range(0, teamPoints.Count)];
    }

    private eTeamType GetBalancedTeam()
    {
        int redCount = 0;
        int blueCount = 0;

        // 현재 등록된 플레이어들의 팀을 셉니다.
        foreach (var team in playerTeams.Values)
        {
            if (team == eTeamType.Red) redCount++;
            else if (team == eTeamType.Blue) blueCount++;
        }

        return redCount <= blueCount ? eTeamType.Red : eTeamType.Blue;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == SceneIndex.GAME_SCENE_INDEX)
        {
            RegisterSpawnPoints();
        }
    }
    
    // 씬에 존재하는 모든 스폰 포인트를 찾아 등록
    private void RegisterSpawnPoints()
    {
        spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None).ToList();
    }

    #endregion
}
//public class GameController : MonoBehaviourPunCallbacks
//{
//    public static GameController Instance;
//    public GameObject player;
//    public List<GameObject> ballPositions;
//    public List<GameObject> activeBalls;
//    public List<GameObject> respownPoints;

//    public SafeBox safeBoxA;
//    public SafeBox safeBoxB;

//    public Text[] teamAText;
//    public Text[] teamBText;

//    private void Start()
//    {
//        Instance = this;
//        PlayerController playerController;

//        if (PhotonNetwork.CurrentRoom.PlayerCount % 2 == 1)
//        {
//            player = PhotonNetwork.Instantiate("PlayerA", new Vector3(-172, 5f, 225), Quaternion.identity);
//            playerController = player.gameObject.GetComponent<PlayerController>();
//            playerController.spawnPoint = respownPoints[0].transform.position;
//            playerController.team = 'A';
//        }
//        else
//        {
//            player = PhotonNetwork.Instantiate("PlayerB", new Vector3(-140, 5f, 298), Quaternion.identity);
//            playerController = player.gameObject.GetComponent<PlayerController>();
//            playerController.spawnPoint = respownPoints[1].transform.position;
//            playerController.team = 'B';
//        }

//        if (PhotonNetwork.IsMasterClient)
//        {
//            photonView.RPC("StartGame", RpcTarget.All);
//        }

//        Cursor.visible = false;
//        Cursor.lockState = CursorLockMode.Locked;
//        InstantiateBalls();
//    }

//    private void InstantiateBalls()
//    {
//        if (!PhotonNetwork.IsMasterClient)
//        {
//            return;
//        }
//        int num = Random.Range(0, 4);
//        Debug.Log(num);
//        Transform[] posArr = ballPositions[num].GetComponentsInChildren<Transform>();
//        foreach (Transform pos in posArr)
//        {
//            if (pos.position == Vector3.zero)
//            {
//                continue;
//            }
//            activeBalls.Add(PhotonNetwork.Instantiate("Baseball", pos.position, Quaternion.identity));
//        }
//    }

//    private void Update()
//    {

//    }

//    public void UpdateGuessBoard()
//    {
//        for (int i = 0; i < safeBoxA.textIndex; i++)
//        {
//            teamAText[i].text = MakeGuessResultString(safeBoxA, i);
//        }
//        for (int i = 0; i < safeBoxB.textIndex; i++)
//        {
//            teamBText[i].text = MakeGuessResultString(safeBoxB, i);
//        }
//    }

//    [PunRPC]
//    public void StartGame()
//    {
//        // Bulls And Cows Number Setting
//        BullsAndCows bullsAndCows = new BullsAndCows();

//        safeBoxA.SetFourNumbers(bullsAndCows.GetRandomNumber());
//        safeBoxB.SetFourNumbers(bullsAndCows.GetRandomNumber());



//        // Respawn Everybody to specific position

//        // 
//    }
//    private string MakeGuessResultString(SafeBox safebox, int i)
//    {
//        string guessResult = safebox.guessNumbers[i * 4 + 0].ToString()
//    + safebox.guessNumbers[i * 4 + 1].ToString()
//    + safebox.guessNumbers[i * 4 + 2].ToString()
//    + safebox.guessNumbers[i * 4 + 3].ToString() +
//    " strike : " + safebox.guessResults[i * 2 + 0] +
//    " ball : " + safebox.guessResults[i * 2 + 1];

//        return guessResult;
//    }
//}
