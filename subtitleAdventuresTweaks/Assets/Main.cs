using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;

namespace SubtitleSystem
{
    public class Main : MonoBehaviour
    {  
        //settings
        public Boolean assignedSpeakerColors;
        public Boolean speakerNamesOptional; //i want to find a way to have the toggle system activated through this class so its easier to implement
        public Boolean speakerNames;
        public Boolean showCompass;
        public Boolean sameSubtitleStyle;
        public Boolean attachSubtitles;
        public Boolean showSpeakerNames;
        public Boolean showSpeakerArrows;
        public Boolean dragSubtitles;
        public Boolean highlightSpeaker;
        public Boolean showSpeakerColors;
        public Boolean showSpeechBubble;
        public Boolean slowGameWhenNotLookingAtSpeaker;
        public Boolean subtitleBackground;
        public Boolean silhouettes;
        public int subtitleFontSizeDif;
        public int subtitleFontSize;
        public TMP_FontAsset currFont;

        public float currentSpeed;
        public float speakerFacingSpeed;
        public float nonSpeakerFacingSpeed;

        public GameObject subtitleCanvasObject;
        public Canvas subtitleDisplay;
        public Dictionary<string, Color> speakerColors;
        public TextAsset speakerColorDoc;
        public GameObject textBackground;
        public Color speakerColor;
        public SpeakerColorParser colorParser;
        public Boolean subtitlesOn;

        void Start()
        {
            //people will adjust this based on how their player moves, but should the player be able to rotate the same speed when the rest of the game is slowed down to try to locatioe the speaker?
            speakerFacingSpeed = 1.0f;
            nonSpeakerFacingSpeed = 0.5f;
            textBackground = null;
            subtitleFontSize= 60;
            subtitleFontSizeDif = 0;
            subtitlesOn = false;
            colorParser = new SpeakerColorParser();
            speakerColors = colorParser.parse(speakerColorDoc);
            speakerNames = false;
        }

        public void adjustFontSizeUp()
        {

        }

        public void adjustFontSizeDown()
        {

        }
    }
}