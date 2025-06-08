using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _networkUI;
    public GameObject NetworkUI => _networkUI;

    [SerializeField]
    private Button _hostButton;
    public Button HostButton => _hostButton;

    [SerializeField]
    private Button _clientButton;
    public Button ClientButton => _clientButton;
}
