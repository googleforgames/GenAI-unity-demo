using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
	public class LookAtCamera : MonoBehaviour
	{
		private Transform characterCamera;

		void Update()
		{
			if(characterCamera)
				transform.LookAt(characterCamera);
			else
			{
				var controllers = FindObjectsOfType<Controller>();

				foreach (var controller in controllers)
				{
					if (!controller.isRemoteCharacter)
						characterCamera = controller.CameraController.MainCamera;
				}
			}
		}
	}
}
