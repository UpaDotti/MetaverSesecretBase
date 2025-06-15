using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _name;

    [SerializeField]
    private SpriteRenderer _texture;

    [SerializeField]
    private CharacterSpriteDB _characterSpriteDB;

    private NetworkVariable<FixedString64Bytes> _playerName = new();
    private NetworkVariable<int> _characterId = new();


    private void OnEnable()
    {
        _playerName.OnValueChanged += OnNameChanged;
        _characterId.OnValueChanged += ONCharacterIdChange;
    }

    private void OnDisable()
    {
        _playerName.OnValueChanged -= OnNameChanged;
        _characterId.OnValueChanged -= ONCharacterIdChange;
    }

    private void OnNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        _name.text = newValue.ToString();
    }

    private void ONCharacterIdChange(int oldValue, int newValue)
    {
        _texture.sprite = _characterSpriteDB.Characters[newValue];
    }

    public override void OnNetworkSpawn()
    {
        _name.text = _playerName.Value.ToString();
        _texture.sprite = _characterSpriteDB.Characters[_characterId.Value];
    }

    [ServerRpc]
    public void SetNameServerRpc(string newName)
    {
        _playerName.Value = newName;
    }

    [ServerRpc]
    public void SetCharacterServerRpc(int characterId)
    {
        _characterId.Value = characterId;
    }
}
