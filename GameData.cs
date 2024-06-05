using UnityEngine;

namespace BlockStackGridLibrary
{
    [CreateAssetMenu(fileName = "GameData", menuName = "MoneyRectSort/GameData", order = 0)]
    public class GameData : ScriptableObject
    {
        public GameObject cellPrefab;

        public static GameData Find()
        {
            return Resources.Load<GameData>("GameData");
        }
    }
}