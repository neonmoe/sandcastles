using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	class LerpedValue {
		public float InterpolationDuration = 0.2f;
		private float TargetValue;
		private float PreviousValue;
		private float SetTime;

		public LerpedValue(float startingValue) {
			TargetValue = PreviousValue = startingValue;
			SetTime = -1;
		}

		public void Set(float value) {
			PreviousValue = TargetValue;
			TargetValue = value;
			SetTime = Time.time;
		}

		public float Get() {
			float t = SetTime == -1 ? 1 : (Time.time - SetTime) / InterpolationDuration;
			return Mathf.Lerp(PreviousValue, TargetValue, t * t);
		}
	}

	class Bar {
		public GameObject GameObject;
		private RectTransform Transform;
		private float MinHeight;
		private float MaxHeight;
		private LerpedValue Value = new LerpedValue(0);

		public Bar(GameObject go, Vector2 position, bool rightAligned, Transform parent) {
			GameObject = Instantiate(go, new Vector3(), new Quaternion(), parent);
			Transform = GameObject.GetComponent<RectTransform>();
			if (rightAligned) {
				Vector2 Pivot = Transform.pivot;
				Pivot.x = 1 - Pivot.x;
				Transform.pivot = Transform.pivot = Pivot;
			}
			MinHeight = 5;
			MaxHeight = Transform.rect.height;
			Transform.anchoredPosition = position;
			GameObject.SetActive(true);
		}

		public void Set(float v) {
			Value.Set(v);
		}

		public void Update() {
			Vector2 NewOffset = Transform.offsetMin;
			NewOffset.y = (MaxHeight - MinHeight) * (1 - Value.Get());
			Transform.offsetMin = NewOffset;
		}
	}

	public GameObject BaseBar;
	public GameObject SecondaryBar;
	public Transform LeftParent;
	public Transform RightParent;
	public Transform Tutorial0;
	public Transform Tutorial1;

	private List<Bar> CurrentBars = new List<Bar>();
	private List<Bar> SavedBars = new List<Bar>();

	private LerpedValue LeftScale = new LerpedValue(0);
	private LerpedValue RightScale = new LerpedValue(0);
	private Vector3 InitialLeftScale;
	private Vector3 InitialRightScale;

	private LerpedValue Tutorial0Scale = new LerpedValue(1);
	private LerpedValue Tutorial1Scale = new LerpedValue(0);
	private Vector3 InitialTutorial0Scale;
	private Vector3 InitialTutorial1Scale;

	private bool Tutorial0Exited = false;
	private bool Tutorial1Entered = false;
	private bool Tutorial1Exited = false;

	private void Start() {
		BaseBar.SetActive(false);
		SecondaryBar.SetActive(false);
		InitialLeftScale = LeftParent.localScale;
		InitialRightScale = RightParent.localScale;
		InitialTutorial0Scale = Tutorial0.localScale;
		InitialTutorial1Scale = Tutorial1.localScale;
	}

	private void Update() {
		foreach (Bar b in CurrentBars) {
			b.Update();
		}
		foreach (Bar b in SavedBars) {
			b.Update();
		}
		LeftParent.localScale = InitialLeftScale * LeftScale.Get();
		RightParent.localScale = InitialRightScale * RightScale.Get();
		Tutorial0.localScale = InitialTutorial0Scale * Tutorial0Scale.Get();
		Tutorial1.localScale = InitialTutorial1Scale * Tutorial1Scale.Get();
	}

	private void SetBar(List<Bar> bars, int index, int length, float width, bool rightAligned, Transform parent, float value) {
		Vector2 position = new Vector2(index * width, 0);
		if (rightAligned) position.x -= (length - 1) * width;
		if (index >= bars.Count) bars.Insert(index, new Bar(rightAligned ? SecondaryBar : BaseBar, position, rightAligned, parent));
		bars[index].Set(value);
	}

	public void HideTutorial0() {
		if (!Tutorial0Exited) {
			Tutorial0Exited = true;
			Tutorial0Scale.Set(0);
			Debug.Log("Hide tutorial 0");
		}
	}

	public void ShowTutorial1() {
		if (!Tutorial1Exited && !Tutorial1Entered){
			Tutorial1Entered = true;
			Tutorial1Scale.Set(1);
			Debug.Log("Show tutorial 1");
		}
	}

	public void HideTutorial1() {
		if (!Tutorial1Exited) { 
			Tutorial1Exited = true;
			Tutorial1Scale.Set(0);
			Debug.Log("Hide tutorial 1");
		}
	}

	public void SetCurrentBars(List<float> values) {
		for (int i = 0; i < values.Count; i++) {
			SetBar(CurrentBars, i, values.Count, 30, false, LeftParent, values[i]);
		}
		LeftScale.Set(1);
	}

	public void SetSavedBars(List<float> values) {
		for (int i = 0; i < values.Count; i++) {
			SetBar(SavedBars, i, values.Count, 30, true, RightParent, values[i]);
		}
		RightScale.Set(1);
	}

	public void DiscardSavedValues() {
		RightScale.Set(0);
	}
}
