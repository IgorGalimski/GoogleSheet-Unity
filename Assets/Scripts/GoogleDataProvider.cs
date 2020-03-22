using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Data;
using Google.GData.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    [ExecuteInEditMode]
    public class GoogleDataProvider : MonoBehaviour
    {
        //TBD
        /*
         *Load tables +
         *Load spreadsheets +
         *Clear +
         *Create +
         *Set value
         *Delete +
         *Save
         */
        
        private GoogleDataStorage _googleDataProvider;

        public ICollection<GoogleSpreadsheet> GoogleSpreadsheets { get; private set; } = new List<GoogleSpreadsheet>();

        public GoogleSpreadsheet this[string spreadsheetName]
        {
            get { return GoogleSpreadsheets.FirstOrDefault(item => item.Name.Equals(spreadsheetName)); }
        }

        public async void Start()
        {
            var str = File.ReadAllText(Application.dataPath + "/test.txt");

            //return;
            await Init();

            await LoadSpreadsheets();

            var d = this["TestTable"];
            await d.LoadGoogleSheets();
            
            Debug.LogError(d.GoogleSheets.Count);
            
            await d.DeleteGoogleSheets(new List<int>()
            {
                553996091
            });
            
            Debug.LogError(d.GoogleSheets.Count);

            var spreadSheet = d["test_list"];
            spreadSheet["A1"].Value = "test" + DateTime.Now.ToLongTimeString();

            await d.Save();
        }

        public async Task Init()
        {
            var loadOperation = Resources.LoadAsync<GoogleDataStorage>("GoogleDataStorage");
            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }

            _googleDataProvider = (GoogleDataStorage) loadOperation.asset;
        }

        public async Task LoadSpreadsheets()
        {
            await _googleDataProvider.RefreshAccessTokenIfExpires();
            
            GoogleSpreadsheets.Clear();

            var urlBuilder = URLBuilder.GetSpreadsheets().AddOrderBy("createdTime").
                            AddRequest("mimeType = 'application/vnd.google-apps.spreadsheet' and 'me' in owners and trashed = false");

            Utils.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _googleDataProvider.AccessToken);
            using (var response = await Utils.HttpClient.GetAsync(urlBuilder.GetURL()))
            {
                var content = await response.Content.ReadAsStringAsync();
                
                var jContainer = JsonConvert.DeserializeObject(content) as JContainer;

                foreach (var jToken in jContainer)
                {
                    var jProperty = jToken as JProperty;

                    if (jProperty.Name == "files")
                    {
                        foreach (var file in jProperty.Values())
                        {
                            var fileInfo = file as JObject;

                            GoogleSpreadsheets.Add(new GoogleSpreadsheet(fileInfo["id"].ToString(), fileInfo["name"].ToString(), _googleDataProvider));
                        }
                    }
                }
            }
        }
    }
}