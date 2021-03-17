using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutManager : MonoBehaviour
{
    [HideInInspector]
    public AgoraInterface m_agoraInterface;
    public GameObject m_UserElementPrefab;

    public int UserNumber = -1;

    [Header("Fields and Objects")]
    public InputField NicknameField;
    public InputField NicknameField2;
    public GameObject InputNameOnly;
    public GameObject InputPassword;
    public GameObject IncorrectPassword;

    [Header("Call Images")]
    public GameObject m_EndCallImg;
    public GameObject m_StartCallImg;

    [Header("Mute Images")]
    public GameObject m_NotMuteImg;
    public GameObject m_MuteImg;

    //Tells Agora I want to register my username
    public void Register()
    {
        string username = NicknameField.GetComponent<InputField>().text + NicknameField2.text;
        m_agoraInterface.RegisterNewUser(username);
    }

    public void UpdateUsername(string uid_string, string username)
    {
        if (GameObject.Find(uid_string) != null)
            GameObject.Find(uid_string).GetComponentInChildren<Text>().text = username;
    }

    //Tells Agora I want to leave Channel
    public void LeaveAgoraChannel()
    {
        m_EndCallImg.SetActive(false);
        m_StartCallImg.SetActive(true);
        m_agoraInterface.LeaveChannel();
    }


    //Gets called by Agora Interface when a new user joins the channel
    public void AddNewUserElement(string go_name, string user_name)
    {
        PhotonManager.instance.ClientRoleSanityCheck();

        GameObject go = Instantiate(m_UserElementPrefab);
        go.name = go_name;
        go.GetComponentInChildren<Text>().text = user_name;
        GameObject userBar = GameObject.Find("UserBar");
        go.transform.SetParent(userBar.transform);
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;

    }
    //Gets called by Agora Interface when a new user joins the channel
    public void AddNewUserElement(string go_name, string user_name, bool self)
    {
        GameObject clientRole = PhotonNetwork.Instantiate("ClientRole", Vector3.zero, Quaternion.identity, 0);
        PhotonView pv = PhotonView.Get(clientRole);
        if (pv.isMine)
            pv.RPC("SetClientRoleTag", PhotonTargets.AllBuffered, agora_gaming_rtc.CLIENT_ROLE.AUDIENCE);
        else
            clientRole.GetComponent<PhotonAgoraClientRoleView>().SetClientRoleTag(agora_gaming_rtc.CLIENT_ROLE.AUDIENCE);
        PhotonManager.m_ClientRole = clientRole;
        PhotonManager.m_ClientRole.name = user_name;

        GameObject go = Instantiate(m_UserElementPrefab);
        go.name = go_name;
        go.GetComponentInChildren<Text>().text = user_name;
        GameObject userBar = GameObject.Find("UserBar");
        go.transform.SetParent(userBar.transform);
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
    }
    IEnumerator cr_AddNewUserElementMute()
    {
        yield return new WaitForSeconds(0.75f);
        //Mute
        m_NotMuteImg.SetActive(false);
        m_MuteImg.SetActive(true);
        m_agoraInterface.ControlMute(true);
        m_agoraInterface.r_Muted = true;
    }

    //Gets called by Agora Interface when a user leaves the channel
    public void RemoveUserElement(string elementName)
    {
        GameObject go = GameObject.Find(elementName);
        Destroy(go);
        UserNumber -= UserNumber - (UserNumber > 0 ? 1 : 0);
    }


    //Removes all user icons from view
    public void removeAllUsers()
    {
        GameObject UserBar = GameObject.Find("UserBar");
        int childCount = UserBar.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(UserBar.transform.GetChild(i).gameObject);
        }
        UserNumber = -1;
    }


    //Agora Interface calls this function when detecting volume
    public void modifySoundIcon(string go_name, uint volume)
    {
        GameObject g_quiet = GameObject.Find(go_name + "/" + "QuietImage");
        GameObject g_speaking = GameObject.Find(go_name + "/" + "SpeakingImage");
        if (g_quiet == null || g_speaking == null)
            return;

        Image quiet = GameObject.Find(go_name + "/" + "QuietImage").GetComponent<Image>();
        Image speaking = GameObject.Find(go_name + "/" + "SpeakingImage").GetComponent<Image>();

        if (volume <= 10)
        {
            quiet.enabled = true;
            speaking.enabled = false;
        }
        else if (volume > 10)
        {
            quiet.enabled = false;
            speaking.enabled = true;
        }
    }

    public void TestPassword()
    {
        if (!DioramaManager.instance)
            return;
        if (DioramaManager.DioramaReferencePoint == null)
            return;
        if (DioramaManager.DioramaReferencePoint.transform.childCount <= 0)
            return;

        if (m_agoraInterface.AgoraPasswordInputted != m_agoraInterface.AgoraPassword)
        {
            IncorrectPassword.SetActive(true);
            return;
        }
        DioramaManager.instance.JoinAgoraRoomProjectId();
        InputNameOnly.SetActive(false);
        InputPassword.SetActive(false);
    }
    public void ToggleCall()
    {


        if (m_EndCallImg.activeSelf == true)
        {
            if (PhotonManager.m_ClientRole != null)
                PhotonNetwork.Destroy(PhotonManager.m_ClientRole);
            AgoraInterface.LeaveRoom();
            m_StartCallImg.SetActive(true);
            m_EndCallImg.SetActive(false);
        }
        else
        {
            if (!DioramaManager.instance)
                return;
            if (string.IsNullOrEmpty(DioramaManager.instance.LoadedProject.name))
                return;

            string loginUsername = "";
            if (GameObject.Find("SurveyObject"))
                loginUsername = GameObject.Find("SurveyObject").GetComponent<MenuManagement>().username;
            if (!string.IsNullOrEmpty(loginUsername))
            {
                NicknameField.text = loginUsername;
                InputPassword.SetActive(false);
                DioramaManager.instance.JoinAgoraRoomProjectId();
                m_StartCallImg.SetActive(false);
                m_EndCallImg.SetActive(true);
                return;
            }
            else if (!string.IsNullOrEmpty(AdminController.Instance.SSOAuthTOKEN) && string.IsNullOrEmpty(NicknameField.text) && string.IsNullOrEmpty(NicknameField2.text))
            {
                InputNameOnly.SetActive(!InputNameOnly.activeSelf);
                return;
            }
            else if (m_agoraInterface.AgoraPasswordInputted != m_agoraInterface.AgoraPassword)
            {
                InputPassword.SetActive(!InputPassword.activeSelf);
                return;
            }
            else
            {
                InputPassword.SetActive(false);

            }
            DioramaManager.instance.JoinAgoraRoomProjectId();
            m_StartCallImg.SetActive(false);
            m_EndCallImg.SetActive(true);
        }
    }

    public void SetMute(bool value)
    {
        m_NotMuteImg.SetActive(!value);
        m_MuteImg.SetActive(value);
        m_agoraInterface.ControlMute(value);
        m_agoraInterface.r_Muted = value;
        if (PhotonManager.m_ClientRole != null)
            if (value)
                PhotonManager.m_ClientRole.tag = "crAudience";
            else
                PhotonManager.m_ClientRole.tag = "crBroadcaster";
        if (value)
            m_agoraInterface.SetClientRole(agora_gaming_rtc.CLIENT_ROLE.AUDIENCE);
        else
            m_agoraInterface.SetClientRole(agora_gaming_rtc.CLIENT_ROLE.BROADCASTER);

    }

    //Tells Agora I want to mute or unmute myseld
    public void ToggleMute()
    {
        bool StayMuted = false;
        GameObject count = GameObject.Find("AgoraClientRoles");
        int broadcasters = 0;
        foreach (Transform t in count.transform)
            if (t.tag == "crBroadcaster")
                broadcasters++;
        if (broadcasters >= 7)
            StayMuted = true;

        if (m_EndCallImg.activeSelf == false)
            return;
        if (m_NotMuteImg.activeSelf == true || StayMuted)
        {
            //Mute
            m_NotMuteImg.SetActive(false);
            m_MuteImg.SetActive(true);
            m_agoraInterface.ControlMute(true);
            m_agoraInterface.r_Muted = true;
            if (PhotonManager.m_ClientRole != null)
                PhotonManager.m_ClientRole.tag = "crAudience";
            m_agoraInterface.SetClientRole(agora_gaming_rtc.CLIENT_ROLE.AUDIENCE);

            GameObject[] gs = GameObject.FindGameObjectsWithTag("VideoInTriangle");
            foreach (GameObject g in gs)
            {
                g.transform.Find("QuietImage").GetComponent<Image>().enabled = true;
                g.transform.Find("SpeakingImage").GetComponent<Image>().enabled = false;
            }
        }
        else
        {
            //UnMute
            m_NotMuteImg.SetActive(true);
            m_MuteImg.SetActive(false);
            m_agoraInterface.ControlMute(false);
            m_agoraInterface.r_Muted = false;
            if (PhotonManager.m_ClientRole != null)
                PhotonManager.m_ClientRole.tag = "crBroadcaster";
            m_agoraInterface.SetClientRole(agora_gaming_rtc.CLIENT_ROLE.BROADCASTER);
        }
    }


     
}
