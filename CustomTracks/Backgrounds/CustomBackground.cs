﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using Cinemachine;
using TMPro;
using TrombLoader.Data;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TrombLoader.CustomTracks.Backgrounds;

/// <summary>
///  Custom .trombackgrounds, which are Unity assetbundles
/// </summary>
public class CustomBackground : AbstractBackground
{
    private string _songPath;
    private List<VideoPlayer> _pausedVideoPlayers = new();
    private List<ParticleSystem> _pausedParticleSystems = new();

    public CustomBackground(AssetBundle bundle, string songPath) : base(bundle)
    {
        _songPath = songPath;
    }

    public override GameObject Load(BackgroundContext ctx)
    {
        var bg = Bundle.LoadAsset<GameObject>("assets/_background.prefab");

        // MacOS Shader Handling
        // May need to be expanded to other platforms eventually, but for now only check for invalid shaders on mac
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            LoadShaderBundle(bg);
        }

        var managers = bg.GetComponentsInChildren<TromboneEventManager>();
        foreach (var eventManager in managers)
        {
            eventManager.DeserializeAllGenericEvents();
        }

        var invoker = bg.AddComponent<TromboneEventInvoker>();
        invoker.InitializeInvoker(ctx.controller, managers);

        foreach (var videoPlayer in bg.GetComponentsInChildren<VideoPlayer>())
        {
            if (videoPlayer.url == null || !videoPlayer.url.Contains("SERIALIZED_OUTSIDE_BUNDLE")) continue;
            var videoName = videoPlayer.url.Replace("SERIALIZED_OUTSIDE_BUNDLE/", "");
            var clipURL = Path.Combine(_songPath, videoName);
            videoPlayer.url = clipURL;
        }

        // handle foreground objects
        var fgholder = bg.transform.GetChild(1);
        while (fgholder.childCount < 8)
        {
            var fillerObject = new GameObject("Filler");
            fillerObject.transform.SetParent(fgholder);
        }

        // handle two background images
        while (bg.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>().Length < 2)
        {
            var fillerObject = new GameObject("Filler");
            fillerObject.AddComponent<SpriteRenderer>();
            fillerObject.transform.SetParent(bg.transform.GetChild(0));
        }

        // add confetti holder if missing
        if (bg.transform.childCount < 3)
        {
            var fillerConfettiHolder = new GameObject("ConfettiHolder");
            fillerConfettiHolder.transform.SetParent(bg.transform);
        }

        // layering
        var breathCanvas = ctx.controller.bottombreath.transform.parent.parent.GetComponent<Canvas>();
        if (breathCanvas != null) breathCanvas.planeDistance = 2;

        var champCanvas = ctx.controller.champcontroller.letters[0].transform.parent.parent.parent
            .GetComponent<Canvas>();
        if (champCanvas != null) champCanvas.planeDistance = 2;

        var gameplayCam = GameObject.Find("GameplayCam")?.GetComponent<Camera>();
        if (gameplayCam != null) gameplayCam.depth = 99;

        var removeDefaultLights = bg.transform.Find("RemoveDefaultLights");
        if (removeDefaultLights)
        {
            foreach (var light in Object.FindObjectsOfType<Light>()) light.enabled = false;
            removeDefaultLights.gameObject.AddComponent<SceneLightingHelper>();
        }

        var addShadows = bg.transform.Find("AddShadows");
        if (addShadows)
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 100;
        }

        return bg;
    }

    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        var gameController = controller.gamecontroller;

        foreach (var videoPlayer in bg.GetComponentsInChildren<VideoPlayer>())
        {
            videoPlayer.Prepare();
        }

        var puppetController = bg.AddComponent<BackgroundPuppetController>();

        // puppet handling
        foreach (var trombonePlaceholder in bg.GetComponentsInChildren<TrombonerPlaceholder>())
        {
            int trombonerIndex = trombonePlaceholder.TrombonerType == TrombonerType.DoNotOverride
                ? gameController.puppetnum
                : (int)trombonePlaceholder.TrombonerType;

            foreach (Transform child in trombonePlaceholder.transform)
            {
                if (child != null) child.gameObject.SetActive(false);
            }

            var sub = new GameObject("RealizedTromboner");
            sub.transform.SetParent(trombonePlaceholder.transform);
            sub.transform.SetSiblingIndex(0);
            sub.transform.localPosition = new Vector3(-0.7f, 0.45f, -1.25f);
            sub.transform.localEulerAngles = new Vector3(0, 0f, 0f);
            trombonePlaceholder.transform.Rotate(new Vector3(0f, 19f, 0f));
            sub.transform.localScale = Vector3.one;

            //handle male tromboners being slightly shorter
            if (trombonerIndex > 3 && trombonerIndex != 8)
                sub.transform.localPosition = new Vector3(-0.7f, 0.35f, -1.25f);

            var tromboneRefs = new GameObject("TromboneTextureRefs");
            tromboneRefs.transform.SetParent(sub.transform);
            tromboneRefs.transform.SetSiblingIndex(0);

            var textureRefs = tromboneRefs.AddComponent<TromboneTextureRefs>();
            // a bit of getchild action to mirror game behaviour
            textureRefs.trombmaterials = gameController.modelparent.transform.GetChild(0)
                .GetComponent<TromboneTextureRefs>().trombmaterials;

            // Copy the tromboners in
            var trombonerGameObject =
                Object.Instantiate(gameController.playermodels[trombonerIndex], sub.transform, true);
            trombonerGameObject.transform.localScale = Vector3.one;

            Tromboner tromboner = new(trombonerGameObject, trombonePlaceholder);

            // Store tromboners for later
            var customPuppetTrait = trombonerGameObject.AddComponent<CustomPuppetController>();
            customPuppetTrait.Tromboner = tromboner;
            puppetController.Tromboners.Add(tromboner);

            tromboner.controller.setTromboneTex(trombonePlaceholder.TromboneSkin == TromboneSkin.DoNotOverride
                ? gameController.textureindex
                : (int)trombonePlaceholder.TromboneSkin);
            
            
            if (trombonePlaceholder.TrombonerOutfit == TrombonerOutfit.Christmas) {
                Material[] materials = tromboner.controller.bodymesh.materials;
                materials[0] = tromboner.controller.costume_alt;
                tromboner.controller.bodymesh.materials = materials;
            }
            
            var chosen_hat = trombonePlaceholder.TromboneHat == TromboneHat.DoNotOverride
            ? GlobalVariables.chosen_hat : (int)trombonePlaceholder.TromboneHat;

            if (chosen_hat > 0)
            {
                GameObject hat = Object.Instantiate(gameController.hats[chosen_hat - 1], 
                                tromboner.controller.bellmesh.transform, worldPositionStays: false);
                hat.transform.localPosition = new Vector3(0.189f, 0.332f, 0.309f);
                hat.transform.localEulerAngles = new Vector3(0f, 0f, 45f);
                hat.transform.localScale = new Vector3(0.12f, 0.12f, 0.2f);
            }

            // long long maaaaaan https://youtu.be/6-1Ue0FFrHY
            if(GlobalVariables.show_long_trombone && trombonePlaceholder.TromboneLength 
                == TromboneLength.DoNotOverride){
                trombonePlaceholder.TromboneLength = TromboneLength.Long;
            }
            switch(trombonePlaceholder.TromboneLength)
            {
                case TromboneLength.Short:
                    tromboner.controller.tube_distance = 1.1f;
                    tromboner.controller.p_tube.transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
                case TromboneLength.Long:
                    tromboner.controller.tube_distance = 3.57f;
                    tromboner.controller.p_tube.transform.localScale = new Vector3(1f, 1f, 2.25f);
                    break;
            }

            if  ((GlobalVariables.localsave.cardcollectionstatus[36] >= 10 && GlobalVariables.show_toot_rainbow) 
            || (GlobalVariables.localsave.cardcollectionstatus_gold[36] > 0 && GlobalVariables.show_toot_rainbow))
            {
                tromboner.controller.show_rainbow = true;
            }

            if (tromboner.placeholder.DanceMode == TrombonerDanceMode.DoNotOverride
                && GlobalVariables.localsave.manual_dance)
            {
                tromboner.placeholder.DanceMode = TrombonerDanceMode.ManualDance;
            }
        }
    }

    public override bool CanResume => true;

    private IEnumerable<Behaviour> GetPauseableBehaviours(PauseContext ctx)
    {
        foreach (var animator in ctx.backgroundObj.GetComponentsInChildren<Animator>())
        {
            yield return animator;
        }

        foreach (var animation in ctx.backgroundObj.GetComponentsInChildren<Animation>())
        {
            yield return animation;
        }

        foreach (var director in ctx.backgroundObj.GetComponentsInChildren<PlayableDirector>())
        {
            yield return director;
        }

        foreach (var dollyCart in ctx.backgroundObj.GetComponentsInChildren<CinemachineDollyCart>())
        {
            yield return dollyCart;
        }
    }

    public override void OnPause(PauseContext ctx)
    {
        foreach (var behaviour in GetPauseableBehaviours(ctx))
        {
            behaviour.enabled = false;
        }

        foreach (var videoPlayer in ctx.backgroundObj.GetComponentsInChildren<VideoPlayer>())
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                _pausedVideoPlayers.Add(videoPlayer);
            }
        }

        foreach (var particleSystem in ctx.backgroundObj.GetComponentsInChildren<ParticleSystem>())
        {
            if (particleSystem.isPlaying)
            {
                particleSystem.Pause();
                _pausedParticleSystems.Add(particleSystem);
            }
        }
    }

    public override void OnResume(PauseContext ctx)
    {
        foreach (var behaviour in GetPauseableBehaviours(ctx))
        {
            behaviour.enabled = true;
        }

        foreach (var videoPlayer in _pausedVideoPlayers)
        {
            videoPlayer.Play();
        }

        foreach (var particleSystem in _pausedParticleSystems)
        {
            particleSystem.Play();
        }

        _pausedVideoPlayers.Clear();
        _pausedParticleSystems.Clear();
    }

    public override void Dispose()
    {
        _pausedVideoPlayers.Clear();
        base.Dispose();
    }

    private void LoadShaderBundle(GameObject bg)
    {
        // first add base game shaders, which should NOT be overwritten by shadercache shaders
        var shaderCache = new Dictionary<string, Shader>(Plugin.Instance.ShaderHelper.BaseGameShaderCache);

        foreach (var cachedShader in Plugin.Instance.ShaderHelper.ShaderCache)
        {
            if (!shaderCache.ContainsKey(cachedShader.Key)) shaderCache.Add(cachedShader.Key, cachedShader.Value);
        }

        // bundle auto-built in TrombLoaderBackgroundProject for custom shaders
        // platform is null because we want to load EVERY shader bundle with EVERY name
        // TODO: cache this inbetween song runs
        var songSpecificShaderCache = Plugin.Instance.ShaderHelper.LoadShaderBundleFromPath(_songPath + "/", null);

        foreach (var songSpecificShader in songSpecificShaderCache)
        {
            // shaders in the bundle should actually overwrite the other shaders temporarily, just in case it's a modified version.

            // for backgrounds that don't supply a custom macos shader file but still have custom shaders (eg, legacy songs),
            // the absolute endgame would be to have a "valve steam deck shader cache" type service where recompiled shaders can be automatically downloaded
            // however, it is debatable if it is worth developing this soley for the few legacy songs with custom shaders
            shaderCache[songSpecificShader.Key] = songSpecificShader.Value;
        }

        var materials = new[]
        {
            bg.GetComponentsInChildren<Renderer>(true).SelectMany(renderer => renderer.materials),
            bg.GetComponentsInChildren<TMP_Text>(true).Select(textMesh => textMesh.fontSharedMaterial),
            bg.GetComponentsInChildren<Graphic>(true).Select(graphics => graphics.material)
        }.SelectMany(x => x);

        foreach (var material in materials)
        {
            if (material == null || material.shader == null) continue;

            // FIRST: check if the shader is in base game. Normally if it uses base game, everything is good, the shader is NOT broken.
            // Unfortunately this is wildly inconsistent, and the only shader that seems to consistently work is Standard
            // This does need a closer look if somebody can come up with a list of shaders that always deserialize properly, as switching shaders on a material is fairly expensive
            if (material.shader.name == "Standard") continue;

            if (shaderCache.TryGetValue(material.shader.name, out var shader))
            {
                material.shader = shader;
                Plugin.LogDebug($"Replacing shader on {material.name} ({shader.name})");
            }
            else
            {
                // TODO: Handle more gracefully. Maybe replace with a default shader.
                Plugin.LogDebug($"Could not find shader on {material.name} ({material.shader.name})");
            }
        }
    }
}
