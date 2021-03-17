using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : Photon.PunBehaviour
{
    public static PhotonManager instance = null;

    public static GameObject m_ClientRole = null;

    string currentID;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        PhotonNetwork.ConnectUsingSettings("1");
    }

    public void DeInitialize()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public IEnumerator ConnectToPresentationPhotonRoom(string ID)
    {
        currentID = ID;
        while (!PhotonNetwork.connected)
            yield return new WaitForSeconds(1.5f);
        if (PhotonNetwork.inRoom)
            PhotonNetwork.LeaveRoom();
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.JoinRoom("CommonRoom-" + currentID);
        Debug.Log("Connecting to Room CommonRoom-" + currentID);
    }
    
    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom("CommonRoom-" + currentID);
    }

    public void ClientRoleSanityCheck()
    {
        // Have to be in a call to do a sanity check
        if (AgoraInterface.instance.layoutManager.m_EndCallImg.activeSelf)
            StartCoroutine(cr_ClientRoleSanityCheck());
    }
    IEnumerator cr_ClientRoleSanityCheck()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings("1");
            while (!PhotonNetwork.connected)
                yield return new WaitForSeconds(1.5f);
            PhotonNetwork.JoinRoom("CommonRoom-" + currentID);
            while (!PhotonNetwork.inRoom)
                yield return new WaitForSeconds(1f);
        }
        if (m_ClientRole == null)
        {
            GameObject clientRole = PhotonNetwork.Instantiate("ClientRole", Vector3.zero, Quaternion.identity, 0);
            PhotonView pv = PhotonView.Get(clientRole);
            if (pv.isMine)
            {
                if (AgoraInterface.instance.r_Muted)
                    pv.RPC("SetClientRoleTag", PhotonTargets.AllBuffered, agora_gaming_rtc.CLIENT_ROLE.AUDIENCE);
                else
                    pv.RPC("SetClientRoleTag", PhotonTargets.AllBuffered, agora_gaming_rtc.CLIENT_ROLE.BROADCASTER);
            }
            else
            {
                if (AgoraInterface.instance.r_Muted)
                    clientRole.GetComponent<PhotonAgoraClientRoleView>().SetClientRoleTag(agora_gaming_rtc.CLIENT_ROLE.AUDIENCE);
                else
                    clientRole.GetComponent<PhotonAgoraClientRoleView>().SetClientRoleTag(agora_gaming_rtc.CLIENT_ROLE.BROADCASTER);
            }
            m_ClientRole = clientRole;
            m_ClientRole.name = AgoraInterface.instance.layoutManager.NicknameField.text + AgoraInterface.instance.layoutManager.NicknameField2.text;
        } else if (m_ClientRole != null)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            ClientRoleSanityCheck();
        }
    }
    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            ClientRoleSanityCheck();
        }
    }

    private void OnApplicationQuit()
    {
        DeInitialize();
    }
}
