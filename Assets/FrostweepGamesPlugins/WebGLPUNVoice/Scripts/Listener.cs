using UnityEngine;
using System.Collections.Generic;

namespace FrostweepGames.WebGLPUNVoice
{
	public class Listener : MonoBehaviour
	{
		private object _lock = new object();

		private bool _listening;

		private Dictionary<int, Speaker> _speakers;

		public bool startListenAtAwake;

		private void Awake()
		{
			_speakers = new Dictionary<int, Speaker>();

			// Subscribe on PUN events 
			Photon.Pun.PhotonNetwork.NetworkingClient.EventReceived += NetworkEventReceivedHandler;

			if(startListenAtAwake)
			{
				StartListen();
			}
		}

		private void OnDestroy()
		{
			StopListen();

			// Unsubscribe from PUN events 
			Photon.Pun.PhotonNetwork.NetworkingClient.EventReceived += NetworkEventReceivedHandler;

			ResetSpeakers();
		}

		private void Update()
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				foreach (var speaker in _speakers)
				{
					speaker.Value.Update();
				}
			}
		}

		private void ResetSpeakers()
		{
			lock (_lock)
			{
				foreach (var speaker in _speakers)
				{
					speaker.Value.Dispose();
				}
				_speakers.Clear();
			}
		}

		/// <summary>
		/// Handles data from network connected to specific client id
		/// </summary>
		/// <param name="id">unique id of a remote client</param>
		/// <param name="bytes">array of received data (samples)</param>
		private void HandleRawData(int id, byte[] bytes)
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				Speaker speaker;

				if (!_speakers.ContainsKey(id))
				{
					speaker = new Speaker(id, transform);
					_speakers.Add(id, speaker);
				}
				else
				{
					speaker = _speakers[id];
				}

				speaker.HandleRawData(bytes);
			}
		}

		/// <summary>
		/// PUN event handler of network events
		/// </summary>
		/// <param name="photonEvent"></param>
		private void NetworkEventReceivedHandler(ExitGames.Client.Photon.EventData photonEvent)
		{
			if (photonEvent.Code == Constants.VoiceEventCode)
			{
				HandleRawData(photonEvent.Sender, (byte[])photonEvent.CustomData);
			}
		}

		public void StartListen()
		{
			if (_listening)
				return;

			_listening = true;
		}

		public void StopListen()
		{
			if (!_listening)
				return;

			_listening = false;
			ResetSpeakers();
		}

		public void SpeakerLeave(int id)
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				if (_speakers.ContainsKey(id))
				{
					_speakers[id].Dispose();
					_speakers.Remove(id);
				}
			}
		}
	}
}