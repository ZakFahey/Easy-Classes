using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.IO;

namespace EasyClasses {
    [ApiVersion(1, 16)]
    public class EasyClasses : TerrariaPlugin {
        bool canHaveDuplicates { get; set; }
        Dictionary<TSPlayer, string> PlayerStats = new Dictionary<TSPlayer, string>();
        string classFile { get; set; }
        List<int> playerCan = new List<int>();
        internal delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);
        public EasyClasses(Main game) : base(game) {
        }
        public override void Initialize() {
            Commands.ChatCommands.Add(new Command("easyclasses.admin.start", classStart, "classstart") {
                HelpText = "Gives everyone with the canplayclass permission a choice of classes. Format: /classstart <filename> [can have duplicates]"
            });
            Commands.ChatCommands.Add(new Command("easyclasses.admin.start", classList, "classlist") {
                HelpText = "Shows all class set files in the class directory."
            });
            Commands.ChatCommands.Add(new Command("easyclasses.admin.start", classInfo, "classinfo")
            {
                HelpText = "Shows the analytics of a class. Format: /classinfo class"
            });
            Commands.ChatCommands.Add(new Command("easyclasses.guest.play", ClassDo, "class") {
                HelpText = "Commands: /class list [page], /class start <class>, /class info <class>"
            });
            Directory.CreateDirectory(Path.Combine(TShock.SavePath, "Classes"));

            TShockAPI.GetDataHandlers.KillMe += KillMe;
        }
        public override Version Version {
            get { return new Version("1.1"); }
        }
        public override string Name {
            get { return "Easy Classes"; }
        }
        public override string Author {
            get { return "GameRoom"; }
        }
        public override string Description {
            get { return "Let players pick classes without any chests. It's as easy as a single console command!"; }
        }

        void classStart(CommandArgs e) {
            if (e.Parameters.Count == 0) e.Player.SendErrorMessage("You need to specify the filename.");
            else if (e.Parameters[0].Contains("analytics")) e.Player.SendErrorMessage("You cannot have the word \"analytics\" in your filename.");
            else {
                classFile = String.Concat(Path.GetFileNameWithoutExtension(e.Parameters[0]), " analytics", Path.GetExtension(e.Parameters[0]));
                if (!Config.ReadConfig(e.Parameters[0], e.Player, false))
                    Log.ConsoleError(String.Format("Failed to read {0}. Consider generating a new config file.",e.Parameters[0]));
                else if (!Config.ReadConfig(classFile, e.Player, true))
                    Log.ConsoleError(String.Format("Failed to read {0}. Consider generating a new config file.", classFile));
                else {
                    canHaveDuplicates = true;
                    try {
                        if (e.Parameters.Count > 1) canHaveDuplicates = Convert.ToBoolean(e.Parameters[1]);
                    }
                    catch (FormatException) {
                        e.Player.SendErrorMessage("The second argument must be either true or false.");
                    }
                    playerCan.Clear();
                    foreach (TSPlayer player in TShock.Players)
                        if (player != null && player.Active && !player.Dead && player.Group.HasPermission("easyclasses.guest.play")) {
                            if (!playerCan.Contains(player.Index)) playerCan.Add(player.Index);
                            ShowClasses(player, true);
                        }
                    foreach (Config.PlayerClass clas in Config.contents.playerClasses)
                        if (!Config.stats.classStats.Exists(x => x.Name == clas.name))
                            Config.stats.classStats.Add(new Config.ClassStats { Name = clas.name });
                    Config.UpdateConfig(classFile, true);
                }
            }
        }

        void ClassDo(CommandArgs e) {
            if (playerCan.Contains(e.Player.Index)) {
                string action = "";
                if (e.Parameters.Count > 0) action = e.Parameters[0];
                switch (action) {
                    case "list":
                        int par = 1;
                        if (e.Parameters.Count > 1)
                            Int32.TryParse(e.Parameters[1], out par);
                        if (par <= 0) e.Player.SendErrorMessage("The page number must be greater than zero.");
                        else ShowClasses(e.Player, false, par);
                        break;

                    case "start":
                        if (e.Parameters.Count < 2 || e.Parameters[1] == "")
                            e.Player.SendErrorMessage("Incorrect syntax! Correct syntax: /class start <class>.");
                        else {
                            var data = Classes(e.Player, e.Parameters[1]);
                            if (canHaveDuplicates || !data.taken) {
                                if (data != null) {
                                    try {
                                        List<int> pc = playerCan.Where(x => x != e.Player.Index).ToList<int>();
                                        playerCan = pc;
                                        foreach (Config.ClassItem item in data.classItems) {
                                            var itm = TShock.Utils.GetItemByIdOrName(item.name)[0];
                                            e.Player.GiveItem(itm.type, itm.name, itm.width, itm.height, item.stack, item.prefix);
                                        }
                                        e.Player.SendSuccessMessage(String.Format("You are now {0}.", data.name));
                                        data.taken = true;
                                        if (!Config.stats.classStats.Exists(x => x.Name == data.name))
                                            Config.stats.classStats.Add(new Config.ClassStats { Name = data.name });
                                        Config.stats.classStats.First(x => x.Name == data.name).Plays++;
                                        Config.UpdateConfig(classFile, true);
                                        PlayerStats.Add(e.Player, data.name);
                                    }
                                    catch {
                                        e.Player.SendErrorMessage("There was a problem with the config file.");
                                    }
                                }
                            } else e.Player.SendErrorMessage("That class is already taken.");
                        }
                        break;

                    case "info":
                        if (e.Parameters.Count < 2 || e.Parameters[1] == "")
                            e.Player.SendErrorMessage("Incorrect syntax! Correct syntax: /class info <class>.");
                        else {
                            var data = Classes(e.Player, e.Parameters[1]);
                            if (data != null) e.Player.SendInfoMessage(data.description);
                        }
                        break;

                    default:
                        e.Player.SendErrorMessage("Invalid command! Commands: /class list <page>, /class start <class>, /class info <class>");
                        break;
                }
            } else e.Player.SendErrorMessage("You can't select a class now.");
        }

        void classInfo(CommandArgs e) {
            if (e.Parameters.Count < 1 || e.Parameters[0] == "")
                e.Player.SendErrorMessage("Incorrect syntax! Correct syntax: /classinfo <class>.");
            else {
                var data = Classes(e.Player, e.Parameters[0]);
                if (data != null) {
                    if (!Config.stats.classStats.Exists(x => x.Name == data.name))
                        Config.stats.classStats.Add(new Config.ClassStats { Name = data.name });
                    var stats = Config.stats.classStats.First(x => x.Name == data.name);
                    e.Player.SendInfoMessage(String.Format("{0}: Plays: {1}, Deaths: {2}", stats.Name, stats.Plays, stats.Deaths));
                }
            }
        }

        void ShowClasses(TSPlayer player, bool showHelp, int pageNumber = 1) {
            List<string> classNames = new List<string>();
            foreach (Config.PlayerClass clas in Config.contents.playerClasses)
                if (canHaveDuplicates || !clas.taken) classNames.Add(clas.name);
            if (showHelp) player.SendInfoMessage("Choose a class with /class.");
            PaginationTools.SendPage(player, pageNumber, PaginationTools.BuildLinesFromTerms(classNames),
                new PaginationTools.Settings {
                    HeaderFormat = "Classes ({0}/{1}):",
                    FooterFormat = "Type /class list {0} for more."
                });
        }

        void classList(CommandArgs e) {
            int pageNumber = 1;
            if (e.Parameters.Count > 0)
                int.TryParse(e.Parameters[0], out pageNumber);
            if (pageNumber <= 0) e.Player.SendErrorMessage("The page number must be greater than zero.");
            else {
                var files = Directory.GetFiles(Path.Combine(TShock.SavePath, "Classes")).Where(x => !x.ToLower().Contains("analytics"));
                List<string> newFiles = new List<string>();
                foreach (string file in files)
                    newFiles.Add(Path.GetFileName(file));
                PaginationTools.SendPage(e.Player, pageNumber, PaginationTools.BuildLinesFromTerms(newFiles),
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Files ({0}/{1}):",
                        FooterFormat = "Type /classlist {0} for more."
                    });
            }
        }

        Config.PlayerClass Classes(TSPlayer who, string argument) {
            List<Config.PlayerClass> clss = new List<Config.PlayerClass>();
            foreach(Config.PlayerClass cls in Config.contents.playerClasses)
                if (cls.name.ToLower().StartsWith(argument.ToLower())) clss.Add(cls);
            if (clss.Count == 0) {
                who.SendErrorMessage("Invalid class!");
                return null;
            }
            else if (clss.Count > 1) {
                TShock.Utils.SendMultipleMatchError(who, clss.Select(i => i.name));
                return null;
            }
            else return clss[0];
        }

        void KillMe(object sender, TShockAPI.GetDataHandlers.KillMeEventArgs e) {
            if (e.Pvp && PlayerStats.ContainsKey(TShock.Players[e.PlayerId])) {
                Config.stats.classStats.First(y => y.Name == PlayerStats[TShock.Players[e.PlayerId]]).Deaths++;
                Config.UpdateConfig(classFile, true);
            }
        }
    }
}