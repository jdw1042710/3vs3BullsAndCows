using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TitleView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_InputField playerNameInput;

    public UnityEvent OnConnectClicked => connectButton.onClick;

    // 플레이어 이름을 가져오는 프로퍼티
    public string PlayerName => playerNameInput.text;

    private void Start()
    {
        // 초기 상태 설정
        UpdateStatusText("Ready to Connect");
        SetButtonInteractable(true);
    }

    // 상태 텍스트 갱신
    public void UpdateStatusText(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    // 버튼 활성화/비활성화 제어
    public void SetButtonInteractable(bool isInteractable)
    {
        if (connectButton != null) connectButton.interactable = isInteractable;
    }

    // 로비/게임 화면 전환 시 UI 숨김 처리 등이 필요하다면 여기에 추가
    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}