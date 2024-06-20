using OpenAI;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private Text message;
        [SerializeField] private Dropdown dropdown;
        
        private readonly string fileName = "output.wav";
        [SerializeField] private readonly int duration = 5;
        
        private AudioClip clip;
        private bool isRecording;
        private float time;
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
            recordButton.GetComponent<Image>().color = Color.blue;

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            #if !UNITY_WEBGL
            Debug.Log("We are making the clip");
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            #endif
        }

        private async void EndRecording()
        {
            recordButton.enabled = false;
            message.text = "Transcripting...";
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

            progressBar.fillAmount = 0;
            message.text = res.Text;
            Debug.Log(message.text);
            recordButton.enabled = true;
            isRecording = false;
            recordButton.GetComponent<Image>().color = Color.white;
        }

        private void Update()
        {
            // if (isRecording)
            // {
            //     time += Time.deltaTime;
            //     progressBar.fillAmount = time / duration;
                
            //     if (time >= duration)
            //     {
            //         time = 0;
            //         isRecording = false;
            //         EndRecording();
            //     }
            // }
        }
    }
}
