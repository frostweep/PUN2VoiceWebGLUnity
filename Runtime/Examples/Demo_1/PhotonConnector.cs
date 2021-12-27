using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace FrostweepGames.WebGLPUNVoice
{
    public class PhotonConnector : MonoBehaviour
    {
        public Lobby lobby;

        private void Awake()
        {
            lobby.ConnectedEvent += ConnectedEventHandler;
            lobby.RoomListUpdatedEvent += RoomListUpdatedEventHandler;
            lobby.JoinedRoomEvent += JoinedRoomEventHandler;
        }

        private void OnDestroy()
        {
            lobby.ConnectedEvent -= ConnectedEventHandler;
            lobby.RoomListUpdatedEvent -= RoomListUpdatedEventHandler;
            lobby.JoinedRoomEvent -= JoinedRoomEventHandler;
        }

        private void Start()
        {
            lobby.Connect((SystemInfo.deviceUniqueIdentifier + Random.Range(0, 9999999)).GetHashCode().ToString());

            Debug.Log("CONNECT TO SERVER");
        }

        private void ConnectedEventHandler()
        {
            lobby.JoinLobby();

            Debug.Log("ConnectedEventHandler");
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
            Debug.Log("JoinedRoomEventHandler");
        }
    }
}