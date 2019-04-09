// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public static class LuisHelper
    {
        public static bool LuisCredentialsPresent(IConfiguration configuration)
        {
            return string.IsNullOrEmpty(configuration["LuisAppId"]) ||
                   string.IsNullOrEmpty(configuration["LuisAPIKey"]) ||
                   string.IsNullOrEmpty(configuration["LuisAPIHostName"]);
        }

        public static async Task<FAQModel> ExecuteLuisQuery(
            IConfiguration configuration,
            ILogger logger,
            ITurnContext turnContext,
            CancellationToken cancellationToken,
            Welcome faqData)
        {
            FAQModel faqModel = new FAQModel();

            try
            {
                // Create the LUIS settings from configuration.
                var luisApplication = CreateNewLuisApplication(configuration);

                var recognizer = new LuisRecognizer(luisApplication);

                // The actual call to LUIS
                var recognizerResult = await recognizer.RecognizeAsync(turnContext, cancellationToken);

                var (intent, score) = recognizerResult.GetTopScoringIntent();
                if (score > 0.7 && intent != "None")
                {
                    faqModel = MapToFAQModel(intent, faqData, score);
                }

                //if (intent == "Book_flight")
                //{
                //    // We need to get the result from the LUIS JSON which at every level returns an array.
                //    bookingDetails.Destination = recognizerResult.Entities["To"]?.FirstOrDefault()?["Airport"]?.FirstOrDefault()?.FirstOrDefault()?.ToString();
                //    bookingDetails.Origin = recognizerResult.Entities["From"]?.FirstOrDefault()?["Airport"]?.FirstOrDefault()?.FirstOrDefault()?.ToString();

                //    // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                //    // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                //    bookingDetails.TravelDate = recognizerResult.Entities["datetime"]?.FirstOrDefault()?["timex"]?.FirstOrDefault()?.ToString().Split('T')[0];
                //}
            }
            catch (Exception e)
            {
                logger.LogWarning($"LUIS Exception: {e.Message} Check your LUIS configuration.");
            }

            return faqModel;
        }

        private static FAQModel MapToFAQModel(string id, Welcome faqData, double score)
        {
            var kb = faqData.KnowledgeBases.FirstOrDefault(k => k.NodeId == Convert.ToInt32(id));

            if (kb != null)
            {
                return new FAQModel
                           {
                               Id = id,
                               Faq = kb.DisplayText,
                               Answer = kb.SolutionText,
                               Score = score,
                               Categories = kb.Categories
                           };
            }

            return null;
        }

        public static void UpdateUtterance()
        {

        }

        private static LuisApplication CreateNewLuisApplication(IConfiguration configuration)
        {
            return new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                "https://" + configuration["LuisAPIHostName"]);
        }
    }
}
