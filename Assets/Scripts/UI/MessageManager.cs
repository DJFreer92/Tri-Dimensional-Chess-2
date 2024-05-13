using System.Collections.Generic;
using UnityEngine;
using TMPro;

using TriDimensionalChess.Tools;

namespace TriDimensionalChess.UI {
	[DisallowMultipleComponent]
	public sealed class MessageManager : MonoSingleton<MessageManager> {
		[SerializeField] private GameObject _msgPrefab;
		[SerializeField] private int _maxNumMessages = 5;
		[SerializeField] private float _messageSpacing = 40f;
		private List<DisappearingMessage> _messages;
		private Vector3 _translation;

		protected override void Awake() {
			base.Awake();
			_messages = new();
			_translation = Vector3.down * _messageSpacing;
		}

		private void Update() {
			while (_messages.Count > 0 && _messages[0] == null) _messages.RemoveAt(0);
		}

		public void CreateMessage(string txt) {
			if (_messages.Count >= _maxNumMessages) RemoveBottomMessage();

			GameObject msg = Instantiate(_msgPrefab, transform);
			msg.transform.GetComponentInChildren<TMP_Text>(true).text = ' ' + txt;

			if (_messages.Count > 0) MoveMessagesDown();

			_messages.Add(msg.GetComponent<DisappearingMessage>());

			msg.transform.GetChild(0).gameObject.SetActive(true);
		}

		private void RemoveBottomMessage() {
			_messages[0].Disapper();
			_messages.RemoveAt(0);
		}

		private void MoveMessagesDown() {
			foreach (DisappearingMessage msg in _messages)
				msg.gameObject.transform.Translate(_translation);
		}
	}
}
