using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SubtitleSystem
{
    //the intention of the class is to display the subtitles, and to be called
    //on by main monobehvoir classses that are the actual subtitle types

    public class SubtitleBase
    {

        public string speakerTemp;
        public GameObject mainc;
        public SubtitleReader subtitleReader = null;
        public float t;
        public Dictionary<string, Color> tempUselessDictionary;
        public string currLine;
        public TextMeshProUGUI subBox;
        public float place;

        public SubtitleBase(TextMeshProUGUI sbBox, TextAsset subtitles)
        {
            mainc = GameObject.Find("Main Camera");
            t = 0.0f;
            subtitleReader = new SubtitleReader(subtitles.ToString()); //ddoes the subtitle reader need access to the text box?
            subBox = sbBox;
            //okay so this seems to run before main for know known reasons?
        }

        public void assignDict()
        {
            tempUselessDictionary = mainc.GetComponent<Main>().speakerColors;
        }

        public void UpdateSubtitleBase()
        {
            place = mainc.GetComponent<Main>().subtitleFontSize;
            subtitleReader.incrementTime();
            if (true) //previously if tempUselessDictionary != null
            {
                //i think one of my main problems is that the code goes in a werid order and htat causes null exception mistakes
                if (true) //you can be more precise in timing by lowering the number here --ALSO MAKE THID A RANGE INSTEAD 
                {
                    //subtitleReader.incrementTime(100);
                    if (!subtitleReader.shouldProgramEnd())
                     {
                        //WHY IS THIS PROCESSED BEFORE ITS CALLED??????
                        //im leaning towards not mmaking names a second color my default

                        currLine = subtitleReader.readSubtitles();

                        //we use null instead of vblank lines when someone isnt speaking
                        //because sometimes ppl might want to have a speaker's name, but not saying anything
                        //like ...
                        if (currLine != null)
                        {
                            subBox.text = currLine;
                            speakerTemp = subtitleReader.getSpeaker();
                            if (mainc.GetComponent<Main>().assignedSpeakerColors)
                            {
                                mainc.GetComponent<Main>().speakerColor = tempUselessDictionary[speakerTemp];
                                subBox.color = tempUselessDictionary[speakerTemp];
                            }
                        }
                        else
                        {
                            currLine = "";
                            subBox.text = "";
                        }
                    }
                    else
                    {
                        currLine = "";
                        subBox.text = "";
                        speakerTemp = null;
                        mainc.GetComponent<Main>().subtitlesOn = false;
                    }

                }
                t += 1000.0f * Time.deltaTime; //frame rate per SECOND, so t is one second divided by the frame rate
            }
        }
    }
}