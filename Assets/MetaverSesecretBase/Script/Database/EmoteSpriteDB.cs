using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MetaverSesecretBase/EmoteSpriteDB")]
public class EmoteSpriteDB : ScriptableObject
{
    [SerializeField]
    private List<Sprite> _emotes;

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
