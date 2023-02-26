using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Microphone = FrostweepGames.MicrophonePro.Microphone;

namespace FrostweepGames.WebGLPUNVoice
{
	/// <summary>
	/// Basic record system for voice chat
	/// </summary>
	public class Recorder : MonoBehaviour
	{
		/// <summary>
		/// Throws when record successfully started
		/// </summary>
		public event Action RecordStartedEvent;

		/// <summary>
		/// Throws when record successfully ended
		/// </summary>
		public event Action RecordEndedEvent;

		/// <summary>
		/// Throws when record starting failed
		/// </summary>
		public event Action<string> RecordFailedEvent;

		/// <summary>
		/// Last cached sample position
		/// </summary>
		private int _lastPosition = 0;

		/// <summary>
		/// Array of recoreded samples
		/// </summary>
		private List<float> _buffer;

		/// <summary>
		/// Microphone audio clip
		/// </summary>
		private AudioClip _workingClip;

		/// <summary>
		/// Current selected microphone device in usage
		/// </summary>
		private string _microphoneDevice;

		/// <summary>
		/// Sets if transmission over network will be reliable or not
		/// </summary>
		public bool reliableTransmission = true;

		/// <summary>
		/// Sets network receivers in network, if enabled then sends also on this client, if not - only others
		/// </summary>
		public bool debugEcho = false;

		/// <summary>
		/// Says status of recording
		/// </summary>
		public bool recording = false;

		/// <summary>
		/// Saves last position of mic when it stops
		/// </summary>
		private int _stopRecordPosition = -1;

		/// <summary>
		/// Initializes buffer, refreshes microphones list and selects first microphone device if exists
		/// </summary>
		private void Start()
		{
			_buffer = new List<float>();

            Microphone.RecordStreamDataEvent += RecordStreamDataEventHandler;
            Microphone.PermissionChangedEvent += PermissionChangedEventHandler;
        }

        private void OnDestroy()
        {
            Microphone.RecordStreamDataEvent -= RecordStreamDataEventHandler;
            Microphone.PermissionChangedEvent -= PermissionChangedEventHandler;
        }

        /// <summary>
        /// Handles processing of recording each frame
        /// </summary>
        private void Update()
		{
			ProcessRecording();
		}

		/// <summary>
		/// Processes samples data from microphone recording and fills buffer of samples then sends it over network
		/// </summary>
		private void ProcessRecording()
		{
			int currentPosition = Microphone.GetPosition(_microphoneDevice);

			// fix for end record incorrect position
			if (_stopRecordPosition != -1)
				currentPosition = _stopRecordPosition;

			if (recording || currentPosition != _lastPosition)
			{
#if !UNITY_WEBGL || UNITY_EDITOR
				float[] array = new float[Constants.RecordingTime * Constants.SampleRate];
                Microphone.GetData(array, 0);

				if (_lastPosition != currentPosition && array.Length > 0)
				{
					if (_lastPosition > currentPosition)
					{
						_buffer.AddRange(GetChunk(array, _lastPosition, array.Length - _lastPosition));
						_buffer.AddRange(GetChunk(array, 0, currentPosition));
					}
					else
					{
						_buffer.AddRange(GetChunk(array, _lastPosition, currentPosition - _lastPosition));
					}
#endif
					// sends data chunky
					if (_buffer.Count >= Constants.ChunkSize)
					{
						SendDataToNetwork(_buffer.GetRange(0, Constants.ChunkSize));
						_buffer.RemoveRange(0, Constants.ChunkSize);
					}
#if !UNITY_WEBGL || UNITY_EDITOR
                }
#endif

                _lastPosition = currentPosition;
			}
			else
			{
				_lastPosition = currentPosition;

				if (_buffer.Count > 0)
				{
					// sends left data chunky
					if (_buffer.Count >= Constants.ChunkSize)
					{
						SendDataToNetwork(_buffer.GetRange(0, Constants.ChunkSize));
						_buffer.RemoveRange(0, Constants.ChunkSize);
					}
					// sends all left data
					else
					{
						SendDataToNetwork(_buffer);
						_buffer.Clear();
					}
				}
			}
		}

		/// <summary>
		/// Gets range from an array based on start index and length
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">input array</param>
		/// <param name="index">start offset</param>
		/// <param name="length">length of output array and how many items will be copied from initial array</param>
		/// <returns></returns>
		private T[] GetChunk<T>(T[] data, int index, int length)
		{
			if (data.Length < index + length)
				throw new Exception("Input array less than parameters income!");

			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		/// <summary>
		/// Sends data to other clients or if debg echo then sends to all including this client
		/// </summary>
		/// <param name="samples">list of sampels that will be sent over network</param>
		private void SendDataToNetwork(List<float> samples)
		{
			// data in bytes to send over network
			byte[] bytes = AudioConverter.FloatToByte(samples);

			// sending data of recorded samples by using raise event feature
			Photon.Realtime.RaiseEventOptions raiseEventOptions = new Photon.Realtime.RaiseEventOptions { Receivers = debugEcho ? Photon.Realtime.ReceiverGroup.All : Photon.Realtime.ReceiverGroup.Others };
			ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions { Reliability = reliableTransmission };
			Photon.Pun.PhotonNetwork.RaiseEvent(Constants.VoiceEventCode, bytes, raiseEventOptions, sendOptions);
		}

        /// <summary>
        /// Requests microphone perission and refreshes list of microphones if WebGL platform
        /// </summary>
        [Obsolete("Doesn't refresh native devices anymore. Use SetMicrophone instead")]
		public void RefreshMicrophones()
		{
		}

		/// <summary>
		/// Starts recording of microphone
		/// </summary>
		public void StartRecord()
		{
			if (Microphone.IsRecording(_microphoneDevice) || Microphone.devices.Length == 0 || string.IsNullOrEmpty(_microphoneDevice))
			{
				RecordFailedEvent?.Invoke("record already started, no microphone device connected or no microphone selected");
				return;
			}

			_stopRecordPosition = -1;

			recording = true;

			_buffer?.Clear();

			_workingClip = Microphone.Start(_microphoneDevice, true, Constants.RecordingTime, Constants.SampleRate);

			RecordStartedEvent?.Invoke();
		}

		/// <summary>
		/// Stops recording of microphone
		/// </summary>
		public void StopRecord()
		{
			if (!Microphone.IsRecording(_microphoneDevice))
				return;

			recording = false;

			if (Microphone.devices.Length > 0)
			{
				_stopRecordPosition = Microphone.GetPosition(_microphoneDevice);

                Microphone.End(_microphoneDevice);
			}

			if (_workingClip != null)
			{
				Destroy(_workingClip);
			}

			RecordEndedEvent?.Invoke();
		}

		/// <summary>
        /// Set microphone by device name
        /// </summary>
        /// <param name="deviceName"></param>
		public void SetMicrophone(string deviceName)
        {
			_microphoneDevice = deviceName;
        }

        /// <summary>
        /// Fill buffer from stream data from native WebGL
        /// </summary>
        /// <param name="streamData"></param>
        private void RecordStreamDataEventHandler(Microphone.StreamData streamData)
        {
            _buffer.AddRange(AudioConverter.InterleaveChannelsDataFromStream(streamData));
        }

		/// <summary>
        /// Handles changes in native permissions of microphon
        /// </summary>
        /// <param name="granted"></param>
        private void PermissionChangedEventHandler(bool granted)
        {
            if (granted && Microphone.devices.Length > 0 && string.IsNullOrEmpty(_microphoneDevice))
            {
                SetMicrophone(Microphone.devices[0]);
            }
            else
            {
                SetMicrophone(null);
            }
        }
    }
}