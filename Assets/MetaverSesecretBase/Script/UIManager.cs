using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Name Input UI")]
    [SerializeField]
    private GameObject _nameInputUI;
    public GameObject NameInputUI => _nameInputUI;

    [SerializeField]
    private TMP_InputField _nameInputField;
    public TMP_InputField NameInputField => _nameInputField;

    [SerializeField]
    private Button _nameFinishButton;
    public Button NameFinishButton => _nameFinishButton;

    [Header("Characte Select UI")]
    [SerializeField]
    private GameObject _characteSelectUI;
    public GameObject CharacteSelectUI => _characteSelectUI;

    [SerializeField]
    private Button _characterButton0;
    public Button CharacterButton0 => _characterButton0;

    [SerializeField]
    private Button _characterButton1;
    public Button CharacterButton1 => _characterButton1;

    [Header("Network Select UI")]
    [SerializeField]
    private GameObject _networkSelectUI;
    public GameObject NetworkSelectUI => _networkSelectUI;

    [SerializeField]
    private Button _hostButton;
    public Button HostButton => _hostButton;

    [SerializeField]
    private Button _clientButton;
    public Button ClientButton => _clientButton;

    [Header("Network Select UI")]
    [SerializeField]
    private GameObject _joinCodeUI;
    public GameObject JoinCodeUI => _joinCodeUI;

    [SerializeField]
    private TMP_InputField _joinCodeInputField;
    public TMP_InputField JoinCodeInputField => _joinCodeInputField;

    [SerializeField]
    private Button _joinCodeFinishButton;
    public Button JoinCodeFinishButton => _joinCodeFinishButton;



    public void ShowUI(UIState state)
    {
        _nameInputUI.SetActive(state == UIState.NameInput);
        _characteSelectUI.SetActive(state == UIState.CharacterSelect);
        _networkSelectUI.SetActive(state == UIState.NetworkSelect);
        _joinCodeUI.SetActive(state == UIState.JoinCodeInput);
    }
}

public enum UIState
{
    None,
    NameInput,
    CharacterSelect,
    NetworkSelect,
    JoinCodeInput
}
