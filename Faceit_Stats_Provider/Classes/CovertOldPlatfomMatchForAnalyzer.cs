using System.Linq; // Ensure this is present for LINQ methods
using Faceit_Stats_Provider.ModelsForAnalyzer;

namespace Faceit_Stats_Provider.Classes
{
    public class CovertOldPlatformMatchForAnalyzer
    {
        public static AnalyzerMatchPlayers.Rootobject ConvertOldMatchToNew(AnalyzerMatchPlayersOldMatch.Rootobject oldMatch)
        {
            var newMatch = new AnalyzerMatchPlayers.Rootobject
            {
                teams = new AnalyzerMatchPlayers.Teams
                {
                    faction1 = ConvertFaction(oldMatch.teams.faction1),
                    faction2 = ConvertFaction(oldMatch.teams.faction2)
                }
            };

            return newMatch;
        }

        private static AnalyzerMatchPlayers.Faction1 ConvertFaction(AnalyzerMatchPlayersOldMatch.Faction1 oldFaction)
        {
            return new AnalyzerMatchPlayers.Faction1
            {
                faction_id = oldFaction.faction_id,
                leader = oldFaction.leader,
                avatar = oldFaction.avatar,
                name = oldFaction.name,
                substituted = oldFaction.substituted,
                type = oldFaction.type,
                roster = oldFaction.roster_v1.Select(ConvertRoster).ToArray()
            };
        }

        private static AnalyzerMatchPlayers.Faction2 ConvertFaction(AnalyzerMatchPlayersOldMatch.Faction2 oldFaction)
        {
            return new AnalyzerMatchPlayers.Faction2
            {
                faction_id = oldFaction.faction_id,
                leader = oldFaction.leader,
                avatar = oldFaction.avatar,
                name = oldFaction.name,
                substituted = oldFaction.substituted,
                type = oldFaction.type,
                roster = oldFaction.roster_v1.Select(ConvertRoster).ToArray()
            };
        }

        private static AnalyzerMatchPlayers.Roster ConvertRoster(AnalyzerMatchPlayersOldMatch.Roster_V1 oldRoster)
        {
            return new AnalyzerMatchPlayers.Roster
            {
                player_id = oldRoster.guid,
                nickname = oldRoster.nickname,
                avatar = oldRoster.avatar,
                game_player_id = oldRoster.csgo_id,
                game_player_name = oldRoster.nickname,
                game_skill_level = oldRoster.csgo_skill_level,
                membership = oldRoster.csgo_skill_level_label, // Assuming 'membership' maps to 'csgo_skill_level_label'
                anticheat_required = false // No equivalent field in oldRoster, defaulting to false
            };
        }

        private static AnalyzerMatchPlayers.Roster ConvertRoster(AnalyzerMatchPlayersOldMatch.Roster_V11 oldRoster)
        {
            return new AnalyzerMatchPlayers.Roster
            {
                player_id = oldRoster.guid,
                nickname = oldRoster.nickname,
                avatar = oldRoster.avatar,
                game_player_id = oldRoster.csgo_id,
                game_player_name = oldRoster.nickname,
                game_skill_level = oldRoster.csgo_skill_level,
                membership = oldRoster.csgo_skill_level_label, // Assuming 'membership' maps to 'csgo_skill_level_label'
                anticheat_required = false // No equivalent field in oldRoster, defaulting to false
            };
        }
    }
}
