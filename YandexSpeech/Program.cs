using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


//curl -d "{\"yandexPassportOauthToken\":\"<OAuth-token>\"}" "https://iam.api.cloud.yandex.net/iam/v1/tokens"
namespace YandexSpeech
{
        class Program
        {
            static void Main()
            {
                Tts().GetAwaiter().GetResult();
            }

            static async Task Tts()
            {
                const string iamToken = "CggVAgAAABoBMxKABLb95ZSVKyRMObe_f9eAc9AGCQt7ghvJLPt0lTHY39cniMdbkVfBoEz57RTp6MznCA4hpmaUwe67l-7CVs3V-xBeiRYC1luaam5C95vXem958AzfZ0KNuzjXd8URKwDh_6NPxI1w9K5Cy-97UAnUz3If7smRKIsjatF6bJZRxsYj7lyEuSGZL6HZO8ihm1GPsdlbr4h-MNGPQx4u6N79_iTjR-_N9fAYnfXANBeuDK9Fz7t-XXVSEjkhEa51A4lItMU47e7BDrzOrKWkoOxwMl7RdAjnb3A8SbWP18NgHNurfTQhHCZ5H4YwtxfhXtkWWby60SA4xeNmgCxp0lAdW2rCHohsDNtwV9Ni-f4Rtn_8KDXdfVD-pq5bRZyMRHXu0CtRYts0c2nX3fA0aus3uDxWwh1rC4gBJb6hu3vJr4r9RGcsb_kkmqt3RbRanbcA4mTe011a2bm6Dt7uRWYeofYOobxr6pcH_meacJ3xm724iTwefjONzvA20BP45coKMZ25vPbWOKv3ajkud7f-mkLWMtMDxNB0Shz35rJ8hl5a6RSr-Qtfo5urbmtWI_SJ2w4shUsHNGBXzBqna_A1wzPK-eo0Cd2kmx-5S6KccIVDd745wczy8CJOcomT22IsPb3dVwocvSomXZ7Ztyvo4AHgvfwuzjN1ODPd-IammNsJGiQQ--HC8AUYu7PF8AUiFgoUYWplcjNuMzltcXMzZmcxdTA4dnA="; // Укажите IAM-токен.
                const string folderId = "b1gouss6lnh884vsl9eq"; // Укажите ID каталога.

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + iamToken);
                var values = new Dictionary<string, string>
      {
        { "text", "Привет!\nЯ Yandex SpeechKit.\nЯ могу превратить любой текст в речь.\nТеперь и в+ы - можете!" },
        { "lang", "ru-RU" },
        { "speed", "1" },
        { "voice", "alena" },
        { "emotion", "neutral" },
        { "folderId", folderId }
      };

            var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes("speech2.ogg", responseBytes);
            }
        }
}
