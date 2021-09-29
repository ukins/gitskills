using CSV;
using UnityEngine;

namespace Hunter
{
    public static class AudioParam
    {
        public const string Gender = "Gender";
        public const string Surface = "Surface";
    }

    public enum EStopMode
    {
        Allowfadeout,
        Immediate
    }

    public interface IAudioEvent
    {
        string BankName { get; }
        ETTask Create(int id, EFMODBankLife life);
        void Destory();
        void Play(GameObject go = null);
        void Stop(EStopMode mode = EStopMode.Immediate);
        void SetParameter(string name, float value);
        float GetParameter(string name);
        bool IsCompelte { get; }
        bool IsPlaying { get; }
        float Length { get; }
        void KeyOff();
    }

    public interface IIntetfaceAudio
    {
        void InitVCA();
        void Load(string name, bool loadSampleData);
        void Load(TextAsset asset, bool loadSampleData);
        void Unload(string name);
        void WaitForAllLoads();

        void SetVolume(ESettingType kind, float volume);
        void MuteAudio(bool is_muted);
        void AddAudioListener(Transform transform);

        IAudioEvent CreateEvent();
    }

    public static partial class Utility
    {
        public static class Audio
        {
            private static IIntetfaceAudio m_imple;
            public static void Set(IIntetfaceAudio imple)
            {
                m_imple = imple;
            }

            public static void InitVCA()
            {
                m_imple.InitVCA();
            }

            public static void Load(string name, bool loadSampleData)
            {
                m_imple.Load(name, loadSampleData);
            }

            public static void Load(TextAsset asset, bool loadSampleData)
            {
                m_imple.Load(asset, loadSampleData);
            }

            public static void Unload(string name)
            {
                m_imple.Unload(name);
            }

            public static void WaitForAllLoads()
            {
                m_imple.WaitForAllLoads();
            }

            public static void SetVolume(ESettingType kind, float volume)
            {
                m_imple.SetVolume(kind, volume);
            }

            public static void MuteAudio(bool is_muted)
            {
                m_imple.MuteAudio(is_muted);
            }

            public static void AddAudioListener(Transform transform)
            {
                m_imple.AddAudioListener(transform);
            }

            public static IAudioEvent CreateEvent()
            {
                return m_imple.CreateEvent();
            }
        }

        public static int ToSoundId(this string skey)
        {
            var conf = CSVSoundName.Get(skey);
            return conf != null ? conf.iSoundID : 0;
        }

        public static string EventName2BankName(this string evtName)
        {
            string[] strArr = evtName.Split('/');
            string path;
            if (strArr.Length > 0)
            {
                path = strArr.Length > 1 ? strArr[strArr.Length - 2] : strArr[0];
            }
            else
            {
                path = string.Empty;
                Debug.LogError("音效路径为非法，请不要引用该配置，谢谢合作！！！");
            }
            return path;
        }
    }
}
