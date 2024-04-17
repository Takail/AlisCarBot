using AlisCarBot.Jobs;
using AlisCarBot.Services;

using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;

using Quartz;

using AlisCarBot.Common.Options;

namespace AlisCarBot {
    public class Program {
        public static async Task Main(string[] args) {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddOptions<LinksOptions>().BindConfiguration(LinksOptions.Links).ValidateDataAnnotations().ValidateOnStart();
            builder.Services.AddOptions<StartupOptions>().BindConfiguration(StartupOptions.Startup).ValidateDataAnnotations().ValidateOnStart();

            builder.Services.AddQuartz(quartzConfigurator => {
                var today = DateTime.Today;
                quartzConfigurator.SchedulerName = "Quartz Instance";
                quartzConfigurator.SchedulerId = "Quartz Master";
                quartzConfigurator.ScheduleJob<WeeklyTimeCheckJob>(trigger => trigger
                    .WithIdentity("WeeklyTimeCheckJob")
                    //.StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.Now.AddSeconds(5)))
                    .StartAt(today.AddDays(((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7))
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(1)
                        .RepeatForever())
                    .WithDescription("Handles Patreon sync for the predefined interval")
                );
            });

            builder.Services.AddQuartzHostedService(opts => {
                // when shutting down we want jobs to complete gracefully
                opts.AwaitApplicationStarted = true;
                opts.WaitForJobsToComplete = true;
            });

            builder.Services.AddDiscordHost((config, _) => {
                config.SocketConfig = new DiscordSocketConfig {
                    LogLevel = LogSeverity.Info,
                    GatewayIntents = GatewayIntents.All,
                    LogGatewayIntentWarnings = false,
                    UseInteractionSnowflakeDate = false,
                    AlwaysDownloadUsers = false,
                };

                config.Token = builder.Configuration.GetSection(StartupOptions.Startup).Get<StartupOptions>()!.Token;
            });

            builder.Services.AddInteractionService((config, _) => {
                config.LogLevel = LogSeverity.Debug;
                config.DefaultRunMode = RunMode.Async;
                config.UseCompiledLambda = true;
            });
            builder.Services.AddInteractiveService(config => {
                config.LogLevel = LogSeverity.Warning;
                config.DefaultTimeout = TimeSpan.FromMinutes(5);
                config.ProcessSinglePagePaginators = true;
            });

            builder.Services.AddHostedService<InteractionHandler>();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}