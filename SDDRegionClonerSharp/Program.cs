using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SDDRegionClonerSharp
    {
    class Program
        {
        static string defaultExampleHTML = "";

        static void Main(string[] args)
            {
            // read default HTML city example

            readExampleHTML();
            updateAllFiles();
            Console.WriteLine("Done");
            Console.Read();
            }

        static void readExampleHTML()
        {
            try
            {

                StreamReader sr = new StreamReader("example/index.html");
                defaultExampleHTML = sr.ReadToEnd();
                Console.WriteLine("Example file read successfully\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Example HTML can't be read");
                System.Environment.Exit(1);
            };

        }
        static void updateAllFiles()
            {
            CitiesConnectionsDict connectionsDict = JsonConvert.DeserializeObject<CitiesConnectionsDict>(File.ReadAllText("dictionary.json"));
            foreach (Connection connection in connectionsDict.connections)
                {
                try { updateFileInFoilder(connection);
                Console.WriteLine("City " + connection.cname + " updated\n"); } catch (Exception e) { Console.WriteLine("Unable to update city " + connection.cname+ "****************************\n"); };
                }
            }


        static void updateFileInFoilder(Connection connection)
            {
            Console.WriteLine("****************************Updating city: " + connection.phones[0].name+"****************************");
            if (!Directory.Exists(connection.fname))
                {
                Directory.CreateDirectory(connection.fname);
                }
            // copy HTML example
            string resultHTML = defaultExampleHTML;
            StreamWriter sw = new StreamWriter(connection.fname + "/index.html");

            try { replaceCity(ref resultHTML, connection); Console.WriteLine("City name updated"); } catch (Exception) { Console.WriteLine("City name update failed"); };
            try { updatePhones(ref resultHTML, connection); Console.WriteLine("Phones updated"); } catch (Exception) { Console.WriteLine("Phones update failed"); };
            try { updateMails(ref resultHTML, connection); Console.WriteLine("Emails updated"); } catch (Exception) { Console.WriteLine("Emails name update failed"); };
            try { updateGmap(ref resultHTML, connection); Console.WriteLine("Gmap updated"); } catch (Exception) { Console.WriteLine("Gmap update failed"); };
            sw.WriteLine(resultHTML);
            sw.Close();
            }


        static void replaceCity(ref string htmltext, Connection connection)
            {
            htmltext = htmltext.Replace("_city_", connection.cname);
            }


        static void updatePhones(ref string htmltext, Connection connection)
            {
            int startIndex = 0;
            int lineStartIndex = htmltext.IndexOf("<a class=\"phone_link\"", startIndex);
            while (lineStartIndex != -1)
            {
                int lineEndIndex = lineStartIndex + htmltext.Substring(lineStartIndex).IndexOf("</a>");
                string updatedPhoneBlock = "";
                if (startIndex == 0)
                {
                    foreach (Phone phone in connection.phones)
                    {
                        updatedPhoneBlock += "<a class=\"phone_link\" href=\"tel:+" + phone.phone.Replace("+", "").Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "") + "\" rel=\"nofollow\">" + phone.phone + " <b>" + phone.name + "</b></a>";
                    }
                }
                else
                {
                    foreach (Phone phone in connection.phones)
                    {
                        updatedPhoneBlock += "<a class=\"phone_link\" href=\"tel:+" + phone.phone.Replace("+", "").Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "") + "\" rel=\"nofollow\">" + phone.phone + "</a>";
                    }
                }
                htmltext = htmltext.Remove(lineStartIndex, lineEndIndex - lineStartIndex + 4).Insert(lineStartIndex, updatedPhoneBlock);
                startIndex = lineStartIndex + updatedPhoneBlock.Length;
                lineStartIndex = htmltext.IndexOf("<a class=\"phone_link\"", startIndex);
            }
            }

        static void updateMails(ref string htmltext, Connection connection)
        {
            int startIndex = 0;
            int lineStartIndex = htmltext.IndexOf("<a class=\"mail_link\"", startIndex);
            while (lineStartIndex != -1)
            {            
                int lineEndIndex = lineStartIndex + htmltext.Substring(lineStartIndex).IndexOf("</a>");
                string updatedMail = "<a class=\"mail_link\" href=\"mailto:" + connection.email.ToString() + "\">" + connection.email.ToString() + "</a>";
                htmltext = htmltext.Remove(lineStartIndex, lineEndIndex - lineStartIndex + 4).Insert(lineStartIndex, updatedMail);
                startIndex = lineStartIndex + updatedMail.Length;
                lineStartIndex = htmltext.IndexOf("<a class=\"mail_link\"", startIndex);
            }

        }

        static void updateGmap(ref string htmltext, Connection connection)
        {
            removeOldGmap(ref htmltext, connection);
            int startIndex = 0;

            int lineStartIndex = htmltext.IndexOf("Пункты выдачи жаропрочных огнеупорных", startIndex);
            lineStartIndex = htmltext.IndexOf("</h5>", lineStartIndex)+5;
            foreach (Gmap gmap in connection.gmap)
            {
                string small_city = "<div class=\"place small\"><span class=\"place_name\">"+gmap.adress+"</span><iframe class=\"place_map\" src=\"" + gmap.src + "\"frameborder=\"0\" style=\"border:0\" allowfullscreen></iframe></div>";
                htmltext = htmltext.Insert(lineStartIndex, small_city);
                lineStartIndex += small_city.Length;
            }
            
        }

        static void removeOldGmap(ref string htmltext, Connection connection)
        {
            int startIndex = 0;
            int lineStartIndex = htmltext.IndexOf("<div class=\"place small\">", startIndex);
            while (lineStartIndex != -1)
            {
                int lineEndIndex = lineStartIndex + htmltext.Substring(lineStartIndex).IndexOf("</div>");
                htmltext = htmltext.Remove(lineStartIndex, lineEndIndex - lineStartIndex + 6);
                lineStartIndex = htmltext.IndexOf("<div class=\"place small\">", startIndex);
            }
            
        }

    }


    public class Phone
        {
        public string name { get; set; }
        public string phone { get; set; }
        }

    public class Gmap
    {
        public string adress { get; set; }
        public string src { get; set; }
    }

    public class Connection
        {
        public string fname { get; set; }
        public string cname { get; set; }
        public string email { get; set; }
        public List<Phone> phones { get; set; }
        public List<Gmap> gmap { get; set; }
    }


    public class CitiesConnectionsDict
        {
        public List<Connection> connections { get; set; }
        }
    }
