using UnityEditor;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


namespace UnityLibrary
{
    public class OpenAI : MonoBehaviour
    {
        string CODE;
        public void Generate()
        {
            var filePath = "Assets/" + Classname + ".cs";
            File.WriteAllText(filePath, CODE);
            AssetDatabase.Refresh();
        }
        const string url = "https://api.openai.com/v1/completions";

        //ドキュメント
        //https://platform.openai.com/docs/models/gpt-3-5
        //gpt-3.5-turboは小規模で速度が速く、チャット用に最適化されている（ChatGPTがこれを使用している）
        //text-davinci-003はあらゆる言語タスクを、より高品質に、より長く出力し、一貫した指示通りに実行することが可能
        //code-davinci-002は特に自然言語からコードへの変換に優れている。
        string modelName = "text-davinci-003";
        [SerializeField, Tooltip("ここに発行したAPIキーを入力する")]
        string apiKey = "ここに発行したAPIキーを入力する";
        [SerializeField, Tooltip("ここにコード生成中に表示したいオブジェクトを設定する")]
        GameObject loadingIcon;
        [SerializeField, Tooltip("ここに実装したい内容を記述する")]
        [TextArea(3, 4)]
        string wanttodo;
        [SerializeField, Tooltip("ここにクラス名を記述する")]
        [TextArea(3, 4)]
        string Classname;
        [SerializeField]
        [TextArea(20, 21), Tooltip("ここに生成結果が表示される")]
        string inputResults;




        bool isRunning = false;

        void Start()
        {
            LoadAPIKey();
        }

        public void Execute()
        {
            if (isRunning)
            {
                Debug.LogError("すでに稼働中");
                return;
            }
            isRunning = true;
            loadingIcon.SetActive(true);

            RequestData requestData = new RequestData()
            {
                model = modelName,
                prompt = "Unityで" + wanttodo + "するスクリプトを書いてください。" + "クラス名は" + Classname + "で。usingから記述してください。MonoBehaviourを継承してください。",
                temperature = 0.7f,
                max_tokens = 1000,//モデルごとにトークンのMAXは異なる。詳しくはNote参照してください
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            string jsonData = JsonUtility.ToJson(requestData);

            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

            UnityWebRequest request = UnityWebRequest.Post(url, jsonData);
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            UnityWebRequestAsyncOperation async = request.SendWebRequest();

            async.completed += (op) =>
            {
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    Debug.Log(request.downloadHandler.text);
                    OpenAIAPI responseData = JsonUtility.FromJson<OpenAIAPI>(request.downloadHandler.text);
                    string generatedText = responseData.choices[0].text.TrimStart('\n').TrimStart('\n');
                    inputResults = generatedText;
                    CODE = inputResults;
                    Generate();
                }
                loadingIcon.SetActive(false);
                isRunning = false;
            };

        }
        public void LoadAPIKey()
        {
            var keypath = Path.Combine(Application.streamingAssetsPath, "secretkey.txt");
            if (File.Exists(keypath) == false)
            {
                Debug.LogError("Apikey missing: " + keypath);
            }
            apiKey = File.ReadAllText(keypath).Trim();
            Debug.Log("API key loaded, len= " + apiKey.Length);
        }
    }
}