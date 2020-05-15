using UnityEngine;
using System.Collections.Generic;
using System;

namespace FrostweepGames.PhotonWebVoiceWrapper
{
	public class Listener : MonoBehaviour
	{
		private int _sampleRate = 44100;

		private int _recordingTime = 1;

		private bool _audioClipReadyToUse;

		private float _delay;

		private bool _playing;

		private Buffer _buffer;

		private bool _listening;

		public AudioSource _audioSource;

		public void StartListen()
		{
			if (_listening)
				return;

			_listening = true;
			_audioSource.loop = true;
			_buffer = new Buffer();
			_audioSource.clip = AudioClip.Create("BufferedClip", _sampleRate * _recordingTime, 1, _sampleRate, false);
		}

		public void StopListen()
		{
			if (!_listening)
				return;

			_listening = false;
			_audioSource.Stop();
			Destroy(_audioSource.clip);
			_buffer.data.Clear();
			_buffer.position = 0;
		}

		public void HandleRawData(byte[] bytes)
		{
			_buffer.data.AddRange(AudioConverter.ByteToFloat(bytes));
			_audioClipReadyToUse = _buffer.data.Count >= _sampleRate * _recordingTime;
		}

		private void Update()
		{
			try
			{
				if (_listening)
				{
					if (_playing)
					{
						_delay -= Time.deltaTime;

						if (_delay <= 0)
						{
							_playing = false;
						}
					}
					else
					{
						if (_audioClipReadyToUse)
						{
							List<float> chunk;

							if (_buffer.data.Count >= _sampleRate)
							{
								chunk = _buffer.data.GetRange(0, _sampleRate);
								_buffer.data.RemoveRange(0, _sampleRate);
							}
							else
							{
								chunk = _buffer.data;
								_buffer.data.Clear();
								for (int i = chunk.Count; i < _sampleRate; i++)
								{
									chunk.Add(0);
								}
							}

							_audioSource.clip.SetData(chunk.ToArray(), 0);
							_audioSource.Play();

							_delay = (float)_sampleRate / (float)chunk.Count;
							_playing = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message + " | " + ex.StackTrace);
			}
		}

		private class Buffer
		{
			public int position;
			public List<float> data;

			[UnityEngine.Scripting.Preserve]
			public Buffer()
			{
				position = 0;
				data = new List<float>();
			}
		}
	}
}