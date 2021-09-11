using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum EnemySetEnum { Combat1, Ambush1, }
[CreateAssetMenu(fileName = "EnemySets", menuName = "ScriptableObjects/Enemies", order = 1)]
public class EnemySets : ScriptableObject
{
    [System.Serializable]
    public class EnemySet
    {
        public List<EnemySetEnum> enemySetTypes;
        public List<AssetReference> enemies;
    }
    public enum BossSets { Twins }
    [System.Serializable]
    public class BossSet
    {
        public int level;
        public BossSets name;
        public List<AssetReference> bosses;
    }

    public List<EnemySet> EnemySetList = new List<EnemySet>();
    public List<BossSet> BossSetList = new List<BossSet>();

    //Enemies
    public AssetReference EyeCultist;
    public AssetReference Slime;
    public AssetReference Charger;

    //Bosses
    public AssetReference TwinRed;
    public AssetReference TwinBlue;

}
