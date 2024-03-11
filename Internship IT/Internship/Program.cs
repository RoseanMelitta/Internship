using OfficeOpenXml;
using Program.Xlsx.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#pragma warning disable CS0105 // Using directive appeared previously in this namespace
using Program.Xlsx.Models;
#pragma warning restore CS0105 // Using directive appeared previously in this namespace
using ExcelDataReader;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Net.Mail;

namespace Program.Xlsx
{
    internal class Program
    {


        public static DateTime minincident = new DateTime();
        public static DateTime SmallestDate = DateTime.MaxValue;
        public static DateTime SmallestDate1 = DateTime.MaxValue;
        public static DateTime mintask = new DateTime();
        public static string minNumberIncident;
        public static string minNumberTask;
        public static int incidentsOpened = 0;
        public static int incidentsOpenedGlobal = 0;
        public static int avgOpenedIncidentGlobal = 0;
        public static int tasksOpened = 0;
        public static int taskOpenedGlobal = 0;
        public static int avgOpenedTasksGlobal = 0;
        public static int oldestIncidentDay;
        public static int oldestTaskDay;
        public static int avgOpenedIncidents = 0;
        public static int avgOpenedTasks = 0;




        static void Main(string[] args)
        {
            StartProgram();

            Console.ReadLine();

        }
        private static void StartProgram()
        {
            logs("Start Program  " + DateTime.Now.ToString("ddd,dd,MM,yyy hh:mm:ss"));

            StoredProceduresDeleteIncidente();
            StoredProceduresDeleteTask();


            InsertIncidents();
            InsertTasks();


            MoveIncident();
            MoveTask();


            Mail();
            //queri


            logs("END Program " + DateTime.Now.ToString("ddd,dd,MM,yyy hh:mm:ss"));
        }
        #region StoredProceduresDeleteIncidente
        private static void StoredProceduresDeleteIncidente()
        {
            string connectionString = "Data Source=ora-sql-pd-002;" +
                "Initial Catalog=I_Balog;Integrated Security=SSPI";

            string SQL = "DeleteAllDataIncidente";

            // Create ADO.NET objects.

            SqlConnection con = new SqlConnection(connectionString);

            SqlCommand cmd = new SqlCommand(SQL, con);

            cmd.CommandType = CommandType.StoredProcedure;


            // Execute the command.

            con.Open();

            int rowsAffected = cmd.ExecuteNonQuery();

            con.Close();

            // Display the result of the operation.


            Console.WriteLine(" Incidents have been deleted:");

        }
        #endregion
        #region StoredProceduresDeleteTask
        private static void StoredProceduresDeleteTask()
        {

            string connectionString = "Data Source=ora-sql-pd-002;" +
                "Initial Catalog=I_Balog;Integrated Security=SSPI";

            string SQL = "DeleteAllDataTasks";

            // Create ADO.NET objects.

            SqlConnection con = new SqlConnection(connectionString);

            SqlCommand cmd = new SqlCommand(SQL, con);

            cmd.CommandType = CommandType.StoredProcedure;


            // Execute the command.

            con.Open();

            int rowsAffected = cmd.ExecuteNonQuery();

            con.Close();

            // Display the result of the operation.

            Console.WriteLine("Tasks have been deleted:  ");

        }
        #endregion



        #region InsertIncidenteReader
        private static void InsertIncidents()
        {



            var listIncidents = new List<Incidente>();//cream o lista cu numele listIncidents lista goala,cream o lista goala
            Console.Write("\n Reading Incident file:\n");
            try
            {

                listIncidents = getIncidentFromXlsx(@"C:\Report\Input\incidents.xlsx");
                Console.WriteLine("Number of lines:" + listIncidents.Count());

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception getIncidentsFromCsv: " + ex);
                logs("Fisierul nu a fost gasit");

            }



            if (listIncidents.Count > 0) //cand lista are cel putin un element
            {
                try
                {
                    InsertIncidentsInDB(listIncidents);
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Exception getIncidentsFromCsv: " + ex);
                    logs("\t" + "Incidentele nu au fost inserate");
                }
            }


        }
        #endregion
        #region getIncidentFromXlsx 
        private static List<Incidente> getIncidentFromXlsx(string filePath)//returnez o lista pe nume List<Incidente> ii o metoda
        {
            DateTime SmallestDate = DateTime.MaxValue;
            var p = 0;
            var listInc = new List<Incidente>();

            try

            {

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            // Reading each row. 
                            while (reader.Read()) //cat timp mai exista in fisier
                            {
                                var inc = new Incidente();
                                var lineExtract = reader;
                                //reader.NextRezult = 3
                                //if (reader.NextResult ())
                                //{

                                //}

                                var lineV = lineExtract;
                                p++;

                                if ((String)reader[0] != "Opened by")
                                {

                                    inc.Opened_by = (String)reader[0];
                                    inc.Elapsed_Time_Open = (double?)reader[1];
                                    inc.Opened = (Nullable<System.DateTime>)reader[2];

                                    if (inc.Opened < SmallestDate && (inc.State == "In Progress" || inc.State == "New" || inc.State == "On Hold"))
                                    {
                                        SmallestDate = (DateTime)inc.Opened;
                                        minNumberIncident = inc.Number;


                                    }

                                    inc.Resolved = (Nullable<System.DateTime>)reader[3];
                                    inc.Number = (String)reader[4];
                                    inc.Short_description = (String)reader[5];
                                    inc.Priority = (String)reader[6];
                                    inc.State = (String)reader[7];
                                    inc.Incident_state = (String)reader[8];
                                    inc.Assigned_to = (String)reader[9];
                                    inc.Assignmed_group = (String)reader[10];
                                    inc.Caller = (String)reader[11];
                                    inc.Closed = (Nullable<System.DateTime>)reader[12];
                                    inc.Created_by = (String)reader[13];
                                    inc.Location = (String)reader[14];
                                    inc.Rezolved_by = (String)reader[15];
                                    inc.Closed_by = (String)reader[16];
                                    inc.Subcategory = (String)reader[17];
                                    inc.Category = (String)reader[18];
                                    inc.u_resolve_date = (Nullable<System.DateTime>)reader[19];
                                    inc.Made_sla = (bool)(Nullable<bool>)reader[20];
                                    inc.Reassignment_count = (Nullable<double>)reader[21];
                                    inc.Affected_CI = (String)reader[22];
                                    inc.Fix_code = (String)reader[23];
                                    inc.Business_service = (String)reader[24];
                                    inc.SLA_due = (Nullable<System.DateTime>)reader[25];
                                    inc.Resolve_Time = (Nullable<double>)reader[26];
                                    inc.u_classification = (String)reader[27];
                                    inc.Description = (String)reader[28];

                                    if (inc.State == "In Progress" || inc.State == "New" || inc.State == "On Hold")
                                    {
                                        incidentsOpenedGlobal++;

                                        avgOpenedIncidentGlobal += Convert.ToInt32((DateTime.Now - (DateTime)inc.Opened).TotalDays);

                                        if (inc.Assignmed_group == "Kelso" || inc.Assignmed_group == "Livingston" || inc.Assignmed_group == "Darmstadt" || inc.Assignmed_group == "Oradea")
                                        {


                                            incidentsOpened++;
                                            avgOpenedIncidents += Convert.ToInt32((DateTime.Now - (DateTime)inc.Opened).TotalDays);
                                        }
                                    }


                                    listInc.Add(inc);




                                }
                                else
                                {
                                    Console.WriteLine("Numar de Incidente: " + lineV[0] + "  " + lineV[1] + " " + p + " ");
                                    p++;
                                }
                            }
                        } while (reader.NextResult());
                        //stream.Close(); se inchid automat
                        //reader.Close();
                    }
                }
                Console.WriteLine("Insert Incident in db..");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                //LogWrite(ex.ToString());
            }
            oldestIncidentDay = Convert.ToInt32((DateTime.Now - SmallestDate).TotalDays);

            avgOpenedIncidents = avgOpenedIncidents / incidentsOpened;
            avgOpenedIncidentGlobal = avgOpenedIncidentGlobal / incidentsOpenedGlobal;
            minincident = SmallestDate;
            Console.WriteLine("The date of the oldest incident: " + SmallestDate + ", " + oldestIncidentDay + " days have passed since that\n Task number: " + minNumberIncident + "\nTasks opened at EMEA: " + incidentsOpened + "\nAverage open time for the tasks is : " + avgOpenedIncidents + " days");





            return listInc;
        }

        #endregion
        #region InsertIncidentsInDB
        private static void InsertIncidentsInDB(List<Incidente> listIncidents)
        {



            foreach (var Incidente in listIncidents)//itereaza fiecare element
            {

                try
                {
                    StoredProceduresInsertIncident(Incidente);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            //Console.WriteLine("sunt 0 elemente in db");
        }


        #endregion


        #region InsertTaskReader
        private static void InsertTasks()
        {
            // List<Taskf> listTaskf = new List<Taskf>();//cream o lista de obiecte din clasa Taskf cu proprietatile din ea  
            //  var ListTask = new List<taskf>();

            var listTaskf = new List<Taskf>();
            Console.Write("\n Reading Task file:");
            try
            {

                listTaskf = getTaskfFromXlsx(@"C:\Report\Input\tasks.xlsx");
                Console.WriteLine("Number of lines:" + listTaskf.Count());//numara liniile

            }
            catch (Exception ex)                         //trimite-m exceptia care se afiseaza
            {
                Console.WriteLine("Exception getTaskFromCsv:" + ex);
                logs("\t\n" + "Fisierul Task nu a fost gasit" + "\t\n");
            }
            if (listTaskf.Count > 0) //daca lista nu ii populata
            {
                try
                {

                    InsertTaskfInDB(listTaskf);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception getTaskfFromCsv" + ex);
                    logs("\t" + "Lista nu este populata");
                }
            }
            // Mail();
        }

        #endregion
        #region GetTaskfFromXlsx
        private static List<Taskf> getTaskfFromXlsx(string filePath)//avem o functie care returneaza o lista

        {
            var p = 0;
            var listObj = new List<Taskf>();  //cream o lista de clasa respectiva lista de acceasi obiecte
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))//merge pe siruri
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))// continutul fisierului
                    {
                        do
                        {
                            // Readingpp each row.
                            while (reader.Read())//
                            {
                                var obj = new Taskf();// un obiect 


                                var lineExtract = reader;
                                var lineV = lineExtract;
                                //  reader.NextResult
                                // p++;
                                //get luam informatiile cand avem clasa //laum lucrurile si le afisam
                                if ((String)reader[0] != "Number")
                                {
                                    obj.Number = (String)reader[0];
                                    obj.Priority = (String)reader[1];
                                    obj.State = (String)reader[2];
                                    obj.Short_description = (String)reader[3];
                                    obj.Assigned_to = (String)reader[4];
                                    obj.Opened_by = (String)reader[5];
                                    obj.Elapsed_Time_Open = (Nullable<double>)reader[6];
                                    obj.Task_Type = (String)reader[7];
                                    obj.Location = (String)reader[8];
                                    obj.Opened = (Nullable<System.DateTime>)reader[9];
                                    //if (obj.State == "Open" || obj.State == "Work in Progress" || obj.State == "Pending")
                                    //{
                                    //    tasksOpened++;
                                    //    avgOpenedTasks += Convert.ToInt32((DateTime.Now - (DateTime)obj.Opened).TotalDays);
                                    //}
                                    obj.Resolve_Time = (String)reader[10];
                                    if (obj.Opened < SmallestDate && (obj.State == "Open" || obj.State == "Work in Progress" || obj.State == "Pending"))
                                    {
                                        SmallestDate = (DateTime)obj.Opened;
                                        minNumberTask = obj.Number;


                                    }
                                    //if (obj.Assignment_group == "Oradea" || obj.Location == "Kelso" || obj.Location == "Livingston" || obj.Location == "Darmstadt")
                                    //{

                                    //    taskOpenedGlobal++;

                                    //    avgOpenedTasksGlobal += Convert.ToInt32((DateTime.Now - (DateTime)obj.Opened).TotalDays);
                                    //}
                                    //obj.Resolve_Time = (String)reader[10];
                                    obj.Updated_by = (String)reader[11];
                                    obj.Parent = (String)reader[12];
                                    obj.Made_sla = (bool)(Nullable<bool>)reader[13];
                                    obj.Assignment_group = (String)reader[14];
                                    obj.Closed = (Nullable<System.DateTime>)reader[15];
                                    obj.Closed_by = (String)reader[16];
                                    obj.Impact = (String)reader[17];
                                    obj.Active = (bool)(Nullable<bool>)reader[18];
                                    obj.Duration = (Nullable<double>)reader[19];
                                    obj.User_location = (String)reader[20];
                                    obj.Catalog = (String)reader[21];
                                    obj.Catalog_item = (String)reader[22];

                                    if (obj.State == "Open" || obj.State == "Work in Progress" || obj.State == "Pending")
                                    {
                                        tasksOpened++;
                                        avgOpenedTasks += Convert.ToInt32((DateTime.Now - (DateTime)obj.Opened).TotalDays);
                                        //if (obj.Assignment_group == "Oradea" || obj.Location == "Kelso" || obj.Location == "Livingston" || obj.Location == "Darmstadt")
                                        if (obj.Assignment_group == "Oradea" || obj.Assignment_group == "Kelso" || obj.Assignment_group == "Livingston" || obj.Assignment_group == "Darmstadt")

                                        {

                                            taskOpenedGlobal++;

                                            avgOpenedTasksGlobal += Convert.ToInt32((DateTime.Now - (DateTime)obj.Opened).TotalDays);
                                        }
                                    }


                                    listObj.Add(obj);//adaugam cate un obiectul lista noastra

                                }
                                else
                                {
                                    Console.WriteLine(" Numar de Task " + lineV[0] + " " + lineV[1] + " " + p);
                                    p++;

                                }
                            }
                        } while (reader.NextResult());
                        stream.Close();
                        reader.Close();
                    }
                }
                Console.WriteLine("introduceti taskuri in DB..");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                //LogWrite(ex.ToString());
            }

            avgOpenedTasks = avgOpenedTasks / tasksOpened;

            oldestTaskDay = Convert.ToInt32((DateTime.Now - SmallestDate).TotalDays);
            avgOpenedTasksGlobal = avgOpenedTasksGlobal / taskOpenedGlobal;
            Console.WriteLine("The date of the oldest task: " + SmallestDate + ", " + oldestTaskDay + " days have passed since that\n Task number: " + minNumberTask + "\nTasks opened at EMEA: " + tasksOpened + "\nAverage open time for the tasks is : " + avgOpenedTasks + " days");
            mintask = SmallestDate;

            return listObj;//returneaza lista in 

        }

        #endregion       
        #region InsertTaskfInDB
        private static void InsertTaskfInDB(List<Taskf> listtaskf)
        {



            foreach (var Taskf in listtaskf) //itereaza fiecare element 
            {



                try

                {
                    StoredProceduresInsertTask(Taskf);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            // Console.WriteLine("sunt 0 elemente in db");

        }

        #endregion

        #region MoveTask
        private static void MoveTask()
        {
            string pathin = @"C:\Report\Input\tasks.xlsx";
            string pathout = @"C:\Report\Output\tasks.xlsx";
            if (File.Exists(pathin))
            {
                File.Move(pathin, pathout);
                File.Delete(pathin);
                Console.WriteLine("The file Task has been moved: ");

            }

            else
            {
                File.Move(pathout, pathin);
                File.Delete(pathout);

                logs("\t" + "Fisierul Task nu a fost gasit");

            }









        }
        #endregion
        #region MoveIncident

        private static void MoveIncident()
        {
            string pathin2 = @"C:\Report\Input\incidents.xlsx";
            string pathout2 = @"C:\Report\Output\incidents.xlsx";

            if (File.Exists(pathin2))
            {

                File.Move(pathin2, pathout2);
                File.Delete(pathin2);
                Console.WriteLine("The file Incidente has been moved: ");

            }

            else
            {
                File.Move(pathout2, pathin2);
                File.Delete(pathout2);
                // Console.WriteLine("The file Incidente has been moved back:");
                logs("\t" + "Fisierul incidente nu a fost gasit");

            }



        }



        #endregion


        #region logs

        private static void logs(string v)
        {
            {
                string path = @"C:/Report/Input/logs.txt";
                if (!File.Exists(path))
                {   //Create a file to write to.
                    string createText = "" + Environment.NewLine + "\n"; //sa nu inceapa de sus de pe prima linie
                    File.AppendAllText(@"C:/Report/Input/logs.txt", "\n\t" + DateTime.Now.ToString("ddd,dd,MM,yyy hh:mm:ss") + "\n");
                    File.WriteAllText(path, createText);
                }
                else
                {
                    File.AppendAllText(@"C:/Report/Input/logs.txt", "\n\t" + DateTime.Now.ToString("ddd,dd,MM,yyy hh:mm:ss") + "\n");
                    string appendText = v + Environment.NewLine + "\n";
                    File.AppendAllText(path, appendText);
                }

            }

        }
        #endregion


        #region Mail
        private static void Mail()

        {

            Console.WriteLine("Sending mail");
            MailMessage message = new MailMessage();

            message.Subject = "Statistically: Tasks and Incident";
            message.From = new MailAddress("ex.email");
            message.To.Add(new MailAddress("ex.email"));
            message.IsBodyHtml = true;
            message.Body = "<h2> Hello ,</h2>\n" +
            "<h4>Current status of incidents and tasks until: " + DateTime.Now.ToString("MM/dd/yyyy") + "</h4>" +

            "<h2 style='color: #0066FF'> INCIDENTS: </h2>" +

            "<br><span> -<b  style='color:#1F497D'>" + incidentsOpenedGlobal + "</b></span><span> is the number of incidents opened at site Global. </span>" + "<span style='background:lime'> (GREEN) </span>" +
            "<br><span>- <b  style='color:#1F497D'>" + incidentsOpened + "</b></span><span> is the number of incidents opened at site EMEA. </span>" + "<span style='background:lime'> (GREEN) </span>" +

            "<br><span> - Average of time incidents are opened for EMEA assignment groups:  </span> <b  style='color:#1F497D'>" + avgOpenedIncidents + "</b><span> days. </span>" + "<span style='color:white;background:red'> (RED) </span>" +
            "<br><span> - Average of time incidents are opened for Global assignment groups:  </span> <b  style='color:#1F497D'>" + avgOpenedIncidentGlobal + "</b><span> days. </span>" + "<span style='color:white;background:red'> (RED) </span>" +




            "<br><span> - The date of the oldest Incident: </span>" + "<span><b  style='color:#1F497D'>" + minincident + "</b></span><span style='color:white;background:red'> (RED) </span>" +





            "<h2 style='color: #0066FF'> TASKS: </h2>" +

             "<br><span>- <b  style='color:#1F497D'>" + tasksOpened + "</b></span><span> is the number of tasks opened at site Global. </span>" + "<span style='background:lime'> (GREEN) </span>" +

             "<br><span> -<b  style='color:#1F497D'>" + taskOpenedGlobal + "</b></span><span> is the number of tasks opened at site EMEA . </span>" + "<span style='background:lime'> (GREEN) </span>" +


            "<br><span> - Average of time tasks are opened for  EMEA assignment groups:  </span> <b  style='color:#1F497D'>" + avgOpenedTasks + "</b><span> days. </span>" + "<span style='color:white;background:red'> (RED) </span>" +
            "<br><span> - Average of time tasks are opened for  Global assignment groups:  </span> <b  style='color:#1F497D'>" + avgOpenedTasksGlobal + "</b><span> days. </span>" + "<span style='color:white;background:red'> (RED) </span>" +

            "<br><span> - The oldest task: </span> <b  style='color:#1F497D'>" + minNumberTask + "</b><span>, have </span><span><b  style='color:#1F497D'>" + oldestTaskDay + "</b></span><span> days. </span>" + "<span style='color:white;background:red'> (RED) </span>" +
            "<br><span> - The date of the oldest task: </span>" + "<span><b  style='color:#1F497D'>" + mintask + "</b></span><span style='color:white;background:red'> (RED) </span>";







            SmtpClient smtp = new SmtpClient();
            smtp.Port = 25;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = true;
            smtp.Host = "ex";
            smtp.Send(message);
            //pagina de html

            MoveIncident();
            MoveTask();
        }
        #endregion



        #region StoredProceduresInsertTask


        public static void StoredProceduresInsertTask(Taskf task)
        {
            if (task.Closed == default(DateTime))
                task.Closed = new DateTime();



            if (String.IsNullOrEmpty(task.Closed.ToString())) task.Closed = DateTime.Now;
            if (String.IsNullOrEmpty(task.Short_description)) task.Short_description = " ";
            if (String.IsNullOrEmpty(task.Assignment_group)) task.Assignment_group = " ";
            if (String.IsNullOrEmpty(task.Closed_by)) task.Closed_by = " ";
            if (String.IsNullOrEmpty(task.Catalog)) task.Catalog = " ";
            if (String.IsNullOrEmpty(task.Resolve_Time)) task.Resolve_Time = " ";
            if (String.IsNullOrEmpty(task.Location)) task.Location = " ";
            if (String.IsNullOrEmpty(task.Assigned_to)) task.Assigned_to = " ";

            I_BalogEntities _dbSN = new I_BalogEntities();

            var data = new List<Taskf>();



            try
            {




                var Parameter = new[]
                {
                  /*  Guid id= Guid.NewGuid();
                string idAsString = id.ToString();
                console.WriteLine("" + idAsString);*/
               
               // new SqlParameter("@id",task.id)

             new SqlParameter("@Number",task.Number)
            ,new SqlParameter("@Priority",task.Priority)
            ,new SqlParameter("@State",task.State)
            ,new SqlParameter("@ShortDescription",task.Short_description)
            ,new SqlParameter("@AssignedTo",task.Assigned_to)
            ,new SqlParameter("@OpenedBy",task.Opened_by)
            ,new SqlParameter("@ElapsedTimeOpen",task.Elapsed_Time_Open)
            ,new SqlParameter("@TaskType",task.Task_Type)
            ,new SqlParameter("@Location","task.Location")
            ,new SqlParameter("@Opened",task.Opened)
            ,new SqlParameter("@ResolveTime","task.Resolve_Time")
            ,new SqlParameter("@UpdatedBy",task.Updated_by)
            ,new SqlParameter("@Parent",task.Parent)
            ,new SqlParameter("@MadeSla",task.Made_sla)
            ,new SqlParameter("@AssignmentGroup",task.Assignment_group)
            ,new SqlParameter("@Closed",task.Closed)
            ,new SqlParameter("@ClosedBy",task.Closed_by)
            ,new SqlParameter("@Impact",task.Impact)
            ,new SqlParameter("@Active",task.Active)
            ,new SqlParameter("@Duration",task.Duration)
            ,new SqlParameter("@UserLocation",task.User_location)
            ,new SqlParameter("@Catalog","task.Catalog")
            ,new SqlParameter("@CatalogItem",task.Catalog_item)



            };
                _dbSN.Database.ExecuteSqlCommand("exec InsertAllDataTaskf   @Number, @Priority, @State, @ShortDescription, @AssignedTo,  @OpenedBy,@ElapsedTimeOpen, @TaskType, @Location, @Opened,@ResolveTime, @UpdatedBy,@Parent,@MadeSla,@AssignmentGroup,@Closed,@ClosedBy,@Impact,@Active,@Duration,@UserLocation,@Catalog,@CatalogItem", Parameter);


            }
            catch (Exception ex)
            {
                Console.WriteLine("ex.Message" + ex);

            }



        }
        #endregion
        #region StoredProceduresInsertIncident


        public static void StoredProceduresInsertIncident(Incidente incidente)
        {


            if (String.IsNullOrEmpty(incidente.Resolve_Time.ToString())) incidente.Resolve_Time = 0;
            if (String.IsNullOrEmpty(incidente.SLA_due.ToString())) incidente.SLA_due = DateTime.Now;
            if (String.IsNullOrEmpty(incidente.Resolved.ToString())) incidente.Resolved = DateTime.Now;
            if (String.IsNullOrEmpty(incidente.Rezolved_by)) incidente.Rezolved_by = " ";
            if (String.IsNullOrEmpty(incidente.u_resolve_date.ToString())) incidente.u_resolve_date = DateTime.Now;
            if (String.IsNullOrEmpty(incidente.Closed.ToString())) incidente.Closed = DateTime.Now;
            if (String.IsNullOrEmpty(incidente.Description)) incidente.Description = " ";
            if (String.IsNullOrEmpty(incidente.u_classification)) incidente.u_classification = " ";
            if (String.IsNullOrEmpty(incidente.Business_service)) incidente.Business_service = " ";
            if (String.IsNullOrEmpty(incidente.Affected_CI)) incidente.Affected_CI = " ";
            if (String.IsNullOrEmpty(incidente.Fix_code)) incidente.Fix_code = " ";
            if (String.IsNullOrEmpty(incidente.Closed_by)) incidente.Closed_by = " ";
            if (String.IsNullOrEmpty(incidente.Opened_by)) incidente.Opened_by = " ";
            if (String.IsNullOrEmpty(incidente.id.ToString())) incidente.id = 0;
            if (String.IsNullOrEmpty(incidente.Assigned_to)) incidente.Assigned_to = " ";



            I_BalogEntities _dbSN = new I_BalogEntities();

            var data = new List<Incidente>();

            try
            {
                var Parameter = new[]{

                     new SqlParameter("@id",incidente.id)//cheie valoare care se trimit la procedura cheia se foloseste pt a identifica unde se gaseste valoarea
                    ,new SqlParameter("@OpenedBy",incidente.Opened_by)
                    ,new SqlParameter("@ElapsedTimeOpen",incidente.Elapsed_Time_Open)
                    ,new SqlParameter("@Opened",incidente.Opened)
                    ,new SqlParameter("@Resolved",incidente.Resolved)
                    ,new SqlParameter("@Number",incidente.Number)
                    ,new SqlParameter("@Shortdescription",incidente.Short_description)
                    ,new SqlParameter("@Priority",incidente.Priority)
                    ,new SqlParameter("@State",incidente.State)
                    ,new SqlParameter("@IncidentState",incidente.Incident_state)
                    ,new SqlParameter("@AssignedTo",incidente.Assigned_to)
                    ,new SqlParameter("@AssignmedGroup",incidente.Assignmed_group)
                    ,new SqlParameter("@Caller",incidente.Caller)
                    ,new SqlParameter("@Closed",incidente.Closed)
                    ,new SqlParameter("@CreatedBy",incidente.Created_by)
                    ,new SqlParameter("@Location",incidente.Location)
                    ,new SqlParameter("@RezolvedBy",incidente.Rezolved_by)
                    ,new SqlParameter("@ClosedBy",incidente.Closed_by)
                    ,new SqlParameter("@Subcategory",incidente.Subcategory)
                    ,new SqlParameter("@Category",incidente.Category)
                    ,new SqlParameter("@uResolveDate",incidente.u_resolve_date)
                    ,new SqlParameter("@MadeSla",incidente.Made_sla)
                    ,new SqlParameter("@ReassignmentCount",incidente.Reassignment_count)
                    ,new SqlParameter("@AffectedCI",incidente.Affected_CI)
                    ,new SqlParameter("@Fixcode",incidente.Fix_code)
                    ,new SqlParameter("@BusinessService",incidente.Business_service)
                    ,new SqlParameter("@SLAdue",incidente.SLA_due)
                    ,new SqlParameter("@ResolveTime",incidente.Resolve_Time)
                    ,new SqlParameter("@uClassification",incidente.u_classification)
                    ,new SqlParameter("@Description",incidente.Description)

                };

                _dbSN.Database.ExecuteSqlCommand(" exec InsertAllDataIncidente @id, @OpenedBy , @ElapsedTimeOpen , @Opened , @Resolved ,@Number, @Shortdescription, @Priority , @State,  @IncidentState , @AssignedTo , @AssignmedGroup , @Caller  , @Closed  , @CreatedBy , @Location ,  @RezolvedBy   , @ClosedBy , @Subcategory   , @Category, @uResolveDate , @MadeSla  , @ReassignmentCount , @AffectedCI ,  @Fixcode ,@BusinessService   ,  @SLAdue  , @ResolveTime, @uClassification , @Description", Parameter);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ex.Message" + ex);

            }


        }
        #endregion