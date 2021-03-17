// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbodyView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2016 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize rigidbodies via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class helps you to synchronize the velocities of a physics RigidBody.
/// Note that only the velocities are synchronized and because Unitys physics
/// engine is not deterministic (ie. the results aren't always the same on all
/// computers) - the actual positions of the objects may go out of sync. If you
/// want to have the position of this object the same on all clients, you should
/// also add a PhotonTransformView to synchronize the position.
/// Simply add the component to your GameObject and make sure that
/// the PhotonRigidbodyView is added to the list of observed components
/// </summary>
[RequireComponent(typeof(PhotonView))]
[AddComponentMenu("Photon Networking/Photon Agora Client Role View")]
public class PhotonAgoraClientRoleView : MonoBehaviour, IPunObservable
{
    bool FirstPacketReceived = false;
    bool LastPacketReceived = false;

    void Awake()
    {

    }
    
    [PunRPC]
    public void SetClientRoleTag(agora_gaming_rtc.CLIENT_ROLE role)
    {
        transform.SetParent(GameObject.Find("AgoraClientRoles").transform);
        if (role == agora_gaming_rtc.CLIENT_ROLE.AUDIENCE)
            gameObject.tag = "crAudience";
        else if (role == agora_gaming_rtc.CLIENT_ROLE.BROADCASTER)
            gameObject.tag = "crBroadcaster";
    }

    void Update()
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (LastPacketReceived)
            return;

        if (stream.isWriting == true)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(gameObject.name);
            stream.SendNext(bytes);
            if (gameObject.tag == "crAudience")
                stream.SendNext(Convert.ToByte(0));
            else if (gameObject.tag == "crBroadcaster")
                stream.SendNext(Convert.ToByte(1));
        }
        else
        {
            if (!FirstPacketReceived)
            {
                FirstPacketReceived = true;    
            } else
            {
                string name = System.Text.Encoding.UTF8.GetString((byte[])stream.ReceiveNext());
                gameObject.name = name;

                byte b = (byte)stream.ReceiveNext();
                if (b == 0)
                    gameObject.tag = "crAudience";
                else if (b == 1)
                    gameObject.tag = "crBroadcaster";
            }
        }
    }
}