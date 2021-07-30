using System.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

namespace SubtitleSystem
{
	public class MoveRobot : MonoBehaviour
	{
		public GameObject player;
		public GameObject mainCam;
		float moveSpeed = 2.0f;
		public Vector3 moveDirection;
		public float playerYAngle;
		public LineRenderer lineRenderer;

		void Start()
		{
			mainCam = GameObject.Find("Main Camera");
			player = this.gameObject;
			playerYAngle = 0.0f;
		}


		//whatever movement methoid you use, you should keep track of the playerYAngle like I do here
		//to get the calculations for whether the playeyr is facing the speaker or not to work (if you want to use them of course)
		void Update()
		{
			if (Input.GetKey(KeyCode.A))
			{
				transform.Rotate(0.0f, -2.0f, 0.0f, Space.Self);
				playerYAngle += 2.0f;
				if (playerYAngle >= 360.0f)
				{
					playerYAngle %= 360.0f;
				}
			}
			if (Input.GetKey(KeyCode.D))
			{
				transform.Rotate(0.0f, 2.0f, 0.0f, Space.Self);
				playerYAngle -= 2.0f;
				if (playerYAngle < 0.0f)
                {
					playerYAngle += 360.0f;
                }
			}
			cameraFollow();
			Move();
		}

		public void Move()
		{
			float moveForwardAndBackward = Input.GetAxis("Vertical");
			if (Input.GetKey(KeyCode.W))
			{
				moveDirection = (gameObject.transform.forward * moveSpeed * Time.deltaTime);
				transform.position = new Vector3((moveDirection.x + transform.position.x), 0.5f, (moveDirection.z + transform.position.z));
				cameraFollow();
			}
		}

		//this is the script i was using to test the subittle system but of course it can be replaced with whatever movement method
		void cameraFollow()
		{
			float playerZ = this.transform.position.z;
			float playerX = this.transform.position.x;
			float camXSin = Mathf.Sin((float)((Math.PI) / 180.0) * (-playerYAngle));
			float camZCos = Mathf.Cos((float)((Math.PI) / 180.0) * (-playerYAngle));
			float cameraX = playerX - (3.0f) * camXSin;
			float cameraZ = playerZ - (3.0f) * camZCos;
			Vector3 newCameraPosition = new Vector3(cameraX, 1.3f, cameraZ);
			mainCam.transform.rotation = this.transform.rotation;
			mainCam.GetComponent<Transform>().position = newCameraPosition;
		}
	}
}

