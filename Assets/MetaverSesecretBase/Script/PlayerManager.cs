using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private string _name = string.Empty;
    public string Name => _name;

    private int _characterId = -1;
    public int CharacterId => _characterId;

    private PlayerMoveController _playerMoveController;
    private NetworkPlayer _networkPlayer;



    private void Awake()
    {
        _playerMoveController = FindAnyObjectByType<PlayerMoveController>();
    }

    private void Start()
    {
        StartCoroutine(RunPlayerSetup());
    }

    private IEnumerator RunPlayerSetup()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.LocalClient.PlayerObject != null);

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        _networkPlayer = playerObject.GetComponent<NetworkPlayer>();
        _networkPlayer.SetNameServerRpc(_name);
        _networkPlayer.SetCharacterServerRpc(_characterId);

        _playerMoveController.StartMove(playerObject.gameObject);
    }

    public void SetName(string name)
    {
        _name = name;
    }

    public void SetCharacterId(int characterId)
    {
        _characterId = characterId;
    }

    public void SendEmote(int emoteId)
    {
        _networkPlayer.SendEmoteServerRpc(emoteId);
    }
}
