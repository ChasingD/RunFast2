using UnityEngine;
using System.Collections.Generic;
using RunFast2.Scripts.Model;

namespace RunFast2.Scripts.Manager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Sources")]
        public AudioSource BGMSource;
        public AudioSource SFXSource;

        [Header("Clips - BGM")]
        public AudioClip BGM_Lobby;
        public AudioClip BGM_Game;

        [Header("Clips - Common")]
        public AudioClip SFX_Button;
        public AudioClip SFX_Deal;       // 发牌声
        public AudioClip SFX_Win;        // 胜利
        public AudioClip SFX_Lose;       // 失败
        public AudioClip SFX_Alert;      // 倒计时警报
        public AudioClip SFX_Pass;       // 不要/过

        [Header("Clips - Card Types")]
        public AudioClip SFX_Single;
        public AudioClip SFX_Pair;
        public AudioClip SFX_Triplet;
        public AudioClip SFX_Straight;
        public AudioClip SFX_Bomb;
        public AudioClip SFX_Plane;

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

        public void PlayBGM(AudioClip clip)
        {
            if (BGMSource == null || clip == null) return;
            if (BGMSource.clip == clip && BGMSource.isPlaying) return;

            BGMSource.clip = clip;
            BGMSource.loop = true;
            BGMSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (SFXSource == null || clip == null) return;
            SFXSource.PlayOneShot(clip);
        }

        public void PlayCardSound(HandType type)
        {
            switch (type)
            {
                case HandType.Single: PlaySFX(SFX_Single); break;
                case HandType.Pair: PlaySFX(SFX_Pair); break;
                case HandType.Triplet: 
                case HandType.TripletWithOne:
                case HandType.TripletWithTwo:
                    PlaySFX(SFX_Triplet); break;
                case HandType.Straight: 
                case HandType.ConsecutivePairs:
                    PlaySFX(SFX_Straight); break;
                case HandType.Airplane: PlaySFX(SFX_Plane); break;
                case HandType.Bomb: 
                case HandType.FourWithThree:
                    PlaySFX(SFX_Bomb); break;
            }
        }
    }
}