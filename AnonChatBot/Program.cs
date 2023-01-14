using ConfigCreator;
using TL;
using TwoCaptcha.Captcha;
using WTelegram;
using Config = ConfigCreator.Config;

namespace AnonChatBot
{
    internal class AnonChatBot 
    {
        static readonly Dictionary<long, User> Users = new();
        static readonly Dictionary<long, ChatBase> Chats = new();
        private static string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private static string Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        static Client client;
        static void Main() => Initialize().GetAwaiter().GetResult();
        static async Task Initialize()
        {
            // creating or init cfg
            try
            {
                if (!Directory.Exists("session"))
                    Directory.CreateDirectory("session");
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[API] Connecting...");
            client = new Client(TgConfig);
            await client.LoginUserIfNeeded();
            client.OnUpdate += OnNewMessage;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[API] Connected!");
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
                        Console.WriteLine($"[BOT] Received new message: {unm.message}");
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
                                fileStream.Close(); // necessary for the renaming
                                Console.WriteLine("[CAPTCHA] Download finished");
                                if (type is not Storage_FileType.unknown and not Storage_FileType.partial)
                                    File.Move(filename, $"{photo.id}.{type}", true);
                                // solving captcha
                                await client.SendMessageAsync(anonChat, solveCaptcha(Config.GetItem("RuCaptchaApiKey"), $"{photo.id}.{type}"));
                                // removing captcha file
                                File.Delete($"{photo.id}.{type}");
                            }
                        }
                        if (unm.message.ToString().Contains(Config.GetItem("TriggerWord")))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("[BOT] AnonChat message detected. Sending");
                            var text = Config.GetItem("SpammingText");
                            var anonChat = await client.Contacts_ResolveUsername(Config.GetItem("AnonChatBot"));
                            Thread.Sleep(500);
                            await client.SendMessageAsync(anonChat, text);
                            Thread.Sleep(500);
                            await client.SendMessageAsync(anonChat, "/next");
                        }
                        break;
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
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
    }
}