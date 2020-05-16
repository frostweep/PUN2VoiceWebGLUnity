using System.Collections.Generic;
using UnityEngine;

namespace FrostweepGames.WebGLPUNVoice
{
    public class Speaker
    {
        private GameObject _selfObject;

        private AudioSource _source;

        private AudioClip _workingClip;

        private Buffer _buffer;

        private bool _audioClipReadyToUse;

        private float _delay;

        private bool _playing;

        private int _id;

        public Speaker(int id, Transform parent)
        {
            _id = id;
            _selfObject = new GameObject("Speaker_" + _id);
            _selfObject.transform.SetParent(parent);
            _source = _selfObject.AddComponent<AudioSource>();

            _buffer = new Buffer();
            _workingClip = AudioClip.Create("BufferedClip_" + _id, Constants.SampleRate * Constants.RecordingTime, 1, Constants.SampleRate, false);
            _source.clip = _workingClip;
        }

        public void Update()
        {
            try
            {
                _audioClipReadyToUse = _buffer.data.Count >= Constants.SampleRate * Constants.RecordingTime;

                if (_playing)
                {
                    _delay -= Time.deltaTime;

                    if (_delay <= 0)
                    {
                        _source.Stop();
                        _playing = false;
                    }
                }

                if(!_playing)
                {
                    if (_audioClipReadyToUse)
                    {
                        List<float> chunk;

                        if (_buffer.data.Count >= Constants.SampleRate)
                        {
                            chunk = _buffer.data.GetRange(0, Constants.SampleRate);
                            _buffer.data.RemoveRange(0, Constants.SampleRate);
                        }
                        else
                        {
                            chunk = _buffer.data;
                            _buffer.data.Clear();
                            for (int i = chunk.Count; i < Constants.SampleRate; i++)
                            {
                                chunk.Add(0);
                            }
                        }

                        _workingClip.SetData(chunk.ToArray(), 0);
                        _source.Play();

                        _delay = (float)Constants.SampleRate / (float)chunk.Count;
                        _playing = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Message + " | " + ex.StackTrace);
            }
        }

        public void HandleRawData(byte[] bytes)
        {
            _buffer.data.AddRange(AudioConverter.ByteToFloat(bytes));
        }

        public void Dispose()
        {
            _source.Stop(); 
            _buffer.data.Clear();
            _buffer.position = 0;

            Object.Destroy(_workingClip);
            Object.Destroy(_selfObject);
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