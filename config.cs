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
                            name = "Cobalt Headgear",
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

        public static void CreateConfig(string fname)
        {
            string filepath = Path.Combine(TShock.SavePath, "Classes", fname);

            try
            {
                using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (var sr = new StreamWriter(stream))
                    {
                        contents = new Contents();
                        var configString = JsonConvert.SerializeObject(contents, Formatting.Indented);
                        sr.Write(configString);
                    }
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Log.ConsoleError(e.Message);
                contents = new Contents();
            }
        }

        public static bool ReadConfig(string fname, TSPlayer plr)
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
                            contents = JsonConvert.DeserializeObject<Contents>(configString);
                            contents.playerClasses.RemoveRange(0, 3);//Delete default classes
                        }
                        stream.Close();
                    }
                    return true;
                }
                else
                {
                    CreateConfig(fname);
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

        public static bool UpdateConfig(string fname)
        {
            string filepath = Path.Combine(TShock.SavePath, "Classes", fname);

            try
            {
                if (!File.Exists(filepath))
                    return false;

                string query = JsonConvert.SerializeObject(contents, Formatting.Indented);
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
