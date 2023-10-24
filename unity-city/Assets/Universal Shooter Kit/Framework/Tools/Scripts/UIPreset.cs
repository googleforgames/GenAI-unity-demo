using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace GercStudio.USK.Scripts
{
	[ExecuteInEditMode]
	public class UIPreset : MonoBehaviour
	{
		private RectTransform _rectTransform, _parentRectTransform;
		private VerticalLayoutGroup verticalLayoutGroup;

		public MultiplayerHelper.ContentType ContentType;
		
		public Text KD;
		public Text Rank;
		public Text Score;
		public RawImage Icon;
		public Color32 HighlightedColor;

		public Text KillerName;
		public Text VictimName;
		public RawImage ActionIcon;
		
		public Text Mode;
		public Text Map;
		public Text Count;

		public Text requiredStatsAndStatus;

		public Text Name;
		public RawImage ImagePlaceholder;
		public Button Button;
		public Image Background;
		public Image SelectionIndicator;

		void OnEnable()
		{
			UpdateWidth();
		}

		void Update()
		{
			UpdateWidth();
		}

		public void UpdateWidth()
		{
			if (ContentType != MultiplayerHelper.ContentType.Player) return;
			
			verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();

			if (verticalLayoutGroup != null)
			{
				_parentRectTransform = verticalLayoutGroup.GetComponent<RectTransform>();
				_rectTransform = GetComponent<RectTransform>();
				_rectTransform.pivot = new Vector2(0, 1);
				_rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);

				_rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);
			}
		}
	}
}

