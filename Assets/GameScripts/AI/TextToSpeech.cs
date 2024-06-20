using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    // Start is called before the first frame update
    private async void Start()
    {
        var credentials = new BasicAWSCredentials("AKIA3FLD6FTR4EUXNU2M", "flwUC1ewT5Ft0A97/18feWXEOLVa3XbXx+9VvIs7");
        var client = new AmazonPollyClient(credentials, RegionEndpoint.APSouth1);

        var request = new SynthesizeSpeechRequest()
        {
            Text = "Testing amazon polly in unity!",
            Engine = Engine.Neural,
            VoiceId = VoiceId.Aria,
            OutputFormat = OutputFormat.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(request);

        WriteIntoFile(response.AudioStream);

        using (var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/textToSpeech.mp3", AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }

            var clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;

            audioSource.Play();
        }
    }

    private void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/textToSpeech.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}
