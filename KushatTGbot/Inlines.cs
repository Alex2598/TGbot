using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bots.Types;

namespace TelegramBotExperiments
{
    public class Inlines
    {
        
        public static List<string> PlaceMenuOptions()
        {
            return new List<string> { "Кир&Пич", "Другое место", "Не иду на обед" };
        }
        public static List<string> ChooseDrink()
        {
            return new List<string> { "Напиток"};
        }
        public static List<string> PreviosCategoryMenuOptions()
        {
            return new List<string> { "Два блюда", "Три блюда" /*, "Блюдо из меню (не работает)"*/, "Назад" };
        }

        public static List<string> CategoryMenuOptionsThreeDishes()
        {
            return new List<string> { "Салат", "Суп", "Горячее", "Назад" };
        }
        public static List<string> CategoryMenuOptionsTwoDishes()
        {
            return new List<string> { "Салат", "Суп", "Горячее", "Напиток", "Назад" };
        }
        public static List<string> BackMenuOptions()
        {

            return new List<string> { "Назад" };
        }

        public static List<string> RestartMenuOptions()
        {
            return new List<string> { "Перезаказать" };
        }
       

    }

}
