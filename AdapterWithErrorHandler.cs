﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Modified by Jack Butts for non-commercial purposes (https://www.github.com/buttsj)
namespace WestervilleFoodBot
{
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Error handle adapter
    /// </summary>
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
