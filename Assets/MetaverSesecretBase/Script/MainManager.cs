using Unity.Netcode;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private NetworkManager _networkManager;
    private UIManager _uiManager;



    private void Start()
    {
        _networkManager = FindAnyObjectByType<NetworkManager>();
        _uiManager = FindAnyObjectByType<UIManager>();

        _uiManager.HostButton.onClick.AddListener(() => StartHost());
        _uiManager.ClientButton.onClick.AddListener(() => StartClient());
    }

    private void StartHost()
    {
        _networkManager.StartHost();
        _uiManager.NetworkUI.SetActive(false);
    }

    private void StartClient()
    {
        _networkManager.StartClient();
        _uiManager.NetworkUI.SetActive(false);
    }
}
