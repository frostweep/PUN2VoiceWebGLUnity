using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace FrostweepGames.PhotonWebVoiceWrapper
{
    public class PhotonConnector : MonoBehaviour
    {
        public static PhotonConnector Instance { get; private set; }

        public Lobby lobby;

        public Listener listener;
        public Recorder recorder;

        public byte eventId = 199;

        private void Awake()
        {
            Instance = this;

            lobby.ConnectedEvent += ConnectedEventHandler;
            lobby.RoomListUpdatedEvent += RoomListUpdatedEventHandler;
            lobby.JoinedRoomEvent += JoinedRoomEventHandler;

            PhotonNetwork.NetworkingClient.EventReceived += NetworkEventReceivedHandler;
        }

        private void OnDestroy()
        {
            Instance = null;

            lobby.ConnectedEvent -= ConnectedEventHandler;
            lobby.RoomListUpdatedEvent -= RoomListUpdatedEventHandler;
            lobby.JoinedRoomEvent -= JoinedRoomEventHandler;

            PhotonNetwork.NetworkingClient.EventReceived -= NetworkEventReceivedHandler;
        }

        private void Start()
        {
            lobby.Connect((SystemInfo.deviceUniqueIdentifier + Random.Range(0, 9999999)).GetHashCode().ToString());
        }

        private void ConnectedEventHandler()
        {
            lobby.JoinLobby();
        }

        private void RoomListUpdatedEventHandler()
        {
            if(lobby.Rooms.Count == 0)
            {
                lobby.CreateRoom();
            }
            else
            {
                lobby.JoinRoom(lobby.Rooms[0].Name);
            }
        }

        private void JoinedRoomEventHandler()
        {
            listener.StartListen();
            recorder.readyToRecord = true;
        }

        public void SendDataOverNetwork(byte[] array)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(eventId, array, raiseEventOptions, sendOptions);
        }

        public void NetworkEventReceivedHandler(EventData photonEvent)
        {
            if (photonEvent.Code == 0)
            {
                listener.HandleRawData((byte[])photonEvent.CustomData);
            }
        }
    }
}