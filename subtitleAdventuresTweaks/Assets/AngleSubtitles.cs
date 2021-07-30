using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using System.Diagnostics;
using TMPro;

namespace SubtitleSystem
{
    public class AngleSubtitles : MonoBehaviour 
    {
        //text for debugging:
        public TextMeshProUGUI speakerAngleText, TimerText, TText, justSpeakerText, playerZAngleText, speakerPosText, playerYAngleText, totalFinalAngleText, fillCircleConvertAngleText, speedText;

        //Main Functionality:

        public GameObject player;
        public GameObject Speaker;

        //UI elements
        public SubtitleBase subBase;
        public TextMeshProUGUI subtitlesBox;

        //imported text file in srt format for this triggered instance of subtitles
        public TextAsset subtitleFile;

        //checks to display one set of subtitles at a time, and the appriate number of times
        public Boolean triggeredOnce;
        public Boolean subtitlesTriggered;
        public Boolean repeated; //mark this as true in start if you want the diolouge to be repeated on each collision

        public string speakerPos;
        public float playerAngle;
        public float speakerAngle;
        public float justSpeakerAngle;
        public float speakerZ;
        public float speakerX;
        public float playerZ;
        public float playerX;
        public Boolean speakerFaced;
        public float allowance;
        public string speakerTag; //the name o fthe person talkin--null is used to show that no one is talking
        public GameObject mainCamr;
        public float x;
        public float y;

        //for checking if player has a line of sight to speaker using unity raycast
        public Collider speakerCollider;
        public RaycastHit targetHit;

        //Extra Features:

        public GameObject speechBubble;

        public GameObject mostRecentSpeakerLight;

        public GameObject compassBackground;
        public GameObject Compass;

        public GameObject textBackground;
        public GameObject mostRecentSpeaker;
        public Boolean needToSeeSpeaker;

        public Material silhouetteMaterial;
        public Material nonSilhouetteMaterial;

        private LineRenderer line;

        public GameObject leftArrow;
        public GameObject rightArrow;

        void Start()
        {
            Speaker = player;
            mainCamr = GameObject.Find("Main Camera");
            triggeredOnce = false;
            repeated = false;
            subtitlesTriggered = false;
            Speaker = player;

            //HERE THE CODE SHOULD BE EDITED DEPENDING ON WHAT PLAYER MOVEMENT YOU USE
            //"plsyerAngle" SHOULD CONTAIN THE PLAYERS ROTATION AORUND THE Y AXIS IN DEGREES
            playerAngle = (player.GetComponent<MoveRobot>().playerYAngle) % 360.0f;//will this update as the variable updates?

            ///we assume that initially the player isnt facing the speaker--maybe assume that they are??
            mostRecentSpeaker = player; //still feels like weird "fix"
            speakerFaced = false;
            speakerTag = "";
            allowance = 30.0f;

            line = this.gameObject.AddComponent<LineRenderer>();
            // Set the width of the Line Renderer
            line.startWidth = 0.05F;
            line.endWidth = 0.05F;
            // Set the number of vertex fo the Line Renderer
            line.positionCount = 2;
        }

        void OnTriggerEnter(Collider other)
        {
            if (mainCamr != null)
            {
                Boolean subs = !(mainCamr.GetComponent<Main>().subtitlesOn); //im worried what would happen with this line if a differenct (or the same?) instance of subitles were runnign?
                if ((!triggeredOnce || repeated) && subs)
                {
                    subBase = new SubtitleBase(subtitlesBox, subtitleFile);
                    //write check for next line
                    mainCamr.GetComponent<Main>().currentSpeed = mainCamr.GetComponent<Main>().nonSpeakerFacingSpeed;
                    subBase.assignDict();//thhis is only here bc for some reason subtitle base runs Before main??
                    triggeredOnce = true;
                    subtitlesTriggered = true;
                    mainCamr.GetComponent<Main>().subtitlesOn = true;
                } 
            }
        }

        void Update()
        {
            speakerAngle = getSpeakerAngle();
            if (mostRecentSpeaker != Speaker) { showSilhouette(false);}

            if (subBase != null) 
            {
                subBase.subtitleReader.incrementTime();
                TimerText.text = "Subtitle Reader timer: " +  subBase.subtitleReader.getInternalTime() + "";
                TText.text = "Subtitle Base T: " + subBase.t;
                playerAngle = (player.GetComponent<MoveRobot>().playerYAngle) % 360.0f;
                if (subtitlesTriggered)
                {
                    if (mainCamr.GetComponent<Main>().showCompass) { setCompass(true); }
                    if (mainCamr.GetComponent<Main>().attachSubtitles) { attachSubtitles(); }
                    
                    subBase.UpdateSubtitleBase(); 
                    speakerTag = subBase.subtitleReader.getSpeaker();
                    Speaker = GameObject.Find(speakerTag);
                    
                    if (mostRecentSpeaker != Speaker)
                    {
                        if (Speaker != null && (mainCamr.GetComponent<Main>().showSpeechBubble))
                        {
                            float x = Speaker.transform.position.x;
                            float y = Speaker.transform.position.y;
                            float z = Speaker.transform.position.z;
                            speechBubble.SetActive(true);
                            speechBubble.transform.position = new Vector3(x, y + 1.5f, z);

                            Vector3 currentEulerAngles;
                            Quaternion currentRotation;
                            x = Speaker.transform.rotation.x;
                            y = Speaker.transform.rotation.y;
                            z = Speaker.transform.rotation.z;

                            speechBubble.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Compass.transform.rotation.z)); //modifying the Vector3, based on input multiplied by speed and time, apply the Quaternion.eulerAngles change to the gameObject
                        }

                        showSilhouette(false);
                    }


                    if (Speaker == null)
                    {
                        speakerFaced = false;
                        processSpeakerArrows(false, speakerPos);
                        speechBubble.SetActive(false);
                        mainCamr.GetComponent<Main>().currentSpeed = mainCamr.GetComponent<Main>().speakerFacingSpeed; ; //when there's a break in the subtittlwa, thew siubtitle speed speeds up to speakerFacingSpeed
                        lightSpeaker(false);
                        changeText(false);
                        setCompass(false);
                    }
                    else
                    {
                        //basic system functionality: player angle test a
                        speakerAngle = getSpeakerAngle();
                        speakerPos = playerViewContains(speakerTag, playerAngle);
                        mostRecentSpeaker = Speaker;

                        //various settings
                        if (mainCamr.GetComponent<Main>().subtitleBackground) { changeText(true); } else { changeText(false); }//subtitle background
                        if (mainCamr.GetComponent<Main>().showCompass) { setCompass(true); } else { setCompass(false); } //i can move these checks to helper methods but I think this is easier to see/understand?
                        if (mainCamr.GetComponent<Main>().silhouettes) { showSilhouette(true); } else { showSilhouette(false); } //switch all setting clauses to alphabetical order?
                        

                        if (Equals("null", speakerPos))
                        {
                            throw new Exception("speaker is null but shouldnt be");
                        }
                        else if (Equals("true", speakerPos)) 
                        {
                            speakerFaced = true;
                            mainCamr.GetComponent<Main>().currentSpeed = mainCamr.GetComponent<Main>().speakerFacingSpeed;
                            processSpeakerArrows(false, speakerPos);
                            if (mainCamr.GetComponent<Main>().highlightSpeaker) { lightSpeaker(true); } else { lightSpeaker(false); }
                        }
                        else //player not facing speaker
                        {
                            speakerFaced = false;
                            lightSpeaker(false); //not facing speaker so all lights off whether triggered or not
                            if (mainCamr.GetComponent<Main>().slowGameWhenNotLookingAtSpeaker) { mainCamr.GetComponent<Main>().currentSpeed = mainCamr.GetComponent<Main>().nonSpeakerFacingSpeed; } 
                            if (mainCamr.GetComponent<Main>().showSpeakerArrows) { processSpeakerArrows(true, speakerPos); } else {processSpeakerArrows(false, speakerPos); } // subtitle background
                        }
                    }

                    if (subBase.subtitleReader.shouldProgramEnd())
                    {
                        speakerFaced = false;
                        mainCamr.GetComponent<Main>().currentSpeed = mainCamr.GetComponent<Main>().speakerFacingSpeed;
                        Speaker = null;
                        speakerTag = "";
                        subtitlesTriggered = false;
                        lightSpeaker(false);
                        changeText(false);
                        subBase.subBox.text = "";
                    }
                }
            }
        }

        public void attachSubtitles()
        {
            if (!subBase.subtitleReader.shouldProgramEnd() && player != null && Speaker != null)
            {
                if (GameObject.Find("Main Camera").GetComponent<Main>().assignedSpeakerColors)
                {
                    string speakerString = subBase.subtitleReader.getSpeaker();
                    Color speakerColor = mainCamr.GetComponent<Main>().speakerColor;
                    float alpha = 1.0f;
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(speakerColor, 0.0f), new GradientColorKey(speakerColor, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                    );
                    line.colorGradient = gradient;
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.endColor = speakerColor;
                    line.startColor = speakerColor;
                }
                line.startWidth = 0.05F;
                line.endWidth = 0.05F;
                line.SetPosition(0, Speaker.transform.position); // Update position of the two vertex of the Line Renderer
                line.SetPosition(1, player.transform.position);

            }
            else //if no one is speaker OR if the program should end
            {
                line.startWidth = 0.0F;
                line.endWidth = 0.0F;
            }
        }

        public void showSilhouette(Boolean setting)
        {
            if (setting) 
            {
                mostRecentSpeaker.GetComponent<Renderer>().material = silhouetteMaterial;
            } 
            else
            {
                mostRecentSpeaker.GetComponent<Renderer>().material = nonSilhouetteMaterial;
            }
        }

        public Boolean checkVisualSpeaker()
        {
            if (doesRaycastHitSpeaker())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public float processAngle(float prevAngle)
        {
            float temp = (prevAngle + 90) % 360;
            if (temp >= 0)
            {
                return temp;
            }
            else
            {
                return 360 + temp;
            }
        }

        public float getSpeakerAngle()
        {
            //replace "if speaker is not null" with "if subtitles should not end"
            if (Speaker != null) //it entered this if satemnt when it wasnt supposed to??!!
            {
                speakerZ = Speaker.transform.position.z;
                speakerX = Speaker.transform.position.x;
                playerZ = player.transform.position.z;
                playerX = player.transform.position.x;
                
                justSpeakerAngle = (180.0f / Mathf.PI) * (Mathf.Atan(Mathf.Abs((speakerX - playerX) / (speakerZ - playerZ))));
                
                if ((speakerZ - playerZ) == 0)
                {
                    if (speakerZ > 0)
                    {
                        justSpeakerAngle = 90.0f;
                    }
                    else
                    {
                        justSpeakerAngle = -90.0f;
                    }
                }
                if (justSpeakerAngle <0.0f)
                {
                    justSpeakerAngle += 180.0f;
                }
                
                speakerAngle = Mathf.Abs(justSpeakerAngle);
                playerAngle =  Mathf.Abs(player.GetComponent<MoveRobot>().playerYAngle) % 360.0f;
                speakerAngle = fullCircleConvert(speakerAngle);
                return speakerAngle;
            }
            return 0.0f;
        }

        public String playerViewContains(string speakerName, float playerAngle)
        {
            if (Speaker != null && Speaker != player) 
            {
                speakerZ = Speaker.transform.position.z;
                speakerX = Speaker.transform.position.x;
                playerZ = player.transform.position.z;
                playerX = player.transform.position.x;
                playerAngle = (player.GetComponent<MoveRobot>().playerYAngle) % (360.0f);

                if ((speakerAngle - playerAngle)>180.0f)
                {
                    playerAngle += 360.0f;
                }
                else if ((speakerAngle - playerAngle) < -180.0f)
                {
                    speakerAngle += 360.0f;
                }

                if (Math.Abs(playerAngle - speakerAngle) <= allowance)
                {
                    return "true";
                }
                else if ((playerAngle - speakerAngle) > 0.0f)
                { 
                    return "right";
                }
                else
                {
                    return "left";
                }
            }
            return "null"; 
        }

        public float fullCircleConvert(float origAngle)
        {
            if (((speakerZ - playerZ) >= 0) && ((speakerX - playerX) >= 0))
            {
                return 360.0f - speakerAngle;
            }
            if (((speakerZ - playerZ) >= 0) && ((speakerX - playerX) < 0))
            {
                return speakerAngle;
            }
            if (((speakerZ - playerZ) < 0) && ((speakerX - playerX) < 0))
            {
                return 180.0f - speakerAngle;
            }
            if (((speakerZ - playerZ) < 0) && ((speakerX - playerX) >= 0))
            {
                return 180.0f + speakerAngle;
            }
            return speakerAngle;
        }

        public void lightSpeaker(Boolean setting)
        {
            if (Speaker != null)
            {
                mostRecentSpeakerLight = Speaker.transform.GetChild(0).gameObject;
                if (mostRecentSpeakerLight != null)
                {
                    if (setting)
                    {
                        mostRecentSpeakerLight.SetActive(true);
                    }
                    else
                    {
                        mostRecentSpeakerLight.SetActive(false);
                    }
                }
                else
                {
                    throw new ArgumentException("speakerLight null");
                }
            }
            else if (mostRecentSpeakerLight != null)
            {
                mostRecentSpeakerLight.SetActive(false);
            }
        }

        public void changeText(Boolean textSetting)
        {
            if (textSetting)
            {
                textBackground.SetActive(true);
            }
            else
            {
                textBackground.SetActive(false);
            }
        }

        //is there a clear line of sight between the player's forward direction and the center of the speaker
        //using some code from/referencing : https://answers.unity.com/questions/294285/casting-ray-forward-from-transform-position.html
        public Boolean doesRaycastHitSpeaker()
        {
            targetHit = new RaycastHit();
            if (Physics.Raycast(player.transform.position, player.transform.forward, out targetHit, 100))
            {
                if (targetHit.transform.name.Equals(speakerTag))
                {
                    return true;
                }
            }
            return false;
        }

        public void setCompass(Boolean setting)
        {
            if (setting)
            {
                x = Compass.transform.rotation.x;
                y = Compass.transform.rotation.y;
                Compass.transform.rotation = Quaternion.Euler(new Vector3(x, y, Mathf.Abs(justSpeakerAngle) - playerAngle));
            }
            Compass.SetActive(setting);
            compassBackground.SetActive(setting);
        }

        public void processSpeakerArrows(Boolean setting, string speakerPos)
        {
            if (setting)
            {
                if (Equals("left", speakerPos))
                {
                    rightArrow.SetActive(false);
                    leftArrow.SetActive(true);
                }
                else if (Equals("right", speakerPos))
                {
                    rightArrow.SetActive(true);
                    leftArrow.SetActive(false);
                }
                else //in the case that the player is facing the speaker, or the speaker is null
                {
                    rightArrow.SetActive(false);
                    leftArrow.SetActive(false);
                }
            }
            else
            {
                rightArrow.SetActive(false);
                leftArrow.SetActive(false);
            }
        }
    }
}