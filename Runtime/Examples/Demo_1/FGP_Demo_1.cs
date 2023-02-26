using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Microphone = FrostweepGames.MicrophonePro.Microphone;

namespace FrostweepGames.WebGLPUNVoice.Examples
{
    public class FGP_Demo_1 : MonoBehaviour
    {
        private List<RemoteSpeakerItem> _remoteSpeakerItems;

        public Dropdown microphonesDropdown;

        public Button refreshMicrophonesButton;

        public Toggle debugEchoToggle;

        public Toggle reliableTransmissionToggle;

        public Toggle muteRemoteClientsToggle;

        public Text stateText;

        public Text serverText;

        public Text roomNameText;

        public Transform parentOfRemoteClients;

        public Toggle muteMyClientToggle;

        public GameObject remoteClientPrefab;

        public Recorder recorder;

        public Listener listener;

        private void Start()
        {
            refreshMicrophonesButton.onClick.AddListener(RefreshMicrophonesButtonOnClickHandler);
            muteMyClientToggle.onValueChanged.AddListener(MuteMyClientToggleValueChanged);
            muteRemoteClientsToggle.onValueChanged.AddListener(MuteRemoteClientsToggleValueChanged);
            debugEchoToggle.onValueChanged.AddListener(DebugEchoToggleValueChanged);
            reliableTransmissionToggle.onValueChanged.AddListener(ReliableTransmissionToggleValueChanged);
            microphonesDropdown.onValueChanged.AddListener(MicrophonesDropdownValueChanged);

            _remoteSpeakerItems = new List<RemoteSpeakerItem>();
#if !UNITY_WEBGL || UNITY_EDITOR
            RefreshMicrophonesButtonOnClickHandler();
#endif

            listener.SpeakersUpdatedEvent += SpeakersUpdatedEventHandler;

            Microphone.PermissionChangedEvent += PermissionChangedEventHandler;
        }

        private void PermissionChangedEventHandler(bool granted)
        {
            RefreshMicrophonesButtonOnClickHandler();
        }

        private void Update()
        {
            stateText.text = "Client state: " + PhotonNetwork.NetworkClientState.ToString();
            serverText.text = "Server: " + PhotonNetwork.Server.ToString();
            roomNameText.text = "Room: " + (PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "Not Joined");
        }

        private void SpeakersUpdatedEventHandler()
        {
            if(_remoteSpeakerItems.Count > 0)
            {
                foreach(var item in _remoteSpeakerItems)
                {
                    item.Dispose();
                }
                _remoteSpeakerItems.Clear();
            }

            foreach(var speaker in listener.Speakers)
            {
                _remoteSpeakerItems.Add(new RemoteSpeakerItem(parentOfRemoteClients, remoteClientPrefab, speaker.Value));
            }
        }

        private void RefreshMicrophonesButtonOnClickHandler()
        {
            microphonesDropdown.ClearOptions();
            microphonesDropdown.AddOptions(Microphone.devices.ToList());

            MicrophonesDropdownValueChanged(0);
        }

        private void MuteMyClientToggleValueChanged(bool status)
        {
            if(status)
            {
                recorder.StartRecord();
            }
            else
            {
                recorder.StopRecord();
            }
        }

        private void MuteRemoteClientsToggleValueChanged(bool status)
        {
            listener.SetMuteStatus(status);
        }

        private void DebugEchoToggleValueChanged(bool status)
        {
            recorder.debugEcho = status;
        }

        private void ReliableTransmissionToggleValueChanged(bool status)
        {
            recorder.reliableTransmission = status;
        }

        private void MicrophonesDropdownValueChanged(int index)
        {
            if(index < Microphone.devices.Length)
                recorder.SetMicrophone(Microphone.devices[index]);
        }

        private class RemoteSpeakerItem
        {
            private GameObject _selfObject;

            private Text _speakerNameText;

            public Speaker Speaker { get; private set; }

            public RemoteSpeakerItem(Transform parent, GameObject prefab, Speaker speaker)
            {
                Speaker = speaker;
                _selfObject = Instantiate(prefab, parent, false);
                _speakerNameText = _selfObject.transform.Find("Text").GetComponent<Text>();
                _speakerNameText.text = Speaker.Name;
            }

            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}