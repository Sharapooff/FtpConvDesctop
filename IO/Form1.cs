using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BytesRoad.Net.Sockets;
using BytesRoad.Net.Ftp;//http://easylab.net.ua/net-c-windows-forms/biblioteka-dlya-rabotyi-s-ftp-na-c
using System.Diagnostics;
using System.Threading;
using ZedGraph;
using System.Configuration;


namespace IO
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //____________________________________________________сам FTP___________________________________________
        FtpClient client = new FtpClient();
        FtpClient clientSTAT = new FtpClient();
        //__________________________________________________подключение к FTP____________________________________
        private void ConnectFTP()
        {
            try
            {

                //Задаём параметры клиента.
                if (checkBox2.Checked == true)
                { client.PassiveMode = true; } //Включаем пассивный режим.
                else
                { client.PassiveMode = false; }
                int TimeoutFTP;
                if (textBox10.Text != "")
                {
                    TimeoutFTP = Convert.ToInt32(textBox10.Text); //Таймаут.
                }
                else
                {
                    TimeoutFTP = 30000;
                }
                                                    
                string FTP_SERVER = textBox1.Text; //string FTP_SERVER = "192.168.3.10";
                int FTP_PORT = Convert.ToInt32(textBox5.Text); //int FTP_PORT = 21;
                string FTP_USER = textBox2.Text; //string FTP_USER = "FEDOTOV";
                string FTP_PASSWORD = textBox3.Text; //string FTP_PASSWORD = "fed11651382-";                              
              //  client.PassiveMode = false;//активный режим соединения с сервером
                                                           
                //Если используется прокси сервер то можем задать параметры прокси.
               // FtpProxyInfo pinfo = new FtpProxyInfo(); //Это переменная параметров.                                                    
               // pinfo.PreAuthenticate = false; //Если на прокси есть идентификация
               // pinfo.Server = textBox1.Text; //pinfo.Server = "192.168.3.10"; 
               // pinfo.Port = Convert.ToInt32(textBox5.Text); //pinfo.Port = 21; //Порт.
               // pinfo.User = "FEDOTOV";
               // pinfo.Password = "fed11651382-";
               // pinfo.Type = FtpProxyType.HttpConnect; //Тип прокси - всего 4 вида.                                              
               // client.ProxyInfo = pinfo;//Присваиваем параметры прокси клиенту.

                //Подключаемся к FTP серверу.
                client.Connect(TimeoutFTP, FTP_SERVER, FTP_PORT);
                client.Login(TimeoutFTP, FTP_USER, FTP_PASSWORD);
                label1.Text = "Состояние соединения с FTP://" + FTP_SERVER + " - соединено";
               // ovalShape1.BackColor = Color.Lime;
              //  ovalShape2.BackColor = Color.Lime;

                pictureBox3.Image = Image.FromFile(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\ico\\Green Digital Icon 43.ico");//индикатор;
                pictureBox4.Image = Image.FromFile(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\ico\\yes.png");//индикатор;
                Application.DoEvents();                 //(прорисовка индекатора) передача управления операционке 
                // button5.Enabled = true;
                button2.Enabled = true;//отключиться
                button1.Enabled = false;//подключиться
                button5.Enabled = true;//загрузить-конвектировать
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);//ошибка подключения
            }
          
        }
        //_____________________________________________________Отключение от FTP_______________________________________________
        private void DisconnectFTP()
        {
            try
            {
                int TimeoutFTP = 30000; //Таймаут
                client.Disconnect(TimeoutFTP);
                //string FTP_SERVER = "192.168.3.10";
                string FTP_SERVER = textBox1.Text;
                Application.DoEvents();                 //(прорисовка индекатора) передача управления операционке
                pictureBox3.Image = Image.FromFile(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\ico\\earth-stop.png");//индикатор;
                pictureBox4.Image = Image.FromFile(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\ico\\earth-stop.png");//индикатор;
                label1.Text = "Состояние соединения с FTP://" + FTP_SERVER + " - разъединено";
                button1.Enabled = true;//подключиться
                button7.Enabled = true;//подключиться к FTP\\
                button5.Enabled = false;//загрузить-конвектировать
                button2.Enabled = false;//отключиться
                listView1.Items.Clear();
                treeView1.Nodes.Clear();
                treeView2.Nodes.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);//ошибка отключения
            }
        }


        //____________________________________Получение списка файлов текущего каталога с ФТП, сравнение со списком и передача на ПК________
        private void GetFilesFromFtp()
        {
            //  client.Dispose();//||
            // ConnectFTP();//||
            progressBar2.Visible = true;
            button5.Enabled = false;//делаем неактивными кнопки
            button7.Enabled = false;//
            button2.Enabled = false;
            treeView1.Nodes.Clear();
            string data = "", yer = "", day = "", mon = "";
            int TimeoutFTP = Convert.ToInt32(textBox10.Text); //Таймаут 
            string sd = "";//имя папки с архивами на ftp
            string sd_0 = "";//имя папки с архивами на ftp 116
            int kol = 1;//количество папок с архивами подходящих условию папки поиска
            int kol_0 = 1;//количество папок с архивами подходящих условию папки поиска 116
            int kol_failes_ok = 0;//количество файлов аск подходящих по условию
            string dirserch = textBox6.Text;//директория поиска папок на фтп
            string new_file_name_rez = "";
            label6.Text = "Найдено архивов за выбранный период : 0";
            bool nomer_sychestvyet_0 = false;
            bool nomer_sychestvyet = false;
            bool data_sychestvyet = false;

            progressBar1.Value = 0;
            progressBar1.Visible = true;//индикатор процесса
            label7.Visible = true;
            label7.Text = "Поиск и распаковка файлов ...";
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 280;//примерно архивов за день в папке
            progressBar1.Step = 1;//шаг индекатора процесса

            //определяем новый сервер или старый
            bool newftp = false;
            bool oldftp = false;
            if ((textBox1.Text == "88.86.78.118") || (textBox1.Text == "192.168.3.10"))
            {
                oldftp = true;
                newftp = false;
            }
            else
            {
                oldftp = false;
                newftp = true;
            }

            string del_dir_rar = "", del_dir_ask = "";//те самые временные папки в которых храним архивы и файлы аск

            //если новый FTP 

            if (newftp == true)
            {
                string u116 = "";

                string nomer_tep_0 = textBox7.Text;//№тепловоза
                //если же выбран 2-х секционный тепловоз и секция активна, то добавляем к номеру тепловоза букву секции 
                if ((comboBox4.Enabled == true) && ((comboBox1.Text == "2TE116U") || (comboBox1.Text == "3TE116U") || (comboBox1.Text == "2TE25A")))//                                                       //@3te116u//2TE25A
                {
                    nomer_tep_0 = textBox7.Text + comboBox4.Text;
                }


                foreach (FtpItem item_0 in client.GetDirectoryList(TimeoutFTP * 1000, "/" + dirserch + "/")) //ASK по умолчанию
                {
                    TreeNode node_0 = new TreeNode(item_0.Name);
                    node_0.ImageIndex = 0;
                    node_0.SelectedImageIndex = 2;
                    node_0.Tag = 0; //+                
                    if (item_0.ItemType == FtpItemType.Directory)//если папка
                    {
                        node_0.ImageIndex = 0;
                        node_0.SelectedImageIndex = 0;
                        sd_0 = Convert.ToString(node_0);//переводим в строку                    
                        sd_0 = sd_0.Remove(0, 10);//вырезаем имя папки                    
                        string ser_0 = comboBox1.Text;//серия тепловоза                       
                        if (sd_0.IndexOf(ser_0) > -1)//если папка содержит заданную серию тепловоза
                        {
                            //   MessageBox.Show(nomer_tep);
                            if (sd_0.IndexOf(nomer_tep_0) > -1)//если папка содержит указанный номер тепловоза (а для 116 и секцию //2TE25A)
                            {
                                nomer_sychestvyet_0 = true;
                                treeView1.Nodes.Add(node_0);  //отображение дерева коренного каталога 
                                Application.DoEvents();//передача управления ОС для отрисовки
                                kol_0 = kol_0 + 1;//счетчик индекса раздела
                                u116 = sd_0;
                                break;
                            }
                        }
                    }
                }
                sd_0 = u116 + "/";//в sd_0 сохраняем папку подходящую по всем условиям поиска
            }//если новый ftp                

            //формируем нужную дату 
            data = dateTimePicker1.Text;
            yer = data.Remove(0, 6);//вырезаем год
            day = data.Remove(2, 8);//вырезаем день
            mon = data.Remove(5, 5);//вырезаем месяц
            mon = mon.Remove(0, 3);
            data = yer + mon + day;

            string ser = comboBox1.Text;//серия тепловоза
            string nomer_tep = textBox7.Text;//№тепловоза

            //далее приписываем путь sd_0. Если новый сервер, то добавится каталог, если старый то ищем в корневом (ASK) (ищем папки уже с датой)
            foreach (FtpItem item in client.GetDirectoryList(TimeoutFTP * 1000, "/" + dirserch + "/" + sd_0))
            {
                TreeNode node = new TreeNode(item.Name);
                node.ImageIndex = 0;
                node.SelectedImageIndex = 2;
                node.Tag = 0; //+               
                if (item.ItemType == FtpItemType.Directory)//если папка
                {
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                    sd = Convert.ToString(node);//переводим в строку                    
                    sd = sd.Remove(0, 10);//вырезаем имя папки
                    if (sd.IndexOf(ser) > -1)//если папка содержит заданную серию тепловоза
                    {
                        if (sd.IndexOf(nomer_tep) > -1)//если папка содержит указанный номер тепловоза (а для 116 и секцию)
                        {
                            nomer_sychestvyet = true;

                            if (sd.IndexOf(data) > -1)//если папка содержит указанную дату
                            {
                                data_sychestvyet = true;
                                //если новый FTP 
                                if (newftp == true)
                                {
                                    treeView1.Nodes[kol_0 - 2].Nodes.Add(node);   //отображение дерева коренного каталога 3й уровень
                                }
                                //если старый FTP 0
                                if (oldftp == true)
                                {
                                    treeView1.Nodes.Add(node);    //отображение дерева коренного каталога 2уровень
                                }
                                Application.DoEvents();//передача управления ОС для отррисовки процесса                     
                                kol = kol + 1; //№ найденной папки (индекс директории)
                                //
                                foreach (FtpItem item2 in client.GetDirectoryList(TimeoutFTP * 1000, "/" + dirserch + "/" + sd_0 + sd))
                                {
                                    //считываем из нее архивы
                                    TreeNode node2 = new TreeNode(item2.Name);
                                    if (item2.ItemType == FtpItemType.File)//если файл
                                    {
                                        node2.ImageIndex = 1;
                                        node2.SelectedImageIndex = 1;
                                        string sd2 = Convert.ToString(node2);//переводим в строку
                                        sd2 = sd2.Remove(0, 10);//вырезаем имя файла                                  
                                        //если новый FTP
                                        if (newftp == true)
                                        {
                                            treeView1.Nodes[kol_0 - 2].Nodes[kol - 2].Nodes.Add(node2); //вывод архивов
                                        }
                                        //если старый FTP 
                                        if (oldftp == true)
                                        {
                                            treeView1.Nodes[kol - 2].Nodes.Add(node2); //вывод архивов
                                        }
                                        //копируем архивы за выбраный день
                                        System.IO.Directory.CreateDirectory(textBox4.Text + "/" + sd);//подпапка с именем тепловоза
                                        System.IO.Directory.CreateDirectory(textBox4.Text + "/" + sd + "_ask");//подпапка с именем тепловоза
                                        string loc_file_path = textBox4.Text + "/" + sd + "/" + sd2;//копируем в подпапку архивы                                    
                                        string FTP_file_path2 = "/" + dirserch + "/" + sd_0 + sd + "/" + sd2 + "";
                                        client.GetFile(TimeoutFTP * 1000, loc_file_path, FTP_file_path2);
                                        Application.DoEvents();//передача управления ос
                                             //те самые временные папки в которых храним архивы и файлы аск
                                             del_dir_ask = textBox4.Text + "/" + sd + "_ask";
                                             del_dir_rar = textBox4.Text + "/" + sd;

                                             //распаковываем скопированные файлы
                                             progressBar1.PerformStep(); //увеличиваем прогресс бар на один по распаковке файлов
                                             string path_rar = textBox4.Text + "/" + sd + "/" + sd2; ;//путь+архив для распаковки                                      
                                             string path_rar2 = textBox4.Text + "/" + sd + "_ask"; //директория распаковки


                                             Process process = new Process(); //новый процесс

                                             process.StartInfo.Arguments = "e -y -inul \"" + path_rar + "\" \"" + path_rar2 + "\""; //+++++ параметры вызова 
                                             process.StartInfo.CreateNoWindow = true;
                                             process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                             process.StartInfo.UseShellExecute = false;
                                             process.StartInfo.FileName = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Rar.exe"; ;
                                             process.Start();//запуск процесса
                                             Application.DoEvents();//передача управления ос  
                                        
                                        /*    process.StartInfo.Arguments = "e -y -inul \"" + path_rar + "\" \"" + path_rar2 + "\""; //+++++ параметры вызова 
                                           process.StartInfo.CreateNoWindow = true;//особо не повлияло на работу
                                           process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;//так же можно убрать как предыдущую строку
                                           process.StartInfo.FileName = "WinRAR";// вызов winrar
                                           process.Start();//запуск процесса
                                           Application.DoEvents();//передача управления ос                                       
                                      */
                                        // process.EnableRaisingEvents.ToString();  

                                    }
                                }

                               

                                kol_failes_ok = 0;
                                Directory.CreateDirectory(textBox8.Text);//при проверки наличия файла проверял несуществующую директорию
                                //*****______Если TEP70BC то____________________________________________________
                                if (comboBox1.Text == "TEP70BS")
                                {
                                    new_file_name_rez = "BSA_H" + textBox7.Text + day + mon + comboBox2.Text + "00";//имя для итогового файла rez 
                                    if (checkBox1.Checked == true) { new_file_name_rez = new_file_name_rez + "_with_ask"; }//если стоит метка добавления данных аск
                                    if (System.IO.File.Exists(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"))
                                    {
                                        System.IO.File.Delete(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez");
                                    }
                                    //просмотр имени распакованного файла
                                    System.Threading.Thread.Sleep(1000);//приостановка выполнения программы на 1секеунду (если нужнен граничный к 00 или 23ч файл,то может не успеть распк=аковаться раром и в директории найдутся не все файлы)
                                    System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(@"" + textBox4.Text + "/" + sd + "_ask");
                                    string file_name_in_rar = "";//имя файла из архива 

                                    foreach (System.IO.FileInfo f in d.GetFiles("*.ask"))
                                    {
                                        //MessageBox.Show("Обработка файла TEP70BS " + (f.Name));

                                        file_name_in_rar = f.Name;//имя файла с расширением
                                        //Path.GetFileNameWithoutExtension(f.Name);//имя файла без расширения
                                        //MessageBox.Show("" + Path.GetFileNameWithoutExtension(f.Name));
                                        //смотрим дату в зависимости от того новое имя у файла ASK или старое
                                        string dat_ask = "";
                                        if (System.IO.Path.GetFileNameWithoutExtension(f.Name).IndexOf("BSR") > -1)
                                        {
                                            dat_ask = System.IO.Path.GetFileNameWithoutExtension(f.Name).Remove(0, 12);//вырезаем время файла формат: ччмм
                                            dat_ask = dat_ask.Remove(2, 2);//
                                        }
                                        else
                                        {
                                            dat_ask = System.IO.Path.GetFileNameWithoutExtension(f.Name).Remove(0, 13);//вырезаем время файла формат: ччмм
                                            dat_ask = dat_ask.Remove(2, 2);//
                                        }
                                        //MessageBox.Show(dat_ask);
                                        if (dat_ask == comboBox2.Text)
                                        {
                                            //  MessageBox.Show("" + (f.Name));
                                            kol_failes_ok++;

                                            int bytesRead = 0;   //количество байт в читаемом файле
                                            byte[] buffer = new byte[2000000]; //буфер памяти 2м
                                            using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                                            {
                                                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                                                {
                                                    bytesRead = br.Read(buffer, 0, buffer.Length);///////+
                                                    //???                                                 MessageBox.Show("Размер файла = " + bytesRead + " байт");///////+
                                                    int kratnost = bytesRead / 487;
                                                    if (kratnost != 0)
                                                    {
                                                        //                         MessageBox.Show("Файл не кратен размеру пакета!");
                                                    }
                                                    //////        if (kratnost == 0)
                                                    //////        {
                                                    // MessageBox.Show("В файле " + kol_pak + " пакетов");
                                                    int kol_pak = 0;
                                                    int cb = 0;
                                                    for (int i = 0; i <= bytesRead - 1; i++) //от 0 до количества считанных байт в считываемом файле
                                                    {
                                                        cb = cb + 1;//накапливаем сумму байт пакета
                                                        //- result.AppendFormat("{0:x2} ", buffer[i]);//в результирующую строку добавляем байт из буфера в формате х2 (2символа)                          
                                                        if ((buffer[i] == 0XFF) && (buffer[i + 1] == 0X0F) && (buffer[i + 2] == 0X6C) && (buffer[i + 3] == 0X01))
                                                        {
                                                            kol_pak = kol_pak + 1;//накапливаем количество пакетов в файле
                                                            cb = 0;//скидываем сумму байт в пакете
                                                            //  MessageBox.Show(i+"  ");
                                                            //MessageBox.Show(buffer[234] + " " + buffer[235] + " " + buffer[236] + " " + buffer[237]); //вывод последнего байта в файле
                                                        }
                                                    }
                                                    //     MessageBox.Show("Найдено " + kol_pak + " пакетов в файле");
                                                    if (kol_pak > 0)//если пакет
                                                    {
                                                        cb = cb + 1;
                                                    }
                                                    //++                                           MessageBox.Show("Размер пакета   " + cb + " байт");
                                                    //просмотр байт//+   MessageBox.Show(buffer[0] + " " + buffer[210 + 487] + " " + buffer[211 + 487] + " год " + buffer[212 + 487] + " месяц  " + buffer[213] + " день  " + buffer[214] + " час  " + buffer[215] + " минута " + buffer[216 + 487] + " сек "); //вывод даты                                                        
                                                }//конец потока битов читаемого файла
                                            }//конец потока читаемого файла

                                            //ЗАПИСЬ 

                                            System.IO.FileInfo f2 = new System.IO.FileInfo(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"); //файл rez, в который пишем файлы аск
                                            using (System.IO.FileStream fs2 = f2.Open(System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.Read))//открытие записываемого файла как потока
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs2))//открытие потока записи в потоке файла для записи
                                                {
                                                    label7.Text = "Объединение файлов ...";
                                                    progressBar1.Value = 0;
                                                    bw.Seek(0, System.IO.SeekOrigin.End);//УСТАНОВКА записи в конец файла
                                                    for (int i2 = 0; i2 <= bytesRead - 1; i2++) //цикл от 0 до количества считанных байт в читаемом файле
                                                    {
                                                        //***         BSR - BSA H | ТЕП70БС с 215 | ask convert to rez  
                                                        if ((buffer[i2] == 0XFF) && (buffer[i2 + 1] == 0X0F) && (buffer[i2 + 2] == 0X6C) && (buffer[i2 + 3] == 0X01))
                                                        {
                                                            //*пакет rez файла начинается с аналоговых параметров (410 байт)                      
                                                            //АНАЛОГОВЫЕ 70 х 2 байта //двухбайтные в зеркале                              
                                                            for (int i_analog1 = 48; i_analog1 <= 187; i_analog1++)//*140 байт*         48-187 байты ask (44-184 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }

                                                            //АНАЛОГОВЫЕ 16 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog2 = 188; i_analog2 <= 203; i_analog2++)//*32 байта*        188-203 байты ask (185-200 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog2 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }

                                                            //АНАЛОГОВЫЕ 4 х 2 байта //двухбайтные в зеркале
                                                            for (int i_analog3 = 204; i_analog3 <= 211; i_analog3++)//*8 байт*         204-211 байты ask (201-208 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog3 + i2] });//запись в новый rez
                                                            }

                                                            //АНАЛОГОВЫЕ 18 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog4 = 212; i_analog4 <= 229; i_analog4++)//*36 байт*        212-229 байты ask (209-226 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog4 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }

                                                            //АНАЛОГОВЫЕ 11 х 2 байта //двухбайтные в зеркале
                                                            for (int i_analog5 = 230; i_analog5 <= 251; i_analog5++)//*22 байта*       230-251 байты ask (227-248 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog5 + i2] });//запись в новый rez
                                                            }

                                                            //ТЕМПЕРАТУРНЫЕ ПАРАМЕТРЫ
                                                            //АНАЛОГОВЫЕ 48 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog6 = 252; i_analog6 <= 299; i_analog6++)//*96 байт*        252-299 байты ask (249-296 байты в зеркале)
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog6 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }

                                                            //ВЫЧИСЛЯЕМЫЕ ПАРАМЕТРЫ
                                                            //АНАЛОГОВЫЕ 5 х 2 байта //двухбайтные в зеркале
                                                            for (int i_analog7 = 300; i_analog7 <= 309; i_analog7++)//*10 байта*       300-309 байты ask (297-306 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog7 + i2] });//запись в новый rez
                                                            }

                                                            //АНАЛОГОВЫЕ 48 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog8 = 310; i_analog8 <= 318; i_analog8++)//*18 байт*        310-318 байты ask (307-315 байты в зеркале)
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog8 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }

                                                            //АНАЛОГОВЫЕ 24 х 2 байта //двухбайтные в зеркале
                                                            for (int i_analog9 = 319; i_analog9 <= 366; i_analog9++)//*48 байт*       319-366 байты ask (316-362 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog9 + i2] });//запись в новый rez
                                                            }
                                                            //*после аналоговых параметров в пакет rez файла записывается дата - день, месяц, год, час, мин, сек (6 байт)
                                                            //
                                                            //время берется из аск
                                                            //MessageBox.Show(buffer[479 + i2] + " год -");  
                                                            // bw.Write(new byte[] { buffer[***] });//210-211 2 байта из потока      // bw.Write((byte)0X14);                           
                                                            bw.Write(new byte[] { buffer[479 + i2] });//запись в новый rez год из аск  //                                                           
                                                            bw.Write(new byte[] { buffer[480 + i2] });//месяц//212
                                                            bw.Write(new byte[] { buffer[481 + i2] });//день//213
                                                            bw.Write(new byte[] { buffer[482 + i2] });//час//214
                                                            bw.Write(new byte[] { buffer[483 + i2] });//мин//215
                                                            bw.Write(new byte[] { buffer[484 + i2] });//сек//216
                                                            label23.Text = "  " + buffer[481 + i2] + "/" + buffer[480 + i2] + "/" + buffer[479 + i2] + "  " + buffer[482 + i2] + ":" + buffer[483 + i2] + ":" + buffer[484 + i2];
                                                            //  MessageBox.Show("день= " + buffer[213+i2]+" час= "+buffer[214+i2]+" мин= "+buffer[215+i2]+ " сек=  "+buffer[216+i2]);
                                                            //  MessageBox.Show("день= " + buffer[481 + i2] + " час= " + buffer[482 + i2] + " мин= " + buffer[483 + i2] + " сек=  " + buffer[484 + i2]);

                                                            //*после даты в пакет rez файла записывается значения дискретных параметров (44 байта)
                                                            //ДИСКРЕТНЫЕ ВХОДЫ 
                                                            for (int i_d_in = 4; i_d_in <= 10; i_d_in++)//*7 байт*  4-10 байты из файла ask (1-7 байт в зеркале) 1-56 параметр 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_in + i2] });//запись в новый rez
                                                            }

                                                            //ДИСКРЕТНЫЕ ВЫХОДЫ
                                                            for (int i_d_out = 11; i_d_out <= 31; i_d_out++)//*21 байт*  11-31 байты из файла ask (8-28 байт в зеркале) 1-168 параметр
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_out + i2] });//запись в новый rez 
                                                            }

                                                            //БИТОВЫЕ ПЕРЕМЕННЫЕ
                                                            for (int i_bit_perem = 32; i_bit_perem <= 47; i_bit_perem++)//*16 байт*  32-47 байты из файла ask (29-44 байт в зеркале) 169-296 параметр
                                                            {
                                                                bw.Write(new byte[] { buffer[i_bit_perem + i2] });//запись в новый rez
                                                            }
                                                            //общий размер пакета rez файла 460 байт                                      
                                                            //добавление данных АСК
                                                            //выбрать параметры и дописать в файл, если стоит галочка 
                                                            if (checkBox1.Checked == true)
                                                            {                                                               
                                                                //координаты широты
                                                                bw.Write(new byte[] { buffer[367 + i2] });//запись в новый rez данных АСК - градусы широты
                                                                bw.Write(new byte[] { buffer[368 + i2] });//запись в новый rez данных АСК - минуты широты 
                                                                for (int j = 369; j <= 376; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды широты   
                                                                //координаты долготы
                                                                bw.Write(new byte[] { buffer[377 + i2] });//запись в новый rez данных АСК - градусы долготы 
                                                                bw.Write(new byte[] { buffer[378 + i2] });//запись в новый rez данных АСК - минуты долготы
                                                                for (int j = 379; j <= 386; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды долготы

                                                                bw.Write(new byte[] { buffer[387 + i2] });//запись в новый rez данных АСК - полушарие - меридиан
                                                                bw.Write(new byte[] { buffer[388 + i2] });//запись в новый rez данных АСК - количество спутников
                                                                for (int j = 389; j <= 396; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - высота над уровнем моря
                                                                for (int j = 397; j <= 404; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - скорость км/час
                                                                for (int j = 405; j <= 412; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо левый кг
                                                                for (int j = 413; j <= 420; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо правый кг
                                                                for (int j = 421; j <= 428; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива левый
                                                                for (int j = 429; j <= 436; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива правый
                                                                for (int j = 437; j <= 444; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5В
                                                                for (int j = 445; j <= 452; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - среднее топливо
                                                                for (int j = 453; j <= 460; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5ВDUAL
                                                                for (int j = 461; j <= 468; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура CPU, оС
                                                                for (int j = 469; j <= 476; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура сист. платы, оС

                                                                bw.Write(new byte[] { buffer[477 + i2] });//запись в новый rez данных АСК - дискретные входные
                                                                bw.Write(new byte[] { buffer[478 + i2] });//запись в новый rez данных АСК - дискретные выходные
                                                            }
                                                        }
                                                        //***
                                                        progressBar1.Visible = true;
                                                        progressBar1.Minimum = 0;
                                                        progressBar1.Maximum = bytesRead - 1;
                                                        progressBar1.Step = 1;
                                                        progressBar1.PerformStep();
                                                        Application.DoEvents(); //передача управления системе для устранения зависания
                                                    }
                                                    label23.Text = "";
                                                    progressBar1.Visible = false;
                                                }//конец записи потока
                                            }//конец потока записываемого файла              

                                            Array.Clear(buffer, 0, buffer.Length); //очищаем массив буфера                                                                                     
                                        }//end if час выбранный входит в имя файла аск              
                                    } //foreach файлы аск
                                } //end if ТЕП70БС
                                //*****______конец ТЕП70БС________________________________________________________________________________________________

                                //___2TE116Y____****______________________________________________________________________________________________________
                                if (comboBox1.Text == "2TE116U")
                                {
                                    //  MessageBox.Show("Конфигурация для 2ТЭ116У отсутствует");
                                    //++                          MessageBox.Show("Обработка файла 2ТЕ116У " + (f.Name));
                                    new_file_name_rez = "2TE116_" + comboBox4.Text + "_" + textBox7.Text + day + mon + comboBox2.Text + "00";//имя для итогового файла rez 
                                    if (checkBox1.Checked == true) { new_file_name_rez = new_file_name_rez + "_with_ask"; }//если стоит метка добавления данных аск
                                    //удаляем файл перед созданием, если он уже существует
                                    if (System.IO.File.Exists(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"))
                                    {
                                        System.IO.File.Delete(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez");
                                    }

                                    //просмотр имени распакованного файла
                                    System.Threading.Thread.Sleep(1000);//приостановка выполнения программы на 1секеунду (если нужнен граничный к 00 или 23ч файл,то может не успеть распк=аковаться раром и в директории найдутся не все файлы)
                                    System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(@"" + textBox4.Text + "/" + sd + "_ask");
                                    string file_name_in_rar = "";//имя файла из архива 


                                    foreach (System.IO.FileInfo f in d.GetFiles("*.ask"))
                                    {
                                        file_name_in_rar = f.Name;//имя файла с расширением
                                        //Path.GetFileNameWithoutExtension(f.Name);//имя файла без расширения
                                        //    MessageBox.Show("без расширения" + Path.GetFileNameWithoutExtension(f.Name));
                                        string dat_ask = "";
                                        //если старый формат файлов то
                                        if (System.IO.Path.GetFileNameWithoutExtension(f.Name).IndexOf("2TE116U") > -1)
                                        {
                                            dat_ask = System.IO.Path.GetFileNameWithoutExtension(f.Name).Remove(0, 18);//вырезаем время файла формат: ччмм                                                               
                                            dat_ask = dat_ask.Remove(2, 2);//вырезаем минуты и оставляем только чч
                                        }
                                        else//новый формат файлов
                                        {
                                            dat_ask = System.IO.Path.GetFileNameWithoutExtension(f.Name).Remove(0, 13);//вырезаем время файла формат: ччмм                                                               
                                            dat_ask = dat_ask.Remove(2, 2);//вырезаем минуты и оставляем только чч
                                        }
                                        //MessageBox.Show(dat_ask);  
                                          
                                        if (dat_ask == comboBox2.Text)
                                        {
                                            //  MessageBox.Show("время входит в выбранное" + (f.Name));
                                            kol_failes_ok++;

                                            int bytesRead = 0;   //количество байт в читаемом файле
                                            byte[] buffer = new byte[2000000]; //буфер памяти 2м
                                            using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                                            {
                                                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                                                {
                                                    bytesRead = br.Read(buffer, 0, buffer.Length);///////+
                                                    //                                   MessageBox.Show("Размер файла = " + bytesRead + " байт");///////+
                                                    int kratnost = bytesRead % 384;
                                                    if (kratnost != 0)
                                                    {
                                                        //                                    MessageBox.Show("Файл кратен размеру пакета!");
                                                    }
                                                    int kol_pak = 0;
                                                    int cb = 0;
                                                    for (int i = 0; i <= bytesRead - 1; i++) //от 0 до количества считанных байт в считываемом файле
                                                    {
                                                        cb = cb + 1;//накапливаем сумму байт пакета
                                                        //- result.AppendFormat("{0:x2} ", buffer[i]);//в результирующую строку добавляем байт из буфера в формате х2 (2символа)                          
                                                        if ((buffer[i] == 0XFF) && (buffer[i + 1] == 0X0F) && (buffer[i + 2] == 0XFB))
                                                        {
                                                            kol_pak = kol_pak + 1;//накапливаем количество пакетов в файле
                                                            cb = 0;//скидываем сумму байт в пакете
                                                            //  MessageBox.Show(i+"  ");
                                                            //MessageBox.Show(buffer[234] + " " + buffer[235] + " " + buffer[236] + " " + buffer[237]); //вывод последнего байта в файле
                                                        }
                                                    }
                                                    //                                  MessageBox.Show("Найдено " + kol_pak + " пакетов в файле");
                                                    if (kol_pak > 0)//если пакет
                                                    {
                                                        cb = cb + 1;
                                                    }
                                                    //++                                           MessageBox.Show("Размер пакета   " + cb + " байт");
                                                    //просмотр байт//+   MessageBox.Show(buffer[0] + " " + buffer[210 + 487] + " " + buffer[211 + 487] + " год " + buffer[212 + 487] + " месяц  " + buffer[213] + " день  " + buffer[214] + " час  " + buffer[215] + " минута " + buffer[216 + 487] + " сек "); //вывод даты   

                                                }//конец потока битов читаемого файла
                                            }//конец потока читаемого файла      

                                            //ЗАПИСЬ

                                            System.IO.FileInfo f2 = new System.IO.FileInfo(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"); //файл rez, в который пишем файлы аск
                                            using (System.IO.FileStream fs2 = f2.Open(System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.Read))//открытие записываемого файла как потока
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs2))//открытие потока записи в потоке файла для записи
                                                {
                                                    label7.Text = "Объединение файлов ...";
                                                    progressBar1.Value = 0;

                                                    bw.Seek(0, System.IO.SeekOrigin.End);//УСТАНОВКА записи в конец файла
                                                    for (int i2 = 0; i2 <= bytesRead - 1; i2++) //цикл от 0 до количества считанных байт в читаемом файле
                                                    {
                                                        //***         2te116y - ask convert to rez  
                                                        if ((buffer[i2] == 0XFF) && (buffer[i2 + 1] == 0X0F) && (buffer[i2 + 2] == 0XFB))
                                                        {
                                                            //*пакет rez файла начинается с аналоговых параметров (262 байта)                      
                                                            //АНАЛОГОВЫЕ 91 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 29; i_analog1 <= 210; i_analog1++)//*182 байт*         30-211 байты ask (27-208 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 6 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog2 = 211; i_analog2 <= 216; i_analog2++)//6*2= *12 байт*        212-217 байты ask (209-214 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog2 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 18 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 217; i_analog1 <= 252; i_analog1++)//*36 байт*         218-253 байты ask (215-250 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //*после аналоговых параметров в пакет rez файла записывается дата - день, месяц, год, час, мин, сек (6 байт)

                                                            //MessageBox.Show(buffer[377 + i2] + " сек -");                             
                                                            bw.Write(new byte[] { buffer[378 + i2] });//запись в новый rez день из аск  //
                                                            bw.Write(new byte[] { buffer[377 + i2] });//месяц
                                                            bw.Write(new byte[] { buffer[376 + i2] });//год
                                                            bw.Write(new byte[] { buffer[379 + i2] });//час
                                                            bw.Write(new byte[] { buffer[380 + i2] });//мин
                                                            bw.Write(new byte[] { buffer[381 + i2] });//сек
                                                            //MessageBox.Show("  " + buffer[378+i2] + "  " + buffer[377+i2] + "  " + buffer[376+i2] + "  " + buffer[379+i2] + "  " + buffer[380+i2] + "  " + buffer[381 + i2]);
                                                            label23.Text = "  " + buffer[378 + i2] + "/" + buffer[377 + i2] + "/" + buffer[376 + i2] + "  " + buffer[379 + i2] + ":" + buffer[380 + i2] + ":" + buffer[381 + i2];
                                                            //*после даты в пакет rez файла записывается значения дискретных параметров (26 байт)
                                                            //ДИСКРЕТНЫЕ ВХОДЫ 
                                                            for (int i_d_in = 4; i_d_in <= 9; i_d_in++)//*6 байт*  4-9 байты из файла ask (1-6 байт в зеркале) 1-48 параметр 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_in + i2] });//запись в новый rez
                                                            }

                                                            //ДИСКРЕТНЫЕ ВЫХОДЫ
                                                            for (int i_d_out = 10; i_d_out <= 29; i_d_out++)//*20 байт*  10-29 байты из файла ask (7-26 байт в зеркале) 1-160 параметр
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_out + i2] });//запись в новый rez 
                                                            }
                                                            //выбрать параметры и дописать в файл, если стоит галочка 
                                                            if (checkBox1.Checked == true)
                                                            {
                                                                bw.Write(new byte[] { buffer[253 + i2] }); bw.Write(new byte[] { buffer[254 + i2] });//запись в новый rez данных АСК - табельный номер                                                              
                                                                bw.Write(new byte[] { buffer[255 + i2] }); bw.Write(new byte[] { buffer[256 + i2] });//запись в новый rez данных АСК - номер марсшрута
                                                                bw.Write(new byte[] { buffer[257 + i2] }); bw.Write(new byte[] { buffer[258 + i2] });//запись в новый rez данных АСК - вес поезда
                                                                bw.Write(new byte[] { buffer[259 + i2] }); bw.Write(new byte[] { buffer[260 + i2] });//запись в новый rez данных АСК - регион эксплуатации
                                                                bw.Write(new byte[] { buffer[261 + i2] }); bw.Write(new byte[] { buffer[262 + i2] });//запись в новый rez данных АСК - плотность топлива
                                                                bw.Write(new byte[] { buffer[263 + i2] }); //запись в новый rez данных АСК - режим эксплуатации
                                                                //координаты широты
                                                                bw.Write(new byte[] { buffer[264 + i2] });//запись в новый rez данных АСК - градусы широты
                                                                bw.Write(new byte[] { buffer[265 + i2] });//запись в новый rez данных АСК - минуты широты 
                                                                for (int j = 266; j <= 273; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды широты   
                                                                //координаты долготы
                                                                bw.Write(new byte[] { buffer[274 + i2] });//запись в новый rez данных АСК - градусы долготы 
                                                                bw.Write(new byte[] { buffer[275 + i2] });//запись в новый rez данных АСК - минуты долготы
                                                                for (int j = 276; j <= 283; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды долготы

                                                                bw.Write(new byte[] { buffer[284 + i2] });//запись в новый rez данных АСК - полушарие - меридиан
                                                                bw.Write(new byte[] { buffer[285 + i2] });//запись в новый rez данных АСК - количество спутников
                                                                for (int j = 286; j <= 293; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - высота над уровнем моря
                                                                for (int j = 294; j <= 301; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - скорость км/час
                                                                for (int j = 302; j <= 309; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо левый кг
                                                                for (int j = 310; j <= 317; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо правый кг
                                                                for (int j = 318; j <= 325; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива левый
                                                                for (int j = 326; j <= 333; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива правый
                                                                for (int j = 334; j <= 341; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5В
                                                                for (int j = 342; j <= 349; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - среднее топливо
                                                                for (int j = 350; j <= 357; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5ВDUAL
                                                                for (int j = 358; j <= 365; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура CPU, оС
                                                                for (int j = 366; j <= 373; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура сист. платы, оС
                                                               
                                                                bw.Write(new byte[] { buffer[374 + i2] });//запись в новый rez данных АСК - дискретные входные
                                                                bw.Write(new byte[] { buffer[375 + i2] });//запись в новый rez данных АСК - дискретные выходные
                                                            }
                                                        }
                                                        //***
                                                        progressBar1.Visible = true;
                                                        progressBar1.Minimum = 0;
                                                        progressBar1.Maximum = bytesRead - 1;
                                                        progressBar1.Step = 1;
                                                        progressBar1.PerformStep();
                                                        Application.DoEvents(); //передача управления системе для устранения зависания
                                                    }
                                                    progressBar1.Visible = false;
                                                    label23.Text = "";
                                                }//конец записи потока
                                            }//конец потока записываемого файла 


                                            Array.Clear(buffer, 0, buffer.Length); //очищаем массив буфера  
                                        }//end if час выбранный входит в имя файла аск 
                                    }//foreach файлы аск                                                              
                                }//end if 2ТЕ116У
                                //*****______конец 2TE116Y________________________________________________________________________________________________

                                //___3TE116Y____****______________________________________________________________________________________________________
                                if (comboBox1.Text == "3TE116U")
                                {
                                    //  MessageBox.Show("Конфигурация для 2ТЭ116У отсутствует");
                                    //++                          MessageBox.Show("Обработка файла 3ТЕ116У " + (f.Name));
                                    new_file_name_rez = "3TE116_" + comboBox4.Text + "_" + textBox7.Text + day + mon + comboBox2.Text + "00";//имя для итогового файла rez 
                                    if (checkBox1.Checked == true) { new_file_name_rez = new_file_name_rez + "_with_ask"; }//если стоит метка добавления данных аск
                                    //удаляем файл перед созданием, если он уже существует
                                    if (System.IO.File.Exists(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"))
                                    {
                                        System.IO.File.Delete(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez");
                                    }

                                    //просмотр имени распакованного файла
                                    System.Threading.Thread.Sleep(1000);//приостановка выполнения программы на 1секеунду (если нужнен граничный к 00 или 23ч файл,то может не успеть распк=аковаться раром и в директории найдутся не все файлы)
                                    System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(@"" + textBox4.Text + "/" + sd + "_ask");
                                    string file_name_in_rar = "";//имя файла из архива 
                                    
                                    foreach (System.IO.FileInfo f in d.GetFiles("*.ask"))
                                    {
                                        file_name_in_rar = f.Name;//имя файла с расширением
                                        //Path.GetFileNameWithoutExtension(f.Name);//имя файла без расширения
                                        //    MessageBox.Show("без расширения" + Path.GetFileNameWithoutExtension(f.Name));
                                        string dat_ask = "";
                                        dat_ask = System.IO.Path.GetFileNameWithoutExtension(f.Name).Remove(0, 18);//вырезаем время файла формат: ччмм                                                               
                                        dat_ask = dat_ask.Remove(2, 2);//вырезаем минуты и оставляем только чч
                                        //MessageBox.Show(dat_ask);                                            
                                        if (dat_ask == comboBox2.Text)
                                        {
                                            //  MessageBox.Show("время входит в выбранное" + (f.Name));
                                            kol_failes_ok++;

                                            int bytesRead = 0;   //количество байт в читаемом файле
                                            byte[] buffer = new byte[2000000]; //буфер памяти 2м
                                            using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                                            {
                                                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                                                {
                                                    bytesRead = br.Read(buffer, 0, buffer.Length);///////+
                                                    //                                   MessageBox.Show("Размер файла = " + bytesRead + " байт");///////+
                                                    int kratnost = bytesRead % 384;
                                                    if (kratnost != 0)
                                                    {
                                                        //                                    MessageBox.Show("Файл кратен размеру пакета!");
                                                    }
                                                    int kol_pak = 0;
                                                    int cb = 0;
                                                    for (int i = 0; i <= bytesRead - 1; i++) //от 0 до количества считанных байт в считываемом файле
                                                    {
                                                        cb = cb + 1;//накапливаем сумму байт пакета
                                                        //- result.AppendFormat("{0:x2} ", buffer[i]);//в результирующую строку добавляем байт из буфера в формате х2 (2символа)                          
                                                        if ((buffer[i] == 0XFF) && (buffer[i + 1] == 0X0F) && (buffer[i + 2] == 0XFB))
                                                        {
                                                            kol_pak = kol_pak + 1;//накапливаем количество пакетов в файле
                                                            cb = 0;//скидываем сумму байт в пакете
                                                            //  MessageBox.Show(i+"  ");
                                                            //MessageBox.Show(buffer[234] + " " + buffer[235] + " " + buffer[236] + " " + buffer[237]); //вывод последнего байта в файле
                                                        }
                                                    }
                                                    //                                  MessageBox.Show("Найдено " + kol_pak + " пакетов в файле");
                                                    if (kol_pak > 0)//если пакет
                                                    {
                                                        cb = cb + 1;
                                                    }
                                                    //++                                           MessageBox.Show("Размер пакета   " + cb + " байт");
                                                    //просмотр байт//+   MessageBox.Show(buffer[0] + " " + buffer[210 + 487] + " " + buffer[211 + 487] + " год " + buffer[212 + 487] + " месяц  " + buffer[213] + " день  " + buffer[214] + " час  " + buffer[215] + " минута " + buffer[216 + 487] + " сек "); //вывод даты   

                                                }//конец потока битов читаемого файла
                                            }//конец потока читаемого файла      

                                            //ЗАПИСЬ

                                            System.IO.FileInfo f2 = new System.IO.FileInfo(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"); //файл rez, в который пишем файлы аск
                                            using (System.IO.FileStream fs2 = f2.Open(System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.Read))//открытие записываемого файла как потока
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs2))//открытие потока записи в потоке файла для записи
                                                {
                                                    label7.Text = "Объединение файлов ...";
                                                    progressBar1.Value = 0;

                                                    bw.Seek(0, System.IO.SeekOrigin.End);//УСТАНОВКА записи в конец файла
                                                    for (int i2 = 0; i2 <= bytesRead - 1; i2++) //цикл от 0 до количества считанных байт в читаемом файле
                                                    {
                                                        //***         2te116y - ask convert to rez  
                                                        if ((buffer[i2] == 0XFF) && (buffer[i2 + 1] == 0X0F) && (buffer[i2 + 2] == 0XFB))
                                                        {
                                                            //*пакет rez файла начинается с аналоговых параметров (262 байта)                      
                                                            //АНАЛОГОВЫЕ 91 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 29; i_analog1 <= 210; i_analog1++)//*182 байт*         30-211 байты ask (27-208 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 6 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog2 = 211; i_analog2 <= 216; i_analog2++)//6*2= *12 байт*        212-217 байты ask (209-214 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog2 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 18 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 217; i_analog1 <= 252; i_analog1++)//*36 байт*         218-253 байты ask (215-250 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //*после аналоговых параметров в пакет rez файла записывается дата - день, месяц, год, час, мин, сек (6 байт)

                                                            //MessageBox.Show(buffer[377 + i2] + " сек -");                             
                                                            bw.Write(new byte[] { buffer[378 + i2] });//запись в новый rez день из аск  //
                                                            bw.Write(new byte[] { buffer[377 + i2] });//месяц
                                                            bw.Write(new byte[] { buffer[376 + i2] });//год
                                                            bw.Write(new byte[] { buffer[379 + i2] });//час379
                                                            bw.Write(new byte[] { buffer[380 + i2] });//мин
                                                            bw.Write(new byte[] { buffer[381 + i2] });//сек
                                                            label23.Text = "  " + buffer[378 + i2] + "/" + buffer[377 + i2] + "/" + buffer[376 + i2] + "  " + buffer[379 + i2] + ":" + buffer[380 + i2] + ":" + buffer[381 + i2];
                                                            //*после даты в пакет rez файла записывается значения дискретных параметров (26 байт)
                                                            //ДИСКРЕТНЫЕ ВХОДЫ 
                                                            for (int i_d_in = 4; i_d_in <= 9; i_d_in++)//*6 байт*  4-9 байты из файла ask (1-6 байт в зеркале) 1-48 параметр 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_in + i2] });//запись в новый rez
                                                            }

                                                            //ДИСКРЕТНЫЕ ВЫХОДЫ
                                                            for (int i_d_out = 10; i_d_out <= 29; i_d_out++)//*20 байт*  10-29 байты из файла ask (7-26 байт в зеркале) 1-160 параметр
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_out + i2] });//запись в новый rez 
                                                            }
                                                            //выбрать параметры и дописать в файл, если стоит галочка 
                                                            if (checkBox1.Checked == true)
                                                            {
                                                                bw.Write(new byte[] { buffer[253 + i2] }); bw.Write(new byte[] { buffer[254 + i2] });//запись в новый rez данных АСК - табельный номер                                                              
                                                                bw.Write(new byte[] { buffer[255 + i2] }); bw.Write(new byte[] { buffer[256 + i2] });//запись в новый rez данных АСК - номер марсшрута
                                                                bw.Write(new byte[] { buffer[257 + i2] }); bw.Write(new byte[] { buffer[258 + i2] });//запись в новый rez данных АСК - вес поезда
                                                                bw.Write(new byte[] { buffer[259 + i2] }); bw.Write(new byte[] { buffer[260 + i2] });//запись в новый rez данных АСК - регион эксплуатации
                                                                bw.Write(new byte[] { buffer[261 + i2] }); bw.Write(new byte[] { buffer[262 + i2] });//запись в новый rez данных АСК - плотность топлива
                                                                bw.Write(new byte[] { buffer[263 + i2] }); //запись в новый rez данных АСК - режим эксплуатации
                                                                //координаты широты
                                                                bw.Write(new byte[] { buffer[264 + i2] });//запись в новый rez данных АСК - градусы широты
                                                                bw.Write(new byte[] { buffer[265 + i2] });//запись в новый rez данных АСК - минуты широты 
                                                                for (int j = 266; j <= 273; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды широты   
                                                                //координаты долготы
                                                                bw.Write(new byte[] { buffer[274 + i2] });//запись в новый rez данных АСК - градусы долготы 
                                                                bw.Write(new byte[] { buffer[275 + i2] });//запись в новый rez данных АСК - минуты долготы
                                                                for (int j = 276; j <= 283; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды долготы

                                                                bw.Write(new byte[] { buffer[284 + i2] });//запись в новый rez данных АСК - полушарие - меридиан
                                                                bw.Write(new byte[] { buffer[285 + i2] });//запись в новый rez данных АСК - количество спутников
                                                                for (int j = 286; j <= 293; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - высота над уровнем моря
                                                                for (int j = 294; j <= 301; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - скорость км/час
                                                                for (int j = 302; j <= 309; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо левый кг
                                                                for (int j = 310; j <= 317; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо правый кг
                                                                for (int j = 318; j <= 325; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива левый
                                                                for (int j = 326; j <= 333; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива правый
                                                                for (int j = 334; j <= 341; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5В
                                                                for (int j = 342; j <= 349; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - среднее топливо
                                                                for (int j = 350; j <= 357; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5ВDUAL
                                                                for (int j = 358; j <= 365; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура CPU, оС
                                                                for (int j = 366; j <= 373; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура сист. платы, оС

                                                                bw.Write(new byte[] { buffer[374 + i2] });//запись в новый rez данных АСК - дискретные входные
                                                                bw.Write(new byte[] { buffer[375 + i2] });//запись в новый rez данных АСК - дискретные выходные
                                                            }

                                                        }
                                                        //***
                                                        progressBar1.Visible = true;
                                                        progressBar1.Minimum = 0;
                                                        progressBar1.Maximum = bytesRead - 1;
                                                        progressBar1.Step = 1;
                                                        progressBar1.PerformStep();
                                                        Application.DoEvents(); //передача управления системе для устранения зависания
                                                    }
                                                    label23.Text = "";
                                                    progressBar1.Visible = false;
                                                }//конец записи потока
                                            }//конец потока записываемого файла 


                                            Array.Clear(buffer, 0, buffer.Length); //очищаем массив буфера  
                                        }//end if час выбранный входит в имя файла аск 
                                    }//foreach файлы аск                                                              
                                }//end if 3ТЕ116У


                                //___2TE25A____****______________________________________________________________________________________________________
                                if (comboBox1.Text == "2TE25A")
                                {
                                    //  MessageBox.Show("###нет обработки для 2TE25A");
                                    //  MessageBox.Show("Конфигурация для 2ТЭ116У отсутствует");
                                    //++                          MessageBox.Show("Обработка файла 3ТЕ116У " + (f.Name));
                                    new_file_name_rez = "2TE25A_" + comboBox4.Text + "_" + textBox7.Text + day + mon + comboBox2.Text + "00";//имя для итогового файла rez
                                    if (checkBox1.Checked == true) { new_file_name_rez = new_file_name_rez + "_with_ask"; }//если стоит метка добавления данных аск

                                    //удаляем файл перед созданием, если он уже существует
                                    if (System.IO.File.Exists(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"))
                                    {
                                        System.IO.File.Delete(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez");
                                    }

                                    //просмотр имени распакованного файла
                                    System.Threading.Thread.Sleep(1000);//приостановка выполнения программы на 1секеунду (если нужнен граничный к 00 или 23ч файл,то может не успеть распк=аковаться раром и в директории найдутся не все файлы)
                                    System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(@"" + textBox4.Text + "/" + sd + "_ask");
                                    string file_name_in_rar = "";//имя файла из архива 
                                   
                                    foreach (System.IO.FileInfo f in d.GetFiles("*.ask"))
                                    {
                                        file_name_in_rar = f.Name;//имя файла с расширением
                                        // Path.GetFileNameWithoutExtension(f.Name);//имя файла без расширения
                                        // MessageBox.Show("без расширения" + Path.GetFileNameWithoutExtension(f.Name));
                                        string dat_ask = "";
                                        dat_ask = System.IO.Path.GetFileNameWithoutExtension(f.Name).Remove(0, System.IO.Path.GetFileNameWithoutExtension(f.Name).Length - 4);//вырезаем время файла формат: ччмм                                                               
                                        dat_ask = dat_ask.Remove(2, 2);//вырезаем минуты и оставляем только чч                                                                                    
                                        if (dat_ask == comboBox2.Text)
                                        {
                                            //  MessageBox.Show("время входит в выбранное" + (f.Name));
                                            kol_failes_ok++;

                                            int bytesRead = 0;   //количество байт в читаемом файле
                                            byte[] buffer = new byte[2000000]; //буфер памяти 2м
                                            using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                                            {
                                                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                                                {
                                                    bytesRead = br.Read(buffer, 0, buffer.Length);///////+ //длина буфера с 0 элемента
                                                    //MessageBox.Show("Размер файла = " + bytesRead + " байт");///////+
                                                    int kratnost = bytesRead % 621;
                                                    if (kratnost != 0)
                                                    {
                                                        //                   MessageBox.Show("Файл кратен размеру пакета!");
                                                    }
                                                    int kol_pak = 0;
                                                    int cb = 0;
                                                    for (int i = 0; i <= bytesRead - 1; i++) //от 0 до количества считанных байт в считываемом файле
                                                    {
                                                        cb = cb + 1;//накапливаем сумму байт пакета                                                                                       
                                                        if ((buffer[i] == 0XFF) && (buffer[i + 1] == 0X0F))
                                                        {
                                                            //                 MessageBox.Show("Размер пакета   " + cb + " байт");                                    
                                                            kol_pak = kol_pak + 1;//накапливаем количество пакетов в файле
                                                            cb = 0;//скидываем сумму байт в пакете
                                                            //  MessageBox.Show(i+"  ");
                                                            //  MessageBox.Show(buffer[234] + " " + buffer[235] + " " + buffer[236] + " " + buffer[237]); //вывод последнего байта в файле
                                                        }
                                                    }
                                                    //MessageBox.Show("Найдено " + kol_pak + " пакетов в файле");
                                                    if (kol_pak > 0)//если пакет
                                                    {
                                                        cb = cb + 1;
                                                    }
                                                    //++MessageBox.Show("Размер пакета   " + cb + " байт");
                                                    //просмотр байт//+   MessageBox.Show(buffer[0] + " " + buffer[210 + 487] + " " + buffer[211 + 487] + " год " + buffer[212 + 487] + " месяц  " + buffer[213] + " день  " + buffer[214] + " час  " + buffer[215] + " минута " + buffer[216 + 487] + " сек "); //вывод даты   

                                                }//конец потока битов читаемого файла
                                            }//конец потока читаемого файла      

                                            //ЗАПИСЬ

                                            System.IO.FileInfo f2 = new System.IO.FileInfo(@"" + textBox8.Text + "\\" + new_file_name_rez + ".rez"); //файл rez, в который пишем файлы аск
                                            using (System.IO.FileStream fs2 = f2.Open(System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.Read))//открытие записываемого файла как потока
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs2))//открытие потока записи в потоке файла для записи
                                                {
                                                    label7.Text = "Объединение файлов ...";
                                                    progressBar1.Value = 0;

                                                    bw.Seek(0, System.IO.SeekOrigin.End);//УСТАНОВКА записи в конец файла
                                                    for (int i2 = 0; i2 <= bytesRead - 1; i2++) //цикл от 0 до количества считанных байт в читаемом файле
                                                    {
                                                        //***         2te25a - ask convert to rez  
                                                        if ((buffer[i2] == 0XFF) && (buffer[i2 + 1] == 0X0F))//и оставшаяся длинна юолбше пакета(620 байт)
                                                        {
                                                            //АНАЛОГОВЫЕ__________________________________________________
                                                            //*пакет rez файла начинается с аналоговых параметров (___ байта)                      
                                                            //АНАЛОГОВЫЕ 53 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 25; i_analog1 <= 130; i_analog1++)//*106 байт*  26-131 байты из файла ask (24-129 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 10 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 131; i_analog1 <= 150; i_analog1++)//*20 байт*  132-151 байты из файла ask (130-149 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 16 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 151; i_analog1 <= 182; i_analog1++)//*32 байт*  152-183 байты из файла ask (150-181 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 18 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 183; i_analog1 <= 218; i_analog1++)//*36 байт*  184-219 байты из файла ask (182-217 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 4 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 219; i_analog1 <= 222; i_analog1++)//4*2 = *8 байт*  220-223 байты из файла ask (218-221 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 5 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 223; i_analog1 <= 232; i_analog1++)//*10 байт*  224-233 байты из файла ask (222-231 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 4 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 233; i_analog1 <= 236; i_analog1++)//4*2 = *8 байт*  234-237 байты из файла ask (232-235 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 237; i_analog1 <= 238; i_analog1++)//*2 байта*  238-239 байты из файла ask (236-237 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 239; i_analog1 <= 239; i_analog1++)//1*2 = *2 байта*  240 байты из файла ask (238 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 3 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 240; i_analog1 <= 245; i_analog1++)//*6 байта*  241-246 байты из файла ask (239-244 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 246; i_analog1 <= 246; i_analog1++)//1*2 = *2 байта*  247 байты из файла ask (245 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 2 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 247; i_analog1 <= 250; i_analog1++)//*4 байта*  248-251 байты из файла ask (246-249 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 251; i_analog1 <= 251; i_analog1++)//1*2 = *2 байта*  252 байты из файла ask (250 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            for (int i_analog1 = 252; i_analog1 <= 253; i_analog1++)//*2 байта*  243-254 байты из файла ask (251-252 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            for (int i_analog1 = 254; i_analog1 <= 259; i_analog1++)//*6 байта*  255-260 байты из файла ask (253-258 байты в зеркале) работа дизеля
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez // как есть 6 байт
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 260; i_analog1 <= 260; i_analog1++)//1*2 = *2 байта*  261 байты из файла ask (259 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 261; i_analog1 <= 262; i_analog1++)//*2 байта*  262-263 байты из файла ask (260-261 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 3 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 263; i_analog1 <= 265; i_analog1++)//3*2 = *6 байта*  264-266 байты из файла ask (262-264 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 266; i_analog1 <= 267; i_analog1++)//*2 байта*  267-268 байты из файла ask (265-266 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 268; i_analog1 <= 268; i_analog1++)//1*2 = *2 байта*  269 байты из файла ask (267 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 3 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 269; i_analog1 <= 274; i_analog1++)//*6 байта*  270-275 байты из файла ask (268-273 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 275; i_analog1 <= 275; i_analog1++)//1*2 = *2 байта*  276 байты из файла ask (274 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 4 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 276; i_analog1 <= 283; i_analog1++)//*8 байта*  277-284 байты из файла ask (275-282 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 18 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 284; i_analog1 <= 301; i_analog1++)//18*2 = *36 байт*  285-332 байты из файла ask (283-300 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 2 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 302; i_analog1 <= 305; i_analog1++)//*4 байта*  303-306 байты из файла ask (301-304 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 4 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 306; i_analog1 <= 309; i_analog1++)//4*2 = *8 байта*  307-310 байты из файла ask (305-308 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 2 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 310; i_analog1 <= 313; i_analog1++)//*4 байта*  311-314 байты из файла ask (309-312 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 4 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 314; i_analog1 <= 317; i_analog1++)//4*2 = *8 байта*  315-318 байты из файла ask (313-316 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 3 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 318; i_analog1 <= 320; i_analog1++)//3*2 = *6 байта*  319-321 байты из файла ask (317-319 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 3 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 321; i_analog1 <= 326; i_analog1++)//*6 байта*  322-327 байты из файла ask (320-325 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 327; i_analog1 <= 327; i_analog1++)//1*2 = *2 байта*  328 байты из файла ask (326 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 3 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 328; i_analog1 <= 333; i_analog1++)//*6 байт*  329-334 байты из файла ask (327-332 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 334; i_analog1 <= 334; i_analog1++)//1*2 = *2 байта*  335 байты из файла ask (333 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 335; i_analog1 <= 336; i_analog1++)//*2 байта*  336-337 байты из файла ask (334-335 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 4 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 337; i_analog1 <= 344; i_analog1++)//*8 байт*  338-345 байты из файла ask (336-343 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 6 х 1 байт // ДАТА-ВРЕМЯ                              
                                                            for (int i_analog1 = 345; i_analog1 <= 350; i_analog1++)//*6*2=12 байт*  346-351 байты из файла ask (344-349 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0
                                                            }
                                                            //АНАЛОГОВЫЕ 2 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 351; i_analog1 <= 352; i_analog1++)//*2 байта*  352-353 байты из файла ask (350-351 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 8 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 353; i_analog1 <= 368; i_analog1++)//*16 байт*  354-369 байты из файла ask (352-367 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 7 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 369; i_analog1 <= 375; i_analog1++)//*14 байт*  370-376 байты из файла ask (368-374 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 376; i_analog1 <= 376; i_analog1++)//1*2 = *2 байта*  377 байты из файла ask (375 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //АНАЛОГОВЫЕ 1 х 2 байта // двухбайтные в зеркале                              
                                                            for (int i_analog1 = 377; i_analog1 <= 378; i_analog1++)//*2 байта*  378-379 байты из файла ask (376-377 байты в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                            }
                                                            //АНАЛОГОВЫЕ 2 х 1 байт // однобайтовые в зеркале
                                                            for (int i_analog1 = 379; i_analog1 <= 380; i_analog1++)//*4 байта*  380-381 байты из файла ask (378-379 байт в зеркале) 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_analog1 + i2] });//запись в новый rez
                                                                bw.Write(new byte[] { 0X00 });         //к каждому однобайтовому параметру добавляем один байт=0                                 
                                                            }
                                                            //ДАТА ВРЕМЯ_____________________________________________________ 
                                                            //*после аналоговых параметров в пакет rez файла записывается дата - день, месяц, год, час, мин, сек (6 байт)                                                                 
                                                            //MessageBox.Show(buffer[612 + i2] + "г---  " + buffer[613 + i2] + "м---  " + buffer[614 + i2] + "д---  " + buffer[615 + i2] + "ч---  " + buffer[616 + i2] + "мин---  " + buffer[617 + i2] + "сек---  " + BitConverter.ToDouble(buffer, 602 + i2) + "C---  " + BitConverter.ToDouble(buffer, 594 + i2) + "C---  " + BitConverter.ToDouble(buffer, 586 + i2) + "B---  " + BitConverter.ToDouble(buffer, 522 + i2) + "ct---  ");                             
                                                            //-MessageBox.Show(buffer[345 + i2] + "г---  " + buffer[346 + i2] + "м---  " + buffer[347 + i2] + "д---  " + buffer[348 + i2]);
                                                            bw.Write(new byte[] { buffer[347 + i2] });//запись в новый rez день из аск  //
                                                            bw.Write(new byte[] { buffer[346 + i2] });//месяц
                                                            bw.Write(new byte[] { buffer[345 + i2] });//год
                                                            bw.Write(new byte[] { buffer[348 + i2] });//час
                                                            bw.Write(new byte[] { buffer[349 + i2] });//мин
                                                            bw.Write(new byte[] { buffer[350 + i2] });//сек

                                                            //после даты в пакет rez файла записывается значения дискретных параметров (23 байта)
                                                            //ДИСКРЕТНЫЕ ВЫХОДЫ______________________________________________ 
                                                            for (int i_d_out = 2; i_d_out <= 7; i_d_out++)//*6 байт*  3-8 байты из файла ask (1-6 байт в зеркале) 1-48 параметр 
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_out + i2] });//запись в новый rez
                                                            }                                                            
                                                            //ДИСКРЕТНЫЕ ВХОДЫ_________________________________________________
                                                            for (int i_d_in = 8; i_d_in <= 24; i_d_in++)//*17 байт*  9-25 байты из файла ask (7-23 байт в зеркале) 1-136 параметр
                                                            {
                                                                bw.Write(new byte[] { buffer[i_d_in + i2] });//запись в новый rez 
                                                            }
                                                            //чистый rez = 451 байт
                                                            //если стоит отметка писать данные АСК, тогда:
                                                            if (checkBox1.Checked == true)
                                                            {
                                                                //с 402 байта начинаются данные ASK
                                                                for (int ask = 402; ask <= 609; ask++)//*208 байт*  байты из файла ask / параметры аск
                                                                {
                                                                    bw.Write(new byte[] { buffer[ask + i2] });//запись в новый rez 
                                                                }
                                                                for (int ask = 610; ask <= 620; ask++)//*11 байт*  байты из файла ask / параметры аск
                                                                {
                                                                    bw.Write(new byte[] { buffer[ask + i2] });//запись в новый rez 
                                                                }
                                                                //если есть тревожное сообщение,то переписываем его побайтно
                                                                if ((buffer[620 + i2] != 0X00) && (buffer[620 + i2] != 0XFF))
                                                                {
                                                                    //MessageBox.Show("soobchenie est'");
                                                                    for (int ask = 621; ask <= 678; ask++)//
                                                                    {
                                                                        bw.Write(new byte[] { buffer[ask + i2] });//запись в новый rez 
                                                                    }
                                                                }
                                                                else//если сообщения нет, то пишем пустые байты, что бы пакеты в резе были одинаковые
                                                                {
                                                                    for (int ask = 621; ask <= 678; ask++)//
                                                                    {
                                                                        bw.Write(new byte[] { 0X00 });//запись в новый rez 
                                                                    }
                                                                }
                                                            }
                                                            //bw.Flush();
                                                        }
                                                        //***
                                                        progressBar1.Visible = true;
                                                        progressBar1.Minimum = 0;
                                                        progressBar1.Maximum = bytesRead - 1;
                                                        progressBar1.Step = 1;
                                                        progressBar1.PerformStep();
                                                        Application.DoEvents(); //передача управления системе для устранения зависания
                                                    }
                                                    progressBar1.Visible = false;
                                                }//конец записи потока
                                            }//конец потока записываемого файла

                                            Array.Clear(buffer, 0, buffer.Length); //очищаем массив буфера  
                                        }//end if час выбранный входит в имя файла аск 
                                    }//foreach файлы аск
                                }//конец 2тэ25а

                                //нет файлов с нужным временем
                                if (kol_failes_ok < 1)
                                {
                                    MessageBox.Show("Нет файлов за выбранный период времени!");//когда нет файлов подходящих под выбранное время

                                }
                                else
                                {
                                    label6.Text = "Найдено и обработанно файлов за выбранный период времени: " + kol_failes_ok;
                                }


                                //
                            }//end if  папка содержит указанную дату
                        }//end if папка содержит указанный номер тепловоза (а для 116 и секцию)
                    }//end if папка содержит заданную серию тепловоза 
                }//end if если папка

            }//пока что просматривается директория поиска
            progressBar1.Visible = false;
            label7.Visible = false;
            button5.Enabled = true;
            button7.Enabled = true;
            button2.Enabled = true;
            // }//к несуществующему номеру тепловоза на новом ftp
            if (nomer_sychestvyet == false)
            {
                MessageBox.Show("Тепловоз с данным номером не найден!");
            }
            if (data_sychestvyet == false)
            {
                MessageBox.Show("Каталог данных за выбранный период не найден!");//когда нет папки свыбранной датой датой
            }

            progressBar2.Visible = false;
            if ((del_dir_rar != "") && (del_dir_ask != ""))
            {
                System.IO.Directory.Delete(del_dir_ask, true);
                System.IO.Directory.Delete(del_dir_rar, true);
            }
            if ((del_dir_rar != "") && (del_dir_ask != "") && (kol_failes_ok > 0))
            {
                //если файл имеет размер = 0 байт, то собщаем о выключенном МСУ
                System.IO.FileInfo file_rez = new System.IO.FileInfo(textBox8.Text + "/" + new_file_name_rez + ".rez");
                if (file_rez.Exists)
                {
                    if (file_rez.Length == 0)
                    {
                        MessageBox.Show("Сформирован пустой файл, МСУ тепловоза была отключена!");
                    }
                }
                System.Diagnostics.Process.Start(@"" + textBox8.Text + "\\");//открытие папки с файлом rez

                //____сброс СТАТИСТИКИ  на  FTP__________________________________________________________________________________16 байт в файл _____
                bool flag_dost = true;                                                                                                    /*
                //подключение отдельное к новому FTP
                try
                {
                    //Задаём параметры клиента.
                    clientSTAT.PassiveMode = false; //Включаем пассивный режим.
                    int TimeoutFTPSTAT = 30000; //Таймаут.                 
                    bool local_ftp = false;
                    if ((textBox1.Text == "10.0.2.5") || (textBox1.Text == "192.168.3.10"))
                    {
                        local_ftp = true;                     
                    }
                    else
                    {
                        local_ftp = false;                      
                    }

                    string FTP_SERVER_STAT;
                    if (local_ftp == true) { FTP_SERVER_STAT = "10.0.2.5"; } else { FTP_SERVER_STAT = "88.86.83.22"; }
                    int FTP_PORT_STAT = 21;
                    string FTP_USER_STAT = "FEDOTOV";                    
                    string FTP_PASSWORD_STAT = "fed11651382-";
                    
                    //Если используется прокси сервер то можем задать параметры прокси.
                    FtpProxyInfo pinfo = new FtpProxyInfo(); //Это переменная параметров.
                    //pinfo.Server = "192.168.3.10"; 
                    if (local_ftp == true) { pinfo.Server = "10.0.2.5"; } else { pinfo.Server = "88.86.83.22"; }
                    pinfo.Port = 21; //Порт.                    
                    pinfo.Type = FtpProxyType.HttpConnect; //Тип прокси - всего 4 вида.
                    pinfo.PreAuthenticate = true; //Если на прокси есть идентификация
                    pinfo.User = "FEDOTOV";
                    pinfo.Password = "fed11651382-";
                    //Присваиваем параметры прокси клиенту.
                    //clientSTAT.ProxyInfo = pinfo;
                    //Подключаемся к FTP серверу.
                    clientSTAT.Connect(TimeoutFTPSTAT, FTP_SERVER_STAT, FTP_PORT_STAT);
                    clientSTAT.Login(TimeoutFTPSTAT, FTP_USER_STAT, FTP_PASSWORD_STAT);                   
                }
                catch
                {}                                                                                                                                  */
                //количество попыток дозаписи фала = 4                                                                                                   
                /*     for (int i = 0; i < 4; i++)
                  {
                      //MessageBox.Show(Convert.ToString(i+" - попытка записи"));
                      //проверка доступен ли файл статистики для дозаписи или занят другой программой
                      foreach (FtpItem item3 in clientSTAT.GetDirectoryList(TimeoutFTP * 1000, "/"))//просматриваем корень FTP
                      {                        //флаг того что файл занят - наличее флага-файла fl_st.st
                          if (item3.ItemType == FtpItemType.File)//если файл
                          {
                             // MessageBox.Show("" + Convert.ToString(item3.Name));
                              if (Convert.ToString(item3.Name) == "fl_st")
                              {
                                  flag_dost = false;//если есть флаг-файл то доступ закрыт                                
                              }
                          }//если файл
                      }//end просмотр корня FTP


                      if (flag_dost == false)//если файл-флаг найден то ждем пол секунды и проходим цикл еще раз
                      {
                          System.Threading.Thread.Sleep(500);//приостановка выполнения программы на 0.5 сек
                      }
                      else
                      { //файла-флага нет

                                  clientSTAT.AppendToFile(3000000, "/" + "fl_st.st", new byte[] { 0XFF });//создаем файл-флаг 
                                
                                  Byte kod_predpriyatiya = Convert.ToByte("1");// код предприятия от 0 до 255___________//1-local//
                                  Byte den = Convert.ToByte(System.DateTime.Now.Day);// 1 байт - день
                                  Byte mes = Convert.ToByte(System.DateTime.Now.Month);// 1 байт - месяц
                                  Byte god = Convert.ToByte(System.DateTime.Now.Year - 2000);// 1 байт - год
                                  Byte chas = Convert.ToByte(System.DateTime.Now.Hour);// 1 байт - час
                                  Byte min = Convert.ToByte(System.DateTime.Now.Minute);// 1 байт - минута
                                  Byte sec = Convert.ToByte(System.DateTime.Now.Second);// 1 байт - секунда
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { kod_predpriyatiya });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { den });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { mes });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { god });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { chas });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { min });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { sec });
                                  //___________________если новый FTP___________
                                  if ((textBox1.Text == "10.0.2.5") || (textBox1.Text == "88.86.83.22"))
                                  {
                                      Byte server = Convert.ToByte("2");// сервер: 1 - старый, 2 - новый_____________
                                      clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { server });
                                  }
                                  //___________________если старый FTP__________
                                  if ((textBox1.Text == "192.168.3.10") || (textBox1.Text == "88.86.78.118"))
                                  {
                                      Byte server = Convert.ToByte("1");// сервер: 1 - старый, 2 - новый_____________
                                      clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { server });
                                  }
                                  ///////////////////////////////
                                  if (comboBox1.Text == "TEP70BS")
                                  {
                                      Byte seriya_teplovoza = Convert.ToByte("1");// серия: 1 - TEP70BS, 2 - 2TE116U____________
                                      clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { seriya_teplovoza });
                                  }
                                  if (comboBox1.Text == "2TE116U")
                                  {
                                      Byte seriya_teplovoza = Convert.ToByte("2");// серия: 1 - TEP70BS, 2 - 2TE116U____________\
                                      clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { seriya_teplovoza });
                                  }
                                  /////////////////////////////////

                                  Byte nomer_teplovoza1 = Convert.ToByte(textBox7.Text.Remove(1));// 2 байта - номер тепловоза___________1й байт - сотни, 2й байт - десятки с единицами_______
                                  Byte nomer_teplovoza2 = Convert.ToByte(textBox7.Text.Remove(0, 1));//_______________
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { nomer_teplovoza1 });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { nomer_teplovoza2 });

                                  ///////////////////////////////

                                  if (comboBox1.Text == "TEP70BS")
                                  {
                                      Byte sekciya_teplovoza = Convert.ToByte("0");// 1 байт - секция тепловоза____________
                                      clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { sekciya_teplovoza });
                                  }
                                  else
                                  {
                                      if (comboBox4.Text == "A")
                                      {
                                          Byte sekciya_teplovoza = Convert.ToByte("1");// 1 байт - секция тепловоза____________
                                          clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { sekciya_teplovoza });
                                      }
                                      if (comboBox4.Text == "B")
                                      {
                                          Byte sekciya_teplovoza = Convert.ToByte("2");// 1 байт - секция тепловоза____________
                                          clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { sekciya_teplovoza });
                                      }
                                      if (comboBox4.Text == "C")
                                      {
                                          Byte sekciya_teplovoza = Convert.ToByte("3");// 1 байт - секция тепловоза____________
                                          clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { sekciya_teplovoza });
                                      }
                                  }
                                  //////////////////////////////////////
                                  Byte den2 = Convert.ToByte(day);// 1 байт - день из запроса___________________
                                  Byte mes2 = Convert.ToByte(mon);// 1 байт - месяц из запроса___________
                                  Byte god2 = Convert.ToByte(Convert.ToInt16(yer) - 2000);// 1 байт - год из запроса____________
                                  Byte chas2 = Convert.ToByte(comboBox2.Text);// 1 байт - час из запроса (начальный)__________
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { den2 });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { mes2 });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { god2 });
                                  clientSTAT.AppendToFile(3000000, "/" + "stat.st", new byte[] { chas2 });                                

                                  i = 4;//больше не пытаемся дозаписать                                
                                  clientSTAT.DeleteFile(3000000, "/" + "fl_st.st");//удаляем файл-флаг                                                 
                      }//end файла-флага нет                    
                  }//end количество попыток дозаписи файла
                  clientSTAT.Disconnect(300000);                                                                                                  */

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ConnectFTP();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DisconnectFTP();
            treeView1.Nodes.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox8.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox4.Text = folderBrowserDialog1.SelectedPath;
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;//запрет ввода символов
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if ((comboBox1.Text == "2TE116U") || (comboBox1.Text == "3TE116U") || (comboBox1.Text == "2TE25A"))
            {
                comboBox4.Enabled = true;
            }
            else
            {
                comboBox4.Enabled = false;
            }

            if (comboBox1.Text == "2TE25A")
            {
                checkBox4.Checked = true;
            }
        }

        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            // Пропускаем цифровые кнопки
            if ((e.KeyCode >= Keys.D0) && (e.KeyCode <= Keys.D9)) e.SuppressKeyPress = false;
            // Пропускаем цифровые кнопки с NumPad'а
            if ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)) e.SuppressKeyPress = false;
            // Пропускаем Delete, Back, Left и Right
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back) ||
                (e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right)) e.SuppressKeyPress = false;
        }

        private void comboBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;//запрет ввода символов
        }

        private void comboBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;//запрет ввода символов
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            comboBox3.SelectedIndex = comboBox2.SelectedIndex;
        }

        private void comboBox3_SelectionChangeCommitted(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = comboBox3.SelectedIndex;
        }

        private void comboBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;//запрет ввода символов
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool flag_zapolneniya=false;
            ConnectFTP();
            if ((comboBox1.Text == "2TE116U") && (textBox7.Text.Length==3) && (comboBox4.Text!=""))
            {
                flag_zapolneniya = true;
            }
            if ((comboBox1.Text == "TEP70BS") && (textBox7.Text.Length==3) && (comboBox4.Enabled==false))
            {
                flag_zapolneniya=true;
            }
            if ((comboBox1.Text == "3TE116U") && (textBox7.Text.Length == 3) && (comboBox4.Text != ""))
            {
                flag_zapolneniya = true;
            }
            if ((comboBox1.Text == "2TE25A") && (textBox7.Text.Length == 3) && (comboBox4.Text != ""))
            {
                flag_zapolneniya = true;
            }

            if (flag_zapolneniya == false)
            {
                MessageBox.Show("Введены не все условия поиска!");
            }
            else
            { 
                //ConnectFTP();
                GetFilesFromFtp();
            }
            
                // DisconnectFTP();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            Properties.Settings.Default.FTP = textBox1.Text;
            Properties.Settings.Default.PORT = textBox5.Text;
            Properties.Settings.Default.LOGIN = textBox2.Text;
            Properties.Settings.Default.PASS = textBox3.Text;
            Properties.Settings.Default.DIR_SERCH = textBox6.Text;
            Properties.Settings.Default.RAR = textBox4.Text;
            Properties.Settings.Default.SAVE = textBox8.Text;

             Properties.Settings.Default.S_TEP70BS = false;
             Properties.Settings.Default.S_2TE116U = false;
             Properties.Settings.Default.S_3TE116U = false;
             Properties.Settings.Default.S_2TE25A = false;
             Properties.Settings.Default.S_2TE116UM = false;
             Properties.Settings.Default.S_TEM31 = false;
             Properties.Settings.Default.S_CHME3 = false;
             Properties.Settings.Default.S_2TE116 = false;


            comboBox1.Items.Clear();
            comboBox1.Text = "Выбрать";
            if (checkBox3.Checked == true)
            {
             Properties.Settings.Default.FILTER = true;

                foreach (string s in checkedListBox1.CheckedItems)
                {

                    if (s == "TEP70BS") { Properties.Settings.Default.S_TEP70BS = true; comboBox1.Items.Add("TEP70BS"); comboBox1.Text = "TEP70BS"; }
                    if (s == "2TE116U") { Properties.Settings.Default.S_2TE116U = true; comboBox1.Items.Add("2TE116U"); comboBox1.Text = "2TE116U"; }
                    if (s == "3TE116U") { Properties.Settings.Default.S_3TE116U = true; comboBox1.Items.Add("3TE116U"); comboBox1.Text = "3TE116U"; }
                    if (s == "2TE25A") { Properties.Settings.Default.S_2TE25A = true; comboBox1.Items.Add("2TE25A"); comboBox1.Text = "2TE25A"; }
                    if (s == "2TE116UM") { Properties.Settings.Default.S_2TE116UM = true; comboBox1.Items.Add("2TE116UM"); comboBox1.Text = "2TE116UM"; }
                    if (s == "TEM31") { Properties.Settings.Default.S_TEM31 = true; comboBox1.Items.Add("TEM31"); comboBox1.Text = "TEM31"; }
                    if (s == "CHME3") { Properties.Settings.Default.S_CHME3 = true; comboBox1.Items.Add("CHME3"); comboBox1.Text = "CHME3"; }
                    if (s == "2TE116") { Properties.Settings.Default.S_2TE116 = true; comboBox1.Items.Add("2TE116"); comboBox1.Text = "2TE116"; }
                }
            }
            else
            {
                Properties.Settings.Default.FILTER = false;

                comboBox1.Items.Add("TEP70BS");
                comboBox1.Items.Add("2TE116U");
                comboBox1.Items.Add("3TE116U");
                comboBox1.Items.Add("2TE25A");
                comboBox1.Items.Add("2TE116UM");
                comboBox1.Items.Add("TEM31");
                comboBox1.Items.Add("CHME3");
                comboBox1.Items.Add("2TE116");
                foreach (string s in checkedListBox1.CheckedItems)
                {

                    if (s == "TEP70BS") { Properties.Settings.Default.S_TEP70BS = true; comboBox1.Items.Add("TEP70BS"); comboBox1.Text = "TEP70BS"; }
                    if (s == "2TE116U") { Properties.Settings.Default.S_2TE116U = true; comboBox1.Items.Add("2TE116U"); comboBox1.Text = "2TE116U"; }
                    if (s == "3TE116U") { Properties.Settings.Default.S_3TE116U = true; comboBox1.Items.Add("3TE116U"); comboBox1.Text = "3TE116U"; }
                    if (s == "2TE25A") { Properties.Settings.Default.S_2TE25A = true; comboBox1.Items.Add("2TE25A"); comboBox1.Text = "2TE25A"; }
                    if (s == "2TE116UM") { Properties.Settings.Default.S_2TE116UM = true; comboBox1.Items.Add("2TE116UM"); comboBox1.Text = "2TE116UM"; }
                    if (s == "TEM31") { Properties.Settings.Default.S_TEM31 = true; comboBox1.Items.Add("TEM31"); comboBox1.Text = "TEM31"; }
                    if (s == "CHME3") { Properties.Settings.Default.S_CHME3 = true; comboBox1.Items.Add("CHME3"); comboBox1.Text = "CHME3"; }
                    if (s == "2TE116") { Properties.Settings.Default.S_2TE116 = true; comboBox1.Items.Add("2TE116"); comboBox1.Text = "2TE116"; }
                }

                comboBox1.Text = "TEP70BS";
            }

            Properties.Settings.Default.Save();

            textBox1.Enabled = false;
            textBox5.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox6.Enabled = false;
            //checkBox2.Enabled = false;
            checkBox3.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            checkedListBox1.Enabled = false;

            textBox9.Text = "";

            
            

       //     try
       //     {
       //         System.IO.File.Delete(System.IO.Directory.GetCurrentDirectory() + "/setings.ini");
       //     }
       //     catch
       //     { }
       //     textBox9.Text = "";
       //     textBox1.Enabled = false;
       //     textBox5.Enabled = false;
       //     textBox2.Enabled = false;
       //     textBox3.Enabled = false;
       //     textBox6.Enabled = false;
       //     checkBox2.Enabled = false;
       //     checkBox3.Enabled = false;            
       //
       //     //File.AppendAllText(Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + "1");
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", textBox1.Text);
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + textBox5.Text);
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + textBox2.Text);
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + textBox3.Text);
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + textBox6.Text);
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + textBox4.Text);
       //     System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "/setings.ini", "\r\n" + textBox8.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConnectFTP();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //показ всплывающего окна с сообщением из трея
            notifyIcon1.ShowBalloonTip(500, "Сообщение", "Конвертор IO. Версия 1.0", ToolTipIcon.Info);

            textBox1.Text = Properties.Settings.Default.FTP;
            textBox5.Text = Properties.Settings.Default.PORT;
            textBox2.Text = Properties.Settings.Default.LOGIN;
            textBox3.Text = Properties.Settings.Default.PASS;
            textBox6.Text = Properties.Settings.Default.DIR_SERCH;
            textBox4.Text = Properties.Settings.Default.RAR;
            textBox8.Text = Properties.Settings.Default.SAVE;


            if (Properties.Settings.Default.S_TEP70BS == true) { checkedListBox1.SetItemChecked(0, true); }
            if (Properties.Settings.Default.S_2TE116U == true) { checkedListBox1.SetItemChecked(1, true); }
            if (Properties.Settings.Default.S_3TE116U == true) { checkedListBox1.SetItemChecked(2, true); }
            if (Properties.Settings.Default.S_2TE25A == true) { checkedListBox1.SetItemChecked(3, true); }
            if (Properties.Settings.Default.S_2TE116UM == true) { checkedListBox1.SetItemChecked(4, true); }
            if (Properties.Settings.Default.S_TEM31 == true) { checkedListBox1.SetItemChecked(5, true); }
            if (Properties.Settings.Default.S_CHME3 == true) { checkedListBox1.SetItemChecked(6, true); }
            if (Properties.Settings.Default.S_2TE116 == true) { checkedListBox1.SetItemChecked(7, true); }

            comboBox1.Items.Clear();
            comboBox1.Text = "Выбрать";
            if ( Properties.Settings.Default.FILTER == true)//
            {
                checkBox3.Checked = true;

                foreach (string s in checkedListBox1.CheckedItems)
                {

                    if (s == "TEP70BS") { Properties.Settings.Default.S_TEP70BS = true; comboBox1.Items.Add("TEP70BS"); comboBox1.Text = "TEP70BS"; }
                    if (s == "2TE116U") { Properties.Settings.Default.S_2TE116U = true; comboBox1.Items.Add("2TE116U"); comboBox1.Text = "2TE116U"; }
                    if (s == "3TE116U") { Properties.Settings.Default.S_3TE116U = true; comboBox1.Items.Add("3TE116U"); comboBox1.Text = "3TE116U"; }
                    if (s == "2TE25A") { Properties.Settings.Default.S_2TE25A = true; comboBox1.Items.Add("2TE25A"); comboBox1.Text = "2TE25A"; }
                    if (s == "2TE116UM") { Properties.Settings.Default.S_2TE116UM = true; comboBox1.Items.Add("2TE116UM"); comboBox1.Text = "2TE116UM"; }
                    if (s == "TEM31") { Properties.Settings.Default.S_TEM31 = true; comboBox1.Items.Add("TEM31"); comboBox1.Text = "TEM31"; }
                    if (s == "CHME3") { Properties.Settings.Default.S_CHME3 = true; comboBox1.Items.Add("CHME3"); comboBox1.Text = "CHME3"; }
                    if (s == "2TE116") { Properties.Settings.Default.S_2TE116 = true; comboBox1.Items.Add("2TE116"); comboBox1.Text = "2TE116"; }
                }
            }
            else
            {
                checkBox3.Checked = false;
                comboBox1.Text = "TEP70BS";
                comboBox1.Items.Add("TEP70BS");
                comboBox1.Items.Add("2TE116U");
                comboBox1.Items.Add("3TE116U");
                comboBox1.Items.Add("2TE25A");
                comboBox1.Items.Add("2TE116UM");
                comboBox1.Items.Add("TEM31");
                comboBox1.Items.Add("CHME3");
                comboBox1.Items.Add("2TE116");
            }

            if (Properties.Settings.Default.Regim_Pasivnii == true)//
            {
                checkBox2.Checked = true;
            }
            else
            {
                checkBox2.Checked = false;
            }
        //    //ini файл с настройками
        //      StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "\\setings.ini"); //считываем строки из ini файла
        //       textBox1.Text = sr.ReadLine();
        //       textBox5.Text = sr.ReadLine();
        //       textBox2.Text = sr.ReadLine();
        //       textBox3.Text = sr.ReadLine();
        //       //sr.ReadLine();
        //        textBox6.Text = sr.ReadLine();
        //        textBox4.Text = sr.ReadLine();
        //        textBox8.Text = sr.ReadLine();
        //        sr.Close();


            //к вкладке с графиком
            GraphPane pane1 = zedGraphControl1.GraphPane;
            // Закрасим фон всего компонента ZedGraph
            // Заливка будет сплошная
            pane1.Fill.Type = FillType.Solid;
            pane1.Fill.Color = Color.FromArgb(135, 206, 235);
            // Закрасим область графика (его фон) в черный цвет
            pane1.Chart.Fill.Type = FillType.Solid;
            pane1.Chart.Fill.Color = Color.FromArgb(135, 206, 235);
            pane1.Border.Color = Color.FromArgb(135, 206, 235);
            pane1.Title.Text = "Просмотр файла тепловоза №___ за период __ - __";
            pane1.XAxis.Title.FontSpec.Size = 12;
            pane1.YAxis.Title.FontSpec.Size = 12;
            pane1.XAxis.Title.Text = "Период времени в секундах";
            pane1.YAxis.Title.Text = "Значения параметров";
            pane1.XAxis.Scale.Min = 0;// Устанавливаем интересующий нас интервал по оси X
            pane1.XAxis.Scale.Max = 3500;

            // Включим показ всплывающих подсказок при наведении курсора на график
            zedGraphControl1.IsShowPointValues = true;
            // Обновим данные об осях
            zedGraphControl1.AxisChange();
            // Обновляем график
            zedGraphControl1.Invalidate();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            DisconnectFTP();
        }


       private ContextMenuStrip docMenu;

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectFTP();
                button10.Visible = false;
                label4.Visible = true;

                listView1.Clear();
                treeView2.Nodes.Clear();
                Application.DoEvents();//передача управления ос
                TreeNode node_koren = new TreeNode(textBox6.Text);
                treeView2.Nodes.Add(node_koren);
                int TimeoutFTP = Convert.ToInt32(textBox10.Text); //Таймаут 
                string sd = "";//имя папки с архивами на ftp       
                string dirserch = textBox6.Text;//директория поиска папок на фтп
                int kol = 1;
                bool ftp_new;
                ftp_new = false;
                //если старый FTP 
                if ((textBox1.Text == "192.168.3.10") || (textBox1.Text == "88.86.78.118"))
                { ftp_new = false; }
                else
                { ftp_new = true; }

                foreach (FtpItem item in client.GetDirectoryList(TimeoutFTP * 1000, "/" + dirserch + "/")) //ASK по умолчанию
                {
                    TreeNode node = new TreeNode(item.Name);
                   // MessageBox.Show(item.Name);                                                
                    if (item.ItemType == FtpItemType.Directory)//если папка
                    {
                        sd = Convert.ToString(node);//переводим в строку                    
                        sd = sd.Remove(0, 10);//вырезаем имя папки                    
                        string ser = comboBox1.Text;//серия тепловоза
                        string nomer_tep = textBox7.Text;//№тепловоза
                   
                        if (checkBox3.Checked == false) //если не фильтруем по сериям тепловоза
                        {
                            treeView2.Nodes[0].Nodes.Add(node);       //отображение дерева коренного каталога 
                            listView1.Items.Add(Convert.ToString(item.Name), 0);                
                            kol = kol + 1; //№найденной папки
                            //
                            if (ftp_new == true)
                            {
                                foreach (FtpItem item2 in client.GetDirectoryList(TimeoutFTP * 1000, "/" + dirserch + "/" + sd))
                                {
                                    //считываем из нее вложеннные папки
                                    TreeNode node2 = new TreeNode(item2.Name);
                                    if (item2.ItemType == FtpItemType.Directory)//если файл
                                    {
                                        string sd2 = Convert.ToString(node2);//переводим в строку
                                        sd2 = sd2.Remove(0, 10);//вырезаем имя файла             
                                        treeView2.Nodes[0].Nodes[kol - 2].Nodes.Add(node2); //вывод архивов
                                        //     Application.DoEvents();//передача управления ос
                                    }
                                }
                            }
                        }
                        else //если стоит филтр по серии тепловоза
                        {
                            if (sd.IndexOf(ser) > -1) //если папка содержит заданную серию тепловоза
                            {
                                treeView2.Nodes[0].Nodes.Add(node);       //отображение дерева коренного каталога 
                                listView1.Items.Add(Convert.ToString(item.Name), 0);
                                kol = kol + 1; //№найденной папки
                                if (ftp_new == true)
                                {
                                    foreach (FtpItem item2 in client.GetDirectoryList(TimeoutFTP * 1000, "/" + dirserch + "/" + sd))
                                    {
                                        //считываем из нее вложеннные папки
                                        TreeNode node2 = new TreeNode(item2.Name);
                                        if (item2.ItemType == FtpItemType.Directory)//если файл
                                        {
                                            string sd2 = Convert.ToString(node2);//переводим в строку
                                            sd2 = sd2.Remove(0, 10);//вырезаем имя файла             
                                            treeView2.Nodes[0].Nodes[kol - 2].Nodes.Add(node2); //вывод архивов
                                            //     Application.DoEvents();//передача управления ос
                                        }
                                    }
                                }
                            }
                        }

                                       

                     

                    }
                }

                docMenu = new ContextMenuStrip();
                //Create some menu items.
                ToolStripMenuItem loadLabel = new ToolStripMenuItem();
                loadLabel.Enabled = false;
                loadLabel.Text = "Скачать";
                ToolStripMenuItem openLabel = new ToolStripMenuItem();
                openLabel.Text = "Открыть";

                //Add the menu items to the menu.
                docMenu.Items.AddRange(new ToolStripMenuItem[] { loadLabel, openLabel });

                // Set the ContextMenuStrip property to the ContextMenuStrip.
                treeView2.ContextMenuStrip = docMenu;
                label4.Visible = false;

            }
            catch
            { }
           
        }

        private void openLabel_Click(object sender, DrawTreeNodeEventArgs e)//просто дублирует клик по items проводника
        {
            ConnectFTP();
            listView1.Items.Clear();
            string pyt = "";//выбранная деректория
            string parent_pyt = "";//родительская папка выбранной директории
            pyt = Convert.ToString(treeView2.SelectedNode);//выбранная директория
            parent_pyt = Convert.ToString(treeView2.SelectedNode.Parent);//родительская папка выбранной директории
            // MessageBox.Show(parent_pyt);
            if (pyt != "")//если выбранная директория имеет не пустое название, то
            {
                pyt = pyt.Remove(0, 10);//вырезаем служебные символы и получаем ее название
            }
            if (pyt == textBox6.Text)// если выбранная директория совпадает с корневой директорией просмотра, то
            {
                pyt = "";//выбранную директорию очищаем
            }
            if (parent_pyt != "")//если родительская директория существует, то
            {
                parent_pyt = parent_pyt.Remove(0, 10);//вырезаем служебные  символы и получаем ее название
            }
            if (parent_pyt == textBox6.Text)
            { parent_pyt = textBox6.Text; }
            else
            { parent_pyt = textBox6.Text + "/" + parent_pyt; }
            foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, parent_pyt + "/" + pyt + "/")) //ASK по умолчанию
            {
                TreeNode node = new TreeNode(item.Name);

                if (item.ItemType == FtpItemType.File)//если файл
                {
                    listView1.Items.Add(Convert.ToString(item.Name), 1);
                }
                if (item.ItemType == FtpItemType.Directory)//если директория
                {
                    listView1.Items.Add(Convert.ToString(item.Name), 0);
                }

                //Application.DoEvents();//передача управления ос
            }
        }

        private void treeView2_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {

           // this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            //this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ConnectFTP();//////
            listView1.Items.Clear();
            button10.Visible = false;
            string pyt = "";//выбранная деректория
            string parent_pyt = "";//родительская папка выбранной директории
            pyt = Convert.ToString(treeView2.SelectedNode);//выбранная директория
            parent_pyt = Convert.ToString(treeView2.SelectedNode.Parent);//родительская папка выбранной директории
           // MessageBox.Show(parent_pyt);
            if (pyt != "")//если выбранная директория имеет не пустое название, то
            {
                pyt = pyt.Remove(0, 10);//вырезаем служебные символы и получаем ее название
            }
            if (pyt == textBox6.Text)// если выбранная директория совпадает с корневой директорией просмотра, то
            {
                pyt = "";//выбранную директорию очищаем
            }
            if (parent_pyt != "")//если родительская директория существует, то
            {
                parent_pyt = parent_pyt.Remove(0, 10);//вырезаем служебные  символы и получаем ее название
            }
            if (parent_pyt == textBox6.Text)
            { parent_pyt = textBox6.Text; }
            else
            { parent_pyt = textBox6.Text + "/" + parent_pyt; }
            foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, parent_pyt + "/" + pyt + "/")) //ASK по умолчанию
            {
                TreeNode node = new TreeNode(item.Name);

                if (item.ItemType == FtpItemType.File)//если файл
                {
                    listView1.Items.Add(Convert.ToString(item.Name), 1);
                }
                if (item.ItemType == FtpItemType.Directory)//если директория
                {
                    if (checkBox3.Checked == false)//если фильтра нет выводим все
                    {
                        listView1.Items.Add(Convert.ToString(item.Name), 0);
                    }
                    else
                    {
                        if (item.Name.IndexOf(comboBox1.Text) > -1) //если папка содержит заданную серию тепловоза
                        { listView1.Items.Add(Convert.ToString(item.Name), 0); }
                    }
                }

                //Application.DoEvents();//передача управления ос
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(textBox8.Text);//при проверки наличия файла проверял несуществующую директорию
            System.Diagnostics.Process.Start(@"" + textBox8.Text + "\\");
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.FTP;
            textBox5.Text = Properties.Settings.Default.PORT;
            textBox2.Text = Properties.Settings.Default.LOGIN;
            textBox3.Text = Properties.Settings.Default.PASS;
            textBox6.Text = Properties.Settings.Default.DIR_SERCH;
            textBox4.Text = Properties.Settings.Default.RAR;
            textBox8.Text = Properties.Settings.Default.SAVE;

            for (int i = 0; i <= 7; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }

            if (Properties.Settings.Default.S_TEP70BS == true) { checkedListBox1.SetItemChecked(0, true); }
            if (Properties.Settings.Default.S_2TE116U == true) { checkedListBox1.SetItemChecked(1, true); }
            if (Properties.Settings.Default.S_3TE116U == true) { checkedListBox1.SetItemChecked(2, true); }
            if (Properties.Settings.Default.S_2TE25A == true) { checkedListBox1.SetItemChecked(3, true); }
            if (Properties.Settings.Default.S_2TE116UM == true) { checkedListBox1.SetItemChecked(4, true); }
            if (Properties.Settings.Default.S_TEM31 == true) { checkedListBox1.SetItemChecked(5, true); }
            if (Properties.Settings.Default.S_CHME3 == true) { checkedListBox1.SetItemChecked(6, true); }
            if (Properties.Settings.Default.S_2TE116 == true) { checkedListBox1.SetItemChecked(7, true); }

            textBox1.Enabled = false;
            textBox5.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox6.Enabled = false;
           // checkBox2.Enabled = false;
            checkBox3.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            checkedListBox1.Enabled = false;
            
            textBox9.Text = "";

            if (Properties.Settings.Default.FILTER == true) { checkBox3.Checked = true; } else { checkBox3.Checked = false; }

           // //ini файл с настройками
           // StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "\\setings.ini"); //считываем строки из ini файла
           // textBox1.Text = sr.ReadLine();
           // textBox5.Text = sr.ReadLine();
           // textBox2.Text = sr.ReadLine();
           // textBox3.Text = sr.ReadLine();
           // //sr.ReadLine();
           // textBox6.Text = sr.ReadLine();
           // textBox4.Text = sr.ReadLine();
           // textBox8.Text = sr.ReadLine();
           // sr.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (textBox1.Text == "10.0.2.5")
            { textBox1.Text = "192.168.3.10"; }
            else
            {
                if (textBox1.Text == "192.168.3.10")
                { textBox1.Text = "88.86.78.118"; }
                else
                {
                    if (textBox1.Text == "88.86.78.118")
                   { textBox1.Text = "88.86.83.22"; }
                    else
                    {
                        if (textBox1.Text == "88.86.83.22")
                        { textBox1.Text = "10.0.2.5"; }
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            listView1.Clear();
            this.WindowState = FormWindowState.Minimized;
        }
        string ico = "";
    
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)//двойной клик по списку файлов
        {
            bool new_ftp = false;
            if ((textBox1.Text == "192.168.3.10") || (textBox1.Text == "88.86.78.118"))//88.86.78.118 //если старый фтп то
            { new_ftp = false; }
            else
            { new_ftp = true; }

            ico = listView1.FocusedItem.Text;//запоминаем выбранную иконку


            if (ico.IndexOf(".rar") > -1)//если ico не содержит заданную строку тоесть не файл архив
            {
                //MessageBox.Show("RAR!" + ico);
            }
            else //тогда
            {
                // MessageBox.Show("no" + ico);
                ConnectFTP();//переподключаемся дабы не вылететь
                listView1.Items.Clear();//чистим лист
                if (new_ftp == false) //если старый фтп то
                {
                    foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/" + ico + "/")) //открываем корень + выбранная иконка
                    {
                        TreeNode node = new TreeNode(item.Name);

                        if (item.ItemType == FtpItemType.File)//если файл
                        {
                            listView1.Items.Add(Convert.ToString(item.Name), 1);
                        }
                        if (item.ItemType == FtpItemType.Directory)//если директория
                        {
                            listView1.Items.Add(Convert.ToString(item.Name), 0);
                        }

                        //Application.DoEvents();//передача управления ос
                    }
                }

                if (new_ftp == true)//88.86.83.22 //если новый фтп
                {
                    string dir = "";//переменная пути

                    if (ico.Length < 13)//если имя иконки не длинное то это не внутренние вложженные папки
                    {
                        dir = textBox6.Text;//значит они лежат в коневой директроии
                    }
                    else //иначе имя иконки длинное и папка лежит в вложении
                    {
                        dir = textBox6.Text + "/" + ico.Remove(ico.Length - 9);//поэтому путь = корень + вырезанная папка кореная + сама ико
                        //MessageBox.Show(dir);
                    }
                    foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, dir + "/" + ico + "/")) //ASK по умолчанию
                    {
                        TreeNode node = new TreeNode(item.Name);

                        if (item.ItemType == FtpItemType.File)//если файл
                        {
                            listView1.Items.Add(Convert.ToString(item.Name), 1);
                        }
                        if (item.ItemType == FtpItemType.Directory)//если директория
                        {
                            listView1.Items.Add(Convert.ToString(item.Name), 0);
                        }

                        //Application.DoEvents();//передача управления ос
                    }
                }

            }
                           

        }
        int klic=0;
        
        private void button10_Click(object sender, EventArgs e)
        {
            bool new_ftp = false;
            if ((textBox1.Text == "192.168.3.10") || (textBox1.Text == "88.86.78.118"))//88.86.78.118 //если старый фтп то
            { new_ftp = false; }
            else
            { new_ftp = true; }
                ConnectFTP();
                listView1.Items.Clear();
                if (new_ftp==false)
                {
                    label4.Visible = true;
                    Application.DoEvents();//передача управления ос
                    foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/")) //ASK по умолчанию
                    {
                        TreeNode node = new TreeNode(item.Name);

                        if (item.ItemType == FtpItemType.File)//если файл
                        {
                            listView1.Items.Add(Convert.ToString(item.Name), 1);
                        }
                        if (item.ItemType == FtpItemType.Directory)//если директория
                        { //реализация фильтра
                            if (checkBox3.Checked == false) //если не фильтруем по сериям тепловоза
                            {
                                listView1.Items.Add(Convert.ToString(item.Name), 0);
                            }
                            else
                            {
                                if (item.Name.IndexOf(comboBox1.Text) > -1) //если папка содержит заданную серию тепловоза
                                { listView1.Items.Add(Convert.ToString(item.Name), 0); }
                            }
                        }

                        //Application.DoEvents();//передача управления ос
                    }
                    label4.Visible = false;
                }

                if (new_ftp==true)
                {
                    
                   // Application.DoEvents();//передача управления ос
                    if (ico.Length < 13)//корневая для определенного тепловоза
                    {
                        label4.Visible = true;
                        foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/")) //ASK по умолчанию
                        {
                            TreeNode node = new TreeNode(item.Name);

                            if (item.ItemType == FtpItemType.File)//если файл
                            {
                                listView1.Items.Add(Convert.ToString(item.Name), 1);
                            }
                            if (item.ItemType == FtpItemType.Directory)//если директория
                            {
                                //listView1.Items.Add(Convert.ToString(item.Name), 0); //заменено под фильтр
                                if (checkBox3.Checked == false) //если не фильтруем по сериям тепловоза
                                {
                                    listView1.Items.Add(Convert.ToString(item.Name), 0);
                                }
                                else
                                {
                                    if (item.Name.IndexOf(comboBox1.Text) > -1) //если папка содержит заданную серию тепловоза
                                    { listView1.Items.Add(Convert.ToString(item.Name), 0); }
                                }
                            }

                            //Application.DoEvents();//передача управления ос
                        }
                        label4.Visible = false;
                    }
                    
                    if (ico.Length > 13)//папка с датой в которой есть архивы
                    {
                         
                       if (ico.IndexOf(".rar") > -1)//если ico содержит заданную строку тоесть файл архив
                        {
                        //для тэп70бс
                            if (ico.IndexOf("5100") > -1)//если имя иконки содержит код тэп70бс
                            {
                                string dirtep70bs = "";
                                dirtep70bs = ico.Remove(7);
                                dirtep70bs = dirtep70bs.Remove(0, 4);
                                dirtep70bs = "TEP70BS-" + dirtep70bs;
                             //   MessageBox.Show(dirtep70bs);
                                //и соответственно открытие этой папки
                                label4.Visible = true;
                                foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/"+dirtep70bs+"/")) //ASK по умолчанию
                                {
                                    TreeNode node = new TreeNode(item.Name);

                                    if (item.ItemType == FtpItemType.File)//если файл
                                    {
                                        listView1.Items.Add(Convert.ToString(item.Name), 1);
                                    }
                                    if (item.ItemType == FtpItemType.Directory)//если директория
                                    {
                                        listView1.Items.Add(Convert.ToString(item.Name), 0);
                                    }

                                    //Application.DoEvents();//передача управления ос
                                }
                                label4.Visible = false;
                            }
                            //для 2т116y
                            if (ico.IndexOf("6060") > -1)//если имя иконки содержит код тэп70бс
                            {
                                string dir116 = "";
                                dir116 = ico.Remove(7);
                                dir116 = dir116.Remove(0, 4);
                                dir116 = "2TE116U-" + dir116;
                                //+секция
                                string sekciya116y = "";
                                sekciya116y = ico.Remove(8);
                                sekciya116y = sekciya116y.Remove(0,7);
                                if (sekciya116y == "1")
                                { sekciya116y = "A"; }
                                else
                                { sekciya116y = "B"; }
                                dir116 = dir116 + sekciya116y;
                              //  MessageBox.Show(dir116);
                                //и соответственно открытие этой папки
                                label4.Visible = true;
                                foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/" + dir116 + "/")) //ASK по умолчанию
                                {
                                    TreeNode node = new TreeNode(item.Name);

                                    if (item.ItemType == FtpItemType.File)//если файл
                                    {
                                        listView1.Items.Add(Convert.ToString(item.Name), 1);
                                    }
                                    if (item.ItemType == FtpItemType.Directory)//если директория
                                    {
                                        listView1.Items.Add(Convert.ToString(item.Name), 0);
                                    }

                                    //Application.DoEvents();//передача управления ос
                                }
                                label4.Visible = false;
                            }
                            //для 3т116y

                        }
                        else
                        {
                        if (klic == 1)
                        {
                            label4.Visible = true;
                            foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/")) //ASK по умолчанию
                            {
                                TreeNode node = new TreeNode(item.Name);

                                if (item.ItemType == FtpItemType.File)//если файл
                                {
                                    listView1.Items.Add(Convert.ToString(item.Name), 1);
                                }
                                if (item.ItemType == FtpItemType.Directory)//если директория
                                {
                                    //listView1.Items.Add(Convert.ToString(item.Name), 0);//замена на фильтр
                                    if (checkBox3.Checked == false) //если не фильтруем по сериям тепловоза
                                    {
                                        listView1.Items.Add(Convert.ToString(item.Name), 0);
                                    }
                                    else
                                    {
                                        if (item.Name.IndexOf(comboBox1.Text) > -1) //если папка содержит заданную серию тепловоза
                                        { listView1.Items.Add(Convert.ToString(item.Name), 0); }
                                    }
                                }

                                //Application.DoEvents();//передача управления ос
                                klic = 0;
                            }
                            label4.Visible = false;
                        }
                        else
                        {
                            //длинные папки
                            foreach (FtpItem item in client.GetDirectoryList(3000 * 1000, textBox6.Text + "/" + ico.Remove(ico.Length - 9) + "/")) //ASK по умолчанию
                            {
                                TreeNode node = new TreeNode(item.Name);

                                if (item.ItemType == FtpItemType.File)//если файл
                                {
                                    listView1.Items.Add(Convert.ToString(item.Name), 1);
                                }
                                if (item.ItemType == FtpItemType.Directory)//если директория
                                {
                                    listView1.Items.Add(Convert.ToString(item.Name), 0);
                                }
                                klic = 1;


                            }
                        }
                    }
                    }
                }        
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button10.Visible = true;//при выборе в листе иконки становится видна кнопка "назад"
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Width == 262)
            {
                this.Width = 1282;
            }
            else
            {
                this.Width = 262;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(treeView2.Nodes.ToString());
            if (this.Width == 262)
            {
                this.Width = 1282;
                button11.Text = "<";
            }
            else
            {
                this.Width = 262;
                button11.Text = ">";
            }
        }




        int shirina_form1 = 0;//переменная для хранения размера (ширины) формы

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            
            if (tabControl1.SelectedIndex == 0)//если выбрать главное меню
            {
                if (shirina_form1 == 0)//если ширина = 0 тоесть не заходили в настройки
                {
                    this.Width = 1282;//ширину установим развернутой
                }
                else
                {
                    this.Width = shirina_form1;//или вернемся к предыдущей ширине формы
                }
            }

            if (tabControl1.SelectedIndex == 1)//если выбрать меню настроек
            {
                shirina_form1 = this.Width;//запоминаем размер (ширину) формы
                this.Width = 1282;//устанавливаем размер (ширину) формы - развернутой
            }


        }


        private void button12_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button13_Click(object sender, EventArgs e)
        {
           
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            if (textBox9.Text == "obama")
            {
                textBox1.Enabled = true;
                textBox5.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                textBox6.Enabled = true;
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                checkedListBox1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Пароли не совпадают");
            }
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
      
            
        }
        int tik = 0;
        int tik2 = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            tik++;
            tik2++;

            if (tik<=20)
            {
                pictureBox7.Location = new System.Drawing.Point(pictureBox7.Location.X+1, pictureBox7.Location.Y);
                pictureBox8.Location = new System.Drawing.Point(pictureBox8.Location.X-1, pictureBox8.Location.Y);
            }
            if ((tik>20) && (tik<=40))
            {
                pictureBox7.Location = new System.Drawing.Point(pictureBox7.Location.X, pictureBox7.Location.Y-1);
                pictureBox8.Location = new System.Drawing.Point(pictureBox8.Location.X, pictureBox8.Location.Y + 1);
            }
            if ((tik>40) && (tik<=60))
            {
                pictureBox7.Location = new System.Drawing.Point(pictureBox7.Location.X-1, pictureBox7.Location.Y);
                pictureBox8.Location = new System.Drawing.Point(pictureBox8.Location.X + 1, pictureBox8.Location.Y);
            }
            if ((tik > 60) && (tik <= 80))
            {
                pictureBox7.Location = new System.Drawing.Point(pictureBox7.Location.X, pictureBox7.Location.Y+1);
                pictureBox8.Location = new System.Drawing.Point(pictureBox8.Location.X, pictureBox8.Location.Y - 1);
            }
            if (tik == 81) { tik = 0; }
        }

        private void button13_Click_2(object sender, EventArgs e)
        {
           
            
        }

        private void button14_Click(object sender, EventArgs e)
        {
           
        }

        private void textBox10_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            // Пропускаем цифровые кнопки
            if ((e.KeyCode >= Keys.D0) && (e.KeyCode <= Keys.D9)) e.SuppressKeyPress = false;
            // Пропускаем цифровые кнопки с NumPad'а
            if ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)) e.SuppressKeyPress = false;
            // Пропускаем Delete, Back, Left и Right
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back) ||
                (e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right)) e.SuppressKeyPress = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = textBox8.Text;
            openFileDialog1.Filter = "rez files (*.rez)|*.rez|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;
            if (checkedListBox2.SelectedItem != null) // это если хотя бы один отмечен то он 
            {
                try
                {
                    openFileDialog1.ShowDialog();
                    // MessageBox.Show("");

                    System.IO.FileInfo f = new System.IO.FileInfo(@"" + openFileDialog1.FileName);
                    GraphPane pane1 = zedGraphControl1.GraphPane;
                    pane1.Title.Text = "Просмотр файла тепловоза " + f.Name.Remove(12) + " за период " + f.Name.Remove(0, 12).Remove(8);
                    // Обновляем график
                    zedGraphControl1.Invalidate();


                    int bytesRead = 0;   //количество байт в читаемом файле
                    byte[] buffer = new byte[2000000]; //буфер памяти 2м
                    using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                    {
                        using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                        {
                            bytesRead = br.Read(buffer, 0, buffer.Length);///////+                                                          
                        }//конец потока битов читаемого файла
                    }//конец потока читаемого файла

                    //MessageBox.Show(buffer[230] + " день " + buffer[231] + " месяц  " + buffer[232] + " год  " + buffer[233] + " час  " + buffer[234] + " минута " + buffer[235] + " сек "); //вывод даты                                                        


                    GraphPane pane = zedGraphControl1.GraphPane;// Получим панель для рисования   
                    pane.CurveList.Clear();

                    PointPairList list_1 = new PointPairList();
                    PointPairList list_2 = new PointPairList();
                    PointPairList list_3 = new PointPairList();
                    PointPairList list_4 = new PointPairList();
                    PointPairList list_5 = new PointPairList();
                    PointPairList list_6 = new PointPairList();
                    PointPairList list_7 = new PointPairList();
                    PointPairList list_8 = new PointPairList();
                    PointPairList list_9 = new PointPairList();
                    PointPairList list_10 = new PointPairList();
                    PointPairList list_11 = new PointPairList();
                    PointPairList list_12 = new PointPairList();
                    PointPairList list_13 = new PointPairList();
                    PointPairList list_14 = new PointPairList();
                    PointPairList list_15 = new PointPairList();
                    PointPairList list_16 = new PointPairList();
                    PointPairList list_17 = new PointPairList();
                    PointPairList list_18 = new PointPairList();
                    PointPairList list_19 = new PointPairList();

                    /*//262 байта - чистый рез +
                    bw.Write(new byte[] { buffer[253 + i2] }); bw.Write(new byte[] { buffer[254 + i2] });//запись в новый rez данных АСК - табельный номер              //2                                                 
                    bw.Write(new byte[] { buffer[255 + i2] }); bw.Write(new byte[] { buffer[256 + i2] });//запись в новый rez данных АСК - номер марсшрута              //2
                    bw.Write(new byte[] { buffer[257 + i2] }); bw.Write(new byte[] { buffer[258 + i2] });//запись в новый rez данных АСК - вес поезда                   //2
                    bw.Write(new byte[] { buffer[259 + i2] }); bw.Write(new byte[] { buffer[260 + i2] });//запись в новый rez данных АСК - регион эксплуатации          //2
                    bw.Write(new byte[] { buffer[261 + i2] }); bw.Write(new byte[] { buffer[262 + i2] });//запись в новый rez данных АСК - плотность топлива            //2
                    bw.Write(new byte[] { buffer[263 + i2] }); //запись в новый rez данных АСК - режим эксплуатации                                                     //1
                    //координаты широты
                    bw.Write(new byte[] { buffer[264 + i2] });//запись в новый rez данных АСК - градусы широты                                                          //1
                    bw.Write(new byte[] { buffer[265 + i2] });//запись в новый rez данных АСК - минуты широты                                                           //1
                    for (int j = 266; j <= 273; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды широты                //8
                    //координаты долготы
                    bw.Write(new byte[] { buffer[274 + i2] });//запись в новый rez данных АСК - градусы долготы                                                         //1
                    bw.Write(new byte[] { buffer[275 + i2] });//запись в новый rez данных АСК - минуты долготы                                                          //1
                    for (int j = 276; j <= 283; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды долготы               //8

                    bw.Write(new byte[] { buffer[284 + i2] });//запись в новый rez данных АСК - полушарие - меридиан                                                    //1
                    bw.Write(new byte[] { buffer[285 + i2] });//запись в новый rez данных АСК - количество спутников                                                    //1
                    for (int j = 286; j <= 293; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - высота над уровнем моря       //8
                    for (int j = 294; j <= 301; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - скорость км/час               //8
                    for (int j = 302; j <= 309; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо левый кг              //8
                    for (int j = 310; j <= 317; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо правый кг             //8
                    for (int j = 318; j <= 325; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива левый     //8
                    for (int j = 326; j <= 333; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива правый    //8 
                    for (int j = 334; j <= 341; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5В                //8
                    for (int j = 342; j <= 349; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - среднее топливо               //8
                    for (int j = 350; j <= 357; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5ВDUAL            //8
                    for (int j = 358; j <= 365; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура CPU, оС           //8
                    for (int j = 366; j <= 373; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура сист. платы, оС   //8

                    bw.Write(new byte[] { buffer[374 + i2] });//запись в новый rez данных АСК - дискретные входные                                                      //1            
                    bw.Write(new byte[] { buffer[375 + i2] });//запись в новый rez данных АСК - дискретные выходные                                                     //1
                    */
                    int k = 0;
                    for (int i = 0; i <= buffer.Length - 385; i = i + 385)
                    {

                        //list_1.Add(k, buffer[i+200]); 
                        list_1.Add(k, BitConverter.ToDouble(buffer, i + 295));  //высота над уровнем моря
                        list_2.Add(k, BitConverter.ToDouble(buffer, i + 303));  //скорость км/ч
                        list_3.Add(k, BitConverter.ToDouble(buffer, i + 311));  //топливо кг левый
                        list_4.Add(k, BitConverter.ToDouble(buffer, i + 319));  //топливо кг правый
                        list_5.Add(k, BitConverter.ToDouble(buffer, i + 327));  //температура топлива левый
                        list_6.Add(k, BitConverter.ToDouble(buffer, i + 335));  //температура топлива правый
                        list_7.Add(k, BitConverter.ToDouble(buffer, i + 343));  //Напряжение +5В 
                        list_8.Add(k, BitConverter.ToDouble(buffer, i + 351));  //среднее топливо 
                        list_9.Add(k, BitConverter.ToDouble(buffer, i + 359));  //Напряжение +5ВDUAL                  
                        list_10.Add(k, BitConverter.ToDouble(buffer, i + 367)); //Температура CPU, оС     
                        list_11.Add(k, BitConverter.ToDouble(buffer, i + 375)); //Температура сист. платы, оС                     
                        list_12.Add(k, (buffer[i + 376] & 0X01)); //крышка модуля 
                        list_13.Add(k, (buffer[i + 376] & 0X02)); //питание модуля
                        list_14.Add(k, (buffer[i + 376] & 0X03)); //обмен с ДМ
                        list_15.Add(k, (buffer[i + 376] & 0X04)); //обмен с GPS приемником
                        list_16.Add(k, (buffer[i + 376] & 0X05)); //обмен с ДТ
                        list_17.Add(k, (buffer[i + 377] & 0X01)); //рестарт модема
                        list_18.Add(k, (buffer[i + 377] & 0X02)); //рестарт модуля по GPS
                        list_19.Add(k, (buffer[i + 377] & 0X03)); //рестарт модуля по ДМ

                        k++;
                    }

                    foreach (int s in checkedListBox2.CheckedIndices)
                    {
                        if (s == 0) { LineItem myCurve_1 = pane.AddCurve("Высота над уровнем моря", list_1, Color.Orange, SymbolType.None); }
                        if (s == 1) { LineItem myCurve_2 = pane.AddCurve("Скорость км/ч", list_2, Color.BlueViolet, SymbolType.None); }
                        if (s == 2) { LineItem myCurve_3 = pane.AddCurve("Топливо кг левый", list_3, Color.Orange, SymbolType.None); }
                        if (s == 3) { LineItem myCurve_4 = pane.AddCurve("Топливо кг правый", list_4, Color.Green, SymbolType.None); }
                        if (s == 4) { LineItem myCurve_5 = pane.AddCurve("Температура топлива левый", list_5, Color.Aqua, SymbolType.None); }
                        if (s == 5) { LineItem myCurve_6 = pane.AddCurve("Температура топлива правый", list_6, Color.Violet, SymbolType.None); }
                        if (s == 6) { LineItem myCurve_7 = pane.AddCurve("Напряжение +5В", list_7, Color.White, SymbolType.None); }
                        if (s == 7) { LineItem myCurve_8 = pane.AddCurve("Среднее топливо", list_8, Color.Yellow, SymbolType.None); }
                        if (s == 8) { LineItem myCurve_9 = pane.AddCurve("Напряжение +5ВDUAL", list_9, Color.DimGray, SymbolType.None); }
                        if (s == 9) { LineItem myCurve_10 = pane.AddCurve("Температура CPU, оС", list_10, Color.DeepPink, SymbolType.None); }
                        if (s == 10) { LineItem myCurve_11 = pane.AddCurve("Температура сист. платы, оС", list_11, Color.SaddleBrown, SymbolType.None); }
                        if (s == 11) { LineItem myCurve_12 = pane.AddCurve("Крышка модуля", list_12, Color.Red, SymbolType.None); }
                        if (s == 12) { LineItem myCurve_13 = pane.AddCurve("Питание модуля", list_13, Color.Red, SymbolType.None); }
                        if (s == 13) { LineItem myCurve_14 = pane.AddCurve("Обмен с ДМ", list_14, Color.Red, SymbolType.None); }
                        if (s == 14) { LineItem myCurve_15 = pane.AddCurve("Обмен с GPS приемником", list_15, Color.Red, SymbolType.None); }
                        if (s == 15) { LineItem myCurve_16 = pane.AddCurve("Обмен с ДТ", list_16, Color.Red, SymbolType.None); }
                        if (s == 16) { LineItem myCurve_17 = pane.AddCurve("Рестарт модема", list_17, Color.Red, SymbolType.None); }
                        if (s == 17) { LineItem myCurve_18 = pane.AddCurve("Рестарт модуля по GPS", list_18, Color.Red, SymbolType.None); }
                        if (s == 18) { LineItem myCurve_19 = pane.AddCurve("Рестарт модуля по ДМ", list_19, Color.Red, SymbolType.None); }
                    }
                    // Включаем отображение сетки напротив крупных рисок по оси X
                    pane.XAxis.MajorGrid.IsVisible = true;
                    // Включаем отображение сетки напротив крупных рисок по оси Y
                    pane.YAxis.MajorGrid.IsVisible = true;
                    zedGraphControl1.AxisChange(); zedGraphControl1.Invalidate();


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Файл не найден. Ошибка: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один из параметров!");
            }
        }

        private void button13_Click_3(object sender, EventArgs e)
        {
            //показ всплывающего окна с сообщением из трея
            notifyIcon1.ShowBalloonTip(500, "Сообщение", "Конвертор IO. Версия 1.0", ToolTipIcon.Info);           
        }

        private void zedGraphControl1_MouseMove(object sender, MouseEventArgs e)
        {
            // Сюда будут записаны координаты в системе координат графика
            double x, y;

            // Пересчитываем пиксели в координаты на графике
            // У ZedGraph есть несколько перегруженных методов ReverseTransform.
            zedGraphControl1.GraphPane.ReverseTransform(e.Location, out x, out y);

            // Выводим результат
            string text = string.Format("X: {0};    Y: {1}", x, y);
            coordLabel.Text = text;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            button15_Click(sender, e);
        }

        private void button14_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = textBox8.Text;
            openFileDialog1.Filter = "rez files (*.rez)|*.rez|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;
            if (checkedListBox2.SelectedItem != null) // это если хотя бы один отмечен то он 
            {
                try
                {
                    openFileDialog1.ShowDialog();
                    System.IO.FileInfo f = new System.IO.FileInfo(@"" + openFileDialog1.FileName);
                    GraphPane pane1 = zedGraphControl1.GraphPane;
                    pane1.Title.Text = "Просмотр файла тепловоза " + f.Name.Remove(8) + " за период " + f.Name.Remove(0, 8).Remove(8);
                    // Обновляем график
                    zedGraphControl1.Invalidate();

                    int bytesRead = 0;   //количество байт в читаемом файле
                    byte[] buffer = new byte[2000000]; //буфер памяти 2м
                    using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                    {
                        using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                        {
                            bytesRead = br.Read(buffer, 0, buffer.Length);///////+                                                          
                        }//конец потока битов читаемого файла
                    }//конец потока читаемого файла

                    //MessageBox.Show(buffer[410] + " год " + buffer[411] + " месяц  " + buffer[412] + " день  " + buffer[413] + " час  " + buffer[414] + " минута " + buffer[415] + " сек "); //вывод даты                                                        


                    GraphPane pane = zedGraphControl1.GraphPane;// Получим панель для рисования   
                    pane.CurveList.Clear();

                    PointPairList list_1 = new PointPairList();
                    PointPairList list_2 = new PointPairList();
                    PointPairList list_3 = new PointPairList();
                    PointPairList list_4 = new PointPairList();
                    PointPairList list_5 = new PointPairList();
                    PointPairList list_6 = new PointPairList();
                    PointPairList list_7 = new PointPairList();
                    PointPairList list_8 = new PointPairList();
                    PointPairList list_9 = new PointPairList();
                    PointPairList list_10 = new PointPairList();
                    PointPairList list_11 = new PointPairList();
                    PointPairList list_12 = new PointPairList();
                    PointPairList list_13 = new PointPairList();
                    PointPairList list_14 = new PointPairList();
                    PointPairList list_15 = new PointPairList();
                    PointPairList list_16 = new PointPairList();
                    PointPairList list_17 = new PointPairList();
                    PointPairList list_18 = new PointPairList();
                    PointPairList list_19 = new PointPairList();

                    /*
                    //общий размер пакета rez файла 460 байт                                      
                                                                //координаты широты
                                                                bw.Write(new byte[] { buffer[367 + i2] });//запись в новый rez данных АСК - градусы широты                                                      //1
                                                                bw.Write(new byte[] { buffer[368 + i2] });//запись в новый rez данных АСК - минуты широты                                                       //1
                                                                for (int j = 369; j <= 376; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды широты            //8
                                                                //координаты долготы                                                                                                                            //
                                                                bw.Write(new byte[] { buffer[377 + i2] });//запись в новый rez данных АСК - градусы долготы                                                     //1
                                                                bw.Write(new byte[] { buffer[378 + i2] });//запись в новый rez данных АСК - минуты долготы                                                      //1
                                                                for (int j = 379; j <= 386; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - секунды долготы           //8                                                                                                                                                                                                                                
                                                                bw.Write(new byte[] { buffer[387 + i2] });//запись в новый rez данных АСК - полушарие - меридиан                                                //1
                                                                bw.Write(new byte[] { buffer[388 + i2] });//запись в новый rez данных АСК - количество спутников                                                //1
                                                                for (int j = 389; j <= 396; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - высота над уровнем моря   //8
                                                                for (int j = 397; j <= 404; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - скорость км/час           //8
                                                                for (int j = 405; j <= 412; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо левый кг          //8
                                                                for (int j = 413; j <= 420; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - топливо правый кг         //8
                                                                for (int j = 421; j <= 428; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива левый //8
                                                                for (int j = 429; j <= 436; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - температура топлива правый//8
                                                                for (int j = 437; j <= 444; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5В            //8
                                                                for (int j = 445; j <= 452; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - среднее топливо           //8
                                                                for (int j = 453; j <= 460; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Напряжение +5ВDUAL        //8
                                                                for (int j = 461; j <= 468; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура CPU, оС       //8
                                                                for (int j = 469; j <= 476; j++) { bw.Write(new byte[] { buffer[j + i2] }); }//8байт//запись в новый rez данных АСК - Температура сист. платы,оС//8
                                                                bw.Write(new byte[] { buffer[477 + i2] });//запись в новый rez данных АСК - дискретные входные                                                  //1
                                                                bw.Write(new byte[] { buffer[478 + i2] });//запись в новый rez данных АСК - дискретные выходные                                                 //1
                    */
                    int k = 0;
                    for (int i = 0; i <= buffer.Length - 572; i = i + 572)
                    {
                        //list_1.Add(k, buffer[i+200]); 
                        list_1.Add(k, BitConverter.ToDouble(buffer, i + 482));  //высота над уровнем моря
                        list_2.Add(k, BitConverter.ToDouble(buffer, i + 490));  //скорость км/ч
                        list_3.Add(k, BitConverter.ToDouble(buffer, i + 498));  //топливо кг левый
                        list_4.Add(k, BitConverter.ToDouble(buffer, i + 506));  //топливо кг правый
                        list_5.Add(k, BitConverter.ToDouble(buffer, i + 514));  //температура топлива левый
                        list_6.Add(k, BitConverter.ToDouble(buffer, i + 522));  //температура топлива правый
                        list_7.Add(k, BitConverter.ToDouble(buffer, i + 530));  //Напряжение +5В 
                        list_8.Add(k, BitConverter.ToDouble(buffer, i + 538));  //среднее топливо 
                        list_9.Add(k, BitConverter.ToDouble(buffer, i + 546));  //Напряжение +5ВDUAL                  
                        list_10.Add(k, BitConverter.ToDouble(buffer, i + 554)); //Температура CPU, оС     
                        list_11.Add(k, BitConverter.ToDouble(buffer, i + 562)); //Температура сист. платы, оС                     
                        list_12.Add(k, (buffer[i + 563] & 0X01)); //крышка модуля
                        list_13.Add(k, (buffer[i + 563] & 0X02)); //питание модуля
                        list_14.Add(k, (buffer[i + 563] & 0X03)); //обмен с ДМ
                        list_15.Add(k, (buffer[i + 563] & 0X04)); //обмен с GPS приемником
                        list_16.Add(k, (buffer[i + 563] & 0X05)); //обмен с ДТ
                        list_17.Add(k, (buffer[i + 564] & 0X01)); //рестарт модема
                        list_18.Add(k, (buffer[i + 564] & 0X02)); //рестарт модуля по GPS
                        list_19.Add(k, (buffer[i + 564] & 0X03)); //рестарт модуля по ДМ
                        k++;
                    }

                    foreach (int s in checkedListBox2.CheckedIndices)
                    {
                        if (s == 0) { LineItem myCurve_1 = pane.AddCurve("Высота над уровнем моря", list_1, Color.Orange, SymbolType.None); }
                        if (s == 1) { LineItem myCurve_2 = pane.AddCurve("Скорость км/ч", list_2, Color.BlueViolet, SymbolType.None); }
                        if (s == 2) { LineItem myCurve_3 = pane.AddCurve("Топливо кг левый", list_3, Color.Orange, SymbolType.None); }
                        if (s == 3) { LineItem myCurve_4 = pane.AddCurve("Топливо кг правый", list_4, Color.Green, SymbolType.None); }
                        if (s == 4) { LineItem myCurve_5 = pane.AddCurve("Температура топлива левый", list_5, Color.Aqua, SymbolType.None); }
                        if (s == 5) { LineItem myCurve_6 = pane.AddCurve("Температура топлива правый", list_6, Color.Violet, SymbolType.None); }
                        if (s == 6) { LineItem myCurve_7 = pane.AddCurve("Напряжение +5В", list_7, Color.White, SymbolType.None); }
                        if (s == 7) { LineItem myCurve_8 = pane.AddCurve("Среднее топливо", list_8, Color.Yellow, SymbolType.None); }
                        if (s == 8) { LineItem myCurve_9 = pane.AddCurve("Напряжение +5ВDUAL", list_9, Color.DimGray, SymbolType.None); }
                        if (s == 9) { LineItem myCurve_10 = pane.AddCurve("Температура CPU, оС", list_10, Color.DeepPink, SymbolType.None); }
                        if (s == 10) { LineItem myCurve_11 = pane.AddCurve("Температура сист. платы, оС", list_11, Color.SaddleBrown, SymbolType.None); }
                        if (s == 11) { LineItem myCurve_12 = pane.AddCurve("Крышка модуля", list_12, Color.Red, SymbolType.None); }
                        if (s == 12) { LineItem myCurve_13 = pane.AddCurve("Питание модуля", list_13, Color.Red, SymbolType.None); }
                        if (s == 13) { LineItem myCurve_14 = pane.AddCurve("Обмен с ДМ", list_14, Color.Red, SymbolType.None); }
                        if (s == 14) { LineItem myCurve_15 = pane.AddCurve("Обмен с GPS приемником", list_15, Color.Red, SymbolType.None); }
                        if (s == 15) { LineItem myCurve_16 = pane.AddCurve("Обмен с ДТ", list_16, Color.Red, SymbolType.None); }
                        if (s == 16) { LineItem myCurve_17 = pane.AddCurve("Рестарт модема", list_17, Color.Red, SymbolType.None); }
                        if (s == 17) { LineItem myCurve_18 = pane.AddCurve("Рестарт модуля по GPS", list_18, Color.Red, SymbolType.None); }
                        if (s == 18) { LineItem myCurve_19 = pane.AddCurve("Рестарт модуля по ДМ", list_19, Color.Red, SymbolType.None); }
                    }
                    // Включаем отображение сетки напротив крупных рисок по оси X
                    pane.XAxis.MajorGrid.IsVisible = true;
                    // Включаем отображение сетки напротив крупных рисок по оси Y
                    pane.YAxis.MajorGrid.IsVisible = true;
                   
                    zedGraphControl1.AxisChange(); zedGraphControl1.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Файл не найден. Ошибка: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один из параметров!");
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;

            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {           
         //   MessageBox.Show("");//клик по всплывающему окошку в паели уведомлений
        }

        private void button18_Click(object sender, EventArgs e)
        {
          
        }

        private void button17_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = textBox8.Text;
            openFileDialog1.Filter = "rez files (*.rez)|*.rez|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;
            if (checkedListBox2.SelectedItem != null) // это если хотя бы один отмечен то он 
            {
                try
                {
                    openFileDialog1.ShowDialog();
                    // MessageBox.Show("");

                    System.IO.FileInfo f = new System.IO.FileInfo(@"" + openFileDialog1.FileName);
                    GraphPane pane1 = zedGraphControl1.GraphPane;
                    pane1.Title.Text = "Просмотр файла тепловоза " + f.Name.Remove(12) + " за период " + f.Name.Remove(0, 12).Remove(8);
                    // Обновляем график
                    zedGraphControl1.Invalidate();


                    int bytesRead = 0;   //количество байт в читаемом файле
                    byte[] buffer = new byte[2000000]; //буфер памяти 2м
                    using (System.IO.FileStream fs = f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) //поток для читаемого файла
                    {
                        using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs)) //поток для чтения битов 
                        {
                            bytesRead = br.Read(buffer, 0, buffer.Length);///////+                                                          
                        }//конец потока битов читаемого файла
                    }//конец потока читаемого файла

                    //MessageBox.Show(buffer[661] + " год " + buffer[662] + " месяц  " + buffer[663] + " день  " + buffer[664] + " час  " + buffer[665] + " минута " + buffer[666] + " сек "); //вывод даты                                                        
                    MessageBox.Show(BitConverter.ToDouble(buffer, 651).ToString());

                    GraphPane pane = zedGraphControl1.GraphPane;// Получим панель для рисования   
                    pane.CurveList.Clear();

                    PointPairList list_1 = new PointPairList();
                    PointPairList list_2 = new PointPairList();
                    PointPairList list_3 = new PointPairList();
                    PointPairList list_4 = new PointPairList();
                    PointPairList list_5 = new PointPairList();
                    PointPairList list_6 = new PointPairList();
                    PointPairList list_7 = new PointPairList();
                    PointPairList list_8 = new PointPairList();
                    PointPairList list_9 = new PointPairList();
                    PointPairList list_10 = new PointPairList();
                    PointPairList list_11 = new PointPairList();
                    PointPairList list_12 = new PointPairList();
                    PointPairList list_13 = new PointPairList();
                    PointPairList list_14 = new PointPairList();
                    PointPairList list_15 = new PointPairList();
                    PointPairList list_16 = new PointPairList();
                    PointPairList list_17 = new PointPairList();
                    PointPairList list_18 = new PointPairList();
                    

                    /*//451 байта - чистый рез +
                    
                    */
                    int k = 0;
                    for (int i = 0; i <= buffer.Length - 728; i = i + 728)//451+дописываемые байты
                    {
                        //MessageBox.Show(buffer[660+i] + " год ");
                        //list_1.Add(k, buffer[i+200]); 
                        list_1.Add(k, BitConverter.ToDouble(buffer, i + 571));  //высота над уровнем моря
                        list_2.Add(k, BitConverter.ToDouble(buffer, i + 579));  //скорость км/ч
                        list_3.Add(k, BitConverter.ToDouble(buffer, i + 587));  //топливо кг левый
                        list_4.Add(k, BitConverter.ToDouble(buffer, i + 595));  //топливо кг правый
                        list_5.Add(k, BitConverter.ToDouble(buffer, i + 603));  //температура топлива левый
                        list_6.Add(k, BitConverter.ToDouble(buffer, i + 611));  //температура топлива правый
                        list_7.Add(k, BitConverter.ToDouble(buffer, i + 619));  //Напряжение +5В 
                        list_8.Add(k, BitConverter.ToDouble(buffer, i + 627));  //среднее топливо 
                        list_9.Add(k, BitConverter.ToDouble(buffer, i + 635));  //Напряжение +5ВDUAL                  
                        list_10.Add(k, BitConverter.ToDouble(buffer, i + 643)); //Температура CPU, оС     
                        list_11.Add(k, BitConverter.ToDouble(buffer, i + 651)); //Температура сист. платы, оС                     
                        list_12.Add(k, (buffer[i + 659] & 0X01)); //крышка модуля 
                        list_13.Add(k, (buffer[i + 659] & 0X02)); //питание модуля
                        list_14.Add(k, (buffer[i + 659] & 0X03)); //обмен с ДМ
                        list_15.Add(k, (buffer[i + 659] & 0X04)); //обмен с GPS приемником
                        list_16.Add(k, (buffer[i + 659] & 0X05)); //обмен с ДТ
                        list_17.Add(k, (buffer[i + 660] & 0X01)); //рестарт модема
                        list_18.Add(k, (buffer[i + 660] & 0X02)); //рестарт модуля по GPS
                        

                        k++;
                    }

                    foreach (int s in checkedListBox2.CheckedIndices)
                    {
                        if (s == 0) { LineItem myCurve_1 = pane.AddCurve("Высота над уровнем моря", list_1, Color.Orange, SymbolType.None); }
                        if (s == 1) { LineItem myCurve_2 = pane.AddCurve("Скорость км/ч", list_2, Color.BlueViolet, SymbolType.None); }
                        if (s == 2) { LineItem myCurve_3 = pane.AddCurve("Топливо кг (левый)", list_3, Color.Orange, SymbolType.None); }
                        if (s == 3) { LineItem myCurve_4 = pane.AddCurve("Топливо кг (правый)", list_4, Color.Green, SymbolType.None); }
                        if (s == 4) { LineItem myCurve_5 = pane.AddCurve("Температура топлива левый", list_5, Color.Aqua, SymbolType.None); }
                        if (s == 5) { LineItem myCurve_6 = pane.AddCurve("Температура топлива правый", list_6, Color.Violet, SymbolType.None); }
                        if (s == 6) { LineItem myCurve_7 = pane.AddCurve("Напряжение +5В", list_7, Color.White, SymbolType.None); }
                        if (s == 7) { LineItem myCurve_8 = pane.AddCurve("Среднее топливо", list_8, Color.Yellow, SymbolType.None); }
                        if (s == 8) { LineItem myCurve_9 = pane.AddCurve("Напряжение +5B DUAL", list_9, Color.DimGray, SymbolType.None); }
                        if (s == 9) { LineItem myCurve_10 = pane.AddCurve("Температура CPU, оС", list_10, Color.DeepPink, SymbolType.None); }
                        if (s == 10) { LineItem myCurve_11 = pane.AddCurve("Температура системной платы, оС", list_11, Color.SaddleBrown, SymbolType.None); }
                        if (s == 11) { LineItem myCurve_12 = pane.AddCurve("Крышка модуля", list_12, Color.Red, SymbolType.None); }
                        if (s == 12) { LineItem myCurve_13 = pane.AddCurve("Питание модуля", list_13, Color.Red, SymbolType.None); }
                        if (s == 13) { LineItem myCurve_14 = pane.AddCurve("Обмен с ДМ", list_14, Color.Red, SymbolType.None); }
                        if (s == 14) { LineItem myCurve_15 = pane.AddCurve("Обмен с GPS приемником", list_15, Color.Red, SymbolType.None); }
                        if (s == 15) { LineItem myCurve_16 = pane.AddCurve("Обмен с ДТ", list_16, Color.Red, SymbolType.None); }
                        if (s == 16) { LineItem myCurve_17 = pane.AddCurve("Рестарт модема", list_17, Color.Red, SymbolType.None); }
                        if (s == 17) { LineItem myCurve_18 = pane.AddCurve("Рестарт модуля по GPS", list_18, Color.Red, SymbolType.None); }
                        if (s == 18) { MessageBox.Show("Параметр 'Рестарт модуля по GPS' отсутствует для серии 2ТЭ25А!","Выбран несуществующий параметр"); }
                        

                    }
                    // Включаем отображение сетки напротив крупных рисок по оси X
                    pane.XAxis.MajorGrid.IsVisible = true;
                    // Включаем отображение сетки напротив крупных рисок по оси Y
                    pane.YAxis.MajorGrid.IsVisible = true;
                    zedGraphControl1.AxisChange(); zedGraphControl1.Invalidate();


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Файл не найден. Ошибка: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один из параметров!");
            }
        }

     










        
    }
}