using CSV;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Hunter.Util
{
    public class AnimCurveMaker
    {
        public static AnimationCurve MakeCurve(int id)
        {
            CSVAnimCurve conf = CSVAnimCurve.Get(id);
            List<Keyframe> keyframes = conf.Keyframes;
            return MakeCurve(keyframes);
        }
        public static AnimationCurve MakeCurve(int id, float startTime, float startValue, float endValue, float duration)
        {
            CSVAnimCurve conf = CSVAnimCurve.Get(id);
            if (conf == null)
            {
                Debug.LogError("id->>>" + id);
                return null;
            }
            List<Keyframe> keyframes = new List<Keyframe>(conf.Keyframes);
            if (keyframes.Count < 2)
            {
                Debug.LogError("关键帧数据不正确");
                return null;
            }
            AnimationCurve sourceCurve = MakeCurve(id);
            Keyframe endKey = sourceCurve[sourceCurve.length - 1];
            AnimationCurve curve = new AnimationCurve();
            float oldTime = default, oldValue = default, startValueDiff = 0;
            for (int i = 0; i < keyframes.Count; ++i)
            {
                Keyframe keyframe = keyframes[i];
                if (i == 0)
                {
                    oldTime = keyframe.time;
                    oldValue = keyframe.value;
                    keyframe.time = startTime;
                    keyframe.value = startValue;
                    startValueDiff = startValue - oldValue;
                }
                else
                {
                    float timeDiff = keyframe.time - oldTime;
                    oldTime = keyframe.time;
                    if (i != keyframes.Count - 1)
                    {
                        float vd = keyframe.value - oldValue;
                        oldValue = keyframe.value;
                        keyframe.value = keyframes[i - 1].value + vd;
                        keyframe.time = keyframes[i - 1].time + timeDiff;
                    }
                    else
                    {
                        keyframe.value += startValueDiff;
                    }
                }
                keyframes[i] = keyframe;
                curve.AddKey(keyframe);
            }
            Keyframe lastKey = curve[curve.length - 1];

            if (lastKey.value > endValue)
            {
                Keyframe newKey = new Keyframe(endKey.time, endValue, endKey.inTangent, endKey.outTangent,
                    endKey.inWeight, endKey.outWeight);
                curve.AddKey(newKey);
            }
            else
            {
                lastKey.time = endKey.time;
                endKey.value = endValue;
            }
            return curve;
        }
        public static AnimationCurve MakeCurve(string keyframeStr)
        {
            if (string.IsNullOrEmpty(keyframeStr))
            {
                Debug.LogError("字符串为空");
                return null;
            }
            string[] strs = keyframeStr.Split(';');
            List<Keyframe> keyframes = new List<Keyframe>(strs.Length);
            if (!GetKeyframes(strs.ToList(), keyframes))
            {
                return null;
            }
            return MakeCurve(keyframes);
        }
        private static AnimationCurve MakeCurve(List<Keyframe> keyframes)
        {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i < keyframes.Count; ++i)
            {
                curve.AddKey(keyframes[i]);
            }
            return curve;
        }
        public static bool GetKeyframes(List<string> keyframeStrs, List<Keyframe> keyframes)
        {
            for (int i = 0; i < keyframeStrs.Count; ++i)
            {
                string[] infos = keyframeStrs[i].Split('#');
                if (infos.Length != 6)
                {
                    Debug.LogError("字符串格式不正确");
                    return false;
                }
                float time = float.Parse(infos[0]);
                float value = float.Parse(infos[1]);
                float inTangent = float.Parse(infos[2]);
                float outTangent = float.Parse(infos[3]);
                float inWeight = float.Parse(infos[4]);
                float outWeight = float.Parse(infos[5]);
                Keyframe keyframe = new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
                keyframes.Add(keyframe);
            }
            return true;
        }
    }
}