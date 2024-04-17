using AlisCarBot.Extensions;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Newtonsoft.Json;

using Common_ModuleBase = AlisCarBot.Common.ModuleBase;
using ModuleBase = AlisCarBot.Common.ModuleBase;

namespace AlisCarBot.Modules {
    [Group("timer", "Timer commands")]
    public class TimerCommand(DiscordSocketClient discordSocketClient) : Common_ModuleBase {
        public static readonly Dictionary<ulong, DateTime> ActiveTimers = new();
        
        private async void NotifyActiveTimer(ulong userId) {
            await Task.Delay(TimeSpan.FromHours(1));
            while (ActiveTimers.TryGetValue(userId, out var value)) {
                var curTime = DateTime.UtcNow - value;
                await discordSocketClient.GetUser(userId).SendMessageAsync($"Your timer is now at {curTime:hh\\:mm\\:ss}");
                await Task.Delay(TimeSpan.FromHours(1));
            }
        }
        
        [SlashCommand("start", "Starts a timer.")]
        public async Task StartTimer() {
            if (ActiveTimers.TryGetValue(Context.User.Id, out var value)) {
                await RespondAsync($"You already have an active timer that's currently at: {DateTime.UtcNow - value:hh\\:mm\\:ss}\nUse /timer stop to stop that timer and start a new one.");
                return;
            }
            ActiveTimers.Add(Context.User.Id, DateTime.UtcNow);
            await RespondAsync("Your timer has been started");
            NotifyActiveTimer(Context.User.Id);
        }
        
        [SlashCommand("stop", "Stops your timer.")]
        public async Task StopTimer() {
            if (!ActiveTimers.TryGetValue(Context.User.Id, out var value)) {
                await RespondAsync($"You don't have an active timer.");
                return;
            }
            
            var finalTime = DateTime.UtcNow - value;
            ActiveTimers.Remove(Context.User.Id);
            Context.User.SaveTicksForDiscordId(finalTime.Ticks);
            await RespondAsync($"Your timer has been stopped. Final time was: {finalTime:hh\\:mm\\:ss}");
        }
        
        [SlashCommand("weekly-total", "See your weekly total time.")]
        public async Task WeeklyTotalTime() {
            long totalTicks = 0;
            if (ActiveTimers.TryGetValue(Context.User.Id, out var value)) {
                totalTicks = (DateTime.UtcNow - value).Ticks;
            }

            totalTicks += Context.User.GetTicksFromDiscordUserId();
            var totalTime = new TimeSpan(totalTicks);
            await RespondAsync($"Your current weekly total is: {totalTime:hh\\:mm\\:ss}", ephemeral: true);
        }
    }
}