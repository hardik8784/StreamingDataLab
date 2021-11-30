using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartySharingManager : MonoBehaviour
{

    public GameObject SharePartyButton, SharingRoomNameInputField, JoinSharingRoomButton;

    NetworkedClient _NetwroekdClient;
    // Start is called before the first frame update
    void Start()
    {
        SharePartyButton.GetComponent<Button>().onClick.AddListener(SharePartyButtonPressed);
        JoinSharingRoomButton.GetComponent<Button>().onClick.AddListener(JoinSharingRoomButtonPressed);

        _NetwroekdClient = GetComponent<NetworkedClient>();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SharePartyButtonPressed()
    {
        //Debug.Log("SharePartyButtonPressed");
    }

    public void JoinSharingRoomButtonPressed()
    {
        string Name = SharingRoomNameInputField.GetComponent<InputField>().text;
        //Debug.Log("JoinSharingRoomButtonPressed");
        _NetwroekdClient.SendMessageToHost(ClientToServerSignifiers.JoinSharingRoom + "," + Name);
    }
}
