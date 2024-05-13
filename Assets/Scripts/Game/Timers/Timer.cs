using System;
using UnityEngine;
using System.Text;
using TMPro;

namespace TriDimensionalChess.Game.Timers {
	[DisallowMultipleComponent]
	public class Timer : MonoBehaviour {
		private const int _SECONDS_PER_HOUR = 3600;
		private const int _SECONDS_PER_MINUTE = 60;
		private const int _MAX_TIME_SECONDS = 36000;
		private const int _MAX_INCREMENT_SECONDS = 3600;
		private const int _TIME_WARNING_THRESHOLD_SECONDS = 10;
		private static readonly Color _WHITE_COLOR = new Color(230, 230, 230, 255);
		private static readonly Color _RED_COLOR = new Color(240, 0, 0, 255);
		public bool Paused {get; set;} = true;
		public bool Stopped {get; set;} = true;
		[SerializeField] private TMP_Text _txt;
		[SerializeField] private bool _isWhiteTimer;
		private string _prefix;
		private float _time;
		private int _increment;

		private void Awake() {
			_prefix = (_isWhiteTimer ? "W" : "B") + ": ";
			DisplayTime();
		}

		private void Update() {
			if (Stopped || Paused) return;

			_time -= Time.deltaTime;

			if (_time <= 0) {
				_time = 0;
				Stopped = true;
			}

			DisplayTime();
		}

		///<summary>
		///Returns the time left on the timer
		///</summary>
		///<returns>The time left on the timer</returns>
		public float GetTime() => _time;

		///<summary>
		///Sets the time control
		///</summary>
		///<param name="time">The time to set to</param>
		///<param name="increment">The increment to set to</param>
		public void SetTimeControl(float time, int increment) {
			_time = Math.Clamp(time * _SECONDS_PER_MINUTE, 0, _MAX_TIME_SECONDS);
			_increment = Math.Clamp(increment, 0, _MAX_INCREMENT_SECONDS);
		}

		///<summary>
		///Sets the time to the given time
		///</summary>
		///<param name="time">The timeto set to</param>
		public void UpdateTime(float time) {
			_time = time;
			DisplayTime();
		}

		///<summary>
		///Returns an array of the hours, minutes, seconds, and milliseconds left on the timer
		///</summary>
		///<returns>An array of the hours, minutes, seconds, and milliseconds left on the timer</returns>
		public int[] GetLongTime() {
			int hr = (int) _time / _SECONDS_PER_HOUR;
			int min = (int) _time / _SECONDS_PER_MINUTE;
			int sec = (int) _time % _SECONDS_PER_MINUTE;
			int ms = (int) (Math.Round(_time % 1 * 100, MidpointRounding.AwayFromZero) / 10);

			return new int[] {hr, min, sec, ms};
		}

		///<summary>
		///Displays the time left on the timer on the game UI
		///</summary>
		public void DisplayTime() {
			int[] longTime = GetLongTime();

			string hr = longTime[0].ToString();
			string min = longTime[1].ToString();
			string sec = longTime[2].ToString();
			string ms = longTime[3].ToString();

			if (longTime[0] > 0 && min.Length == 1) min = "0" + min;
			if (sec.Length == 1) sec = "0" + sec;

			var txt = new StringBuilder(_prefix);
			if (longTime[0] > 0) txt.Append(hr).Append(":");
			txt.Append(min).Append(":").Append(sec);
			if (_time < _TIME_WARNING_THRESHOLD_SECONDS) txt.Append(".").Append(ms);

			_txt.text = txt.ToString();
			_txt.color = (_time <= _TIME_WARNING_THRESHOLD_SECONDS) ? _RED_COLOR : _WHITE_COLOR;
		}

		///<summary>
		///Pause and increment or unpause the timer
		///</summary>
		public void Switch() {
			if (Stopped) return;
			Paused = !Paused;
			if (!Paused) return;

			_time += _increment;
			DisplayTime();
		}

		///<summary>
		///Returns a string of data about the timer
		///</summary>
		///<returns>A string of data about the timer</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append("Time: ").Append(_time);
			str.Append("Increment: ").Append(_increment);
			str.Append("Paused? ").Append(Paused);
			str.Append("Stopped? ").Append(Stopped);
			return str.ToString();
		}
	}
}
