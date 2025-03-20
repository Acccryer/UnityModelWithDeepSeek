using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NPCInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeepSeekDialogueManager dialogueManager;
    [SerializeField] private InputField inputField;
    [SerializeField] private Text dialogueText;   

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果速度

    private string characterName;

    void Start()
    {
        characterName = dialogueManager.npcCharacter.name;
        inputField.onSubmit.AddListener((text) =>
        {
            dialogueManager.SendDialogueRequest(text, HandleAIResponse);
        });
    }

    private void HandleAIResponse(string response, bool success)
    {
        StartCoroutine(TypewriterEffect(success ? characterName + ": " + response : characterName + ":（通讯中断）"));
    }

    private IEnumerator TypewriterEffect(string text)
    {
        string currentText = "";
        foreach (char c in text)
        {
            currentText += c;
            dialogueText.text = currentText;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}