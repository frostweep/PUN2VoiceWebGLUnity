using UnityEngine;
using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System.Linq;
using System;

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

			RefreshMicrophones();
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
			int currentPosition = CustomMicrophone.GetPosition(_microphoneDevice);

			// fix for end record incorrect position
			if (_stopRecordPosition != -1)
				currentPosition = _stopRecordPosition;

			if (recording || currentPosition != _lastPosition)
			{
				float[] array = new float[Constants.RecordingTime * Constants.SampleRate];
				CustomMicrophone.GetRawData(ref array, _workingClip);

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

					// sends data chunky
					if (_buffer.Count >= Constants.ChunkSize)
					{
						SendDataToNetwork(_buffer.GetRange(0, Constants.ChunkSize));
						_buffer.RemoveRange(0, Constants.ChunkSize);
					}
				}

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
		public void RefreshMicrophones()
		{
			CustomMicrophone.RequestMicrophonePermission();
			CustomMicrophone.RefreshMicrophoneDevices();

			if (CustomMicrophone.HasConnectedMicrophoneDevices())
			{
				_microphoneDevice = CustomMicrophone.devices[0];
			}
		}

		/// <summary>
		/// Starts recording of microphone
		/// </summary>
		public void StartRecord()
		{
			if (CustomMicrophone.IsRecording(_microphoneDevice) || !CustomMicrophone.HasConnectedMicrophoneDevices())
			{
				RecordFailedEvent?.Invoke("record already started or no microphone device conencted");
				return;
			}

			_stopRecordPosition = -1;

			recording = true;

			_buffer?.Clear();

			_workingClip = CustomMicrophone.Start(_microphoneDevice, true, Constants.RecordingTime, Constants.SampleRate);

			RecordStartedEvent?.Invoke();
		}

		/// <summary>
		/// Stops recording of microphone
		/// </summary>
		public void StopRecord()
		{
			if (!CustomMicrophone.IsRecording(_microphoneDevice))
				return;

			recording = false;

			if (CustomMicrophone.HasConnectedMicrophoneDevices())
			{
				_stopRecordPosition = CustomMicrophone.GetPosition(_microphoneDevice);

				CustomMicrophone.End(_microphoneDevice);
			}

			if (_workingClip != null)
			{
				Destroy(_workingClip);
			}

			RecordEndedEvent?.Invoke();
		}
	}
}