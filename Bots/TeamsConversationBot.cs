// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Modified by Jack Butts for non-commercial purposes (https://www.github.com/buttsj)

namespace WestervilleFoodBot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using WestervilleFoodBot.API;
    using WestervilleFoodBot.Bots;

    public class TeamsConversationBot : TeamsActivityHandler
    {
        private readonly string _appId;
        private readonly string _appPassword;
        private readonly string _googlePlacesApi;
        private readonly string imageUrlBlock = "{{url1}}";
        private readonly string linkUrlBlock = "{{url2}}";
        private readonly string foodBlock = "{{food}}";
        private readonly string ratingBlock = "{{rating}}";
        private string adaptiveCardTemplate = "{\r\n    \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\r\n    \"type\": \"AdaptiveCard\",\r\n    \"version\": \"1.0\",\r\n    \"body\": [\r\n        {\r\n            \"type\": \"Container\",\r\n            \"items\": [\r\n                {\r\n                    \"type\": \"TextBlock\",\r\n                    \"text\": \"Westerville Food of the Day:\",\r\n                    \"weight\": \"Bolder\",\r\n                    \"size\": \"Small\"\r\n                },\r\n                {\r\n                    \"type\": \"ColumnSet\",\r\n                    \"columns\": [\r\n                        {\r\n                            \"type\": \"Column\",\r\n                            \"width\": \"auto\",\r\n                            \"items\": [\r\n                                {\r\n                                    \"size\": \"Small\",\r\n                                    \"style\": \"\",\r\n                                    \"type\": \"Image\",\r\n                                    \"url\": \"{{url1}}\"\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"type\": \"Column\",\r\n                            \"width\": \"stretch\",\r\n                            \"items\": [\r\n                                {\r\n                                    \"type\": \"TextBlock\",\r\n                                    \"text\": \"{{food}}\",\r\n                                    \"weight\": \"Bolder\",\r\n                                    \"wrap\": true,\r\n                                    \"size\": \"Large\"\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"actions\": [\r\n        {\r\n            \"type\": \"Action.OpenUrl\",\r\n            \"title\": \"{{rating}} out of 5 stars\",\r\n            \"card\": {\r\n                \"type\": \"CardAction\",\r\n                \"version\": \"1.0\",\r\n                \"body\": [\r\n                    {\r\n                        \"type\": \"Action.OpenUrl\",\r\n                        \"id\": \"Rating\",\r\n                        \"value\": \"\"\r\n                    }\r\n                ]\r\n            },\r\n            \"url\": \"\\\"\\\"\"\r\n        },\r\n        {\r\n            \"type\": \"Action.OpenUrl\",\r\n            \"title\": \"Let me Google that for you?\",\r\n            \"card\": {\r\n                \"type\": \"CardAction\",\r\n                \"version\": \"1.0\",\r\n                \"body\": [\r\n                    {\r\n                        \"type\": \"Action.OpenUrl\",\r\n                        \"id\": \"comment\"\r\n                    }\r\n                ]\r\n            },\r\n            \"url\": \"{{url2}}\"\r\n        }\r\n    ]\r\n}";

        #region API

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        private ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        #endregion

        /// <summary>
        /// Constructor for class
        /// </summary>
        /// <param name="config">Configurations</param>
        public TeamsConversationBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            _googlePlacesApi = config["GooglePlacesApi"];
        }

        /// <summary>
        /// Message activity triggered
        /// </summary>
        /// <param name="turnContext">Turn context</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            
            // Check the message sent from a user and act accordingly
            switch (turnContext.Activity.Text.Trim())
            {
                case "Find Food":
                    await FindFoodAsync(turnContext, cancellationToken);
                    break;
                case "FindFood":
                    await FindFoodAsync(turnContext, cancellationToken);
                    break;
            }
        }




        /// <summary>
        /// Async task to find food around Westerville OH
        /// </summary>
        /// <param name="turnContext">TurnContext to send a message</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        private async Task FindFoodAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Configuration = new Configuration { BasePath = "https://maps.googleapis.com" };
            ExceptionFactory = Configuration.DefaultExceptionFactory;
            ApiResponse<object> responseObject;
            List<Restaurant> restaurantList = new List<Restaurant>();

            try
            {
                responseObject = MapsApiPlaceNearbysearchJsonGetWithHttpInfo("restaurant", "40.1267,-82.9319", "1000", _googlePlacesApi);
                JObject jResponse = JObject.Parse(responseObject.Data.ToString());
                JArray responseParsed = (JArray)jResponse["results"];

                foreach (JObject innerObject in responseParsed)
                {
                    JToken name;
                    JToken stars;
                    JToken icon;

                    // Only add to Dictionary if it has both a Name and Rating
                    if (innerObject.ContainsKey("name"))
                    {
                        name = innerObject["name"];

                        if (innerObject.ContainsKey("rating"))
                        {
                            stars = innerObject["rating"];
                            
                            if (innerObject.ContainsKey("icon"))
                            {
                                icon = innerObject["icon"];
                                restaurantList.Add(new Restaurant(name, stars, icon));
                            }
                        }
                    }
                }

                // If any food was found, pick one and send it
                if (restaurantList.Count > 0)
                {
                    Random rand = new Random();
                    int randomIndex = rand.Next(0, restaurantList.Count);
                    Restaurant selection = restaurantList[randomIndex];
                    Uri letMeGoogleThat = new Uri("https://lmgtfy.com/?q=" + Uri.EscapeUriString(selection.GetName()) + " Westerville", UriKind.Absolute);

                    AdaptiveCard newAdaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
                    adaptiveCardTemplate = adaptiveCardTemplate.Replace(imageUrlBlock, selection.GetIcon().ToString());
                    adaptiveCardTemplate = adaptiveCardTemplate.Replace(linkUrlBlock, letMeGoogleThat.ToString());
                    adaptiveCardTemplate = adaptiveCardTemplate.Replace(foodBlock, selection.GetName());
                    adaptiveCardTemplate = adaptiveCardTemplate.Replace(ratingBlock, selection.GetStars());
                    newAdaptiveCard = AdaptiveCard.FromJson(adaptiveCardTemplate).Card;

                    Attachment attachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = newAdaptiveCard
                    };

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment));
                }
            }
            catch (AdaptiveSerializationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Call the Google Places API
        /// </summary>
        /// <param name="types">Types of places to query</param>
        /// <param name="location">Latitutde/longitude</param>
        /// <param name="radius">The radius</param>
        /// <param name="key">Secret key</param>
        /// <returns></returns>
        public ApiResponse<object> MapsApiPlaceNearbysearchJsonGetWithHttpInfo(string types = null, string location = null, string radius = null, string key = null)
        {
            var localVarPath = "/maps/api/place/nearbysearch/json";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (types != null) localVarQueryParams.AddRange(Configuration.ApiClient.ParameterToKeyValuePairs("", "types", types)); // query parameter
            if (location != null) localVarQueryParams.AddRange(Configuration.ApiClient.ParameterToKeyValuePairs("", "location", location)); // query parameter
            if (radius != null) localVarQueryParams.AddRange(Configuration.ApiClient.ParameterToKeyValuePairs("", "radius", radius)); // query parameter
            if (key != null) localVarQueryParams.AddRange(Configuration.ApiClient.ParameterToKeyValuePairs("", "key", key)); // query parameter

            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("MapsApiPlaceNearbysearchJsonGet", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
                localVarResponse.Content);
        }
    }
}
