using RunFast2.Scripts.Model;
using UnityEngine;

namespace RunFast2.Scripts.Manager
{
    public class CardAssetManager : MonoBehaviour
    {
        public static CardAssetManager Instance;

        public CardSpriteAsset SpriteAsset;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public Sprite GetCardSprite(CardSuit suit, CardRank rank)
        {
            if (SpriteAsset != null)
            {
                return SpriteAsset.GetSprite(suit, rank);
            }
            return null;
        }
        
        public Sprite GetCardBack()
        {
            return SpriteAsset != null ? SpriteAsset.CardBack : null;
        }
    }
}