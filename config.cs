using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace EasyClasses
{
    class Config
    {
        public static Contents contents;
        public static Stats stats;

        public class Stats {
            public List<ClassStats> classStats = new List<ClassStats>();
        }

        public class Contents {
            public List<PlayerClass> playerClasses = new List<PlayerClass> {
                new PlayerClass {
                    name = "Warrior",
                    description = "He swings a huge sword. Good at close range.",
                    classItems = {
                        new ClassItem {
                            name = "Breaker Blade",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Helmet",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Breastplate",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Leggings",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cloud in a Bottle",
                            stack = 1,
                            prefix = 65
                        },
                        new ClassItem {
                            name = "Life Crystal",
                            stack = 10,
                            prefix = 0
                        }
                    }
                },
                new PlayerClass {
                    name = "Gunner",
                    description = "He wields the finest of shotguns. Watch out.",
                    classItems = {
                        new ClassItem {
                            name = "Shotgun",
                            stack = 1,
                            prefix = 60
                        },
                        new ClassItem {
                            name = "Cobalt Helmet",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Breastplate",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Leggings",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Life Crystal",
                            stack = 8,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cloud in a Bottle",
                            stack = 1,
                            prefix = 0
                        }
                    }
                },
                new PlayerClass {
                    name = "Wizard",
                    description = "He uses the darkest of magic.",
                    classItems = {
                        new ClassItem {
                            name = "Cursed Flames",
                            stack = 1,
                            prefix = 83
                        },
                        new ClassItem {
                            name = "Cobalt Hat",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Breastplate",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cobalt Leggings",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Life Crystal",
                            stack = 6,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Cloud in a Bottle",
                            stack = 1,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Mana Crystal",
                            stack = 9,
                            prefix = 0
                        },
                        new ClassItem {
                            name = "Mana Regeneration Band",
                            stack = 1,
                            prefix = 66
                        }
                    }
                }
            };
        }

        public class ClassItem {
            public string name { get; set; }
            public int stack { get; set; }
            public int prefix { get; set; }
        }

        public class PlayerClass {
            public bool taken = false;
            public List<ClassItem> classItems = new List<ClassItem>();
            public string name { get; set; }
            public string description { get; set; }
        }

        public class ClassStats {
            public string Name { get; set; }
            public uint Kills = 0;
            public uint Deaths = 0;
            public uint Plays = 0;
        }

        public static void CreateConfig(string fname, bool statistics)
        {
            string filepath = Path.Combine(TShock.SavePath, "Classes", fname);

            try
            {
                using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (var sr = new StreamWriter(stream))
                    {
                        string configString;
                        if (statistics)
                        {
                            stats = new Stats();
                            configString = JsonConvert.SerializeObject(stats, Formatting.Indented);
                        }
                        else
                        {
                            contents = new Contents();
                            configString = JsonConvert.SerializeObject(contents, Formatting.Indented);
                        }
                        sr.Write(configString);
                    }
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Log.ConsoleError(e.Message);
                if (statistics) stats = new Stats();
                else contents = new Contents();
            }
        }

        public static bool ReadConfig(string fname, TSPlayer plr, bool statistics)
        {
            string filepath = Path.Combine(TShock.SavePath, "Classes", fname);

            try
            {
                if (File.Exists(filepath))
                {
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var configString = sr.ReadToEnd();
                            if (statistics)
                                stats = JsonConvert.DeserializeObject<Stats>(configString);
                            else
                            {
                                contents = JsonConvert.DeserializeObject<Contents>(configString);
                                contents.playerClasses.RemoveRange(0, 3);//Delete default classes
                            }
                        }
                        stream.Close();
                    }
                    return true;
                }
                else
                {
                    CreateConfig(fname, statistics);
                    Log.ConsoleInfo(String.Format("Created {0}", fname));
                    plr.SendSuccessMessage(String.Format("Created {0}", fname));
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.ConsoleError(e.Message);
            }
            return false;
        }

        public static bool UpdateConfig(string fname, bool statistics)
        {
            string filepath = Path.Combine(TShock.SavePath, "Classes", fname);

            try
            {
                if (!File.Exists(filepath))
                    return false;

                string query;
                if (statistics)
                    query = JsonConvert.SerializeObject(stats, Formatting.Indented);
                else
                    query = JsonConvert.SerializeObject(contents, Formatting.Indented);
                using (var stream = new StreamWriter(filepath, false))
                {
                    stream.Write(query);
                }
                return true;
            }
            catch (Exception e)
            {
                Log.ConsoleError(e.Message);
                return false;
            }
        }
    }
}
