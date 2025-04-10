#define CONSOLE
//#define LIBRARY
using System;
using Intermech.Bars;
using Intermech.Interfaces.Plugins;
using System.Windows.Forms;
using TeamCenterTry;
using Intermech.Interfaces;
using TeamCenterTry.XML_structures;
using Intermech.Interfaces.XmlExchange.XmlScripts;
using System.IO;

namespace TeamCenterAndIPSConnector
{
#if CONSOLE
    public class Start
    {

        public static void Main(string[] args)
        {
            Form1 form = new Form1();
            form.ShowDialog();

        }
    }
#endif
#if LIBRARY
    public class Main : IPackage
    { 


        /// <summary>
        /// Контейнер сервисов
        /// </summary>
        internal static IServiceProvider _serviceProvider;

        public Main()
        {

        }

        public string Name
        {
            get { return "Плагин расширения для выгрузки данных из TeamCenter"; }
        }

        /// <summary>
		/// Метод, вызываемый при загрузке модуля расширения
		/// </summary>
		/// <param name="serviceProvider">Контейнер сервисов</param>
        public void Load(IServiceProvider serviceProvider)
        {
            // Сохраняем ссылку на контейнер сервисов
            _serviceProvider = serviceProvider;

            Add_Bar_button();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        private void Add_Bar_button()
        {
            // Получаем ссылку на менеджер панелей инструментов и главного меню универсального клиента
            BarManager barManager = _serviceProvider.GetService(typeof(BarManager)) as BarManager;

            // Получаем ссылку на главное меню универсального клиента
            MenuBar menuBar = barManager.MenuBar;

            // Создаём элемент меню "Примеры"
            MenuItemBase ConnectorButton = new MenuBarItem("Импорт деталей из TeamCenter");

            // Присваиваем этому элементу уникальное имя
            ConnectorButton.CommandName = "Connector TeamCenter";

            MenuButtonItem itemSample1 = new MenuButtonItem("Запуск модального окна",
                new EventHandler(FormOpen));

            // Добавляем этот элемент в главное меню приложения
            ConnectorButton.Items.Add(itemSample1);
            menuBar.Items.Add(ConnectorButton);

        }
            public static void FormOpen(object Sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.ShowDialog();
        }
}
#endif
}
