// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples
{
    public class FAQDialog : CancelAndHelpDialog
    {
        public FAQDialog()
            : base(nameof(FAQDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FAQStepAsync,
     //           OriginStepAsync,
     //           TravelDateStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FAQStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var faqModel = (FAQModel)stepContext.Options;

            if (faqModel.Answer == null)
            {
//                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Say what?") }, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Say what?"), cancellationToken);

                return await stepContext.EndDialogAsync(null);
            }
            else
            {
                return await stepContext.NextAsync(faqModel.Answer);
            }
        }

        private async Task<DialogTurnResult> OriginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;

            bookingDetails.Destination = (string)stepContext.Result;

            if (bookingDetails.Origin == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Where are you traveling from?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(bookingDetails.Origin);
            }
        }
        //private async Task<DialogTurnResult> TravelDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var faqModel = (FAQModel)stepContext.Options;

        //    faqModel.Answer = (string)stepContext.Result;

        //    if (faqModel.TravelDate == null || IsAmbiguous(faqModel.TravelDate))
        //    {
        //        return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), faqModel.TravelDate, cancellationToken);
        //    }
        //    else
        //    {
        //        return await stepContext.NextAsync(faqModel.TravelDate);
        //    }
        //}

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var faqModel = (FAQModel)stepContext.Options;

            faqModel.Answer = (string)stepContext.Result;

            var msg = faqModel.Answer;

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result == true)
            {
                var faqModel = (FAQModel)stepContext.Options;

                return await stepContext.EndDialogAsync(faqModel);
            }
            else
            {
                return await stepContext.EndDialogAsync(null);
            }
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexPropery = new TimexProperty(timex);
            return !timexPropery.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
