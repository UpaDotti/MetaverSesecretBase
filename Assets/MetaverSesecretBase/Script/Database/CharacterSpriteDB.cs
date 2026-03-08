using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MetaverSesecretBase/CharacterSpriteDB")]
public class CharacterSpriteDB : ScriptableObject
{
    [SerializeField]
    private List<Sprite> _characters;
    public List<Sprite> Characters => _characters;
}
