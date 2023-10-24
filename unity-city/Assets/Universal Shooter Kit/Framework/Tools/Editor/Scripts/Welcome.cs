using System;
using System.Collections.Generic;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
	// [CreateAssetMenu(fileName = "Welcome", menuName = "Assets")]
	public class Welcome : ScriptableObject
	{
		public Texture2D icon;
		public string title;
		public List<Section> sections = new List<Section>();
		public bool loadedLayout;

		[Serializable]
		public class Section
		{
			public string heading, text, linkText, url;
		}
	}
}
