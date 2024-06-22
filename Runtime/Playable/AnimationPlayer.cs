using DG.Tweening;
using Game.Basic;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game.Basic {
    public class AnimationPlayer {
        private Animator animator;
        private PlayableGraph graph;
        private AnimationMixerPlayable outputMixer;

        private AnimationScriptPlayable IKPlayable;
        private AnimationLayerMixerPlayable mixerPlayable;

        private Dictionary<string, AnimationClipInfo> animationClips;
        private List<AnimationLayer> animLayers;

        public void Initialized(Animator animator, string config) {
            this.animator = animator;
            var jsonText = File.ReadAllText(Config.assetPath + config);
            var animationData = JsonMapper.ToObject(jsonText);
            animLayers = new List<AnimationLayer>();
            animationClips = new Dictionary<string, AnimationClipInfo>();
            graph = PlayableGraph.Create("");
            var output = AnimationPlayableOutput.Create(graph, "", animator);
            outputMixer = AnimationMixerPlayable.Create(graph);
            output.SetSourcePlayable(outputMixer, 0);


            LoadAnimationClip(animationData);
            LoadAnimationLayer(animationData);
            graph.Play();
        }

        public void Tick() {
            for(int i = 0; i < animLayers.Count; i++) {
                var currLayer = animLayers[i];
                if(string.IsNullOrEmpty(currLayer.current) && !string.IsNullOrEmpty(currLayer.defaultAnim)) {
                    currLayer.Play(currLayer.defaultAnim);
                }
            }
        }

        public void Play(string clipName) {
            for(int i = 0; i < animLayers.Count; i++) {
                if(animLayers[i].Play(clipName)) { return; }
            }
        }

        public void CrossFade(string clipName, float crossTime = 0.3f) {
            for(int i = 0; i < animLayers.Count; i++) {
                if(animLayers[i].CrossFade(clipName, crossTime)) { return; }
            }
        }

        public void SetValue(string para, Vector2 value) {
            for(int i = 0; i < animLayers.Count; i++) {
                animLayers[i].SetValue(para, value);
            }
        }

        private void LoadAnimationClip(JsonData data) {
            var clipsArray = data["clips"];
            for(int i = 0; i < clipsArray.Count; i++) {
                var clipInfo = new AnimationClipInfo {
                    clip = AssetsLoad.Instance.Load<AnimationClip>(clipsArray[i]["path"].ToString()),
                    speed = (float)(clipsArray[i]["speed"].ValueAsDouble())
                };
                animationClips.Add(clipsArray[i]["name"].ToString(), clipInfo);
            }
        }

        private void LoadAnimationLayer(JsonData data) {
            var layers = data["layers"];
            for(int i = 0; i < layers.Count; i++) {
                var layer = layers[i];
                AvatarMask avatarMask = null;
                if(!string.IsNullOrEmpty(layer["mask"].ToString())) {
                    avatarMask = AssetsLoad.Instance.Load<AvatarMask>(layer["mask"].ToString());
                }
                AnimationLayer newLayer = new Basic.AnimationLayer();
                newLayer.layerMixer = AnimationLayerMixerPlayable.Create(graph);
                newLayer.defaultAnim = layer["default"].ToString();
                outputMixer.AddInput(newLayer.layerMixer, 0, 1);
                // load clips
                var clipArray = layer["clips"];
                Dictionary<string, AnimationClipPlayable> playableClips = new Dictionary<string, AnimationClipPlayable>();
                for(int j = 0; j < clipArray.Count; j++) {
                    if(animationClips.TryGetValue(clipArray[j].ToString(), out var clip)) {
                        playableClips.Add(clipArray[j].ToString(), AnimationClipPlayable.Create(graph, clip.clip));
                    }
                }
                newLayer.playableClips = playableClips;

                // load blendtree
                List<AnimationBlend2DPlayable> blendPlayables = new List<AnimationBlend2DPlayable>();
                var blendTreeArray = layer["blendTree"];
                for(int j = 0; j < blendTreeArray.Count; j++) {
                    var blendTree = blendTreeArray[j];
                    var newBlendPlayable = new AnimationBlend2DPlayable();
                    newBlendPlayable.name = blendTree["name"].ToString();
                    newBlendPlayable.parameterName = blendTree["parameter"].ToString();
                    newBlendPlayable.mixer = AnimationMixerPlayable.Create(graph);
                    newBlendPlayable.clipInfos = new List<Blend2DClipInfo>();

                    var blendClips = blendTree["blend"];
                    for(int k = 0; k < blendClips.Count; k++) {
                        var item = blendClips[k];
                        if(animationClips.TryGetValue(item["clipName"].ToString(), out var clip)) {
                            var newClipPlayable = AnimationClipPlayable.Create(graph, clip.clip);
                            newClipPlayable.SetSpeed(clip.speed);
                            var newPos = new Vector2((float)(item["pos"][0].ValueAsDouble()), (float)(item["pos"][1].ValueAsDouble()));
                            newBlendPlayable.AddClip(newClipPlayable, newPos);
                        }
                    }
                    for(int k = 0; k < newBlendPlayable.clipInfos.Count; k++) {
                        newBlendPlayable.mixer.AddInput((AnimationClipPlayable)newBlendPlayable.clipInfos[k].clip, 0);
                    }
                    blendPlayables.Add(newBlendPlayable);
                }
                newLayer.blendClips = blendPlayables;
                animLayers.Add(newLayer);
            }
        }
    }

    internal class AnimationLayer {
        public string defaultAnim;
        public string current;
        public AnimationLayerMixerPlayable layerMixer;
        public Dictionary<string, AnimationClipPlayable> playableClips;
        public List<AnimationBlend2DPlayable> blendClips;
        public readonly static Vector2 normalVector = Vector2.up;
        private int currentIndex;
        private int crossIndex;
        private float currentWeight;
        private Tweener tweener;

        public bool Play(string clipName) {
            if(layerMixer.GetInputCount() == 0) {
                layerMixer.SetInputCount(2);
            }

            if(playableClips.TryGetValue(clipName, out var clipPlayable)) {
                for(int j = 0; j < layerMixer.GetInputCount(); j++) {
                    layerMixer.DisconnectInput(j);
                }
                layerMixer.ConnectInput(0, clipPlayable, 0, 1);
                current = clipName;
                return true;
            }

            for(int i = 0; i < blendClips.Count; i++) {
                if(blendClips[i].name == clipName) {
                    for(int j = 0; j < layerMixer.GetInputCount(); j++) {
                        layerMixer.DisconnectInput(j);
                    }
                    layerMixer.ConnectInput(0, blendClips[i].mixer, 0, 1);
                    current = clipName;
                    return true;
                }
            }
            return false;
        }

        public bool CrossFade(string clipName, float crossTime) {
            if(layerMixer.GetInputCount() == 0) {
                layerMixer.SetInputCount(2);
            }

            if(clipName  == current) {
                return true;
            }
            crossIndex = (currentIndex + 1) % layerMixer.GetInputCount();
            if(playableClips.TryGetValue(clipName, out var clipPlayable)) {
                layerMixer.DisconnectInput(crossIndex);
                layerMixer.ConnectInput(crossIndex, clipPlayable, 0, 1);
                currentIndex ^= crossIndex;
                crossIndex ^= currentIndex;
                currentIndex ^= crossIndex;
                currentWeight = 0;
                if(tweener != null && tweener.IsActive()) {
                    tweener.Kill();
                }
                tweener = DOTween.To(() => currentWeight, (x) => currentWeight = x, 1, crossTime).OnUpdate(() => {
                    layerMixer.SetInputWeight(currentIndex, currentWeight);
                    layerMixer.SetInputWeight(crossIndex, 1 - currentWeight);
                });
                current = clipName;
                return true;
            }

            for(int i = 0; i < blendClips.Count; i++) {
                if(blendClips[i].name == clipName) {
                    layerMixer.DisconnectInput(crossIndex);
                    layerMixer.ConnectInput(crossIndex, blendClips[i].mixer, 0, 1);
                    currentIndex ^= crossIndex;
                    crossIndex ^= currentIndex;
                    currentIndex ^= crossIndex;
                    currentWeight = 0;
                    if(tweener != null && tweener.IsActive()) {
                        tweener.Kill();
                    }
                    tweener = DOTween.To(() => currentWeight, (x) => currentWeight = x, 1, crossTime).OnUpdate(() => {
                        layerMixer.SetInputWeight(currentIndex, currentWeight);
                        layerMixer.SetInputWeight(crossIndex, 1 - currentWeight);
                    });
                    current = clipName;
                    return true;

                }
            }
            return false;
        }

        public void SetValue(string para, Vector2 value) {
            for(int i = 0; i < blendClips.Count; i++) {
                var current = blendClips[i];
                if(current.parameterName != para || current.paraValue == value) {
                    continue;
                }

                current.paraValue = value;
                int length = current.clipInfos.Count;
                NativeArray<float> weights = new NativeArray<float>(length, Allocator.Temp);

                if(value != Vector2.zero && length > 0) {
                    float tempAngle = math.acos(math.dot(value.normalized, AnimationLayer.normalVector));
                    tempAngle = value.x >= 0 ? tempAngle : 2 * math.PI - tempAngle;
                    int pIndex = 0, nIndex = 0;
                    for(; nIndex < length; nIndex++) {
                        if(tempAngle < current.clipInfos[nIndex].angle) {
                            break;
                        }
                    }
                    nIndex %= length;
                    pIndex = (nIndex - 1 + length) % length;
                    float mod = 2 * math.PI;
                    float pWeight = (tempAngle - current.clipInfos[pIndex].angle + mod) % mod;
                    float nWeight = (current.clipInfos[nIndex].angle - tempAngle + mod) % mod;
                    weights[pIndex] = pWeight == 0 ? 1 : 1 - pWeight / (pWeight + nWeight);
                    weights[nIndex] = nWeight == 0 ? 1 : 1 - nWeight / (pWeight + nWeight);
                }

                for(int j = 0; j < current.clipInfos.Count; j++) {
                    current.mixer.SetInputWeight(j, weights[j]);
                }
                blendClips[i] = current;
            }
        }
    }
}

internal struct AnimationClipInfo {
    public AnimationClip clip;
    public float speed;
}
internal struct Blend2DClipInfo {
    public IPlayable clip;
    public float angle;
}

internal struct AnimationBlend2DPlayable {
    public string name;
    public string parameterName;
    public Vector2 paraValue;
    public AnimationMixerPlayable mixer;
    public List<Blend2DClipInfo> clipInfos;

    public void AddClip(IPlayable clip, Vector2 pos) {
        if(clipInfos == null) {
            clipInfos = new List<Blend2DClipInfo>();
        }
        float cosAngle = math.acos(math.dot(pos.normalized, AnimationLayer.normalVector));
        clipInfos.Add(new Blend2DClipInfo { angle = pos.x >= 0 ? cosAngle : 2 * math.PI - cosAngle, clip = clip });
        clipInfos.Sort((x, y) => {
            return x.angle > y.angle ? 1 : -1;
        });
    }
}
