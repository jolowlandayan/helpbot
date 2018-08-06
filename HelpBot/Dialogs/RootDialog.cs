using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace HelpBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        string user = "";
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> argument)
        {
            PromptDialog.Text(context, EmployeeIdReceived, "Hello, please enter your Employee Id so I can better help you.");
        }

        public async Task EmployeeIdReceived(IDialogContext context, IAwaitable<object> results)
        {
            //await context.PostAsync("Thanks this is noted.");
            //var activity = await results as Activity;
            await context.Forward(new SearchEmployeeId(), ResumeAfterLuisDialog, context.Activity, CancellationToken.None);

        }
        public async Task ResumeAfterLuisDialog(IDialogContext context, IAwaitable<object> results)
        {
            var res = await results;
            user = res.ToString();

            if (DateTime.Now.Hour < 12)
            {
                await context.PostAsync(String.Format("Good Morning! Mr.{0}, how can I help you today?", user));
            }
            else if (DateTime.Now.Hour < 17)
            {
                await context.PostAsync(String.Format("Good Afternoon! Mr.{0}, how can I help you today?", user));
            }
            else
            {
                await context.PostAsync(String.Format("Good Evening! Mr.{0}, how can I help you today?", user));
            }

            PromptDialog.Choice(context, SelectionMessageReceivedAsync, new List<string>() { "Device-related help", "Application-related help" }, $"Please choose your concern.");
        }

        public async Task SelectionMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;

            if (choice == "Device-related help")
            {
                await DeviceRelatedConcernMessageReceived(context);
            }
            else if (choice == "Application-related help")
            {
                await ApplicationRelatedConcernMessageReceived(context);

            }
        }

        public async Task DeviceRelatedConcernMessageReceived(IDialogContext context)
        {
            PromptDialog.Text(context, DeviceRelatedAnswer, "What is your concern?");
        }

        public async Task ApplicationRelatedAnswer(IDialogContext context, IAwaitable<string> result)
        {
            string test = @"Welcome to <a href=""http://google.com"">Google</a> ";
            string answer = "A user requires a specific Pfizer app installed on their iOS device. Follow this <a href=\"http://www.fabpro1.com\" > link</a> to install the Pfizer Enterprise (In-House) Application on your iPhone/iPad.";
           await context.PostAsync(test);
            PromptDialog.Choice(context, SelectionLoopMessageReceivedAsync, new List<string> { "Yes", "No" }, $"I hope this answers your inquiry. Is there anything else I can help you with?");
        }

        public async Task ApplicationRelatedConcernMessageReceived(IDialogContext context)
        {
            PromptDialog.Text(context, ApplicationRelatedAnswer, "What is your concern?");
        }

        public async Task DeviceRelatedAnswer(IDialogContext context, IAwaitable<string> result)
        {
            string answer = "When you have received a new iOS device and after setting up the device it needs to be enrolled. Please visit this <a href=\"https://pfizer.rightanswers.com/portal/app/portlets/results/view2.jspk2dockey=110630130428498 \"> link</a>";
            await context.PostAsync(answer);
            PromptDialog.Choice(context, SelectionLoopMessageReceivedAsync, new List<string> { "Yes", "No" }, $"I hope this answers your inquiry. Is there anything else I can help you with?");

        }

        public async Task SelectionLoopMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;

            if (choice == "Yes")
            {
                await MessageReceivedAsync(context, argument);
            }
            else if (choice == "No")
            {
                await context.PostAsync("Thank you and have a nice day.");

            }
        }
    }
}