using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
	public class Blip : MonoBehaviour
	{

		[HideInInspector] public UIHelper.MinimapImage blipImage;

		[Tooltip("If this option is active, the blip will be rotated with the object")]
		public bool rotateWithObject;

		[Tooltip("This image will be displayed on the minimap")]
		public Texture icon;

		[HideInInspector] public UIManager uiManager;
		
		void Start()
		{
			uiManager = FindObjectOfType<UIManager>();
			
			if (icon && uiManager && uiManager.CharacterUI.mapMask && (blipImage != null && blipImage.image))
			{
				blipImage = UIHelper.CreateNewBlip(uiManager, ref blipImage.image, icon, Color.white, "Blip Item", true);
				uiManager.allMinimapImages.Add(blipImage);
			}
		}

		private void LateUpdate()
		{
			if (blipImage != null && blipImage.image && uiManager && uiManager.CharacterUI.mapMask)
			{
				uiManager.SetBlip(transform, !rotateWithObject ? "positionOnly" : "positionAndRotation", blipImage);
			}
		}
	}
}
