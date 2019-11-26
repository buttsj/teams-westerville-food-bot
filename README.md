
# Westerville Food Bot

Spun off of the Bot Framework v4 Conversation Bot sample for Teams.

This bot has been created using [Bot Framework](https://dev.botframework.com). This sample shows
how to incorporate basic conversational flow into a Teams application. It also illustrates a few of the Teams specific calls you can make from your bot.

## Prerequisites

- Microsoft Teams is installed and you have an account

## Interacting with the bot

You can interact with this bot by selecting a command from the command list. The bot will respond to the following strings.

1. **Find Food**
  - **Result:** The bot will access the Google Places API to find a random restaurant within the Westerville, OH area
  - **Valid Scopes:** personal, group chat, team chat
2. **FindFood**
  - **Result:** Another option to accomplish the above
  - **Valid Scopes:** personal, group chat, team chat

You can select an option from the command list by typing ```@TeamsConversationBot``` into the compose message area and ```What can I do?``` text above the compose area.

## Further reading

- [How Microsoft Teams bots work](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics-teams?view=azure-bot-service-4.0&tabs=javascript)

