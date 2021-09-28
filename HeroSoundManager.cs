/*
* @Author: xiaopan
* @LastEditors: xiaopan
* @Description:
* @Date: 2021-06-15 15:04:19
* @Modify:
*/

using System.Collections.Generic;
using CSV;
using SXH.Character;
using SXH.Scene;
using UnityEngine;

namespace Hunter
{
    public class HeroSoundManager : Singleton<HeroSoundManager>
    {
        public static string YH_SIGN = "yh_sign";
        public static string YH_DINNER = "yh_dinner";
        public static string YH_BOND = "yh_bond";
        public static string YH_HEROSWITCH = "YH_HeroSwitch";
        public static string YH_WORKBENCH = "YH_Workbench";
        public static string YH_KILLBOSS = "YH_KillBoss";
        public static string YH_DEATH = "YH_Death";
        public static string YH_REBORN = "YH_Reborn";
        public static string YH_REBORN_ENTERMAP = "YH_Reborn_Entermap";
        public static string YH_USEITEM = "YH_UseItem";
        public static string YH_HOTSPRING = "YH_Hotspring";
        public static string YH_SLEEP = "YH_Sleep";
        public static string YH_FIRST_MEET = "YH_First_Meet";
        public static string YH_UNLOCK_CHARACTERISTIC = "YH_Unlock_Characteristic";
        public static string YH_TASKCOMPLETE = "YH_TaskComplete";
        public static string YH_TASKFAILED = "YH_TaskFailed";
        public static string YH_EXPLOREEND = "YH_ExploreEnd";
        public static string YH_ENCOUNTER_BOSS = "YH_Encounter_Boss";

        private List<IConditionSlotElement> _mNormalConditions = new List<IConditionSlotElement>() { };
        private List<IConditionSlotElement> _mYingHunConditions = new List<IConditionSlotElement>() { };
        private IAudioEvent _mCurAudio;
        private int _mCurrentUid = 0;
        private bool _mIsPlayingAudio = false;
        // private Dictionary<int, IAudioEvent> _mAllAudio = new Dictionary<int, IAudioEvent>();

		
        public void InitYingHunState()
        {
            var mainplayer = SceneUnitContainer.Instance.MainPlayer as MainPlayerComponent;
            if (mainplayer == null || mainplayer.Property.CurrHeroId <= 0)
                return;

            var heroId = mainplayer.Property.CurrHeroId;

            var element = ReferencePool.Acquire<Element_YH_Dinner>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_KillBoss>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Death>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Medicine>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Item>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_TaskComplete>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_TaskFailed>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_ExploreEnd>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_EncounterBoss>().Fill(heroId);
            _mYingHunConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Reborn>().Fill(heroId);
            _mYingHunConditions.Add(element);

            foreach (var condition in _mYingHunConditions)
            {
                condition.TryListen(true);
                condition.PreCheck();
            }

            UpdateManager.Instance.UpdateEvt += YingHunUpdate;
        }

        private void YingHunUpdate()
        {
            UpdateEvt();
        }

        public void InitNormalState()
        {
            var element = ReferencePool.Acquire<Element_YH_Bond>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Sign>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_HeroSwitch>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Workbench>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Sleep>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_Hotspring>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_FirstMeet>().Fill();
            _mNormalConditions.Add(element);
            element = ReferencePool.Acquire<Element_YH_UnlockCharacteristic>().Fill();
            _mNormalConditions.Add(element);

            foreach (var condition in _mNormalConditions)
            {
                condition.TryListen(true);
                condition.PreCheck();
            }

            UpdateManager.Instance.UpdateEvt += NormalUpdate;
        }

        private void NormalUpdate()
        {
            UpdateEvt();
        }

        private void UpdateEvt()
        {
            if (_mCurAudio != null)
            {
                if (_mCurAudio.IsCompelte && _mIsPlayingAudio)
                {
                    Debug.Log("声音播放结束");
                    _mIsPlayingAudio = false;
                    this.ClearAudio();
                }
            }
        }

        public void StopNormalState()
        {
            UpdateManager.Instance.UpdateEvt -= NormalUpdate;
            foreach (var element in _mNormalConditions)
            {
                ReferencePool.Release(element);
            }

            _mNormalConditions.Clear();
        }

        public void StopYingHunState()
        {
            UpdateManager.Instance.UpdateEvt -= YingHunUpdate;
            foreach (var element in _mYingHunConditions)
            {
                ReferencePool.Release(element);
            }

            _mYingHunConditions.Clear();
        }

        private bool isWillPlaySound = false;
        public async void PlaySound(int uniqueId, int soundId)
        {
            if (_mCurrentUid != uniqueId)
            {
                if (_mCurAudio != null)
                {
                    AudioManager.Instance.Recycle(_mCurAudio);
                    _mCurAudio = null;
                }
            }
            else
            {
                if (_mCurAudio.IsPlaying || isWillPlaySound)
                    return;
            }
            _mCurrentUid = uniqueId;

            isWillPlaySound = true;
            _mCurAudio      = await AudioManager.Instance.Spawn(soundId, false);
            var cfg = CSVSoundEvent.Get(soundId);
            if (cfg != null && cfg.fStartTime > 0)
            {
                await TimerManager.Instance.WaitForSeconds(cfg.fStartTime);
            }
            _mCurAudio?.Play();
            isWillPlaySound = false;

            _mIsPlayingAudio = true;
            // _mAllAudio.Add(uniqueId, _mCurAudio);
            Debug.Log("声音开始播放");
        }

        private void ClearAudio()
        {
            //_mCurAudio.Stop();
            //_mCurAudio.Destory();
            AudioManager.Instance.Recycle(_mCurAudio);
            _mCurAudio = null;
            // _mAllAudio.Remove(_mCurrentUid);
            _mCurrentUid = 0;
        }
    }
}