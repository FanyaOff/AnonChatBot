using ConfigCreator;
using System.Drawing;
using System.Text;
using TL;
using TL.Methods;
using TwoCaptcha.Captcha;
using WTelegram;
using Config = ConfigCreator.Config;
using Console = Colorful.Console;

namespace AnonChatBot
{
    public class AnonChatBot
    {
        private static int msgCount = 0;
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
            try
            {
                if (!Directory.Exists("session"))
                    Directory.CreateDirectory("session");
                // log into WTelegram.log
                Helpers.Log = (lvl, str) =>
                {
                    Console.Write(str.Contains("FLOOD", StringComparison.OrdinalIgnoreCase) ? $"\nFLOOD WAIT! {str}" : string.Empty);
                    WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
                };
                Console.WriteAscii("AnonChatBot", Color.FromArgb(244, 212, 255));
                InitializeConfig(true);
                checkConfig();
                Console.WriteLine("[API] Connecting...", Color.Cyan);
                client = new Client(TgConfig);
                await client.LoginUserIfNeeded();
                client.OnUpdate += OnNewMessage;
                Console.WriteLine($"[API] Connected!\n[BOT] Waiting for message from {Config.GetItem("AnonChatBot")}", Color.Green);
                if (Convert.ToBoolean(Config.GetItem("BypassMode")))
                    Console.WriteLine($"[BypassMode] Bypass Mode is enabled! Current settings:\nMessages to spam: {Config.GetItem("BypassMessageCount")}\nCooldown: {Config.GetItem("BypassCooldown")}", Color.AntiqueWhite);
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Проверьте свой конфиг файл. Кажется, вы не заполнили пункт ApiHash или же не написали SpammingText в 1 строку!", Color.IndianRed);
                Console.WriteLine($"Ошибка: {ex}\n\nЕсли вы не можете пофиксить ошибку, отправьте скрин исключения разработчику", Color.IndianRed);
                Console.ReadKey();
                return;
            }
        }

        private static void checkConfig()
        {
            int nullValues = 0;
            Console.WriteLine("[ConfigValidator] Checking botConfig.ini", Color.LightGray);
            if (Config.GetItem("SpammingText") == null) { Console.WriteLine("[ConfigValidator] SpammingText is null"); nullValues++; }
            if (Config.GetItem("ApiID") == null) { Console.WriteLine("[ConfigValidator] ApiID is null"); nullValues++; }
            if (Config.GetItem("ApiHash") == null) { Console.WriteLine("[ConfigValidator] ApiHash is null"); nullValues++; }
            if (Config.GetItem("PhoneNumber") == null) { Console.WriteLine("[ConfigValidator] PhoneNumber is null"); nullValues++; }
            if (Config.GetItem("AnonChatBot") == null) { Console.WriteLine("[ConfigValidator] AnonChatBot is null"); nullValues++; }
            if (Config.GetItem("TriggerWord") == null) { Console.WriteLine("[ConfigValidator] TriggerWord is null"); nullValues++; }
            if (Config.GetItem("AutosolveCaptcha") == null) { Console.WriteLine("[ConfigValidator] AutosolveCaptcha is null"); nullValues++; }
            if (Config.GetItem("RuCaptchaApiKey") == null) { Console.WriteLine("[ConfigValidator] RuCaptchaApiKey is null"); nullValues++; }
            if (Config.GetItem("TextDelay") == null) { Console.WriteLine("[ConfigValidator] TextDelay is null"); nullValues++; }
            if (Config.GetItem("NextCommandDelay") == null) { Console.WriteLine("[ConfigValidator] NextCommandDelay is null"); nullValues++; }
            if (Config.GetItem("BypassMode") == null) { Console.WriteLine("[ConfigValidator] BypassMode is null"); nullValues++; }
            if (Config.GetItem("BypassMessageCount") == null) { Console.WriteLine("[ConfigValidator] BypassMessageCount is null"); nullValues++; }
            if (Config.GetItem("BypassCooldown") == null) { Console.WriteLine("[ConfigValidator] BypassCooldown is null"); nullValues++; }
            if (nullValues > 1)
            {
                Console.WriteLine($"[ConfigValidator] Количество пунктов не прошедших проверку: {nullValues}, пересоздаю конфиг файл", Color.GreenYellow);
                File.Delete("botConfig.ini");
                InitializeConfig(false);
                Console.WriteLine("[ConfigValidator] Конфиг был пересоздан, заполните его заного НЕ пропуская никаких значений!", Color.BlueViolet);
                Console.ReadKey();
                return;
            }
        }

        public static void InitializeConfig(bool isFirstCreated)
        {
            try
            {
                Config.Initialize("botConfig", null);
            }
            catch
            {
                Config.Add("SpammingText", "spamTextHere", "Текст который бот будет писать после триггер фразы");
                Config.Add("ApiID", "apiIdHere", "Берется тут: my.telegram.org");
                Config.Add("ApiHash", "apiHashHere", "Берется тут: my.telegram.org");
                Config.Add("PhoneNumber", "+79996665544", null);
                Config.Add("AnonChatBot", "AnonRuBot", "Айди анонимного бота, указывать без @");
                Config.Add("TriggerWord", "Собеседник найден", "Фраза, на которую бот будет срабатывать");
                Config.Add("AutosolveCaptcha", "false", "true, если вы хотите чтобы капча решалась автоматически. Требуется RuCaptcha api key");
                Config.Add("RuCaptchaApiKey", "apiKeyHere", "Апи ключ можно взять тут - https://rucaptcha.com/enterpage");
                Config.Add("TextDelay", "500", "Задержка отправления текста после триггер фразы");
                Config.Add("NextCommandDelay", "500", "Задержка отправления команды /next после отправления текста");
                Config.Add("BypassMode", "false", "Режим, который спамит n количество сообщений, затем уходит на перерыв");
                Config.Add("BypassMessageCount", "100", "Количество отправленных сообщений перед остановкой");
                Config.Add("BypassCooldown", "10000", "Задержка между отправленными сообщениями (в милисекундах)");
                Config.CreateConfigFile("botConfig", null);
                if (isFirstCreated)
                {
                    Console.WriteLine("[CONFIG] Кажется, вы не заполнили конфиг файл. Пожалуйста, заполните его и повторите попытку", Color.IndianRed);
                    Console.ReadKey();
                    return;
                }
                return;
            }
        }

        private static async Task OnNewMessage(IObject arg)
        {
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(Users, Chats);
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm:
                        Console.WriteLine($"[BOT] Received new message: {unm.message}", Color.Gray);
                        if (unm.message is Message message && message.media is MessageMediaPhoto { photo: Photo photo })
                        {
                            if (!Convert.ToBoolean(Config.GetItem("AutosolveCaptcha")))
                            {
                                Console.WriteLine("[CAPTCHA] Captcha detected, but auto solver turned off. Skipping...", Color.IndianRed);
                            }
                            else
                            {
                                var anonChat = await client.Contacts_ResolveUsername(Config.GetItem("AnonChatBot"));
                                // downloading captcha
                                var filename = $"{photo.id}.jpg";
                                Console.WriteLine("[CAPTCHA] Downloading " + filename, Color.Gray);
                                using var fileStream = File.Create(filename);
                                var type = await client.DownloadFileAsync(photo, fileStream);
                                fileName = $"{photo.id}.{type}";
                                fileStream.Close(); // necessary for the renaming
                                Console.WriteLine("[CAPTCHA] Download finished", Color.Gray);
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
                            var anonChat = await client.Contacts_ResolveUsername(Config.GetItem("AnonChatBot"));
                            var text = Config.GetItem("SpammingText");
                            if (Convert.ToBoolean(Config.GetItem("BypassMode")))
                            {
                                if (msgCount == Convert.ToInt32(Config.GetItem("BypassMessageCount")))
                                {
                                    await client.SendMessageAsync(anonChat, "/stop");
                                    msgCount = 0;
                                    Console.WriteLine($"[Bypass] Stop spamming. Delay: {Config.GetItem("BypassCooldown")}", Color.AntiqueWhite);
                                    Thread.Sleep(Convert.ToInt32(Config.GetItem("BypassCooldown")));
                                    await client.SendMessageAsync(anonChat, "/start");
                                }
                                Console.WriteLine($"[BOT] AnonChat message detected. Sending", Color.BlueViolet);
                                msgCount++;
                                Thread.Sleep(Convert.ToInt32(Config.GetItem("TextDelay")));
                                await client.SendMessageAsync(anonChat, StringRandomizator.RandomizateString(text));
                                Thread.Sleep(Convert.ToInt32(Config.GetItem("NextCommandDelay")));
                                await client.SendMessageAsync(anonChat, "/next");
                            }
                            else
                            {
                                Console.WriteLine("[BOT] AnonChat message detected. Sending", Color.BlueViolet);
                                Thread.Sleep(Convert.ToInt32(Config.GetItem("TextDelay")));
                                await client.SendMessageAsync(anonChat, StringRandomizator.RandomizateString(text));
                                Thread.Sleep(Convert.ToInt32(Config.GetItem("NextCommandDelay")));
                                await client.SendMessageAsync(anonChat, "/next");
                            }
                        }
                        break;
                }
        }

        static string solveCaptcha(string captchaApiKey, string captchaPath)
        {
            var solver = new TwoCaptcha.TwoCaptcha(captchaApiKey);
            Normal captcha = new Normal(captchaPath);
            Console.WriteLine("[CAPTCHA] Starting solving", Color.Magenta);
            try
            {
                solver.Solve(captcha).Wait();
                Console.WriteLine($"[CAPTCHA] Captcha Answer: {captcha.Code}");
                return captcha.Code;
            }
            catch (AggregateException e)
            {
                Console.WriteLine($"[CAPTCHA] Invalid captcha!", Color.Red);
                File.Delete(fileName);
                return null;
            }
        }
        static string TgConfig(string what)
        {
            switch (what)
            {
                case "api_id": return Config.GetItem("ApiID");
                case "api_hash": return Config.GetItem("ApiHash").Replace(" ", "");
                case "phone_number": return Config.GetItem("PhoneNumber");
                case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                case "session_pathname": return $"session/{Config.GetItem("PhoneNumber").Replace(" ", string.Empty)}.session";
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
    }
}