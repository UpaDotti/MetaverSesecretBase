using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MetaverSesecretBase/EmoteSpriteDB")]
public class EmoteSpriteDB : ScriptableObject
{
    [SerializeField]
    private List<Sprite> _emotes;

    /// <summary>
    /// 登録済みエモート数を返す
    /// </summary>
    public int EmoteCount => _emotes?.Count ?? 0;

    /// <summary>
    /// エモートIDからスプライトを取得する
    /// </summary>
    public bool TryGetEmoteSprite(int emoteId, out Sprite sprite)
    {
        sprite = null;

        if (_emotes == null || emoteId < 0 || emoteId >= _emotes.Count)
        {
            return false;
        }

        sprite = _emotes[emoteId];
        return sprite != null;
    }
}
