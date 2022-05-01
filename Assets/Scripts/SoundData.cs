using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SoundData")]
public class SoundData : ScriptableObject
{
    [SerializeField] private List<SoundInfo> sounds;
    public IEnumerable<SoundInfo> Sounds => sounds;
}
