using System;
using UnityEngine;

namespace RunFast2.Scripts.Model
{
    public enum ItemType
    {
        Tomato = 0,   // 番茄 (致盲/遮挡)
        Bucket = 1,   // 水桶 (泼水/模糊)
        Flower = 2,   // 鲜花 (赞美/特效)
        Bomb = 3,     // 炸弹 (震屏/扣分?)
        UpsideDown = 4, // 上下颠倒
        Earthquake = 5  // 地震
    }

    [Serializable]
    public class ItemData
    {
        public ItemType Type;
        public string Name;
        public Sprite Icon;
        public int Cost; // 价格
        public GameObject EffectPrefab; // 对应的特效预制体
    }
}