using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace ParserBot
{
    public class DurationCountStats
    {
        public int duration;
        public int count;

        public DurationCountStats(int duration, int count)
        {
            this.duration = duration;
            this.count = count;
        }
    }

    public class GameModePlayerStats
    {
        public Dictionary<string, int> nicknames { get; private set; }
        public Dictionary<string, DurationCountStats> maps { get; private set; }
        public Dictionary<string, DurationCountStats> opponents { get; private set; }
        public int totalTime { get; private set; }
        public int timesPlayed { get; private set; }

        public GameModePlayerStats()
        {
            nicknames = new Dictionary<string, int>();
            maps = new Dictionary<string, DurationCountStats>();
            opponents = new Dictionary<string, DurationCountStats>();
            totalTime = 0;
            timesPlayed = 0;
        }

        public void Add(string name, string map, int duration, List<string> opponents)
        {
            AddNick(name);
            AddMap(map, 1, duration);
            foreach (var opp in opponents)
            {
                AddOpp(opp, 1, duration);
            }
            totalTime += duration;
            timesPlayed += 1;
        }

        public void AddNick(string name, int count = 1)
        {
            if (nicknames.ContainsKey(name))
                nicknames[name] += count;
            else
                nicknames[name] = count;
        }

        public void AddMap(string map, int count, int duration)
        {
            if (maps.ContainsKey(map))
            {
                maps[map].count += count;
                maps[map].duration += duration;
            }
            else
            {
                maps[map] = new DurationCountStats(duration, count);
            }
        }

        public void AddOpp(string name, int count, int duration)
        {
            if (opponents.ContainsKey(name))
            {
                opponents[name].count += count;
                opponents[name].duration += duration;
            }
            else
            {
                opponents[name] = new DurationCountStats(duration, count);
            }
        }

        public static GameModePlayerStats operator +(GameModePlayerStats a, GameModePlayerStats b)
        {
            var nicknames = mergeDictionaries(a.nicknames, b.nicknames);
            var maps = mergeDictionaries(a.maps, b.maps);
            var opponents = mergeDictionaries(a.opponents, b.opponents);

            var result = new GameModePlayerStats();
            result.nicknames = nicknames;
            result.maps = maps;
            result.opponents = opponents;
            result.totalTime = a.totalTime + b.totalTime;
            result.timesPlayed = a.timesPlayed + b.timesPlayed;
            return result;

        }

        private static Dictionary<K, int> mergeDictionaries<K>(Dictionary<K, int> first, Dictionary<K, int> second)
        {
            var result = new Dictionary<K, int>();
            foreach (var pair in first)
            {
                result.Add(pair.Key, pair.Value);
            }
            foreach (var pair in second)
            {
                if (result.ContainsKey(pair.Key))
                {
                    result[pair.Key] = result[pair.Key] + pair.Value;
                }
                else
                {
                    result[pair.Key] = pair.Value;
                }
            }
            return result;
        }

        private static Dictionary<K, DurationCountStats> mergeDictionaries<K>(Dictionary<K, DurationCountStats> first, Dictionary<K, DurationCountStats> second)
        {
            var result = new Dictionary<K, DurationCountStats>();
            foreach (var pair in first)
            {
                result.Add(pair.Key, pair.Value);
            }
            foreach (var pair in second)
            {
                if (result.ContainsKey(pair.Key))
                {
                    var duration = result[pair.Key].duration + pair.Value.duration;
                    var count = result[pair.Key].count + pair.Value.count;
                    result[pair.Key] = new DurationCountStats(duration, count);
                }
                else
                {
                    result[pair.Key] = pair.Value;
                }
            }
            return result;
        }
    }
}
