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
        //text for debugging;
        public TextMeshProUGUI speakerAngleText, TimerText, TText, justSpeakerText, playerZAngleText, speakerPosText, playerYAngleText, totalFinalAngleText, fillCircleConvertAngleText, speedText;

        public GameObject player;
        public GameObject Speaker;

        public SubtitleBase subBase;
        public TextMeshProUGUI subtitlesBox;
        public TextAsset subtitleFile;

        public Boolean triggeredOnce;
        public Boolean subtitlesTriggered;
        public Boolean repeated; //mark this as true in start if you want the diolouge to be repeated on each collision

        public Boolean speakerFaced;
        public float playerAngle;
        public float speakerAngle;
        public float justSpeakerAngle;
        public float defaultFloat;
        public float speakerZ;
        public float speakerX;
        public float playerZ;
        public float playerX;

        public Collider speakerCollider;
        public RaycastHit targetHit;

        public GameObject speechBubble;

        public GameObject mostRecentSpeakerLight;

        public GameObject compassBackground;
        public GameObject Compass;

        public GameObject textBackground;
        public GameObject mostRecentSpeaker;
        public Boolean needToSeeSpeaker;

        public Material silhouetteMaterial;
        public Material nonSilhouetteMaterial;
        public double internalTimeRate;

        public Vector3 playerForward;
        public float x;
        public float y;
        public Canvas subtitleDisplay;

        public string speakerTag; //the name o fthe person talkin--null is used to show that no one is talking
        public GameObject mainCamr;
        
        public float allowance;

        private LineRenderer line;

        public GameObject leftArrow;
        public GameObject rightArrow;

        void Start()
        {
            Speaker = player;
            mainCamr = GameObject.Find("Main Camera");
            subtitleDisplay = mainCamr.GetComponent<Main>().subtitleDisplay;
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
            //wont except just a declaration in the main part of the class for some reason
            GameObject mainCamr = GameObject.Find("Main Camera");
            if (mainCamr != null)
            {
                //it keeps saying that main isnt declared for some reason
                //so im going to assign the thing to a boolean instead
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

            if (subBase != null) //order of checking most recent and assigneing speaker conflicts?
            {
                speedText.text = (subBase.subtitleReader.getInternalTimerRate()).ToString();
                subBase.subtitleReader.incrementTime();
                TimerText.text = "Subtitle Reader timer: " +  subBase.subtitleReader.getInternalTime() + "";
                TText.text = "Subtitle Base T: " + subBase.t;
                playerAngle = (player.GetComponent<MoveRobot>().playerYAngle) % 360.0f;
                String speakerPos = playerViewContains(speakerTag, playerAngle);
                speakerPosText.text = "speakerPos: " + speakerPos;
                if (subtitlesTriggered)
                {
                    if (mainCamr.GetComponent<Main>().showCompass) { setCompass(true); }
                    if (mainCamr.GetComponent<Main>().attachSubtitles) { attachSubtitles(); }
                    
                    subBase.UpdateSubtitleBase(); //do our counters and time adjustments match up?
                    speakerTag = subBase.subtitleReader.getSpeaker();//hmmmm
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
                        else if (Equals("true", speakerPos)) //what difference should there be between facing and seeing and just facing??
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
                            //Compass.SetActive(false);
                            speakerFaced = false;
                        //could just have a fiunction that did all the updates related to speakerFaced?
                        double tempor = mainCamr.GetComponent<Main>().speakerFacingSpeed;
                        mainCamr.GetComponent<Main>().currentSpeed = mainCamr.GetComponent<Main>().speakerFacingSpeed;
                        Speaker = null;
                        speakerTag = "";
                        subtitlesTriggered = false;
                        lightSpeaker(false);
                        changeText(false);
                        subBase.subBox.text = "";
                        //i dont think i can set the whole script inactive...especially if its repeatable?
                    }
                }
            }
        }

        public void attachSubtitles()
        {
            if (!subBase.subtitleReader.shouldProgramEnd() && player != null && Speaker != null)
            {
                //i could seperate each kind of subtitles into classes, for user readability, but it seems more convent for the user to _use_ if they're in one class, where you can _pick_ the features
                Boolean AssignCol = GameObject.Find("Main Camera").GetComponent<Main>().assignedSpeakerColors;//have a final variable so ppl can chpoose the nam of their naim object?
                if (AssignCol)
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
                // Update position of the two vertex of the Line Renderer
                line.SetPosition(0, Speaker.transform.position);
                line.SetPosition(1, player.transform.position);

            }
            else //if no one is speaker OR if the program should end
            {
                line.startWidth = 0.0F;
                line.endWidth = 0.0F;
            }
        }

        //check spelling
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
                UnityEngine.Debug.Log(360.0f - (player.transform.rotation.y % (360.0f)));
                //float tempPlayerAngle = ((360 / (2 * Mathf.PI) * player.transform.rotation.y % (360.0f)));
               
                //}(player.transform.rotation.y * (180.0f/(Mathf.PI))) % (360.0f));
                playerAngle =  Mathf.Abs(player.GetComponent<MoveRobot>().playerYAngle) % 360.0f;
                playerYAngleText.text = "play Y angle: " + Mathf.Abs(player.GetComponent<MoveRobot>().playerYAngle) %360.0f;
                playerZAngleText.text = "playerZ: " + player.transform.rotation.z;
                speakerAngleText.text = "speakerAngle: " + fullCircleConvert(speakerAngle);
                speakerAngle = fullCircleConvert(speakerAngle);
                justSpeakerText.text = "justSpeakerAngle: " + speakerAngle;
                return speakerAngle;
            }
            playerYAngleText.text = "play Y angle: " + Mathf.Abs(player.GetComponent<MoveRobot>().playerYAngle) % 360.0f;
            UnityEngine.Debug.Log("speaker null: ");
            return 0.0f;
        }

        //this seems to run before some of the update before this is called?
        public String playerViewContains(string speakerName, float playerAngle)
        {
            ///ugh!! i could include both of the answers in the stirng to return
            ///ibut i think the most elegant would be to include the compass shyerAngleit in HERE
            ///GRar
            ///i mean what if we like seeprate this class into angle subtitles main setting class and helper classes...
            ///in an attemot to do this, i exported the version _Before_ attempting this on 1/21/2021, labeled and COmplexSubtitleSystem with said date
            if (Speaker != null && Speaker != player) //situation where the speaker is the player??
            {
                //if theres no distance, it counts as looking at the speaker--implement
                speakerZ = Speaker.transform.position.z;
                speakerX = Speaker.transform.position.x;
                playerZ = player.transform.position.z;
                playerX = player.transform.position.x;
                playerAngle = (player.GetComponent<MoveRobot>().playerYAngle) % (360.0f);
                //the coefficent converst it to degrees, which i think are easier to sdeall with in grid form
                //perhaps try to trouble shoot by puttingn aline at the player angle angle??
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
                        // sets the public variable emissionSourceMaterial's emission property active. Use DisableKeyword to disable emission.
                    }
                    else
                    {
                        mostRecentSpeakerLight.SetActive(false);
                    }
                }
                else
                {
                    //this system defaults to jsut not doing anything if 
                    //maybe edit it to throw an exception if the light component of a speaker is null?
                    //what about if the speaker doesnt have a light component???
                    UnityEngine.Debug.Log("speakerLight null");
                }
            }
            else if (mostRecentSpeakerLight != null)
            {
                mostRecentSpeakerLight.SetActive(false);
            }
        }

        public void changeText(Boolean textSetting)
        {
            //color text when you face speaker??
            //like because you know who's talking sometimes when you see them??
            if (textSetting)
            {
                ///TO DO: change to havetext backgorund not havr to be assigned for every subtitle trigger 
                textBackground.SetActive(true);
            }
            else
            {
                textBackground.SetActive(false);
            }
        }

        //is there a clear line between the player's forward direction and the center of the speaker
        //using some code from/referencing : https://answers.unity.com/questions/294285/casting-ray-forward-from-transform-position.html
        public Boolean doesRaycastHitSpeaker()
        {
            targetHit = new RaycastHit();
            //do i neeed to initilize targetHit?
            //targetHit is a RaycastHit declared in the beguinning of this class
            //how to get this to check specifically to see if it hits the speaker?
            playerForward = player.transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(player.transform.position, player.transform.forward, out targetHit, 100))
            {
                //UnityEngine.Debug.DrawRay(player.transform.position, player.transform.TransformDirection(Vector3.forward) * targetHit.distance, Color.yellow);
                //UnityEngine.Debug.Log("hit");
                //UnityEngine.Debug.Log("hit: " + targetHit.transform.name);
                if (targetHit.transform.name.Equals(speakerTag))
                {

                    return true;
                }
            }
            //UnityEngine.Debug.DrawRay(player.transform.position, player.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            //UnityEngine.Debug.Log("Did not Hit");
            return false;
        }

        //idk if i need this second one
        public void setCompass (string SpeakerPos, Boolean setting)
        {
            if (setting)
            {
                //if (justSpeakerAngle != defaultFloat)  idk what i had this for im seein what happens without it
                x = Compass.transform.rotation.x;
                y = Compass.transform.rotation.y;
                //negative speaker agles should have negative compass angle too
                //modifying the Vector3, based on input multiplied by speed and time
                UnityEngine.Debug.Log("justSpeakerAngle: " + Mathf.Abs(justSpeakerAngle));
                UnityEngine.Debug.Log("playerYAngle: " + player.GetComponent<MoveRobot>().playerYAngle);
                UnityEngine.Debug.Log("x: " + x);
                UnityEngine.Debug.Log("y: " + y);
                Compass.transform.rotation = Quaternion.Euler(new Vector3(x, y, Mathf.Abs(justSpeakerAngle) - playerAngle));
                
            }
            Compass.SetActive(setting);
            compassBackground.SetActive(setting);


        }

        public void setCompass(Boolean setting)
        {
            if (setting)
            {
                //if (justSpeakerAngle != defaultFloat)  idk what i had this for im seein what happens without it
                x = Compass.transform.rotation.x;
                y = Compass.transform.rotation.y;
                //negative speaker agles should have negative compass angle too
                //modifying the Vector3, based on input multiplied by speed and time
                UnityEngine.Debug.Log("justSpeakerAngle: " + justSpeakerAngle);
                UnityEngine.Debug.Log("playerYAngle: " + playerAngle);
                UnityEngine.Debug.Log("x: " + x);
                UnityEngine.Debug.Log("y: " + y);
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
                else
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