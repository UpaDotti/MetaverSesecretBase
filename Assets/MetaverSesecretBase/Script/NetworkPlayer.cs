using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _name;

    [SerializeField]
    private SpriteRenderer _texture;

    [SerializeField]
    private Image _emoteImage;

    [SerializeField]
    private CharacterSpriteDB _characterSpriteDB;

    [SerializeField]
    private EmoteSpriteDB _emoteSpriteDB;

    private NetworkVariable<FixedString64Bytes> _playerName = new();
    private NetworkVariable<int> _characterId = new();
    private NetworkVariable<int> _emoteId = new(-1);
    private NetworkVariable<int> _emoteSequence = new(0);

    private float _emoteHideAtTime = -1f;


    private void OnEnable()
    {
        _playerName.OnValueChanged += OnNameChanged;
        _characterId.OnValueChanged += OnCharacterIdChange;
        _emoteSequence.OnValueChanged += OnEmoteSeqChanged;
    }

    private void OnDisable()
    {
        _playerName.OnValueChanged -= OnNameChanged;
        _characterId.OnValueChanged -= OnCharacterIdChange;
        _emoteSequence.OnValueChanged -= OnEmoteSeqChanged;
    }

    /// <summary>
    /// プレイヤー名が変更されたときの処理
    /// </summary>
    private void OnNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        _name.text = newValue.ToString();
    }

    /// <summary>
    /// キャラクターIDが変更されたときの処理
    /// </summary>
    private void OnCharacterIdChange(int oldValue, int newValue)
    {
        _texture.sprite = _characterSpriteDB.Characters[newValue];
    }

    /// <summary>
    /// エモートが変更されたときの処理
    /// </summary>
    private void OnEmoteSeqChanged(int oldValue, int newValue)
    {
        ApplyEmoteVisual(_emoteId.Value);
    }

    /// <summary>
    /// オブジェクトがスポーンしたときの処理
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _name.text = _playerName.Value.ToString();
        _texture.sprite = _characterSpriteDB.Characters[_characterId.Value];
        ApplyEmoteVisual(_emoteId.Value);
    }

    private void Update()
    {
        TryHideExpiredEmote();
    }

    /// <summary>
    /// 表示期限を過ぎたエモートを閉じる
    /// </summary>
    private void TryHideExpiredEmote()
    {
        if (_emoteImage == null || !_emoteImage.gameObject.activeSelf)
        {
            return;
        }

        if (Time.time >= _emoteHideAtTime)
        {
            _emoteImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// エモートを表示
    /// </summary>
    private void ApplyEmoteVisual(int emoteId)
    {
        if (_emoteImage == null)
        {
            return;
        }

        if (_emoteSpriteDB == null || !_emoteSpriteDB.TryGetEmoteSprite(emoteId, out Sprite emoteSprite))
        {
            _emoteImage.gameObject.SetActive(false);
            return;
        }

        _emoteImage.sprite = emoteSprite;
        _emoteImage.gameObject.SetActive(true);
        _emoteHideAtTime = Time.time + 2f;
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

    [ServerRpc]
    public void SendEmoteServerRpc(int emoteId)
    {
        if (_emoteSpriteDB == null || !_emoteSpriteDB.TryGetEmoteSprite(emoteId, out _))
        {
            return;
        }

        _emoteId.Value = emoteId;
        _emoteSequence.Value++;
    }
}
