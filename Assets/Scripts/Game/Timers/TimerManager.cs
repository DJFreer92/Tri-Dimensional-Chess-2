using UnityEngine;
using TMPro;

using TriDimensionalChess.Networking;
using TriDimensionalChess.Networking.NetMessages;
using TriDimensionalChess.Tools;

namespace TriDimensionalChess.Game.Timers {
	[DisallowMultipleComponent]
	public class TimerManager : MonoSingleton<TimerManager> {
		private const float _NET_TIMER_TICK_RATE = 5f;
		public bool TimersEnabled {get; private set;}
		private float _lastNetTimer;

		[Header("References")]
		[SerializeField] private GameObject _timersParent;
		[SerializeField] private Timer _whiteTimer;
		[SerializeField] private Timer _blackTimer;
		[SerializeField] private TMP_InputField _customTimeInput;
		[SerializeField] private TMP_InputField _customIncrementInput;

		protected override void Awake() {
			base.Awake();

			//unpause the white timer
			if (TimersEnabled) _whiteTimer.Paused = false;
		}

		private void Update() {
			//SyncTimers();
		}

		private void OnEnable() => RegisterToEvents();

		private void OnDisable() => UnregisterToEvents();

		///<summary>
		///Adds local methods to listeners
		///</summary>
		private void RegisterToEvents() {
			Game.Instance.OnCurrentPlayerChange += SwitchTimers;

			//networking
			NetUtility.C_TIMERS += OnTimersClient;
		}

		///<summary>
		///Removes local methods from listeners
		///</summary>
		private void UnregisterToEvents() {
			if (Game.IsCreated()) Game.Instance.OnCurrentPlayerChange -= SwitchTimers;

			//networking
			NetUtility.C_TIMERS -= OnTimersClient;
		}

		///<summary>
		///Initialize the timers with time control entered on the custom time control page
		///</summary>
		public void OnInitializeTimersWithCustomControlButton() =>
			InitializeTimers(
				int.Parse(_customTimeInput.text),
				string.IsNullOrEmpty(_customIncrementInput.text) ? int.Parse(_customIncrementInput.text) : 0
			);

		///<summary>
		///Initialize the timers with the given time control
		///</summary>
		///<param name="control">The time control</param>
		public void OnInitializeTimersWithControlButton(string control) {
			int index = control.IndexOf("+");
			InitializeTimers(int.Parse(control.Substring(0, index)), int.Parse(control.Substring(index + 1)));
		}

		///<summary>
		///Initializes the timer with the given time control
		///</summary>
		///<param name="time">The allotated time for each timer</param>
		///<param name="increment">The increment for each timer</param>
		public void InitializeTimers(int time, int increment) {
			_whiteTimer.SetTimeControl((float) time, increment);
			_blackTimer.SetTimeControl((float) time, increment);
			TimersEnabled = true;
			StopTimers(false);
		}

		///<summary>
		///Start white's timer
		///</summary>
		public void StartFirstTimer() => _whiteTimer.Paused = false;

		///<summary>
		///Returns the times on each timer
		///</summary>
		///<returns>The times on each timer</returns>
		public float[] GetTimes() => new float[] {_whiteTimer.GetTime(), _blackTimer.GetTime()};

		///<summary>
		///Switch which timer is active
		///</summary>
		private void SwitchTimers() {
			_whiteTimer.Switch();
			_blackTimer.Switch();
		}

		///<summary>
		///Stops both player's timers
		///</summary>
		///<param name="stop">Whether to stop or unstop the timers</param>
		public void StopTimers(bool stop) {
			_whiteTimer.Stopped = stop;
			_blackTimer.Stopped = stop;
		}

		///<summary>
		///Toggle the timers on or off
		///</summary>
		///<param name="toggle">Whether to toggle the timers on or off</param>
		public void ToggleTimers(bool toggle) {
			TimersEnabled = false;
			StopTimers(true);
			_timersParent.SetActive(false);
		}

		//Networking
		///<summary>
		///Update the timers to the information in the message
		///</summary>
		///<param name="msg">Incoming message</param>
		public void OnTimersClient(NetMessage msg) {
			NetTimers timerMsg = msg as NetTimers;
			_whiteTimer.UpdateTime(timerMsg.WhiteTime);
			_blackTimer.UpdateTime(timerMsg.BlackTime);
			_whiteTimer.Paused = timerMsg.WhiteTimePaused;
			_blackTimer.Paused = timerMsg.BlackTimePaused;
			StopTimers(timerMsg.TimersStopped);
		}

		///<summary>
		///Syncs the timers on the server with the clients
		///</summary>
		private void SyncTimers() {
			if (Server.Instance == null) return;
			if (Time.time - _lastNetTimer <= _NET_TIMER_TICK_RATE) return;
			_lastNetTimer = Time.time;
			Server.Instance.Broadcast(
				new NetTimers() {
					WhiteTime = _whiteTimer.GetTime(),
					BlackTime =  _blackTimer.GetTime(),
					WhiteTimePaused = _whiteTimer.Paused,
					BlackTimePaused = _blackTimer.Paused,
					TimersStopped = TimersEnabled
				}
			);
		}
	}
}
