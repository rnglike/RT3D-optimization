

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SendData : MonoBehaviour
{   
    
    public WS_Client wsClient;
    public TMP_InputField inputTextName;
    public TMP_InputField inputTextRoom;
    public GameObject canvasActive;
    public GameObject canvasInactive;
    public GameObject wsGameObject;

    public Button startButton;
    public TextMeshProUGUI waitMessageText;

    private bool allPlayersReady = false;



    void Start()
    {
        if (wsGameObject == null)
        {
            Debug.LogWarning("GameObject do WS não foi acessado");
            return;
        }

        wsClient = wsGameObject.GetComponent<WS_Client>();

        if (wsClient == null)
        {
            Debug.LogWarning("O Componente do WS não foi encontrado no GameObject");
        }

        startButton.onClick.AddListener(OnStartScene);

    }

    public void OnStartScene()
    {
        string name = inputTextName.text;
        string room = 1.ToString();

        Debug.Log("Name: " + name);
        Debug.Log("Room Number: " + room);

        WS_Client.Message message = new WS_Client.Message
        {
            action = "start",
            name = name,
            room = room
        };

        string jsonData = JsonUtility.ToJson(message);
        wsClient.WebSocketInstance.Send(jsonData);
        Debug.Log("Mensagem enviada para mudar de cena: " + jsonData);

                // Desativa o botão de iniciar e mostra a mensagem de espera
        startButton.interactable = false;
        waitMessageText.gameObject.SetActive(true);

    }

    public void OnConnect()
    {
        string name = inputTextName.text;
        string room = 1.ToString();

        Debug.Log("Name: " + name);
        Debug.Log("Room Number: " + room);

        WS_Client.Message message = new WS_Client.Message
        {
            action = "connect",
            name = name,
            room = room
        };

        string jsonData = JsonUtility.ToJson(message);
        wsClient.WebSocketInstance.Send(jsonData);
        Debug.Log("Mensagem enviada para mudar de cena: " + jsonData);
    }

    public void CreateRoom()
    {
        string name = inputTextName.text;
        string room = 1.ToString();

        Debug.Log("Name: " + name);
        Debug.Log("Room Number: " + room);

        WS_Client.Message message = new WS_Client.Message
        {
            action = "new_room",
            name = name,
            room = room
        };

        string jsonData = JsonUtility.ToJson(message);
        wsClient.WebSocketInstance.Send(jsonData);
        Debug.Log("Mensagem enviada para mudar de cena: " + jsonData);
    }

    public void ChangeCanvasToCreated()
    { 
        canvasActive.SetActive(true);                     
        canvasInactive.SetActive(false);
    }


    public void exitGame()
    {
        Application.Quit();
    }


        public void AllPlayersReady()
    {
        allPlayersReady = true;

        if (allPlayersReady)
        {
            OnStartScene();
        }
        else
        {
            waitMessageText.gameObject.SetActive(true);
        }
    }


    [System.Serializable]
public class Message
{
    public string action;
    public string name; // Adicionei essa propriedade para representar o nome do jogador, como visto em algumas mensagens.
    public string room; // Adicionei essa propriedade para representar o número da sala, como visto em algumas mensagens.
}

}
