using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teamcenter.ClientX;
using Teamcenter.Services.Strong.Query._2012_10.Finder;
using User = Teamcenter.Soa.Client.Model.Strong.User;
using TeamCenterTry;
using Teamcenter.Services.Strong.Core;
using Teamcenter.Schemas.Soa._2006_03.Base;
using Teamcenter.Soa.Client.Model.Strong;
using System.Xml.Linq;
using Teamcenter.Soa.Exceptions;
using Session = Teamcenter.ClientX.Session;
using Teamcenter.Soa.Client.Model;
using System.Runtime.InteropServices;
using Teamcenter.Soa.Common;
using ObjectPropertyPolicy = Teamcenter.Soa.Common.ObjectPropertyPolicy;
using PolicyType = Teamcenter.Soa.Common.PolicyType;
using PolicyProperty = Teamcenter.Soa.Common.PolicyProperty;
using Teamcenter.Soa.Internal.Client.Model;
using Teamcenter.Hello;
using Teamcenter.Services.Internal.Strong.Core._2011_06.ICT;
using TeamCenterTry.XML_structures;
using Teamcenter.Services.Strong.Query._2007_09.SavedQuery;

namespace TeamCenterTry
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        String serverHost = "SecretServerHostName";
        String[] attributes = { "item_revision", "object_name", "item_id", "revision_list", "item_revision_id", "h47_HR03", "user_name" };
        new string Name = string.Empty;
        string ItemID = string.Empty;
        // Home - не используется и не должен!
        HomeFolder home = new HomeFolder();
        Query query = new Query();
        // dm - не используется и пока не был нужен. Но может пригодиться.
        DataManagement dm = new DataManagement();

        public Form1()
        {
            
            InitializeComponent();


        }


        /// <summary>
        /// Отображает данные в форме(которая предназначана для показа аттрибутов или первостепенных данных детали)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListForItems.Clear();
            Detail SelectedItemFromConnector = (Detail)listBox1.SelectedItem;
            ListForItems.Items.Add($"Наименование - {SelectedItemFromConnector.Name}");
            ListForItems.Items.Add($"Обозначение - {SelectedItemFromConnector.Designation}");
        }


        /// <summary>
        /// Запускает и дает начало всей структуре, связанной с TeamCenter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Start();
        }
        

        /// <summary>
        /// запускает программу, получает данные из ячеек и выводит результат в лист бокс
        /// </summary>
        private void Start()
        {
            Teamcenter.ClientX.Session session = new Teamcenter.ClientX.Session(serverHost);
            DataManagementService dmService = DataManagementService.getService(Teamcenter.ClientX.Session.getConnection());
            listView1.Clear();
            listBox1.DisplayMember = "Designation";
            GetAndSetQuery();
            CreateAndSettingProgressBar();
            try
            {
                User user = session.login();
                AddLog("Успешное подключение к TeamCenter");
                Teamcenter.Soa.Client.Model.ModelObject[] AllObject = GetObjectFromQuery(dmService);
                SetStepProgressBar(AllObject);
                FillListBox(dmService, AllObject);
                AddLog($"Успешно найдено - {AllObject.Count()} деталей");
            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                session.logout();
                AddLog($"Успешно закрыта сессия");
                RefreshProgressBar();
            }
        }


        /// <summary>
        /// Задает шаг ProgressBar
        /// </summary>
        /// <param name="AllObject">количество объектов в запросе</param>
        private void SetStepProgressBar(Teamcenter.Soa.Client.Model.ModelObject[] AllObject)
        {
            progressBar1.Step = progressBar1.Maximum / AllObject.Length;
        }


        /// <summary>
        /// Заполнение listView объектами из TeamCenter
        /// </summary>
        /// <param name="dmService"></param>
        /// <param name="AllObject"></param>
        private void FillListBox(DataManagementService dmService, Teamcenter.Soa.Client.Model.ModelObject[] AllObject)
        {
            for (int i = 0; i < AllObject.Length; i++)
            {
                progressBar1.PerformStep();

                if (!(AllObject[i] is WorkspaceObject))
                    continue;
                WorkspaceObject wo = (WorkspaceObject)AllObject[i];

                try
                {

                    Teamcenter.Soa.Client.Model.ModelObject[] objects = { wo };
                    dmService.GetProperties(objects, attributes);
                    Teamcenter.Soa.Client.Model.ModelObject[] attr = wo.GetProperty("revision_list").ModelObjectArrayValue;
                    ItemRevision[] Array = new ItemRevision[attr.Length];
                    attr.CopyTo(Array, 0);
                    dmService.GetProperties(Array, attributes);

                    // Array[j].GetProperty("h47_HR03").DoubleValue - получение свойства массы.

                    //polnaya xyita но пока оставим так)
                    Detail Detail = new Detail();
                    Detail.Designation = wo.GetProperty("item_id").StringValue;
                    Detail.Name = wo.Object_name;
                    Detail.Versions = Array.ToList();
                    listBox1.Items.Add(Detail);
                    AddLog($"Успешно добавлена в список деталь под номером {i}");
                    FinallyMessage(AllObject, i);
                }
                catch (NotLoadedException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("The Object Property Policy ($TC_DATA/soa/policies/Default.xml) is not configured with this property.");
                }

                // Вызываем мисье ak(сборщик мусора)
                GC.Collect();
            }
        }

        private void FinallyMessage(Teamcenter.Soa.Client.Model.ModelObject[] AllObject, int i)
        {
            if (i + 1 == AllObject.Length)
            {
                CompleteProgressBar();
                MessageBox.Show("Поиск завершен успешно");
            }
        }


        /// <summary>
        /// Добавление логгирования в форму listview
        /// </summary>
        /// <param name="Log">Строковое значение, обозначающее этот лог</param>
        private void AddLog(string Log)
        {
            listView1.Items.Add($"{DateTime.Now.ToLongTimeString()} {Log}"); // ПОД ВОПРОСОМ ??
            listView1.Update();
        }


        /// <summary>
        /// По полученным uids загружает фактические объекты в массив
        /// </summary>
        /// <param name="dmService"></param>
        /// <returns>возвращает массив найденных объектов</returns>
        private Teamcenter.Soa.Client.Model.ModelObject[] GetObjectFromQuery( DataManagementService dmService)
        {
            string[] uids = CollectingObjectsUids();

            Teamcenter.Soa.Client.Model.ServiceData sd = dmService.LoadObjects(uids);
            Teamcenter.Soa.Client.Model.ModelObject[] AllObject = new Teamcenter.Soa.Client.Model.ModelObject[sd.sizeOfPlainObjects()];

            for (int k = 0; k < sd.sizeOfPlainObjects(); k++)
            {
                AllObject[k] = sd.GetPlainObject(k);
            }

            return AllObject;
        }


        /// <summary>
        /// По запросу находит uids объектов и копирует в массив
        /// </summary>
        /// <returns>возвращает массив uids</returns>
        private string[] CollectingObjectsUids()
        {
            // TODO Подумать над тем, нужно ли вытаскивать в отдельные методы для читаемости
            QueryResults found = query.queryItems(Name, ItemID);
            string[] uids = new String[found.ObjectUIDS.Length];
            found.ObjectUIDS.CopyTo(uids, 0);
            return uids;
        }


        /// <summary>
        /// Принудительное выполнение в прогрессбара,
        /// Делает прогресБар зеленым и 100%
        /// </summary>
        private void CompleteProgressBar()
        {
            progressBar1.Value = 100;
            progressBar1.ForeColor = Color.LightGreen;
        }


        /// <summary>
        /// Перезагрузка прогрессбара до начальных настроек в рамках этого проекта.
        /// </summary>
        private void RefreshProgressBar()
        {
            progressBar1.Refresh();
            progressBar1.Value = 0;
        }


        /// <summary>
        /// Создание прогрессбара, задание начальных параметров.
        /// </summary>
        private void CreateAndSettingProgressBar()
        {
            progressBar1.ForeColor = Color.LightBlue;
            progressBar1.Style = ProgressBarStyle.Continuous;
        }


        /// <summary>
        /// Задает значения для поиска в TeamCenter
        /// </summary>
        private void GetAndSetQuery()
        {
            Name = textBox1.Text;  // Наименование 
            ItemID = textBox2.Text; // Обозначение
        }


        /// <summary>
        /// Сохраняет файлы в виде XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine(listBox1.SelectedIndex);
                Detail SelectedItem = new Detail();
                // Проверка, выбран ли объект
                if (listBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Важно! \nВыберите объект из списка!");
                }
                else
                {
                    SelectedItem = (Detail)listBox1.SelectedItem;
                    ListForItems.Items.Add(SelectedItem.Name);
                    CreateXml Connector = new CreateXml();
                    Connector.SelectedItem = SelectedItem;
                    Connector.StartWriteAndSaveXml();
                }
            }
            catch
            {
                Console.WriteLine(e);
            }
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}
