using Discord.WebSocket;

using Newtonsoft.Json;

namespace AlisCarBot.Extensions {
    public class SavedTicksObject(ulong discordId, long totalWeeklyTicks) {
        public ulong DiscordId { get; } = discordId;
        public long TotalWeeklyTicks { get; set; } = totalWeeklyTicks;
    }
    
    public static class TimerHelper {
        public static List<SavedTicksObject> GetAllSavedTimes() {
            return JsonConvert.DeserializeObject<List<SavedTicksObject>>(File.ReadAllText("savedtimes.json")) ?? [];
        }
        private static void SaveNewTimes(List<SavedTicksObject> newSave) => File.WriteAllText("savedtimes.json", JsonConvert.SerializeObject(newSave));
        public static long GetTicksFromDiscordUserId(this SocketUser user) => GetAllSavedTimes().FirstOrDefault(o => o.DiscordId == user.Id)?.TotalWeeklyTicks ?? 0;

        public static void SaveTicksForDiscordId(this SocketUser discordUser, long ticks) {
            var savedTimes = GetAllSavedTimes().ToList();
            var existingSave = savedTimes.FirstOrDefault(o => o.DiscordId == discordUser.Id);
            if (existingSave != null) {
                savedTimes[savedTimes.IndexOf(existingSave)].TotalWeeklyTicks += ticks;
            }
            else {
                savedTimes.Add(new SavedTicksObject(discordUser.Id, ticks));
            }
            SaveNewTimes(savedTimes);
        }
    }
}