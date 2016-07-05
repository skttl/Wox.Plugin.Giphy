using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace Wox.Plugin.Giphy
{
    internal class Main : IPlugin
    {
        public Main()
        {
        }

        public void Init(PluginInitContext context)
        {
        }

        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public string SlugToFriendlyName(string slug)
        {
            var words = slug.Split('-');
            return string.Join(" ", words.Take(words.Length - 1).Select(x => FirstLetterToUpper(x)));
        }

        public string AuthorName(string author)
        {
            return string.IsNullOrEmpty(author) || string.IsNullOrWhiteSpace(author) ? "N/A" : author;
        }

        public List<Result> Query(Query query)
        {

            WebClient client = new WebClient();
            var rawResponse = client.DownloadString("https://api.giphy.com/v1/gifs/search?limit=5&q=" + query.Search + "&api_key=dc6zaTOxFJmzC");
            JObject response = JObject.Parse(rawResponse);
            JArray photos = (JArray)response["data"];

            var result = new List<Result>();

            foreach (var photo in photos)
            {
                client.DownloadFile(new Uri(photo["images"]["fixed_width_small"]["url"].ToString()), photo["slug"] + ".gif");
                result.Add(new Result()
                {
                    Title = SlugToFriendlyName(photo["slug"].ToString()),
                    SubTitle = "By: " + AuthorName(photo["username"].ToString()) + " - URL: " + photo["url"],
                    IcoPath = AppDomain.CurrentDomain.BaseDirectory + photo["slug"] + ".gif",
                    Action = c =>
                    {
                        try
                        {
                            Clipboard.SetText(photo["embed_url"].ToString());
                            return true;
                        }
                        catch (ExternalException e)
                        {
                            MessageBox.Show("Copy failed, please try later");
                            return false;
                        }
                    }
                });
            }

            result.Add(new Result()
            {
                Title = "Search for '" + query.Search + "' on giphy.com",
                SubTitle = "Opens a browser with the search results",
                IcoPath = "giphy.png",
                Action = c =>
                {
                    try
                    {
                        Process.Start("http://giphy.com/search/" + query.Search);
                        return true;
                    }
                    catch (ExternalException e)
                    {
                        MessageBox.Show("Open failed, please try later");
                        return false;
                    }
                }
            });

            return result;
        }
    }
}