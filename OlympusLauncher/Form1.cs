using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using WinSCP;
using Xdelta;

namespace OlympusLauncher
{
    public partial class Form1 : Form
    {
        private int currentVersion;
        private Statuses status = Statuses.fetchingVersion;

        public List<string> PatchTypes = new List<string>
        {
            "String.xdelta","UI.xdelta","Etc.xdelta","Item.xdelta","Map.xdelta","Effect.xdelta","Quest.xdelta","Skill.xdelta","Morph.xdelta","Sound.xdelta","Mob.xdelta","Npc.xdelta",
            "Base.xdelta","List.xdelta","TamingMob.xdelta","Character.xdelta","Reactor.xdelta"
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           currentVersion = int.Parse(File.ReadAllText(Application.StartupPath + @"\currentVersion.txt"));
           checkConnection();
           int latestVersion = int.Parse(getLatestVersion());

            if (latestVersion > currentVersion)
            {
                Label_Status.Text = "There are updates available.";
                Button_Start.Enabled = true;
                Button_Start.Text = "Update";
                status = Statuses.update;
            }
            else
            {
                Label_Status.Text = "Ready to launch.";
                Button_Start.Enabled = true;
                Button_Start.Text = "Launch";
                status = Statuses.launch;
            }



        }

        private void checkConnection()
        {
                bool exists;
                try
                {
                    //Creating the HttpWebRequest
                    HttpWebRequest request;
                    request = WebRequest.Create("http://149.210.194.144/deltapatch/") as HttpWebRequest;
                    //Setting the Request method HEAD, you can also use GET too.
                    request.Method = "HEAD";
                    //Getting the Web Response.
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    //Returns TURE if the Status code == 200
                    exists = (response.StatusCode == HttpStatusCode.OK);
                    response.Close();
                    request.Abort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Any exception will returns false.
                    exists = false;
                }

            if (!exists)
            {
                MessageBox.Show("Could not connect to server");
                Application.Exit();
            }            
        }

        private string getLatestVersion()
        {
            using (var client = new WebClient())
            {
                var content = client.DownloadData("http://149.210.194.144/deltapatch/latestVersion.txt");
                using (var stream = new MemoryStream(content))
                {
                    TextReader tr = new StreamReader(stream);
                    string latestVersion = tr.ReadLine();
                    return latestVersion;
                }
            }
       
        }

        private void Button_Start_Click(object sender, EventArgs e)
        {
            switch (status)
            {
                case Statuses.update:
                    startUpdate();
                    break;
                case Statuses.launch:
                    launchGame();
                    break;
            }
        }

        private void startUpdate()
        {
            double nextVersion = currentVersion + 1;


            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = "149.210.194.144",
                    UserName = "updater",
                    Password = "D35+MF22rx!!h;E8",
                    SshHostKeyFingerprint = "ssh-rsa 2048 W85AFfondvUPAeX12l1WTfvfmgkKYTrKWUbllWLsiio="
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    string tempDirectory = Application.StartupPath + @"\TempPatches\" + nextVersion;
                    Directory.CreateDirectory(tempDirectory);

                    // Download files
                    string downloadDirectory = "/home/updater/deltapatches/" + nextVersion;
                    session.GetFiles(downloadDirectory, tempDirectory).Check();

                    patch(nextVersion.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e);
            }

        }

        private void patch(string version)
        {
            string tempPatchDirectory = Application.StartupPath + @"\TempPatches\" + version;

            foreach (string patchType in PatchTypes)
            {
                string patchFile = tempPatchDirectory + "\\" + patchType;
                string wzFile = patchType.Replace(".xdelta", ".wz");
                string newFile = "NEW" + wzFile;

                if (File.Exists(patchFile))
                {
                    MessageBox.Show(patchType + " exists!");
                    DeltaPatcher.Patch(wzFile, patchFile, newFile);
                    if (File.Exists(newFile))
                    {
                        File.Delete(wzFile);
                        File.Move(newFile, wzFile);
                    }
                }
            }
        }

        private void launchGame()
        {
            Process.Start(Application.StartupPath + @"\OlympusMS.exe");
        }

  
    }
}
