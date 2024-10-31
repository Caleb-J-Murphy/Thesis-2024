using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        LevelController levelController = LevelController.Instance;
        [SerializeField] private string chatLogsFilePath;
        [SerializeField] private TextAsset toChatGPT;
        [SerializeField] private TextAsset responseRequirements;
        //Includes the solution to the level.
        [SerializeField] private TextAsset levelExplanation;
        [SerializeField] private InputProcessor inputProcessor;

        [SerializeField] private TextToSpeech textToSpeech;

        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Say 'Something is broken, look in ChatGPT.cs file for more details'";

        private void Awake()
        {
            if (toChatGPT && responseRequirements && levelExplanation) prompt = responseRequirements.text + levelExplanation.text + toChatGPT.text;
        }

        //Need to add each reply to a text file to be added in the response
        public void AddMessegeToChatLog(string messege)
        {
            if (levelController == null || levelController.isUsingAI)
            {
                bool fileExists = System.IO.File.Exists(chatLogsFilePath);
                bool isFileEmpty = fileExists && new FileInfo(chatLogsFilePath).Length == 0;


                using (StreamWriter writer = new StreamWriter(chatLogsFilePath, true))
                {
                    // Write the header only if the file is empty
                    if (!fileExists || isFileEmpty)
                    {
                        writer.WriteLine("ChatGPT chat logs");
                    }
                    if (levelController)
                    {
                        writer.WriteLine("User ID: " + levelController.currentUserID + " -> " + messege);
                    } else
                    {
                        writer.WriteLine("User ID: None" + " -> " + messege);
                    }
                    
                    Debug.Log(messege);
                }
            }
            
        }

        public async void SendReply(string reply)
        {
            
            Debug.Log($"Creating reply: {reply} to send to chatGPT");
            AddMessegeToChatLog(reply);
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = reply
            };

            if (inputProcessor)
            {
                //Add the previous attempt to the response.
                prompt += "\n" + "This is my previous attempt at the problem:\n" + inputProcessor.getPreviousAttempt();
            }
            if (messages.Count == 0) newMessage.Content = prompt + "\n" + reply;
            
            messages.Add(newMessage);

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4o",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                messages.Add(message);

                if (textToSpeech != null)
                {
                    Debug.Log($"Sending {message.Content} to be spoken in text to speech");
                    textToSpeech.MessageToSpeak(message.Content);
                    AddMessegeToChatLog("Response: ==> " + message.Content);
                }
                
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }
}
