using ConfigCreator;
using System.Text;
using TL;
using TwoCaptcha.Captcha;
using WTelegram;
using Config = ConfigCreator.Config;

namespace AnonChatBot
{
    internal class AnonChatBot
    {
        static StreamWriter WTelegramLogs = new StreamWriter("WTelegram.log", true, Encoding.UTF8) { AutoFlush = true };
        private static string fileName;
        static readonly Dictionary<long, User> Users = new();
        static readonly Dictionary<long, ChatBase> Chats = new();
        private static string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private static string Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        static Client client;
        static void Main() => Initialize().GetAwaiter().GetResult();
        static async Task Initialize()
        {
            // log into WTelegram.log
            Helpers.Log = (lvl, str) =>
            {
                Console.Write(str.Contains("FLOOD", StringComparison.OrdinalIgnoreCase) ? $"\nFLOOD WAIT! {str}" : string.Empty);
                WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
            };
            InitializeConfig();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[API] Connecting...");
            client = new Client(TgConfig);
            await client.LoginUserIfNeeded();
            client.OnUpdate += OnNewMessage;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[API] Connected!\n[BOT] Waiting for message from {Config.GetItem("AnonChatBot")}");
            Console.ForegroundColor = ConsoleColor.White;
            await Task.Delay(-1);
        }

        private static async Task OnNewMessage(IObject arg)
        {
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(Users, Chats);
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"[BOT] Received new message: {unm.message}");
                        Console.ForegroundColor = ConsoleColor.White;
                        if (unm.message is Message message && message.media is MessageMediaPhoto { photo: Photo photo })
                        {
                            if (!Convert.ToBoolean(Config.GetItem("AutosolveCaptcha")))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("[CAPTCHA] Captcha detected, but auto solver turned off. Skipping...");
                            }
                            else
                            {
                                var anonChat = await client.Contacts_ResolveUsername(Config.GetItem("AnonChatBot"));
                                Console.ForegroundColor = ConsoleColor.Gray;
                                // downloading captcha
                                var filename = $"{photo.id}.jpg";
                                Console.WriteLine("[CAPTCHA] Downloading " + filename);
                                using var fileStream = File.Create(filename);
                                var type = await client.DownloadFileAsync(photo, fileStream);
                                fileName = $"{photo.id}.{type}";
                                fileStream.Close(); // necessary for the renaming
                                Console.WriteLine("[CAPTCHA] Download finished");
                                if (type is not Storage_FileType.unknown and not Storage_FileType.partial)
                                    File.Move(filename, $"{photo.id}.{type}", true);
                                // solving captcha
                                await client.SendMessageAsync(anonChat, solveCaptcha(Config.GetItem("RuCaptchaApiKey"), $"{photo.id}.{type}"));
                                // removing captcha file
                                File.Delete(fileName);
                            }
                        }
                        if (unm.message.ToString().Contains(Config.GetItem("TriggerWord")))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("[BOT] AnonChat message detected. Sending");
                            Console.ForegroundColor = ConsoleColor.White;
                            var text = Config.GetItem("SpammingText");
                            var anonChat = await client.Contacts_ResolveUsername(Config.GetItem("AnonChatBot"));
                            Thread.Sleep(500);
                            await client.SendMessageAsync(anonChat, StringRandomizator.RandomizateString(text));
                            Thread.Sleep(500);
                            await client.SendMessageAsync(anonChat, "/next");
                        }
                        break;
                }
        }

        public static void InitializeConfig()
        {
            try
            {
                Config.Initialize("botConfig", null);
            }
            catch
            {
                Config.Add("SpammingText", null, "Текст который бот будет писать после триггер фразы");
                Config.Add("ApiID", null, "Берется тут: my.telegram.org");
                Config.Add("ApiHash", null, "Берется тут: my.telegram.org");
                Config.Add("PhoneNumber", "+79996665544", null);
                Config.Add("AnonChatBot", "AnonRuBot", "Айди анонимного бота, указывать без @");
                Config.Add("TriggerWord", "Собеседник найден", "Фраза, на которую бот будет срабатывать");
                Config.Add("AutosolveCaptcha", "false", "true, если вы хотите чтобы капча решалась автоматически. Требуется RuCaptcha api key");
                Config.Add("RuCaptchaApiKey", "apiKeyHere", "Апи ключ можно взять тут - https://rucaptcha.com/enterpage");
                Config.CreateConfigFile("botConfig", null);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[CONFIG] Кажется, вы не заполнили конфиг файл. Пожалуйста, заполните его и повторите попытку");
                Console.ReadKey();
                return;
            }
        }

        static string solveCaptcha(string captchaApiKey, string captchaPath)
        {
            var solver = new TwoCaptcha.TwoCaptcha(captchaApiKey);
            Normal captcha = new Normal(captchaPath);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[CAPTCHA] Starting solving");
            try
            {
                solver.Solve(captcha).Wait();
                Console.WriteLine($"[CAPTCHA] Captcha Answer: {captcha.Code}");
                return captcha.Code;
            }
            catch (AggregateException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CAPTCHA] Invalid captcha!");
                File.Delete(fileName);
                return null;
            }
        }
        static string TgConfig(string what)
        {
            switch (what)
            {
                case "api_id": return Config.GetItem("ApiID");
                case "api_hash": return Config.GetItem("ApiHash");
                case "phone_number": return Config.GetItem("PhoneNumber");
                case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                case "session_pathname": return $"session/{Config.GetItem("PhoneNumber").Replace(" ", string.Empty)}.session";
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
    }
}