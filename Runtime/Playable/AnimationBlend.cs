using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game.Basic {
    internal struct AnimationClipInfo {
        public AnimationClip clip;
        public float speed;
    }
    internal struct Blend2DClipInfo {
        public Vector2 Pos;
        public IPlayable Clip;
        public float Angle; // 仅直接混合使用
        public List<LimitPos> LimitPos; // 仅复杂混合使用
    }
    
    public struct LimitPos {
        public Vector2 Pos;
        public Vector2 Direct;
        public float Distance;
    }

    internal abstract class IAnimaBlendTree {
        public string name;
        public string parameterName;
        public Vector2 paraValue;
        public AnimationMixerPlayable mixer;
        public List<Blend2DClipInfo> clipInfos;
        
        public abstract void AddClip(IPlayable clip, Vector2 pos);

        public abstract void CalcuClipsWeight(Vector2 value);

        public virtual void ResetTime() {
            for(int i = 0; i < mixer.GetInputCount(); i++) {
                mixer.GetInput(i).SetTime(0);
            }
        }
    }

    /// <summary>
    /// 将坐标转化成角度，按角度混合
    /// pos -> angle
    /// </summary>
    internal class AnimDirectBlender : IAnimaBlendTree {
        public override void AddClip(IPlayable clip, Vector2 pos) {
            if(clipInfos == null) {
                clipInfos = new List<Blend2DClipInfo>();
            }
            float cosAngle = math.acos(math.dot(pos.normalized, AnimationLayer.normalVector));
            clipInfos.Add(new Blend2DClipInfo {
                Angle = pos.x >= 0 ? cosAngle : 2 * math.PI - cosAngle,
                Pos = pos,
                Clip = clip,
                LimitPos = new List<LimitPos>(),
            });
            clipInfos.Sort((x, y) => {
                return x.Angle > y.Angle ? 1 : -1;
            });
        }

        public override void CalcuClipsWeight(Vector2 value) {
            int length = clipInfos.Count;
            NativeArray<float> weights = new NativeArray<float>(length, Allocator.Temp);
            if(value != Vector2.zero && length > 0) {
                float tempAngle = math.acos(math.dot(value.normalized, AnimationLayer.normalVector));
                tempAngle = value.x >= 0 ? tempAngle : 2 * math.PI - tempAngle;
                int pIndex = 0, nIndex = 0;
                for(; nIndex < length; nIndex++) {
                    if(tempAngle < clipInfos[nIndex].Angle) {
                        break;
                    }
                }
                nIndex %= length;
                pIndex = (nIndex - 1 + length) % length;
                float mod = 2 * math.PI;
                float pWeight = (tempAngle - clipInfos[pIndex].Angle + mod) % mod;
                float nWeight = (clipInfos[nIndex].Angle - tempAngle + mod) % mod;
                weights[pIndex] = pWeight == 0 ? 1 : 1 - pWeight / (pWeight + nWeight);
                weights[nIndex] = nWeight == 0 ? 1 : 1 - nWeight / (pWeight + nWeight);
            }

            for(int j = 0; j < clipInfos.Count; j++) {
                mixer.SetInputWeight(j, weights[j]);
            }
        }
    }

    /// <summary>
    /// 按照方向和距离进行综合计算
    /// </summary>
    internal class AnimComposeBlender : IAnimaBlendTree {
        private readonly int TEX_SIZE = 256;
        private readonly int MAP_SIZE = 2;
        private readonly float MAP_INFLUENCE = 2;
        private Blend2DClipInfo m_currentPoint;

        public override void AddClip(IPlayable clip, Vector2 pos) {
            if(clipInfos == null) {
                clipInfos = new List<Blend2DClipInfo>();
            }
            float cosAngle = math.acos(math.dot(pos.normalized, AnimationLayer.normalVector));
            clipInfos.Add(new Blend2DClipInfo {
                Angle = pos.x >= 0 ? cosAngle : 2 * math.PI - cosAngle,
                Pos = pos,
                Clip = clip,
                LimitPos = new List<LimitPos>(),
            });
            clipInfos.Sort((x, y) => {
                return x.Angle > y.Angle ? 1 : -1;
            });
        }

        public override void CalcuClipsWeight(Vector2 input) {
            float[] rate = new float[clipInfos.Count];
            float allSum = 0f;
            allSum = 0f;
            for(int k = 0; k < clipInfos.Count; k++) {
                if(Vector2.Dot(input, clipInfos[k].Pos) < 0) {
                    rate[k] = 0f;
                    continue;
                }
                float influence = GetInfluenceRange(clipInfos[k], input);
                rate[k] = influence == 0 ? 0 : (influence - Vector2.Distance(clipInfos[k].Pos, input)) / influence;
                allSum += rate[k];
            }
            
            for(int j = 0; j < clipInfos.Count; j++) {
                mixer.SetInputWeight(j, rate[j]/allSum);
            }
        }
        
        // 计算每个点周边的限制点
        public void CalculateLimitPos() {
            Blend2DClipInfo[] tempPointList = new Blend2DClipInfo[clipInfos.Count];
            clipInfos.CopyTo(tempPointList);
            for(int i = 0; i < clipInfos.Count; i++) {
                m_currentPoint = clipInfos[i];
                Array.Sort(tempPointList,
                    (x, y) => { return (Vector2.Distance(x.Pos, m_currentPoint.Pos) - Vector2.Distance(y.Pos, m_currentPoint.Pos)) >= 0 ? 1 : -1; });

                for(int j = 1; j < tempPointList.Length; j++) {
                    Vector2 targetPos = tempPointList[j].Pos;
                    if(GetInfluenceRange(m_currentPoint, targetPos) > 0) {
                        m_currentPoint.LimitPos.Add(new LimitPos() {
                            Pos = targetPos,
                            Direct = (targetPos - m_currentPoint.Pos).normalized,
                            Distance = Vector2.Distance(targetPos, m_currentPoint.Pos)
                        });
                    }
                }
            }
        }

        /*private Vector2 TexPositionToMap(Vector2 pos) {
            return (pos / TEX_SIZE - Vector2.one / MAP_SIZE) * MAP_SIZE;
        }*/

        /// <summary>
        /// 计算动画节点在targetPos方向上的影响范围
        /// </summary>
        /// <param name="animPoint"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        private float GetInfluenceRange(Blend2DClipInfo animPoint, Vector2 targetPos) {
            Vector3 direct = (targetPos - animPoint.Pos).normalized;
            List<LimitPos> limitPos = animPoint.LimitPos;
            LimitPos leftPos = new LimitPos();
            LimitPos rightPos = new LimitPos();
            float nearLeft = 0;
            float nearRight = 0;
            for(int i = 0; i < limitPos.Count; i++) {
                Vector3 limitDir = limitPos[i].Direct;
                float dot = Vector3.Dot(direct, limitDir);
                if(dot > 0) {
                    float dirZ = Vector3.Cross(direct, limitDir).z;
                    if(dirZ >= 0 && dot > nearLeft) {
                        leftPos = limitPos[i];
                        nearLeft = dot;
                    }

                    if(dirZ <= 0 && dot > nearRight) {
                        rightPos = limitPos[i];
                        nearRight = dot;
                    }
                }
            }

            if(nearLeft == 0 || nearRight == 0) {
                return MAP_INFLUENCE;
            }

            float sum = nearLeft + nearRight;
            float range = (nearLeft * leftPos.Distance + nearRight * rightPos.Distance) / sum;
            if(leftPos.Pos == rightPos.Pos) {
                range /= 2;
            }

            return range >= Vector2.Distance(animPoint.Pos, targetPos) ? range : 0;
        }
    }
}