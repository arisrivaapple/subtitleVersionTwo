using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Net;

namespace SubtitleSystem
{
    public class SubtitleReader
    {
        public float timer;
        public float internalTimerRate;
        public string speaker;
        public Queue<Subtitle> subtitleQueue;
        public GameObject player;
        public GameObject mainc;
        public Subtitle nextSubtitle;
        int timeToClose;

        public SubtitleReader(String subtitleSetText)
        {
            mainc = GameObject.Find("Main Camera");
            player = GameObject.Find("player");
            timer = 0.0f;
            internalTimerRate = 1000.0f * Time.deltaTime;
            speaker = "";
            subtitleQueue = new Queue<Subtitle>();
            parseSubtitles(subtitleSetText);
        }

        public void parseSubtitles(String text)
        {
            string[] seperator = { "\r\n", "\n" };
            string[] lines = (text.ToString()).Split(seperator, StringSplitOptions.None);
            int i = 0;
            Regex line1 = new Regex(@"\d+");
            Regex line2 = new Regex(@"([\d]{2}:){2}(\d){2},(\d){3} --> ([\d]{2}:){2}(\d){2},(\d){3}");
            Regex line3 = new Regex(@"\[.*\].*");
            Regex blankSpace = new Regex(@"");
            while (i < lines.Length)
            {
                
                Subtitle newSubtitleObject = new Subtitle();
                if (line1.IsMatch(lines[i]))
                {
                    //i kept this syntax because its more readbale
                    //if the code reaches here, its currentally matching the format
                }
                else
                {
                    throw new System.InvalidOperationException("Text not in srt format.");
                }
                if (i + 1 < lines.Length && line2.IsMatch(lines[i + 1]))
                {
                    newSubtitleObject.inputTimeInfo(lines[i + 1]);
                }
                else
                {
                    throw new System.InvalidOperationException("Text not in srt format.");
                }
                if (i + 2 < lines.Length && line3.IsMatch(lines[i + 2]))
                {
                    newSubtitleObject.inputTextInfo(lines[i + 2]);
                    //checking if this line is the last line
                    if (lines.Length - 1 == i + 2 || lines.Length - 1 == i + 3)
                    {
                        timeToClose = newSubtitleObject.getEndTimeInt();
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("Text not in srt format.");
                }
                if (i + 2 < lines.Length - 1 && !(blankSpace.IsMatch(lines[i + 3])))
                {
                    {
                        throw new System.InvalidOperationException("Text not in srt format.");
                    }

                }
                i += 4;
                subtitleQueue.Enqueue(newSubtitleObject);
            }
        }


        public string readSubtitles() 
        {
            nextSubtitle = (Subtitle)subtitleQueue.Peek();
            if (subtitleQueue.Count > 0)
            {
                if (nextSubtitle.getStartTimeInt() <= timer && !(nextSubtitle.getEndTimeInt() <= timer))
                {
                    speaker = nextSubtitle.getSpeaker();
                    if (mainc.GetComponent<Main>().showSpeakerNames) 
                    {
                        return "[" + speaker + "] " + nextSubtitle.getText();
                    }
                    return nextSubtitle.getText();
                }
                if (nextSubtitle.getEndTimeInt() <= timer)
                {
                    //get rid of any subtitle instance currentally on screen
                    subtitleQueue.Dequeue();
                    speaker = null;
                }
                return null;
            }
            return null;
        }

        //i made a seperate to check for the subtitle instance ending method because im already using speaker = null to not display subtitles
        public Boolean shouldProgramEnd()
        {
            if (timer <= timeToClose)
            {
                return false;
            }
            return true;
        }

        public void incrementTime()
        {
            timer = internalTimerRate + timer;
        }

        public void incrementTime(float incrementTimeBy)
        {
            timer += incrementTimeBy;
        }

        public float getInternalTime()
        {
            return timer;
        }

        public void setInternalTimerRate(float newRate)
        {
            internalTimerRate = newRate;
        }

        public float getInternalTimerRate()
        {
            return internalTimerRate;
        }

        public string getSpeaker()
        {
            return speaker;
        }
    }
}
