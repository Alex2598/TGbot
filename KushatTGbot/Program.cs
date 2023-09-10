using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;


using Telegram.Bot.Args;


namespace TelegramBotExperiments
{
    class Program
    {
        public static List<string> selectedSalads = new List<string>();
        public static List<string> selectedMainDishes = new List<string>();
        public static List<string> selectedHotDishes = new List<string>();
        static ITelegramBotClient bot = new TelegramBotClient("5851097204:AAGAwwzbpojD82XLW0z4rA2JBbaukweg_vE");
        static Dictionary<long, int> userStep = new Dictionary<long, int>();
        static List<long> chatIds = new List<long>();
        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, 
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
           
            var timer = new System.Timers.Timer();
            timer.Elapsed += TimerElapsed;
            timer.Interval = 6000; // интервал в миллисекундах (1 минута)
            timer.Start();
            Console.ReadLine();
           
        }
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            var dishRecorder = new DishRecorder( "Database\\database.xlsx");
            string databasePath = "Database\\database.xlsx";
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                var chatId = message.Chat.Id;
                if (!chatIds.Contains(chatId))
                {
                    chatIds.Add(chatId);
                }
                if (message == null || message.Type != MessageType.Text)
                    return;
                else if (!await IsUserInChannelAsync(chatId, "-1001959615737"))
                {
                    await bot.SendTextMessageAsync(chatId, "Вы не состоите в нужном канале.");
                    return;
                }
                else if (message.Text.ToLower() == "/start")
                {
                    
                    await using Stream stream = System.IO.File.OpenRead("IMG_0092-H.jpg");
                    await botClient.SendPhotoAsync(chatId: chatId, photo: InputFile.FromStream(stream: stream, fileName: "IMG_0092-H.jpg"));
                    // Отправить сообщение с кнопками выбора
                    await botClient.SendTextMessageAsync(message.Chat, "Выберите место:", replyMarkup: GetInlineKeyboard(Inlines.PlaceMenuOptions()));

                    userStep[message.Chat.Id] = 1;
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Я не понимаю вас, воспользуйтесь клавиатурой для выбора блюд. Если ничего не появляется напишите /start в чате");
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var chatId = callbackQuery.Message.Chat.Id;
                if (userStep.ContainsKey(chatId))
                {
                    var currentStep = userStep[chatId];
                    if (currentStep == 1)
                    {
                        var selectedPlace = callbackQuery.Data;
                        switch (selectedPlace)
                        {
                            case "Кир&Пич":
                                userStep[chatId] = 2;
                                
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.PreviosCategoryMenuOptions()));
                                break;
                            case "Другое место":
                                userStep[chatId] = 2;
                                dishRecorder.DeleteSelectedDishes(chatId);
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Для возмещения стоимости обедов необходимо ПРЕДЪЯВИТЬ кассовый чек администратору! (не более 350р/чел)", replyMarkup: GetInlineKeyboard(Inlines.BackMenuOptions()));
                                break;
                            case "Не иду на обед":
                                userStep[chatId] = 2;
                                dishRecorder.DeleteSelectedDishes(chatId);
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Удачного дня!", replyMarkup: GetInlineKeyboard(Inlines.BackMenuOptions()));
                                break;
                            default:
                                dishRecorder.DeleteSelectedDishes(chatId);
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите место:", replyMarkup: GetInlineKeyboard(Inlines.PlaceMenuOptions()));
                                break;
                        }
                    }
                    else if (currentStep == 2)
                    {
                        var selectedCategory = callbackQuery.Data;
                        dishRecorder.DeleteSelectedDishes(chatId);
                        switch (selectedCategory)
                        {
                            case "Два блюда":
                                userStep[chatId] = 3;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                                break;
                            case "Три блюда":
                                userStep[chatId] = 5;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsThreeDishes()));
                                break;
                            //case "Блюдо из меню (не работает)":
                            //    userStep[chatId] = 3;
                            //    await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите горячее:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptions()));
                            //    break;
                            case "Назад":
                                userStep[chatId] = 1;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите место:", replyMarkup: GetInlineKeyboard(Inlines.PlaceMenuOptions()));
                                break;
                            default:
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.PreviosCategoryMenuOptions()));
                                break;
                        }
                    }
                    else if (currentStep == 3)
                    {
                        string salad1, salad2, salad3, maindish1, maindish2, hotdish1, hotdish2, hotdish3,drink1,drink2,drink3;
                        var selectedCategoryDish = callbackQuery.Data;
                        switch (selectedCategoryDish)
                        {
                            case "Салат":         
                                userStep[chatId] = 4;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите салат:", replyMarkup: GetInlineKeyboard(DatabasesMenu.SaladMenuOptions(out salad1, out salad2, out salad3)));
                                break;
                            case "Суп":
                                userStep[chatId] = 4;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите основное:", replyMarkup: GetInlineKeyboard(DatabasesMenu.MainDishMenuOptions(out maindish1, out maindish2)));
                                break;
                            case "Горячее":
                                userStep[chatId] = 4;

                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите горячее:", replyMarkup: GetInlineKeyboard(DatabasesMenu.HotDishMenuOptions(out hotdish1, out hotdish2, out hotdish3)));
                                break;
                            case "Напиток":
                                userStep[chatId] = 4;

                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите Напиток:", replyMarkup: GetInlineKeyboard(DatabasesMenu.DrinkMenuOptions(out drink1, out drink2, out drink3)));
                                break;
                            case "Назад":
                                userStep[chatId] = 2;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите место:", replyMarkup: GetInlineKeyboard(Inlines.PreviosCategoryMenuOptions()));
                                break;
                            default:
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                                break;
                        }
                    }
                    else if (currentStep == 4)
                    {
                        string salad1, salad2, salad3, maindish1, maindish2, hotdish1, hotdish2, hotdish3, drink1 ,drink2, drink3 ;
                        DatabasesMenu.SaladMenuOptions(out salad1, out salad2, out salad3);
                        DatabasesMenu.MainDishMenuOptions(out maindish1, out maindish2);
                        DatabasesMenu.HotDishMenuOptions(out hotdish1, out hotdish2, out hotdish3);
                        DatabasesMenu.DrinkMenuOptions(out drink1, out drink2, out drink3);
                        var selectedDish = callbackQuery.Data;
                        var user = await botClient.GetChatMemberAsync(chatId, callbackQuery.From.Id);
                        var userFirstName = user.User.FirstName;
                        var userLastName = user.User.LastName;
                        if (selectedDish == salad1 || selectedDish == salad2 || selectedDish == salad3)
                        {
                            List<string> salads = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName,  salads, new List<string>(), new List<string>(), new List<string>());
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 3;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                        }
                       else if (selectedDish == maindish1 || selectedDish == maindish2)
                        {
                            List<string> mainDishes = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName,  new List<string>(), new List<string>(), mainDishes, new List<string>());
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 3;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                        }
                        else if (selectedDish == hotdish1 || selectedDish == hotdish2 || selectedDish == hotdish3)
                        {
                            List<string> hotDishes = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName,  new List<string>(), hotDishes, new List<string>(), new List<string>());
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 3;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                        }
                        else if (selectedDish == drink1 || selectedDish == drink2 || selectedDish == drink3)
                        {
                            List<string> drinks = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName, new List<string>(), new List<string>(), new List<string>(), drinks);
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 3;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                        }
                        else if (selectedDish == "Назад")
                        {
                            userStep[chatId] = 3;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsTwoDishes()));
                        }
                         databasePath = "Database\\database.xlsx";
                        DishRecorder dishRecorders = new DishRecorder(databasePath);
                        var selectedDishes = dishRecorders.CheckTwoDishFields(chatId, bot);
                        foreach (var dish in selectedDishes)
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали следующие блюда:\n{dish}");
                            userStep[chatId] = 1;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Если вы хотите изменить заказ нажмите кнопку ниже(Внимание!!! Нажатие оставшихся кнопок выше очистит ваш заказ!):", replyMarkup: GetInlineKeyboard(Inlines.RestartMenuOptions()));
                        }
                    }
                    else if (currentStep == 5)
                    {
                        string salad1, salad2, salad3, maindish1, maindish2, hotdish1, hotdish2, hotdish3;
                        var selectedCategoryDish = callbackQuery.Data;
                        switch (selectedCategoryDish)
                        {
                            case "Салат":
                                userStep[chatId] = 6;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите салат:", replyMarkup: GetInlineKeyboard(DatabasesMenu.SaladMenuOptions(out salad1, out salad2, out salad3)));
                                break;
                            case "Суп":
                                userStep[chatId] = 6;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите основное:", replyMarkup: GetInlineKeyboard(DatabasesMenu.MainDishMenuOptions(out maindish1, out maindish2)));
                                break;
                            case "Горячее":
                                userStep[chatId] = 6;

                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите горячее:", replyMarkup: GetInlineKeyboard(DatabasesMenu.HotDishMenuOptions(out hotdish1, out hotdish2, out hotdish3)));
                                break;
                            case "Назад":
                                userStep[chatId] = 2;
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите место:", replyMarkup: GetInlineKeyboard(Inlines.PreviosCategoryMenuOptions()));
                                break;
                            default:
                                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsThreeDishes()));
                                break;
                        }
                    }
                    else if (currentStep == 6)
                    {
                        string salad1, salad2, salad3, maindish1, maindish2, hotdish1, hotdish2, hotdish3;
                        DatabasesMenu.SaladMenuOptions(out salad1, out salad2, out salad3);
                        DatabasesMenu.MainDishMenuOptions(out maindish1, out maindish2);
                        DatabasesMenu.HotDishMenuOptions(out hotdish1, out hotdish2, out hotdish3);
                        var selectedDish = callbackQuery.Data;
                        var user = await botClient.GetChatMemberAsync(chatId, callbackQuery.From.Id);
                        var userFirstName = user.User.FirstName;
                        var userLastName = user.User.LastName;
                        if (selectedDish == salad1 || selectedDish == salad2 || selectedDish == salad3)
                        {
                            List<string> salads = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName, salads, new List<string>(), new List<string>(), new List<string>());
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 5;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsThreeDishes()));
                        }
                        else if (selectedDish == maindish1 || selectedDish == maindish2)
                        {
                            List<string> mainDishes = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName, new List<string>(), new List<string>(), mainDishes, new List<string>());

                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 5;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsThreeDishes()));
                        }
                        else if (selectedDish == hotdish1 || selectedDish == hotdish2 || selectedDish == hotdish3)
                        {
                            List<string> hotDishes = new List<string>() { selectedDish };
                            dishRecorder.RecordDishes(chatId, userFirstName, userLastName, new List<string>(), hotDishes, new List<string>(), new List<string>());
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали {selectedDish}");
                            userStep[chatId] = 5;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выбирайте категорию", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsThreeDishes()));
                        }
                        else if (selectedDish == "Назад")
                        {
                            userStep[chatId] = 5;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: GetInlineKeyboard(Inlines.CategoryMenuOptionsThreeDishes()));
                        }
                        databasePath = "Database\\database.xlsx";
                        DishRecorder dishRecorders = new DishRecorder(databasePath);
                        var selectedDishes = dishRecorders.CheckThreeDishFields(chatId);
                        foreach (var dish in selectedDishes)
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали следующие блюда:\n{dish}");
                            userStep[chatId] = 1;
                            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, "Если вы хотите изменить заказ нажмите кнопку ниже (Внимание!!! Нажатие оставшихся кнопок выше очистит ваш заказ!):", replyMarkup: GetInlineKeyboard(Inlines.RestartMenuOptions()));
                        }
                    }
                }
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
        }
        static async Task<bool> IsUserInChannelAsync(long chatId, string channelId)
        {
            var chatMember = await bot.GetChatMemberAsync(channelId, chatId);
            return chatMember.Status != ChatMemberStatus.Left && chatMember.Status != ChatMemberStatus.Kicked;
        }
        public static InlineKeyboardMarkup GetInlineKeyboard(List<string> options)
        {
            if (options != null && options.Count > 0)
            {
                var keyboard = new List<List<InlineKeyboardButton>>();

                foreach (var option in options)
                {
                    keyboard.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(option) });
                }
                var inlineKeyboard = new InlineKeyboardMarkup(keyboard);
                return inlineKeyboard;
            }
            return null;
        }
        private static async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Отправка уведомления в определенное время
            var currentTime = DateTime.Now.TimeOfDay;
            var targetTime1 = new TimeSpan(20, 05, 00); // время для первого уведомления (12:00:00)
            //var targetTime2 = new TimeSpan(19, 56, 0); // время для второго уведомления (11:30:00)

            if (currentTime == targetTime1)  // проверка времени для первого уведомления
            {
                foreach (var chatId in chatIds)
                {
                    await bot.SendTextMessageAsync(chatId, "Заказ обедов завершиться через 15 минут. Успейте заказать!");
                }
            }

           
        }
    }
}
