using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System;
using ExitGames.Client.Photon;

namespace FrostweepGames.WebGLPUNVoice
{
	public class Lobby : MonoBehaviourPunCallbacks, ILobbyCallbacks
	{
		public event Action ConnectedEvent;

		public event Action RoomListUpdatedEvent;

		public event Action JoinedLobbyEvent;

		public event Action JoinedRoomEvent;

		public List<RoomInfo> Rooms { get; private set; }

		public Room CurrentRoom => PhotonNetwork.CurrentRoom;

		public bool Connect(string userId)
		{
			if (PhotonNetwork.IsConnected)
				return true;

			PhotonNetwork.AuthValues = new AuthenticationValues();
			PhotonNetwork.AuthValues.UserId = userId;
			return PhotonNetwork.ConnectUsingSettings();
		}

		public void Disconnect()
		{
			if (!PhotonNetwork.IsConnected)
				return;

			PhotonNetwork.Disconnect();
		}

		public bool JoinLobby()
		{
			if (PhotonNetwork.InLobby)
				return true;

			return PhotonNetwork.JoinLobby();
		}

		public void GetRooms()
		{
			PhotonNetwork.GetCustomRoomList(TypedLobby.Default, string.Empty);
		}

		public void CreateRoom()
		{
			RoomOptions roomOptions = new RoomOptions()
			{
				MaxPlayers = 0,
				IsOpen = true,
				IsVisible = true,
				PublishUserId = false,
				CleanupCacheOnLeave = false,
			};

			PhotonNetwork.CreateRoom("VoiceRoom_" + System.DateTime.Now.Ticks, roomOptions);
		}

		public void JoinRoom(string roomName = null)
		{
			if (PhotonNetwork.InRoom)
				return;

			LeaveLobby();

			if (string.IsNullOrEmpty(roomName))
			{
				PhotonNetwork.JoinRandomRoom();
			}
			else
			{
				PhotonNetwork.JoinRoom(roomName);
			}
		}

		public void LeaveRoom()
		{
			if (!PhotonNetwork.InRoom)
				return;

			PhotonNetwork.LeaveRoom();
		}

		public void LeaveLobby()
		{
			if (!PhotonNetwork.InLobby)
				return;

			PhotonNetwork.LeaveLobby();
		}
		public override void OnRoomListUpdate(List<RoomInfo> roomList)
		{
			base.OnRoomListUpdate(roomList);

			Rooms = roomList;

			RoomListUpdatedEvent?.Invoke();
		}

		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();

			JoinedRoomEvent?.Invoke();
		}

		public override void OnJoinedLobby()
		{
			base.OnJoinedLobby();

			JoinedLobbyEvent?.Invoke();
		}

		public override void OnConnectedToMaster()
		{
			base.OnConnectedToMaster();

			ConnectedEvent?.Invoke();
		}
	}
}