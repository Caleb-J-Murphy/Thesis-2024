using OpenAI;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Image recordButton;
        [SerializeField] private Color onColor;
        [SerializeField] private Color offColor;
        [SerializeField] private Dropdown dropdown;
        
        private readonly string fileName = "output.wav";
        [SerializeField] private readonly int duration = 5;

        [SerializeField] private ChatGPT chatGpt;
        
        private AudioClip clip;
        private bool isRecording;
        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            Debug.Log("Index is:" + index);
            dropdown.SetValueWithoutNotify(index);
            
            #endif
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        public void ToggleRecording() {
            Debug.Log($"Are we recording: {isRecording}");
            if (isRecording) {
                EndRecording();
            } else {
                StartRecording();
            }
        }
        
        private void StartRecording()
        {
            Debug.Log("Recording started");
            isRecording = true;
            recordButton.color = onColor;


            #if !UNITY_WEBGL
            string defaultMic = Microphone.devices[0];
            Debug.Log("Using microphone: " + defaultMic);
            Debug.Log("We are making the clip");
            clip = Microphone.Start(defaultMic, false, duration, 44100);
            #endif
        }

        private async void EndRecording()
        {
            Debug.Log("Ended recording");
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            #if !UNITY_WEBGL
            Microphone.End(dropdown.options[index].text);
            #endif

            Debug.Log("Filename = " + fileName);
            byte[] data = SaveWav.Save(fileName, clip);
            
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            AddMessageToFile("\n" + res.Text);
            isRecording = false;
            recordButton.color = offColor;
        }

        private void AddMessageToFile(string message)
        {
            //Send the reply to ChatGPT
            if (!chatGpt)
            {
                Debug.LogError("ChatGPT is null");
                return;
            }
            Debug.Log($"Sending {message} to chatGPT");
            chatGpt.SendReply(message);
        }
    }
}
