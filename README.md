# AnonChatBot
Простой спаммер для анонимного чата в Telegram

# [Последний релиз](https://github.com/FanyaOff/AnonChatBot/releases/tag/random)

# Используемые библиотеки
- [WTelegramClient](https://github.com/wiz0u/WTelegramClient)
- [2captcha-csharp](https://github.com/2captcha/2captcha-csharp)
- [ConfigCreator](https://github.com/FanyaOff/Config-Creator)

# [Демонстрация работы бота](https://www.youtube.com/watch?v=tdgiPBhBsfg)

# Настройка

- **Перед настройкой запустите программу, чтобы создался конфиг файл**

![image](https://user-images.githubusercontent.com/73064979/212462756-f374db31-b069-4cdd-b4cb-7e3bba51b63e.png)

Приступим к настройке конфиг файла

![image](https://user-images.githubusercontent.com/73064979/212462788-a9ce07b7-ef6f-4a9b-a9b1-53a92f362906.png)

## [SpammingText]
Данный пункт отвечает за текст, который бот будет спамить
## [ApiID] & [ApiHash]
1) Заходим на сайт https://my.telegram.org

2) Вводим номер телефона к которому привязан аккаунт(код придет вам в личные сообщения вашего телеграм аккаунта, номер не нужен)

3) После введения кода попадаем в меню, в котором вы должны выбрать "API development tools"

4) Тут мы заполняем ТОЛЬКО "App title" & "Short name", выбираем Android и жмем "Create Aplication"

5) После создания мы получаем ApiID и ApiHash, вставляем его в конфиг и сохраняемся

## [TriggerWord]
Данный пункт отвечает за фразу, на которой бот будет срабатывать. Под каждый бот свой разный, например под AnonRuBot подойдет фраза "Собеседник найден"

## [AutosolveCaptcha]
Данный параметр отвечает за то, будет ли капча решаться автоматически
true - Включить авторешение капчи
false - Выключить авторешение капчи
Если вы выбрали true, то вам нужен будет "RuCaptchaApiKey"

## [RuCaptchaApiKey]
- Для его получения регистрируемся на сайте https://rucaptcha.com/enterpage и пополняем баланс на рублей 30, поверьте, вам этого хватит на 1000 капч

После пополнения спускаемся чуть ниже и копируем апи ключ 

![image](https://user-images.githubusercontent.com/73064979/212463224-29376ed1-7f27-471d-bc7b-afe660037626.png)

С настройкой завершили, теперь переходим к использованию

После запуска, вам должен прийти код вам в личные сообщения телеграмма, вводим его

![image](https://user-images.githubusercontent.com/73064979/212463282-0b3c0ca6-95a5-4a53-aff2-7b2a2fcd53da.png)

После введенного кода генерируется сессия, поэтому каждый раз вводить код необязательно
Если вы видите "[API] Connected!", то значит все получилось!

Остается только начать искать собеседника и наблюдать как за вас спамит бот:)



