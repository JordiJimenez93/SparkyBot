// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;

        public Welcome _faqData;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            // load all kana data
            _faqData = GetFromJson();

            _configuration = configuration;
            _logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new FAQDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Welcome GetFromJson()
        {
            JsonSerializer serializer = new JsonSerializer();
            Welcome welcome = null;
            using (FileStream s = File.Open("KanaResults.json", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                welcome = serializer.Deserialize<Welcome>(reader);
                            }
                        }
                    }
                }
            }

            return welcome;
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (LuisHelper.LuisCredentialsPresent(_configuration))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Good morning Laura! I am Sparky! I would like to help you in your Sunweb adventure! ") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var faqModel = stepContext.Result != null
                    ?
                await LuisHelper.ExecuteLuisQuery(_configuration, _logger, stepContext.Context, cancellationToken, _faqData)
                    :
                new FAQModel();

            // In this sample we only have a single Intent we are concerned with. However, typically a scneario
            // will have multiple different Intents each corresponding to starting a different child Dialog.

            // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            return await stepContext.BeginDialogAsync(nameof(FAQDialog), faqModel, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            if (stepContext.Result != null)
            {
                var result = (FAQModel)stepContext.Result;

                //var timeProperty = new TimexProperty(result.TravelDate);
                //var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                //var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                var msg = $"Thanks";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(MainDialog), cancellationToken);

 //               await stepContext.BeginDialogAsync().Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            }
            return await stepContext.EndDialogAsync();
        }
    }
}
