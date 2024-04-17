using System.Text;

using AlisCarBot.Common;
using AlisCarBot.Modules;

using Discord.WebSocket;

using Microsoft.Extensions.Options;

using Quartz;
using AlisCarBot.Extensions;

using AlisCarBot.Common.Options;

namespace AlisCarBot.Jobs {
    public class WeeklyTimeCheckJob(DiscordSocketClient discordSocketClient, IOptions<StartupOptions> options, ILogger<WeeklyTimeCheckJob> logger) : IJob {
        protected StartupOptions Options => options.Value;
        public async Task Execute(IJobExecutionContext context) {
            logger.LogInformation("Weekly time check job running.");
            var allSavedTimes = TimerHelper.GetAllSavedTimes();
            var stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine($"# [{DateTime.UtcNow:dd/MM/yyyy}] Weekly Time Totals.");
            foreach (var time in allSavedTimes) {
                var totalTime = time.TotalWeeklyTicks;
                if (TimerCommand.ActiveTimers.TryGetValue(time.DiscordId, out var activeTimer)) {
                    totalTime += (DateTime.UtcNow - activeTimer).Ticks;
                    TimerCommand.ActiveTimers[time.DiscordId] = DateTime.UtcNow;
                }

                stringBuilder.AppendLine($@"<@{time.DiscordId}>: {new TimeSpan(totalTime):hh\:mm\:ss}");
            }

            if (allSavedTimes.Count == 0) {
                stringBuilder.AppendLine("No time logged for this week.");
            }
            
            var timeChannel = Globals.MainGuild.GetTextChannel(Options.TimeLogChannel);
            if (timeChannel == null) {
                logger.LogError("Time channel was null!");
                return;
            }
            await timeChannel.SendMessageAsync(stringBuilder.ToString());
        }
    }
}