using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubtitleSystem
{
    public class timeScale : MonoBehaviour
    {
        public GameObject mainc;
        // Toggles the time scale between 1 and 0.7
        // whenever the user hits the Fire1 button.

        private float fixedDeltaTime;

        void Start()
        {
            mainc = GameObject.Find("Main Camera");
        }

        void Awake()
        {
            // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
            this.fixedDeltaTime = Time.fixedDeltaTime;
        }

        void Update()
        {
            Time.timeScale = mainc.GetComponent<Main>().currentSpeed;
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
    }
}
