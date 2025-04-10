using Intermech.Interfaces;
using Intermech.Interfaces.XmlExchange.XmlScripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Xml;

namespace TeamCenterTry.XML_structures
{
    class CreateXml
    {
        const string Metadata = "MetadataBrief.xml";
        const string Objects = "Objects.xml";
        const string Relations = "Relations.xml";
        Random random = new Random();
        public Detail SelectedItem;

        /// <summary>
        /// Начинается создание и реализация Xml файлов, включающих информацию про объект,
        /// который будем импортировать в IPS
        /// </summary>
        public void StartWriteAndSaveXml()
        {
            XmlDocument metadata = CreateXmlDocument(Metadata);
            XmlDocument objects = CreateXmlDocument(Objects);
            XmlDocument relations = CreateXmlDocument(Relations);
            SaveXml(metadata,objects,relations);
        }


        /// <summary>
        /// Сохранение файлов и экспорт в IPS
        /// </summary>
        /// <param name="Metadata">Metadata.xml</param>
        /// <param name="Objects">Objects.xml</param>
        /// <param name="Relations">Relations.xml</param>
        public void SaveXml(XmlDocument Metadata, XmlDocument Objects, XmlDocument Relations)
        {
            // Формируем путь к папке в Temp
            string path = Path.Combine(Path.GetTempPath(), "import");

            // Проверяем, существует ли папка, и создаем её, если нет
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            SaveXmlDocument(Metadata, Path.Combine(path, CreateXml.Metadata));
            SaveXmlDocument(Objects, Path.Combine(path, CreateXml.Objects));
            SaveXmlDocument(Relations, Path.Combine(path, CreateXml.Relations));
            // Архивируем папку
            string zipPath = Path.Combine(Path.GetTempPath(), "Importer.zip");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath); // Удаляем старый архив, если он существует
            }
            ZipFile.CreateFromDirectory(path, zipPath);

            // Используем SessionKeeper для выполнения команды
            using (SessionKeeper sessionKeeper = new SessionKeeper())
            {
                IUserSession session = sessionKeeper.Session;
                XmlImportScriptCommand.Execute(zipPath, 1173998, session, out string ImportLog);
                MessageBox.Show(zipPath);
                MessageBox.Show("Лог: " + ImportLog);
            }
        }



        /// <summary>
        /// Сохранение файлов Xml
        /// </summary>
        /// <param name="document">Сам файл Xml</param>
        /// <param name="fileName">Название файла</param>
        private void SaveXmlDocument(XmlDocument document, string fileName)
        {
            using (var writer = new StreamWriter(fileName,false))
            {
                document.Save(writer);
            }
        }


        /// <summary>
        /// Создание документа
        /// в нем реализуются методы для создания трех обязательных Xml 
        /// файлов для выгрзуки в IPS
        /// </summary>
        /// <param name="fileName">Название файла, который будет обрабатываться и создаваться на его основе файл Xml</param>
        /// <returns>возвращает Xml документ</returns>
        private XmlDocument CreateXmlDocument(string fileName)
        {
            XmlDocument doc;
            switch (fileName)
            {
                case Metadata:
                    {
                        doc = new XmlDocument();
                        CreateMetaDataBrief(doc);
                        return doc;
                    }
                case Objects:
                    {
                        doc = new XmlDocument();
                        CreateObjects(doc);
                        return doc;
                    }
                case Relations:
                    {
                        doc = new XmlDocument();
                        CreateRelations(doc);
                        return doc;
                    }
                default:
                    Console.WriteLine("Ошибка в CreateXmlDocument в конструкции switch");
                    break;
            }          
            return doc = new XmlDocument();
        }



        /// <summary>
        /// Создает xml узел для файла "связей" realtions
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Возвращает обработанный xml Документ</returns>
        private XmlDocument CreateRelations(XmlDocument doc)
        {
            XmlElement root = doc.CreateElement(XmlValuesConst.RELATIONSDATASET);
            doc.AppendChild(root);
            return doc;
        }


        /// <summary>
        /// создает xml узел для файла "объектов" objects
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Возвращает обработанный xml Документ</returns>
        private XmlDocument CreateObjects(XmlDocument doc)
        {
            XmlElement root = doc.CreateElement(XmlValuesConst.OBJECTSDATASET);
            doc.AppendChild(root);
                
            int CountOfObjects = SelectedItem.Versions.Count;
            //random.Next(145000, 999999);
            int test = 145900;
            for (int index = 0; index < CountOfObjects; index++)
            {
                XmlElement Object = doc.CreateElement(XmlValuesConst.OBJECT);
                RealizeObject(doc,Object,index,test);
                root.AppendChild(Object);
            }
            
            return doc;
        }


        /// <summary>
        /// Создаю структуру Objects.Xml
        /// В которой будет храниться информация о объекте и объектах, используемых в составе
        /// </summary>
        /// <param name="doc">Xml Документ, в котором создается эта структура</param>
        /// <param name="Object">Узел, в котором будем работать.</param>
        /// <returns></returns>
        private XmlElement RealizeObject(XmlDocument doc,XmlElement Object, int index, int test)
        {
            // локальный идентификатор версии объекта.
            XmlElement FObjId = doc.CreateElement(XmlValuesConst.F_OBJECT_ID);
            // идентификатор типа объекта (в файле MetadataBrief.xml).
            XmlElement FObjectType = doc.CreateElement(XmlValuesConst.F_OBJECT_TYPE);
            XmlElement FVersionId = doc.CreateElement(XmlValuesConst.F_VERSION_ID);
            FObjId.InnerText = test.ToString();
            FVersionId.InnerText = index.ToString();
            switch (SelectedItem.Type)
            {
                case "Детали":
                    FObjectType.InnerText = "1052";
                    break;
            }

            XmlElement Attributes = doc.CreateElement(XmlValuesConst.ATTRIBUTES);

            // Переделать
            string[] AttrValue = new string[3] { XmlValuesConst.META_ATTRIBUTES_OBOZNACHENIE, XmlValuesConst.META_ATTRIBUTES_NAME, XmlValuesConst.META_ATTRIBUTES_WEIGHT};

            RealizeAttributes(doc,Attributes, AttrValue, index);

            Object.AppendChild(FObjId);
            Object.AppendChild(FObjectType);
            Object.AppendChild(FVersionId);
            Object.AppendChild(Attributes);


            return Object;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="Attributes"></param>
        /// <param name="AttrValue"></param>
        /// <returns></returns>
        private XmlElement RealizeAttributes(XmlDocument doc, XmlElement Attributes, string[] AttrValue, int index)
        {
            int CountOfValue = AttrValue.Length;          
            
            for (int AttrIndex = 0; AttrIndex < CountOfValue; AttrIndex++)
            {
                XmlElement attribute = doc.CreateElement(XmlValuesConst.ATTRIBUTE);
                //идентификатор типа атрибута (в файле MetadataBrief.xml).
                XmlElement FAttributeId = doc.CreateElement(XmlValuesConst.F_ATTRIBUTE_ID);
                //порядковый номер значения атрибута (начинается с 0).
                XmlElement FInlistID = doc.CreateElement(XmlValuesConst.F_INLIST_ID);
                //значение атрибута. Формируется в зависимости от типа атрибута.
                XmlElement FValue = doc.CreateElement(XmlValuesConst.F_VALUE);
                XmlElement F_EI = doc.CreateElement(XmlValuesConst.F_EI);
                XmlElement FStringValue = doc.CreateElement(XmlValuesConst.F_STRING_VALUE);

                switch (AttrValue[AttrIndex])
                {
                    case XmlValuesConst.META_ATTRIBUTES_NAME:
                        FAttributeId.InnerText = "10";
                        FInlistID.InnerText = "0";
                        FValue.InnerText = SelectedItem.Name.ToString();
                        break;
                    case XmlValuesConst.META_ATTRIBUTES_OBOZNACHENIE:
                        FAttributeId.InnerText = "9";
                        FInlistID.InnerText = "0";
                        FValue.InnerText = SelectedItem.Designation.ToString();
                        break;
                    case XmlValuesConst.META_ATTRIBUTES_WEIGHT:
                        FStringValue.InnerText = $"{SelectedItem.Versions[index].GetProperty("h47_HR03").DoubleValue} кг";
                        FAttributeId.InnerText = "1000";
                        FInlistID.InnerText = "0";
                        FValue.InnerText = $"{SelectedItem.Versions[index].GetProperty("h47_HR03").DoubleValue} кг";
                        F_EI.InnerText = "кг";
                        attribute.AppendChild(FStringValue);
                        attribute.AppendChild(F_EI);
                        break;
                }
                attribute.AppendChild(FAttributeId);
                attribute.AppendChild(FInlistID);
                attribute.AppendChild(FValue);
                Attributes.AppendChild(attribute);
            }
            return Attributes;
        }




        /// <summary>
        /// создает xml узел для создания файла "мета информации" MetaDataBrief
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Возвращает обработанный xml Документ</returns>
        private XmlDocument CreateMetaDataBrief(XmlDocument doc)
        {
            XmlElement root = doc.CreateElement(XmlValuesConst.XML_METADATABRIEF);
            doc.AppendChild(root);

            XmlElement attributeTypes = doc.CreateElement(XmlValuesConst.ATTRIBUTE_TYPES);
            //Реализация attributeTypes (заполнение данными и т.д.)
            attributeTypes = RealizeAttributeTypes(doc, attributeTypes);
            root.AppendChild(attributeTypes);

            XmlElement objectTypes = doc.CreateElement(XmlValuesConst.OBJECT_TYPES);
            string[] NamesOfObjects = new string[1] {SelectedItem.Type.ToString()};
            objectTypes = RealizeObjectTypes(doc,objectTypes, NamesOfObjects);
            root.AppendChild(objectTypes);

            XmlElement relationTypes = doc.CreateElement(XmlValuesConst.RELATION_TYPES);
            root.AppendChild(relationTypes);
            
            return doc;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="objectTypes"></param>
        /// <param name="NamesOfObjects"></param>
        /// <returns></returns>
        private XmlElement RealizeObjectTypes(XmlDocument doc, XmlElement objectTypes,string[] NamesOfObjects)
        {
            XmlElement objectType = doc.CreateElement(XmlValuesConst.OBJECT_TYPE);
            XmlElement FObjectType = doc.CreateElement(XmlValuesConst.F_OBJ_TYPE);
            XmlElement FObjectTypeName = doc.CreateElement(XmlValuesConst.F_OBJ_TYPE_NAME);
            foreach (string NameOfObject in NamesOfObjects)
            {
                // ?
                switch (NameOfObject)
                {
                    case XmlValuesConst.DETALI:
                        FObjectType.InnerText = "1052";
                        FObjectTypeName.InnerText = XmlValuesConst.DETALI;
                        break;
                }
            }

            objectType.AppendChild(FObjectType);
            objectType.AppendChild(FObjectTypeName);

            objectTypes.AppendChild(objectType);
            return objectTypes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="attributeTypes"></param>
        /// <returns></returns>
        private XmlElement RealizeAttributeTypes(XmlDocument doc, XmlElement attributeTypes)
        {
            // Реализую все аттрибуты, которые передаются из тим центра.
            // ====ЗАМЕНИТЬ==== (чтобы из Тимчика подавались данные по объекту)
            string[] NamesOfAttributes = new string[3] { "Обозначение", "Наименование", "Масса"};
            foreach(string AttributeName in NamesOfAttributes)
            {
                XmlElement attributeType = doc.CreateElement(XmlValuesConst.ATTRIBUTE_TYPE);
                attributeType = RealizeAndFillAttributeType(doc, attributeType, AttributeName);
                attributeTypes.AppendChild(attributeType);
            }
            
            return attributeTypes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="attributeType"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        private XmlElement RealizeAndFillAttributeType(XmlDocument doc, XmlElement attributeType, string AttributeName)
        {
            XmlElement FAttributeID = doc.CreateElement(XmlValuesConst.F_ATTRIBUTE_ID);
            XmlElement FName = doc.CreateElement(XmlValuesConst.F_NAME);
            XmlElement FAttributeType = doc.CreateElement(XmlValuesConst.F_ATTRIBUTE_TYPE);

            switch (AttributeName)
            {
                case XmlValuesConst.META_ATTRIBUTES_OBOZNACHENIE:
                    FAttributeID.InnerText = "9";
                    FName.InnerText = XmlValuesConst.META_ATTRIBUTES_OBOZNACHENIE;
                    break;
                case XmlValuesConst.META_ATTRIBUTES_NAME:
                    FAttributeID.InnerText = "10";
                    FName.InnerText = XmlValuesConst.META_ATTRIBUTES_NAME;
                    break;
                case XmlValuesConst.META_ATTRIBUTES_WEIGHT:
                    FAttributeID.InnerText = "1000";
                    FAttributeType.InnerText = "13";
                    FName.InnerText = XmlValuesConst.META_ATTRIBUTES_WEIGHT;
                    break;
            }
            attributeType.AppendChild(FAttributeType);
            attributeType.AppendChild(FAttributeID);
            attributeType.AppendChild(FName);
            return attributeType;
        }
    }
}
