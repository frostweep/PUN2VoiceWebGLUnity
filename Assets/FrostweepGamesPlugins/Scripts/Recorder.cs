using UnityEngine;
using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System.Linq;

namespace FrostweepGames.PhotonWebVoiceWrapper
{
	public class Recorder : MonoBehaviour
	{
		private int _sampleRate = 44100;

		private int _recordingTime = 1;

		private int _lastPosition = 0;

		private List<float> _samplesBuffer;

		private int _sendDataBlockSize = 44100;

		private AudioClip _workingClip;

		public KeyCode recordingKey = KeyCode.R;

		public bool readyToRecord = false;

		private void Start()
		{
			_samplesBuffer = new List<float>();
			CustomMicrophone.RequestMicrophonePermission();
		}

		private void Update()
		{
			if (!readyToRecord)
				return;

			if(Input.GetKeyDown(recordingKey))
			{
				if (!CustomMicrophone.HasConnectedMicrophoneDevices() || CustomMicrophone.IsRecording(string.Empty))
					return;

				Debug.Log("START RECORD");

				_workingClip = CustomMicrophone.Start(CustomMicrophone.devices[0], true, _recordingTime, _sampleRate);
			}
			else if (Input.GetKeyUp(recordingKey))
			{
				if (!CustomMicrophone.HasConnectedMicrophoneDevices() || !CustomMicrophone.IsRecording(string.Empty))
					return;

				Debug.Log("STOP RECORD");

				CustomMicrophone.End(CustomMicrophone.devices[0]);

				if (_workingClip != null)
				{
					Destroy(_workingClip);
				}
			}

			ProcessRecording();
		}

		private void ProcessRecording()
		{
			if (CustomMicrophone.IsRecording(string.Empty))
			{
				float[] array = new float[_workingClip.frequency * _workingClip.channels];
				CustomMicrophone.GetRawData(ref array, _workingClip);

				if (_lastPosition != CustomMicrophone.GetPosition(CustomMicrophone.devices[0]) && array.Length > 0)
				{
					int lastPosition = _lastPosition;
					_lastPosition = CustomMicrophone.GetPosition(CustomMicrophone.devices[0]);

					if (lastPosition > _lastPosition)
					{
						_samplesBuffer.AddRange(array.ToList().GetRange(lastPosition, array.Length - lastPosition));
						_samplesBuffer.AddRange(array.ToList().GetRange(0, _lastPosition));
					}
					else
					{
						_samplesBuffer.AddRange(array.ToList().GetRange(lastPosition, _lastPosition - lastPosition));
					}
				}

				if (_samplesBuffer.Count >= _sendDataBlockSize)
				{
					SendDataToNetwork(_samplesBuffer.GetRange(0, _sendDataBlockSize));
					_samplesBuffer.RemoveRange(0, _sendDataBlockSize);
				}
			}
			else
			{
				if (_samplesBuffer.Count > 0)
				{
					SendDataToNetwork(_samplesBuffer);
					_samplesBuffer.Clear();
				}
			}
		}

		private void SendDataToNetwork(List<float> samples)
		{
			byte[] bytes = AudioConverter.FloatToByte(samples);

			PhotonConnector.Instance.SendDataOverNetwork(bytes);
		}
	}
}