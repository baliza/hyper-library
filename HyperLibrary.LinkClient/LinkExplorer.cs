﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HyperLibrary.LinkClient
{
    public class LinkExplorer
    {
        private readonly LinkClient _libraryLinkClient;

        public LinkExplorer(Uri serverEndpoint)
        {
            _libraryLinkClient = new LinkClient(serverEndpoint);
        }

        public async Task Explore()
        {
            Console.WriteLine();
            Console.WriteLine("**************** Welcome to the Library ****************");
            dynamic root = await _libraryLinkClient.GetRoot();
            var link = SelectAvailableLinks(root);
            await FollowYourNose(link);
        }

        private async Task FollowYourNose(dynamic linkToFollow)
        {
            Console.WriteLine();
            Console.WriteLine("**** Follow your nose '{0}' ****",linkToFollow.Rel.ToString());
            var response = await _libraryLinkClient.Follow(linkToFollow);
            var link = SelectAvailableLinks(response);
            await FollowYourNose(link);
        }


        private dynamic SelectAvailableLinks(dynamic response)
        {
            int optionIndex = 0;
            var allLinks = FindAllLinks(response);
            foreach (var link in allLinks)
            {
                Console.WriteLine("{0} - {1} -{2} {3}", optionIndex, link.SelectToken("Rel"), link.SelectToken("Name"), link.SelectToken("Uri"));
                optionIndex++;
            }

            Console.WriteLine();
            Console.WriteLine("What would you like to do?");
            int option = GetIntegerOption(optionIndex);
            return allLinks[option];
        }

        private static List<dynamic> FindAllLinks(dynamic response)
        {
            var jObject = (JObject) response;

            var allLinks = new List<dynamic>();
            WalkNode(jObject, n =>
            {
                JToken token = n["Links"];
                if (token != null && token.Type == JTokenType.Array)
                {
                    var tokenArray = (JArray) token;
                    allLinks.AddRange((dynamic) tokenArray);
                }
            });
            return allLinks;
        }


        private static void WalkNode(JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    WalkNode(child, action);
                }
            }
            else if (node.Type == JTokenType.Object)
            {
                action((JObject) node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    WalkNode(child.Value, action);
                }
            }
        }

        private static int GetIntegerOption(int maxOption)
        {
            string enteredText = Console.ReadLine();
            try
            {
                int option = Convert.ToInt32(enteredText);
                if (option < 0 || option > maxOption)
                {
                    Console.WriteLine("What was that Captain? Give us any number from 0 to {0}....", maxOption - 1);
                    return GetIntegerOption(maxOption);
                }
                return option;

            }
            catch (Exception)
            {
                Console.WriteLine("What was that Captain? Give us a number please....");
                return GetIntegerOption(maxOption);
            }
        }
    }
}