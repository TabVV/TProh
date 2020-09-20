using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

using FRACT = System.Decimal;

using SkladAll;
using PDA.Service;

namespace Proh
{
    public class NSI : NSIAll
    {
        // имена таблиц
        public const string NS_USER     = "NS_USER";
        public const string NS_POST     = "NS_SPST";
        public const string NS_SHLG     = "NS_SZMK";
        public const string NS_PURP     = "NS_SCEL";

        public const string BD_TINF     = "BD_TINF";

        public const string TBD_AVT     = "TBD_AVT";
        public const string TBD_DOC     = "TBD_DOC";
        public const string TBD_TTN     = "BD_LTTN";
        public const string TBD_SMSG    = "TBD_SMSG";

        // отношения
        //public const string POST2DEVID  = "Post2DevID";

        // таблицы для обмена
        //public const string BD_ZDOC = "BD_ZTTN";
        //public const string BD_ZDET = "BD_STTN";

        // связи между таблицами
        //public const string REL2TTN = "DOC2TTN";
        //public const string REL2ZVK = "DOC2ZVK";

        //public const string REL2FAKT = "DOC2FAKT";


        // готовность детальной строки заявки
        //public enum READYPROD : int
        //{
        //    NO   = 0,
        //    PART_READY = 20,                             // частично выполнена
        //    FULL_READY = 100                             // совпадение заявки и скан
        //}

        // разрешения ввода детальных строк
        //public enum DESTINPROD: int
        //{
        //    GENCASE = 1,                                // общий случай
        //    TOTALZ = 2,                                 // точное соответствие заявке (EAN-EMK-NP)
        //    PARTZ = 3,                                  // частичное соответствие заявке
        //    USER = 10                                   // подтвердил User
        //}

        // результат контроля документа
        public enum DOCCTRL : int
        {
            UNKNOWN = 0,                                // контроль не выполнялся
            OK = 1,                                     // точное соответствие заявке
            WARNS = 2,                                  // есть предупреждения
            ERRS = 3                                    // есть ошибки
        }

        // путь загрузки
        //public string sPathNSI;     // путь к справочникам
        //public string sPathBD;     // путь к таблицам

        // таблицы текущих данных
        private string sP_CS = @"CS.xml";
        private string sP_CSDat = @"CSDat.xml";

        // режимы загрузки
        //internal const int LOAD_EMPTY = 0;              // загрузка незагруженного
        //internal const int LOAD_ANY = 1;                // загрузка обязательная

        // индексы стилей для гридов

        internal const int GDOC_INV   = 0;              // для инвентаризации

        internal const int GDOC_NEXT  = 999;            // следующую по списку

        // индексы стилей для детальных строк
        //internal const int GDET_SCAN = 0;               // для сканированных
        //internal const int GDET_ZVK = 1;                // для заявок
        //internal const int GDET_INV = 2;                // для инвентаризации

        // происхождение документа
        internal const int DOCSRC_LOAD = 1;             // загружен
        internal const int DOCSRC_CRTD = 2;             // создан вручную
        internal const int DOCSRC_UPLD = 3;             // выгружен
        internal const int DOCSRC_ZVKLOAD = 4;          // загружен из заявки


        public DataSet dsM;
        public DataSet dsNSI;

        // время загрузки всех
        internal float fLoadAll = 0;

        //public Dictionary<string, TableDef> DT;

        private void CreateTables()
        {
            DT = new Dictionary<string, TableDef>();

            // информация о справочниках
            DT.Add(BD_TINF, new TableDef(BD_TINF, new DataColumn[]{
                new DataColumn("DT_NAME", typeof(string)),              // имя таблицы
                new DataColumn("MD5", typeof(string)) }));              // контрольная сумма MD5
            DT[BD_TINF].dt.PrimaryKey = new DataColumn[] { DT[BD_TINF].dt.Columns["DT_NAME"] };
            DT[BD_TINF].nType = TBLTYPE.CREATE | TBLTYPE.INTERN;        // создаю сам

            // справочник пользователей
            DT.Add(NS_USER, new TableDef(NS_USER, new DataColumn[]{
                new DataColumn("KP", typeof(string)),                   // код пользователя
                new DataColumn("NMP", typeof(string)),                  // имя пользователя
                new DataColumn("PP", typeof(string)) }));               // пароль
            DT[NS_USER].dt.PrimaryKey = new DataColumn[] { DT[NS_USER].dt.Columns["KP"] };
            DT[NS_USER].Text = "Пользователи";

            // справочник постов
            DT.Add(NS_POST, new TableDef(NS_POST, new DataColumn[]{
                new DataColumn("KPT", typeof(int)),                     // код поста
                new DataColumn("NAME", typeof(string)) }));             // наименование поста
            DT[NS_POST].dt.PrimaryKey = new DataColumn[] { DT[NS_POST].dt.Columns["KPT"] };
            DT[NS_POST].Text = "Посты";

            // справочник шлагбаумов/замков
            DT.Add(NS_SHLG, new TableDef(NS_SHLG, new DataColumn[]{
                new DataColumn("ID", typeof(int)),                      // ID
                new DataColumn("KPT", typeof(int)),                     // код поста
                new DataColumn("NAME", typeof(string)),                 // наименование устройства
                new DataColumn("SNM", typeof(string)),                  // краткое наименование
                new DataColumn("DEVID", typeof(string)),                // ID устройства
                new DataColumn("DIRIO", typeof(int)),                   // направления
                new DataColumn("PASS", typeof(string)),                 // пароль для устройства
                new DataColumn("USER", typeof(string)) }));             // логин для устройства
            DT[NS_SHLG].dt.PrimaryKey = new DataColumn[] { DT[NS_SHLG].dt.Columns["ID"] };
            DT[NS_SHLG].Text = "Устройства";
            DT[NS_SHLG].dt.Columns["ID"].AutoIncrement = true;
            DT[NS_SHLG].dt.Columns["ID"].AutoIncrementSeed = 1;
            DT[NS_SHLG].dt.Columns["ID"].AutoIncrementStep = 1;

            // справочник целей поездки
            DT.Add(NS_PURP, new TableDef(NS_PURP, new DataColumn[]{
                new DataColumn("KCL", typeof(int)),                     // код цели
                new DataColumn("NAME", typeof(string)),                 // наименование
                new DataColumn("TYPG", typeof(int)),                    // тип груза
                new DataColumn("DIRIO", typeof(int)) }));               // направления
            //DT[NS_PURP].dt.PrimaryKey = new DataColumn[] { DT[NS_PURP].dt.Columns["KCL"] };
            DT[NS_PURP].Text = "Назначение";
            foreach (DataColumn dc in DT[NS_PURP].dt.Columns)
                dc.ReadOnly = true;

            // таблица въехавших/выехавших авто
            DT.Add(TBD_AVT, new TableDef(TBD_AVT, new DataColumn[]{
                new DataColumn("ID", typeof(int)),                      // ID
                new DataColumn("AVT", typeof(string)),                  // № авто
                new DataColumn("PUTLIST", typeof(string)),              // № путевого
                new DataColumn("PRPSK", typeof(string)),                // № пропуска
                new DataColumn("DEVID", typeof(string)) }));            // ID шлагбаума
            DT[TBD_AVT].dt.PrimaryKey = new DataColumn[] { DT[TBD_AVT].dt.Columns["ID"] };
            DT[TBD_AVT].dt.Columns["ID"].AutoIncrement = true;
            DT[TBD_AVT].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[TBD_AVT].dt.Columns["ID"].AutoIncrementStep = -1;
            DT[TBD_AVT].nType = TBLTYPE.BD;
            DT[TBD_AVT].Text = "Автомобили";


            // таблица ТТН авто
            DT.Add(TBD_TTN, new TableDef(TBD_TTN, new DataColumn[]{
                new DataColumn("DATAS", typeof(string)),                // дата символьная
                new DataColumn("DATAD", typeof(DateTime)),              // дата документа
                new DataColumn("PTVL", typeof(string)),                 // № авто
                new DataColumn("NTTN", typeof(string)),                 // № авто
                new DataColumn("MASSA", typeof(FRACT)),                 // № авто
                new DataColumn("SUMMA", typeof(FRACT)),                 // № авто
                new DataColumn("MEST", typeof(int)),                    // мест
                new DataColumn("TD", typeof(string)),                   // тип документа
                new DataColumn("CTRL", typeof(int)) }));                // проведенный контроль
            DT[TBD_TTN].nType = TBLTYPE.BD;
            DT[TBD_TTN].Text = "Документы с сервера";
            DT[TBD_TTN].dt.Columns["CTRL"].DefaultValue = 0;
            DT[TBD_TTN].dt.Columns["DATAD"].DefaultValue = DateTime.MinValue;
            DT[TBD_TTN].dt.Columns["TD"].DefaultValue = "1";








            // заголовки документов
            DT.Add(TBD_DOC, new TableDef(TBD_DOC, new DataColumn[]{
                new DataColumn("TD", typeof(int)),                      // Тип документа (N(2))
                new DataColumn("KPP1", typeof(long)),                    // Код поставщика (N(5))
                new DataColumn("KPP2", typeof(long)),                    // Код получателя (N(5))
                new DataColumn("DT", typeof(string)),                   // Дата (C(8))
                new DataColumn("NUCH", typeof(int)),                    // Номер участка (N(3))
                new DataColumn("SYSN", typeof(int)),                    // Системный номер (N(9))
                //new DataColumn("NOMD", typeof(string)),               // Номер документа (C(10))
                new DataColumn("ID", typeof(int)),                      // ID Код (N(9))

                new DataColumn("BIN2D", typeof(string)),                // данные 2D
                new DataColumn("HEAD2D", typeof(int)),                  // размер данных
                new DataColumn("FULL2D", typeof(int)),                  // размер данных
                new DataColumn("IDSER2D", typeof(long)),                // ID серии 2D

                new DataColumn("SOURCE", typeof(int)),                  // Происхождение N(2))
                new DataColumn("DIFF", typeof(int)),                    // Отклонение от заявки
                   
                new DataColumn("EXPR_DT", typeof(string)),              // выражение для даты
                new DataColumn("EXPR_SRC", typeof(string)),             // выражение для происхождения
                new DataColumn("SRV_STS", typeof(int)),                 // Результат контроля заявки сервером

                new DataColumn("PP1_NAME", typeof(string)),             // Наименование поставщика
                new DataColumn("PP2_NAME", typeof(string)),             // Наименование получателя
                new DataColumn("MEST", typeof(int)),                    // Количество мест(N(3))
                new DataColumn("MESTZ", typeof(int)),                   // Количество мест по заявке(N(3))
                new DataColumn("KOLE", typeof(FRACT)),                  // Количество единиц (N(10,3))
                new DataColumn("KOBJ", typeof(string))  }));            // Код объекта (C(10))

            DT[TBD_DOC].dt.PrimaryKey = new DataColumn[] { DT[TBD_DOC].dt.Columns["ID"] };
            DT[TBD_DOC].dt.Columns["EXPR_DT"].Expression = "substring(DT,7,2) + '.' + substring(DT,5,2)";
            DT[TBD_DOC].dt.Columns["EXPR_SRC"].Expression = "iif(SOURCE=1,'Загр', iif(SOURCE=2,'Ввод','Выгр'))";

            DT[TBD_DOC].dt.Columns["DIFF"].DefaultValue = NSI.DOCCTRL.UNKNOWN;
            DT[TBD_DOC].dt.Columns["MEST"].DefaultValue = 0;
            DT[TBD_DOC].dt.Columns["MESTZ"].DefaultValue = 0;
            DT[TBD_DOC].dt.Columns["SRV_STS"].DefaultValue = NSI.DOCCTRL.UNKNOWN;
            DT[TBD_DOC].dt.Columns["SOURCE"].DefaultValue = NSI.DOCSRC_CRTD;

            DT[TBD_DOC].dt.Columns["ID"].AutoIncrement = true;
            DT[TBD_DOC].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[TBD_DOC].dt.Columns["ID"].AutoIncrementStep = -1;
            DT[TBD_DOC].nType = TBLTYPE.BD;

            UniqueConstraint cstUnique = new UniqueConstraint(new DataColumn[] {
                DT[TBD_DOC].dt.Columns["TD"], DT[TBD_DOC].dt.Columns["DT"], 
                DT[TBD_DOC].dt.Columns["KPP1"], DT[TBD_DOC].dt.Columns["KPP2"], 
                DT[TBD_DOC].dt.Columns["SYSN"] });
            DT[TBD_DOC].dt.Constraints.Add(cstUnique);
            DT[TBD_DOC].sTFilt = String.Format("SOURCE={0}", NSI.DOCSRC_CRTD);

            // таблица серверных сообщений
            DT.Add(TBD_SMSG, new TableDef(TBD_SMSG, new DataColumn[]{
                new DataColumn("KMSG", typeof(string)),                 // Код сообщения(команды)
                new DataColumn("TYPEMSG", typeof(int)),                 // Тип сообщения(команды)
                new DataColumn("MSG", typeof(string)),                  // Сообщение
                new DataColumn("PARS", typeof(string)),                 // Параметры
                new DataColumn("STATE", typeof(int)),                   // статус сообщения
                new DataColumn("TIME", typeof(DateTime)),               // время получения
                new DataColumn("EXPR_TYPE", typeof(string)),            // выражение для типа
                new DataColumn("EXPR_INF", typeof(string)),             // выражение для комментария
                new DataColumn("EXPR_STS", typeof(string)),             // выражение для статуса
                new DataColumn("STATE_REZ", typeof(int))  }));          // результат выполнения
            DT[TBD_SMSG].nType = TBLTYPE.BD;

        }


        public NSI(string sNP, string sDP)
        {
            sPathNSI = sNP;
            sPathBD = sDP;

            dsM = new DataSet("dsM");
            dsNSI = new DataSet("dsNSI");

            CreateTables();
            LoadLocNSI(new string[] { NS_USER, NS_POST, NS_SHLG, NS_PURP }, 0);

            dsNSI.Tables.Add( DT[NS_USER].dt );
            dsNSI.Tables.Add( DT[NS_POST].dt );
            dsNSI.Tables.Add( DT[NS_SHLG].dt );
            dsNSI.Tables.Add( DT[NS_PURP].dt );

            dsM.Tables.Add(DT[TBD_AVT].dt);
            dsM.Tables.Add(DT[TBD_DOC].dt);
/*
            DataColumn dcPostNsi = DT[NS_POST].dt.Columns["KPT"],
                dcPostDevID = DT[NS_SHLG].dt.Columns["KPT"];

            dsNSI.Relations.Add(POST2DEVID, dcPostNsi, dcPostDevID);
            dsNSI.Relations[POST2DEVID].ChildKeyConstraint.DeleteRule = Rule.Cascade;
*/
            DT[TBD_DOC].dt.DefaultView.RowFilter = DT[TBD_DOC].sTFilt;

        }

        // создание стилей просмотра таблиц
        public void ConnDTGrid(DataGrid[] dgDoc){
            DT[TBD_DOC].dg = dgDoc[0];
            dgDoc[0].DataSource = DT[TBD_DOC].dt;
            CreateTableStyles(DT[TBD_DOC].dg);
                        
            DT[TBD_TTN].dg = dgDoc[1];
            CreateTTNStyles(DT[TBD_TTN].dg, DT[TBD_TTN].dt.TableName);
            dgDoc[1].DataSource = DT[TBD_TTN].dt;
                        

                        // по умолчанию - просмотр ТТН
                        /*
                                    ChgGridStyle(I_TFAKT, GDET_SCAN);

                                    // у заявок - тот же Grid
                                    DT[I_TZVK].dg = dgDoc[1];
                                    AddZVKTableStyles(DT[I_TZVK].dg);

                                    dgDoc[2].DataSource = DT[I_TMT].dt;
                        */
            // таблица и грид серверных сообщений
            //dgDoc[3].DataSource = DT[TBD_SMSG].dt;
            //DT[TBD_SMSG].dg = dgDoc[3];
            //CreateMsgStyle(dgDoc[3], TBD_SMSG);
        }

        // стили просмотра таблицы документов в гриде
        private void CreateTableStyles(DataGrid dg)
        {
            DataGridTextBoxColumn t;
            ServClass.DGTBoxColorColumn sC;

            // специальные цвета для результатов контроля
            System.Drawing.Color colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            dg.TableStyles.Clear();
            // Для инвентаризации
            DataGridTableStyle tsi = new DataGridTableStyle();
            tsi.MappingName = GDOC_INV.ToString();

            t = new DataGridTextBoxColumn();
            t.MappingName = "TD";
            t.HeaderText = "Tип";
            t.Width = 30;
            t.NullText = "";
            tsi.GridColumnStyles.Add(t);

            t = new DataGridTextBoxColumn();
            t.MappingName = "EXPR_DT";
            t.HeaderText = "Дата";
            t.Width = 38;
            t.NullText = "";
            tsi.GridColumnStyles.Add(t);

            t = new DataGridTextBoxColumn();
            t.MappingName = "KPP1";
            t.HeaderText = "Поставщик";
            t.Width = 82;
            t.NullText = "";
            tsi.GridColumnStyles.Add(t);

            //t = new DataGridTextBoxColumn();
            //t.MappingName = "KPP2";
            //t.HeaderText = "Получ-ль";
            //t.Width = 62;
            //t.NullText = "";
            //tsi.GridColumnStyles.Add(t);

            sC = new ServClass.DGTBoxColorColumn();
            sC.Owner = dg;
            sC.ReadOnly = true;
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.TableInd = NSI.TBD_DOC;
            sC.MappingName = "SYSN";
            sC.HeaderText = " № док";
            sC.Width = 72;
            sC.NullText = "";
            tsi.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(tsi);
        }

        // стили просмотра таблицы документов в гриде
        private void CreateTTNStyles(DataGrid dg, string sN)
        {
            DataGridTextBoxColumn t;
            ServClass.DGTBoxColorColumn sC;

            // специальные цвета для результатов контроля
            System.Drawing.Color colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            dg.TableStyles.Clear();
            // Для инвентаризации
            DataGridTableStyle tsi = new DataGridTableStyle();
            tsi.MappingName = sN;

            sC = new ServClass.DGTBoxColorColumn();
            sC.Owner = dg;
            sC.ReadOnly = true;
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.TableInd = NSI.TBD_TTN;
            sC.MappingName = "NTTN";
            sC.HeaderText = " № ТТН";
            sC.Width = 50;
            sC.NullText = "";
            tsi.GridColumnStyles.Add(sC);

            t = new DataGridTextBoxColumn();
            t.MappingName = "MASSA";
            t.HeaderText = " Масса";
            t.Width = 65;
            t.NullText = "";
            tsi.GridColumnStyles.Add(t);

            t = new DataGridTextBoxColumn();
            t.MappingName = "SUMMA";
            t.HeaderText = "   Сумма";
            t.Width = 65;
            t.NullText = "";
            tsi.GridColumnStyles.Add(t);

            t = new DataGridTextBoxColumn();
            t.MappingName = "MEST";
            t.HeaderText = "Мест";
            t.Width = 35;
            t.NullText = "";
            tsi.GridColumnStyles.Add(t);

            dg.TableStyles.Add(tsi);
        }



        // стили таблицы полученных сообщений
        private void CreateMsgStyle(DataGrid dg, string sT)
        {
            ServClass.DGTBoxColorColumn sC, nC;
            System.Drawing.Color
                colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            dg.TableStyles.Clear();

            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = sT;

            sC = new ServClass.DGTBoxColorColumn();
            sC.Owner = dg;
            sC.ReadOnly = true;
            sC.MappingName = "EXPR_TYPE";
            sC.HeaderText = "Т";
            sC.Width = 10;
            ts.GridColumnStyles.Add(sC);

            nC = new ServClass.DGTBoxColorColumn();
            nC.Owner = dg;
            nC.ReadOnly = true;
            nC.MappingName = "TIME";
            nC.HeaderText = "Время";
            nC.Format = "HH:mm";
            nC.Width = 33;
            ts.GridColumnStyles.Add(nC);

            DataGridTextBoxColumn cP = new DataGridTextBoxColumn();
            cP.MappingName = "EXPR_INF";
            cP.HeaderText = "    Назначение";
            cP.Width = 123;
            cP.NullText = "";
            ts.GridColumnStyles.Add(cP);

            nC = new ServClass.DGTBoxColorColumn();
            nC.Owner = dg;
            nC.ReadOnly = true;
            nC.MappingName = "EXPR_STS";
            nC.HeaderText = "Статус";
            nC.Width = 45;
            ts.GridColumnStyles.Add(nC);


            dg.TableStyles.Add(ts);
        }


        // смена стиля таблицы
        // nSt - требуемый стиль
        public void ChgGridStyle(string iT, int nSt)
        {
            if (DT[iT].nGrdStyle != -1)
            {                                                           // НЕ первичная установка
                int nOld = DT[iT].nGrdStyle;
                // очистка текущей
                DT[iT].dg.TableStyles[nOld].MappingName = nOld.ToString();
                if (nSt == GDOC_NEXT)
                {                                                       // циклическая смена
                    nSt = ((nOld + 1) == DT[iT].dg.TableStyles.Count) ? 0 : nOld + 1;
                }
            }
            DT[iT].nGrdStyle = nSt;
            DT[iT].dg.TableStyles[nSt].MappingName = DT[iT].dt.TableName;
        }

        //public static string GrdDocStyleName(int i)
        //{
        //    string ret = "";
        //    if (i == GDOC_CENTR)
        //        ret = "Цтр";
        //    else if (i == GDOC_INV)
        //        ret = "Инв";
        //    else if (i == GDOC_SAM)
        //        ret = "Сам";
        //    return (ret);
        //}


        // загрузка НСИ на терминале (локальных)
        // nReg - LOAD_EMPTY или LOAD_ANY (грузить по-любому)
        public void LoadLocNSI(string[] aI, int nR)
        {
            if (aI.Length == 0)
            {// загрузка всех загружаемых НСИ
                fLoadAll = 0;
                foreach(KeyValuePair<string, TableDef> td in DT)
                {
                    Read1NSI(td.Value, nR);
                    fLoadAll += float.Parse(td.Value.sDTStat);
                }
            }
            else
            {
                for (int i = 0; i < aI.Length; i++)
                    Read1NSI(DT[aI[i]], nR);
            }
        }


        // загрузка одной таблицы
//        private void Read1NSI(TableDef td, int nReg)
//        {
//            int tc1, nE = 0;
//            string sPath = "";
//            object dg = null;

//            td.sDTStat = "";
//            if (  ((td.nType & TBLTYPE.NSI) == TBLTYPE.NSI) &&
//                ((td.nType & TBLTYPE.LOAD) == TBLTYPE.LOAD) || (nReg == LOAD_ANY))   // НСИ загружаемое
//                {
//                    if ((td.nState == DT_STATE_INIT) || (nReg == LOAD_ANY))
//                    {
//                        if (td.dg != null)
//                        {
//                            dg = td.dg.DataSource;
//                            td.dg.DataSource = null;
//                        }
                        
//                        td.nErr = 0;
//                        tc1 = Environment.TickCount;
//                        try
//                        {
//                            sPath = sPathNSI + td.sXML;
//                            td.dt.BeginLoadData();
//                            nE = 1;
//                            td.dt.Clear();
//                            nE = 2;
//                            td.dt.ReadXml(sPath);
//                            nE = 3;
//                            td.dt.EndLoadData();
//                            td.nState = DT_STATE_READ;
//                        }
//                        catch
//                        {
//                            td.nErr = nE;
//                            td.nState = DT_STATE_READ_ERR;
//                        }

//                        td.sDTStat = ServClass.TimeDiff(tc1, Environment.TickCount);
///*
//                        if (i == I_MC)
//                            CreateKrEAN(td.dt);
//*/
//                        if (dg != null)
//                            td.dg.DataSource = dg;
//                        dg = null;
//                    }
//                }

//        }


        // возвращает из справочника заданное поле
        //internal RezSrch GetNameSPR(string iT, object[] xSearch, string sFieldName)
        //{
        //    RezSrch zRet = new RezSrch("-???-");
        //    DataRow dr = null;
        //    try
        //    {
        //        dr = DT[iT].dt.Rows.Find(xSearch);
        //        zRet.sName = dr[sFieldName].ToString();
        //        zRet.bFind = true;
        //    }
        //    catch
        //    {
        //    }
        //    return (zRet);
        //}


        // чтение текущей строки в объект панели документов
        public bool InitCurDoc(CurDoc xD)
        {
            bool ret = false;

            if (xD.drCurRow != null)
            {
                try
                {
                    //int i = xD.nCurRec;
                    DocPars x = xD.xDocP;

                    //DataRow dr = DT[NSI.I_DOCOUT].dt.Rows[i];
                    DataRow dr = xD.drCurRow;

                    xD.nId = (int)((dr["ID"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["ID"]);
                    xD.nDocSrc = (int)((dr["SOURCE"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["SOURCE"]);


                    x.nTypD = (int)((dr["TD"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["TD"]);
                    x.nPost = (long)((dr["KPP1"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["KPP1"]);
                    x.sNomDoc = ((dr["SYSN"] == System.DBNull.Value) ? "" : dr["SYSN"].ToString());
/*
                    x.sSmena = ((dr["KSMEN"] == System.DBNull.Value) ? "" : dr["KSMEN"].ToString());
                    x.nUch = (int)((dr["NUCH"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["NUCH"]);
                    x.nEks = (int)((dr["KEKS"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["KEKS"]);
*/
                    try
                    {
                        x.dDatDoc = DateTime.ParseExact(dr["DT"].ToString(), "yyyyMMdd", null);
                    }
                    catch
                    {
                        x.dDatDoc = DateTime.MinValue;
                    }
                    //xD.nStrokZ = xD.drCurRow.GetChildRows(NSI.REL2ZVK).Length;
                
                    Encoding enc866 = Encoding.GetEncoding(866);
                    xD.nHead2D = (int)dr["HEAD2D"];
                    xD.nFull2D = (int)dr["FULL2D"];
                    xD.nIDSer = (long)dr["IDSER2D"];
                    xD.From2D = enc866.GetBytes((string)dr["BIN2D"]);

                    ret = true;
                }
                catch
                {

                }
            }
            return (ret);
        }
        // заполнение строки таблицы документов
        public bool UpdateDocRec(DataRow dr, CurDoc xD)
        {
            bool ret = true;
            DocPars x = xD.xDocP;
            try
            {
                if (x.nTypD != AppC.EMPTY_INT)
                    dr["TD"] = x.nTypD;
                else
                    dr["TD"] = System.DBNull.Value;

                if (x.dDatDoc != DateTime.MinValue)
                    dr["DT"] = x.dDatDoc.ToString("yyyyMMdd");
                else
                    dr["DT"] = System.DBNull.Value;
                if (x.sNomDoc != "")
                    dr["SYSN"] = int.Parse(x.sNomDoc);
                else
                    dr["SYSN"] = System.DBNull.Value;

                if (x.nPost != AppC.EMPTY_INT)
                    dr["KPP1"] = x.nPost;
                else
                    dr["KPP1"] = System.DBNull.Value;

                Encoding enc866 = Encoding.GetEncoding(866);
                dr["BIN2D"] = enc866.GetString(xD.From2D, 0, xD.nFull2D);
                dr["HEAD2D"] = xD.nHead2D;
                dr["FULL2D"] = xD.nFull2D;
                dr["IDSER2D"] = xD.nIDSer;

                dr["PP2_NAME"] = "2D-данные";

            }
            catch
            {
                ret = false;
            }
            return(ret);
        }

        // добавление новой записи в документы
        public bool AddDocRec(CurDoc xD, DataGrid dgDoc)
        {
            bool ret = true;

            try
            {
                DataRow dr = DT[NSI.TBD_DOC].dt.NewRow();
                dr["SOURCE"] = DOCSRC_CRTD;

                ret = UpdateDocRec(dr, xD);
                if (ret == true)
                {
                    DT[NSI.TBD_DOC].dt.Rows.Add(dr);
                    xD.nId = (int)dr["ID"];
                    xD.drCurRow = dr;
                    if (dgDoc != null)
                        dgDoc.CurrentRowIndex = ((System.ComponentModel.IListSource)dgDoc.DataSource).GetList().Count - 1;
                }
            }
            catch (System.Data.ConstraintException )
            {
                ret = false;
                ServClass.ErrorMsg("Документ уже существует");
            }
            catch (Exception ex)
            {
                ret = false;
                object x = ex.GetType();
            }
            return (ret);
        }

        // запись скорректированной строки в таблицу
        public bool ChgDocRec(CurDoc xD)
        {
            bool ret = false;

            try
            {
                //DataRow dr = DT[I_DOCOUT].dt.Rows[xD.nCurRec];
                ret = UpdateDocRec(xD.drCurRow, xD);
            }
            catch
            {
                ret = false;
            }
            return (ret);
        }

        // подготовка DataSet для выгрузки
//        public DataSet MakeWorkDataSet(DataTable dtM, DataTable dtD, DataRow[] drA)
//        {
            
//            DataTable dtMastNew = dtM.Clone();
//            //DataTable dtDetNew = dtD.Clone();

//            //DataRelation myRelation = dtM.ChildRelations[REL2FAKT];


//            foreach (DataRow dr in drA)
//            {
//                DataRow drm = dtMastNew.NewRow();
//                drm.ItemArray = dr.ItemArray;
//                dtMastNew.Rows.Add(drm);
//                //DataRow[] childRows = dr.GetChildRows(myRelation);
//                dr["SRV_STS"] = AppC.RC_UNKNCTRL;
///*
//                foreach (DataRow chRow in childRows)
//                {
//                    DataRow drd = dtDetNew.NewRow();
//                    drd.ItemArray = chRow.ItemArray;
//                    dtDetNew.Rows.Add(drd);
//                    chRow["SRV_STS"] = AppC.RC_UNKNCTRL;
//                }
// */ 
//            }

//            DataSet ds1Rec = new DataSet("dsMOne");
//            dtMastNew.TableName = "BD_DOCOUT";
//            //dtDetNew.TableName = "BD_DOUTD";
//            ds1Rec.Tables.Add(dtMastNew);
//            //ds1Rec.Tables.Add(dtDetNew);
//            return (ds1Rec);
//        }


        //public void SaveCS(Smena xSm, AppPars xP)
        //{
        //    xSm.nDocs = DT[NSI.TBD_DOC].dt.Rows.Count;
        //    Srv.WriteXMLObjTxt(typeof(Smena), xSm, xP.sDataPath + sP_CS);
        //    dsM.WriteXml(xP.sDataPath + sP_CSDat);
        //}


        public int DSSave(string sF)
        {
            int ret = AppC.RC_OK;

            try
            {
                dsM.WriteXml(sF + sP_CSDat);
            }
            catch
            {
                ret = AppC.RC_CANCEL;
            }

            return (ret);
        }

        // восстановление рабочих данных (при необходимости)
        public void TryRestoreUserDat(Smena xSm, AppPars xP, bool bRestAll)
        {
            Smena xSaved;
            object xxx;

            int nRet = Srv.ReadXMLObj(typeof(Smena), out xxx, xP.sDataPath + sP_CS);
            if (nRet == AppC.RC_OK)
            {
                xSaved = (Smena)xxx;
                if (xSaved.nDocs > 0)
                {// данные действительно есть
                    if ( (xSaved.sUser == xSm.sUser) || (bRestAll == true) )
                    {// сохраненный соответствует новому

                        TimeSpan tsDiff = xSm.dBeg.Subtract(xSaved.dBeg);
                        if ((tsDiff.Days <= 7) || (true))
                        {// данные могут быть актуальны
                            nRet = DSRestore(false, xP.sDataPath + sP_CSDat);
                            if (nRet == AppC.RC_OK)
                            {
                                xSm.nDocs = xSaved.nDocs;
                            }
                            xSm.nDocs = DT[NSI.TBD_DOC].dt.Rows.Count;
                        }
                    }
                }
            }
        }



        public int DSRestore(bool bOnlyClear, string sDS)
        {
            int nRet = AppC.RC_OK;
            try
            {
                dsM.BeginInit();
                dsM.EnforceConstraints = false;
                dsM.Clear();
                if (bOnlyClear != true){
                    dsM.ReadXml(sDS);
                    try
                    {
                        //dsM.Tables.Remove(DT[NSI.I_TZVK].dt.TableName);
                    }
                    catch
                    {// ну, значит, не было 
                    }
                }

                dsM.EnforceConstraints = true;
                dsM.EndInit();
            }
            catch 
            {
                nRet = AppC.RC_NOFILE;
            }

            return (nRet);
        }


    }
}
