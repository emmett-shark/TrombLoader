﻿using System;
using UnityEngine;

namespace TrombLoader.Data
{
    public class TrombonerPlaceholder : MonoBehaviour
    {
        public TrombonerType TrombonerType = TrombonerType.DoNotOverride;
        public TrombonerOutfit TrombonerOutfit = TrombonerOutfit.DoNotOverride;
        public TromboneSkin TromboneSkin = TromboneSkin.DoNotOverride;
        public TromboneLength TromboneLength = TromboneLength.DoNotOverride;
        public TromboneHat TromboneHat = TromboneHat.DoNotOverride;
        public TrombonerMovementType MovementType = TrombonerMovementType.DoNotOverride;
        public TrombonerDanceMode DanceMode = TrombonerDanceMode.DoNotOverride;

        [HideInInspector, SerializeField]
        public int InstanceID = 0;
    }

    [Serializable]
    public enum TrombonerType
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Appaloosa")]
        Female1 = 0,
        [InspectorName("Beezerly")]
        Female2 = 1,
        [InspectorName("Kaizyle II")]
        Female3 = 2,
        [InspectorName("Trixiebell")]
        Female4 = 3,
        [InspectorName("Meldor")]
        Male1 = 4,
        [InspectorName("Jermajesty")]
        Male2 = 5,
        [InspectorName("Horn Lord")]
        Male3 = 6,
        [InspectorName("Soda")]
        Male4 = 7,
        [InspectorName("Polygon")]
        Female5 = 8,
        [InspectorName("Servant Of Babi")]
        Male5 = 9,
    }

    [Serializable]
    public enum TrombonerOutfit
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Default")]
        Default = 0,
        [InspectorName("Christmas")]
        Christmas = 1,
    }

    [Serializable]
    public enum TromboneSkin
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Brass")]
        Brass = 0,
        [InspectorName("Silver")]
        Silver = 1,
        [InspectorName("Red")]
        Red = 2,
        [InspectorName("Blue")]
        Blue = 3,
        [InspectorName("Green")]
        Green = 4,
        [InspectorName("Pink")]
        Pink = 5,
        [InspectorName("Polygon")]
        Polygon = 6,
        [InspectorName("Champ")]
        Champ = 7,
    }

    [Serializable]
    public enum TromboneLength
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Short")]
        Short = 0,
        [InspectorName("Long")]
        Long = 1,
    }

    [Serializable]
    public enum TromboneHat
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Disabled")]
        Disabled = 0,
        [InspectorName("Enabled")]
        Enabled = 1,
    }

    [Serializable]
    public enum TrombonerMovementType
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Jubilant")]
        Jubilant = 0,
        [InspectorName("Estudious")]
        Estudious = 1,
    }

    [Serializable]
    public enum TrombonerDanceMode
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Auto Dance")]
        AutoDance = 0,
        [InspectorName("Manual Dance")]
        ManualDance = 1,
    }
}
