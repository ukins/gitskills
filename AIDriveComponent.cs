using CSV;
using Game.ServerCore.Logic.Pb;
using Hunter;
using Hunter.GameData;
using SXH.Network;
using SXH.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SXH.Character
{
    public abstract class AIDriveComponent : CharacterCombat
    {
        public long NetSrl { get; set; }
        protected float mfLastTargetUpdateTime = 0;
        protected Vector3 mPosBorn = Vector3.zero;
        protected bool mbNetMoving = false;
        protected bool mbFinalStep = false;
        protected bool mbPauseAI = false;
        protected int mnLockedTargetSrl = 0;

        public IAIData aiProperty { get; protected set; }

        //距离出生点距离
        public float Dis2Born { get; private set; }

        public override bool PathMovable => true;

        /// <summary>
        /// 初始化AI
        /// </summary>
        /// <param name="myProp"></param>
        /// <param name="enableAI"></param>
        protected virtual void InitAI(IAIData myProp)
        {
            if (!IsEnableAI || myProp == null)
                return;
            aiProperty = myProp;

            Utility.AI.InitAiDriveAI(gameObject);
        }

        /// <summary>
        /// 移动更新
        /// </summary>
        /// <param name="updatedPos"></param>
        protected override void InnerUpdatePosition(Vector3 updatedPos)
        {
            base.InnerUpdatePosition(updatedPos);
            Vector3 dist2born = Utility.Unit.GetPosition(this) - mPosBorn;
            dist2born.y = 0;
            Dis2Born = dist2born.magnitude;
        }

        public override void DestroySelf()
        {
            base.DestroySelf();
            Utility.AI.DestroyAiDriveAI(gameObject);
        }
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateAI();
            UpdateNetMove();
        }

        public override bool MoveTo(Vector3 Pos, EAIMoveType moveType, Action<bool> end_callback = null)
        {
            var src_pos = transform.position;
            var tgt_pos = transform.position;
            transform.position = Utility.Path.GetNearestWalkablePos1(src_pos, tgt_pos, out bool _);

            if (base.MoveTo(Pos, moveType, end_callback))
            {
                return true;
            }
            return false;
        }
        protected virtual void UpdateAI()
        {
        }


        public override void OnAttacked(AttackProperty attackerProp, List<HitContactInfo> contactInfos)
        {
            base.OnAttacked(attackerProp, contactInfos);
            // 发送受击网络消息
            NetworkManager.Instance.Battle_Tcp?.SendAttackChecks(NetSrl, this, attackerProp, contactInfos[0].ColliderCenter, contactInfos[0].PartId);
        }

        /// <summary>
        //net sync
        /// </summary>
        protected Vector3 mCurrentDestination = Vector3.zero;
        protected Vector3 mEndDir = Vector3.forward;

        public void UpdateNetMove()
        {
            if (UDPSender.IsValid == false)
                return;
            if (mCurrentDestination != Vector3.zero && mbNetMoving || mbFinalStep)
            {
                Vector3 dir = mCurrentDestination - transform.position;
                dir.y = 0;
                float minDistance = dir.magnitude;
                dir = dir.normalized;
                var speed = ResolveCurSpeed();
                float goDistance = speed.MoveSpeed * Time.deltaTime;
                if (goDistance > minDistance)
                {
                    InnerUpdatePosition(mCurrentDestination);
                    mCurrentDestination = Vector3.zero;
                    if (mbFinalStep)
                    {
                        mbFinalStep = false;
                        TurnDir(mEndDir, 0);
                        OnStop();
                    }
                }
                else
                {
                    Vector3 actualDiff = dir * goDistance;
                    MoveDelta(ref actualDiff, false);
                }
                if (CanRot() && !mRequestingRotating && dir.magnitude > 0)
                {
                    if (speed.RotateSpeed <= 0)
                    {
                        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                    }
                    else
                    {
                        TurnDir(dir, speed.RotateSpeed);
                    }
                }
            }
        }

        /// <summary>
        /// 是否使用技能
        /// </summary>
        /// <param name="skillConf"></param>
        /// <returns></returns>
        public override bool CanUseSkill(CSVSkill skillConf = null)
        {
            var mapInfo = NetData.Instance.Find<NetMapInfo>();
            if (mapInfo.IsMultiPlayerMap) //多人本的时候完全相信服务器的
            {
                return true;
            }
            if (!IsAlive)
            {
                return false;
            }
            if (SkillMgr == null || (SkillMgr != null && !SkillMgr.CanUseSkill(skillConf)))
            {
                return false;
            }
            if (BuffMgr != null && !BuffMgr.IsAllowSkill)
            {
                return false;
            }
            if (IsInAttacked)
            {
                return false;
            }
            return true;
        }

        public override void OnNetSkillStart(S2CUdpBroadcastUseSkill ret)
        {
            if (SkillMgr == null)
                return;//还未初始化完成
            if (!IsAlive || !CanUseSkill())
                return;
            SkillMgrAI skillMgrAI = SkillMgr as SkillMgrAI;
            SKillRuntime runningSkill = skillMgrAI.GetRunningMainSkill();
            skillMgrAI.ForceStopSkill(runningSkill);
            bool isUseSkillSuccess;
            if (ret.ShiftInfos != null && NetData.Instance.Find<NetMapInfo>().IsMultiPlayerMap)
            {
                var (PosList, DirList, MoveIDs) = GetSkillPosList(ret.ShiftInfos);
                skillMgrAI.SetSkillMoveServerRoutes(ret.Skillid, PosList, DirList, MoveIDs);
            }
            else
            {
                skillMgrAI.SetSkillMoveServerRoutes(ret.Skillid, null, null, null);
            }

            if (ret.DestPos == null)
            {
                isUseSkillSuccess = skillMgrAI.UseNoFixAngleSkill(ret.Skillid);
            }
            else
            {
                Vector3 destPos = NetUtility.ToVec(ret.DestPos);
                destPos.y = Utility.Path.GetGroundHeight(destPos);
                isUseSkillSuccess = skillMgrAI.UseFixAngleSkill(ret.Skillid, destPos);
            }
            if (!CanMove() && isUseSkillSuccess)
            {
                mCurrentDestination = Vector3.zero;
            }
        }
    }
}