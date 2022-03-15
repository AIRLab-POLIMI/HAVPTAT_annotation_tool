using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Diagnostics;
using Xabe.FFmpeg;

namespace VideoLabelTool
{
    public partial class FormFrameCapture : Form
    {        
        float TotalFrame;
        int Fps;
        int currentFrameNum;
        int width;
        int height;
        bool resizeImage = false;
        VideoCapture capture;        
        Timer My_Timer = new Timer();  
        int status = 0;
        OpenFileDialog ofd;
        string openedVideoPath;       
        int widthPictureBox;
        int heightPictureBox;
        int? rotated = null;
        Bitmap bp;
        public List<int> selectedPersonID = new List<int>();
        public int selectedPersonIDUnique;
        public List<int> selectedPersonIndex = new List<int>();
        int selectedPersonIndexUnique;
        Mat m;
        Pen penTemp;

        private static Random rnd = new Random();
        List<FrameObj> listFrames;                
        List<List<Rectangle>> listRec;
        List<List<string>> lineByFrame;
        List<List<string>> listAction;
        List<List<Keypoints>> listKeypoints;
        List<int> listPersonIDAssociated = new List<int>();
        List<PersonColor> listPersonColor;
        List<List<string>> listPredict;

        Font myFont = new Font("Arial", 12, FontStyle.Bold);
        Font serviceDecisionFont = new Font("Arial", 10);        
        Font needServiceFont = new Font("Arial", 14, FontStyle.Underline);
        
        const string message = "You have already labeled this person";
        const string caption = "Warning";

        public string newActionName;

        public class PersonColor
        {
            public int personID { get; set; }
            public Pen pen { get; set; }
        }        
        
        public class Prediction
        {
            public List<float> keypoints { get; set; }
            public List<float> bbox { get; set; }
            public float score { get; set; }
            public int category_id { get; set; }
            public int id_ { get; set; }
            public string action { get; set; }
            public string predict { get; set; }
        }

        public class FrameObj
        {
            public int frame { get; set; }
            public List<Prediction> predictions { get; set; }
        }
        public FormFrameCapture()
        {
            InitializeComponent();
            this.bntNextFrame.Enabled = false;
            this.bntPrevFrame.Enabled = false;
            this.Text = "HAVPTAT";
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            pictureBox1.Paint += new PaintEventHandler(this.plotROI);
            this.WindowState = FormWindowState.Maximized;            
            
            widthPictureBox = pictureBox1.Width;
            heightPictureBox = pictureBox1.Height;

            pictureBox1.Width = 1280;
            pictureBox1.Height = 720;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            openVideo();
        }

        /*  COCO Person Keypoints mapping
         *  "categories": [
            {
                "supercategory": "person",
                "id": 1,
                "name": "person",
                "keypoints": [
                    "nose","left_eye","right_eye","left_ear","right_ear",
                    "left_shoulder","right_shoulder","left_elbow","right_elbow",
                    "left_wrist","right_wrist","left_hip","right_hip",
                    "left_knee","right_knee","left_ankle","right_ankle"
                ],
                "skeleton": [
                    [16,14],[14,12],[17,15],[15,13],[12,13],[6,12],[7,13],[6,7],
                    [6,8],[7,9],[8,10],[9,11],[2,3],[1,2],[1,3],[2,4],[3,5],[4,6],[5,7]
                ]
                 "skeleton_index": [
                    [15,13],[13,11],[16,14],[14,12],[11,12],[5,11],[6,12],[5,6],
                    [5,7],[6,8],[7,9],[8,10],[1,2],[0,1],[0,2],[1,3],[2,4],[3,5],[4,76]
                ]
        
            }
        ]*/


        private void drawPose(PaintEventArgs e, Pen pen, List<FrameObj> listFrames, int frameNum, int personNum, int pointA, int pointB)
        {                    
            if (listKeypoints[listFrames[frameNum].frame - 1][personNum].pose[pointA].visibility != 0 && listKeypoints[listFrames[frameNum].frame - 1][personNum].pose[pointB].visibility != 0)
                e.Graphics.DrawLine(pen, new PointF(listKeypoints[listFrames[frameNum].frame - 1][personNum].pose[pointA].x, listKeypoints[listFrames[frameNum].frame - 1][personNum].pose[pointA].y), new PointF(listKeypoints[listFrames[frameNum].frame - 1][personNum].pose[pointB].x, listKeypoints[listFrames[frameNum].frame - 1][personNum].pose[pointB].y));
        }

        private void plotPose(PaintEventArgs e, Pen myPen, List<FrameObj> listFrames, int currentFrameNum, Rectangle ret)
        {
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 15, 13);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 16, 14);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 14, 12);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 13, 11);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 11, 12);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 5, 11);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 6, 12);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 5, 6);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 5, 7);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 6, 8);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 7, 9);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 8, 10);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 1, 2);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 0, 1);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 0, 2);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 1, 3);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 2, 4);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 3, 5);
            drawPose(e, myPen, listFrames, currentFrameNum, listRec[currentFrameNum].IndexOf(ret), 4, 6);
        }

        private void plotROI(object sender, PaintEventArgs e)
        {
            if (listRec != null)
            {
                string word;
                int currentPersonID;
                Pen myPen;
                string predictRes, decision = "";
                int decisionNum = -1;

                if (listRec != null && currentFrameNum < TotalFrame - 1)
                {
                    foreach (Rectangle ret in listRec[currentFrameNum])
                    {
                        var a = (from n in listPersonColor                                     
                                where n.personID == listFrames[currentFrameNum].predictions[listRec[currentFrameNum].IndexOf(ret)].id_
                                select n).FirstOrDefault();
                        myPen = a.pen;

                        e.Graphics.DrawRectangle(a.pen, ret);
                        currentPersonID = listFrames[currentFrameNum].predictions[listRec[currentFrameNum].IndexOf(ret)].id_;
                        word = currentPersonID.ToString();                            
                        word += listAction[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)];
                        word += '\n';
                        word += listPredict[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)];

                        word += '\n';

                        if (listAction[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)] != null 
                            && listPredict[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)] != null)
                        {
                            // Only if GT == Predict, decide whether to give service
                            if (listAction[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)] == listPredict[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)])
                            {
                                e.Graphics.DrawString(word, myFont, Brushes.LimeGreen, new Point(ret.X, ret.Y));
                                
                                predictRes = listPredict[currentFrameNum][listRec[currentFrameNum].IndexOf(ret)];
                                switch (predictRes)
                                {
                                    case "sitting":
                                    case "sittingWhileHoldingBabyInArms":
                                    case "standing":
                                    case "standingWhileHoldingBabyInArms":
                                    case "standingWhileHoldingCart":
                                    case "standingWhileHoldingStroller":
                                    case "standingWhileLookingAtShops":
                                    case "walkingWhileHoldingBabyInArms":
                                    case "walkingWhileHoldingCart":
                                    case "walkingWhileHoldingStroller":
                                    case "walkingWhileLookingAtShops":
                                        decision = "\n\n\nNEED SERVICE";
                                        decisionNum = 2;
                                        break;

                                    case "crouching":
                                    case "laying":
                                    case "riding":
                                    case "walking":
                                    case "walkingTogether":
                                        decision = "\n\n\nMAYBE NOT NEED SERVICE";
                                        decisionNum = 1;
                                        break;

                                    case "cleaning":
                                    case "jumping":
                                    case "running":
                                    case "scooter":
                                    case "sittingTogether":
                                    case "sittingWhileCalling":
                                    case "sittingWhileDrinking":
                                    case "sittingWhileEating":
                                    case "sittingWhileTalkingTogether":
                                    case "sittingWhileWatchingPhone":
                                    case "standingTogether":
                                    case "standingWhileCalling":
                                    case "standingWhileDrinking":
                                    case "standingWhileEating":
                                    case "standingWhileTalkingTogether":
                                    case "standingWhileWatchingPhone":
                                    case "walkingWhileCalling":
                                    case "walkingWhileDrinking":
                                    case "walkingWhileEating":
                                    case "walkingWhileTalkingTogether":
                                    case "walkingWhileWatchingPhone":
                                        decision = "\n\n\nNOT DISTURB";
                                        decisionNum = 0;
                                        break;
                                }
                                if (decisionNum == 2)
                                    e.Graphics.DrawString(decision, needServiceFont, Brushes.LightCyan, new Point(ret.X, ret.Y));
                                else if (decisionNum == 1)
                                    e.Graphics.DrawString(decision, serviceDecisionFont,Brushes.Cyan, new Point(ret.X, ret.Y));
                                else if (decisionNum == 0)
                                    e.Graphics.DrawString(decision, serviceDecisionFont, Brushes.White, new Point(ret.X, ret.Y));
                            }

                            // GT != Predict result
                            else
                            {
                                e.Graphics.DrawString(word, myFont, Brushes.Red, new Point(ret.X, ret.Y));
                            }
                        }                       
                        // don't have a prediction result                        
                        else
                        {
                            e.Graphics.DrawString(word, myFont, Brushes.Yellow, new Point(ret.X, ret.Y));
                        }                        

                        // Hide/Show Complete Human Pose
                        if (checkBoxShowPose.Checked == true)
                        plotPose(e, myPen, listFrames, currentFrameNum, ret);                        
                    }
                }
                if (listRec != null && currentFrameNum == TotalFrame)
                {
                    foreach (Rectangle ret in listRec[currentFrameNum - 1])
                    {
                        var a = (from n in listPersonColor
                                 where n.personID == listFrames[currentFrameNum - 1].predictions[listRec[currentFrameNum - 1].IndexOf(ret)].id_
                                 select n).FirstOrDefault();
                        myPen = a.pen;

                        e.Graphics.DrawRectangle(a.pen, ret);
                        currentPersonID = listFrames[currentFrameNum - 1].predictions[listRec[currentFrameNum - 1].IndexOf(ret)].id_;
                        word = currentPersonID.ToString();
                        word = currentPersonID.ToString();
                        word += listAction[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)];
                        word += '\n';
                        word += listPredict[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)];

                        word += '\n';
                       
                        if (listAction[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)] != null
                            && listPredict[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)] != null)
                        {
                            // Only if GT == Predict, decide whether to give service
                            if (listAction[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)] == listPredict[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)])
                            {
                                e.Graphics.DrawString(word, myFont, Brushes.LimeGreen, new Point(ret.X, ret.Y));

                                predictRes = listPredict[currentFrameNum - 1][listRec[currentFrameNum - 1].IndexOf(ret)];
                                switch (predictRes)
                                {
                                    case "sitting":
                                    case "sittingWhileHoldingBabyInArms":
                                    case "standing":
                                    case "standingWhileHoldingBabyInArms":
                                    case "standingWhileHoldingCart":
                                    case "standingWhileHoldingStroller":
                                    case "standingWhileLookingAtShops":
                                    case "walkingWhileHoldingBabyInArms":
                                    case "walkingWhileHoldingCart":
                                    case "walkingWhileHoldingStroller":
                                    case "walkingWhileLookingAtShops":
                                        decision = "\n\n\nNEED SERVICE";
                                        decisionNum = 2;
                                        break;

                                    case "crouching":
                                    case "laying":
                                    case "riding":
                                    case "walking":
                                    case "walkingTogether":
                                        decision = "\n\n\nMAYBE NOT NEED SERVICE";
                                        decisionNum = 1;
                                        break;

                                    case "cleaning":
                                    case "jumping":
                                    case "running":
                                    case "scooter":
                                    case "sittingTogether":
                                    case "sittingWhileCalling":
                                    case "sittingWhileDrinking":
                                    case "sittingWhileEating":
                                    case "sittingWhileTalkingTogether":
                                    case "sittingWhileWatchingPhone":
                                    case "standingTogether":
                                    case "standingWhileCalling":
                                    case "standingWhileDrinking":
                                    case "standingWhileEating":
                                    case "standingWhileTalkingTogether":
                                    case "standingWhileWatchingPhone":
                                    case "walkingWhileCalling":
                                    case "walkingWhileDrinking":
                                    case "walkingWhileEating":
                                    case "walkingWhileTalkingTogether":
                                    case "walkingWhileWatchingPhone":
                                        decision = "\n\n\nNOT DISTURB";
                                        decisionNum = 0;
                                        break;
                                }
                                if (decisionNum == 2)
                                    e.Graphics.DrawString(decision, needServiceFont, Brushes.LightCyan, new Point(ret.X, ret.Y));
                                else if (decisionNum == 1)
                                    e.Graphics.DrawString(decision, serviceDecisionFont, Brushes.Cyan, new Point(ret.X, ret.Y));
                                else if (decisionNum == 0)
                                    e.Graphics.DrawString(decision, serviceDecisionFont, Brushes.White, new Point(ret.X, ret.Y));
                            }

                            // GT != Predict result
                            else
                            {
                                e.Graphics.DrawString(word, myFont, Brushes.Red, new Point(ret.X, ret.Y));
                            }
                        }
                        // don't have a prediction result                        
                        else
                        {
                            e.Graphics.DrawString(word, myFont, Brushes.Yellow, new Point(ret.X, ret.Y));
                        }

                        // Hide/Show Complete Human Pose
                        if (checkBoxShowPose.Checked == true)                            
                            plotPose(e, myPen, listFrames, currentFrameNum - 1, ret);
                    }
                }
            }
        }        

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Right))
            {
                NextFrame();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Left))
            {
                PreviousFrame();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Space))
            {
                if (status == 0)
                    Play();
                else
                    Pause();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void openVideo()
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "MP4 files|*.mp4|AVI files|*.avi|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                openedVideoPath = ofd.FileName;

                var FFmpegpath = "C:/ffmpeg/bin";
                FFmpeg.SetExecutablesPath(FFmpegpath, ffmpegExeutableName: "FFmpeg");
                IMediaInfo mInfo = await FFmpeg.GetMediaInfo(openedVideoPath);
                rotated = mInfo.VideoStreams.FirstOrDefault().Rotation;

                capture = new VideoCapture(openedVideoPath);
                m = new Mat();
                capture.Read(m);
                Bitmap bp = m.ToBitmap();
                if (rotated != null && rotated == 180)
                    bp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                pictureBox1.Image = bp;

                TotalFrame = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                Fps = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                width = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
                height = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
                if (width != 1280 && height != 720)
                    resizeImage = true;
                My_Timer.Interval = 1000 / Fps;
                My_Timer.Tick += new EventHandler(My_Timer_Tick);
                counterFrame.Text = (currentFrameNum).ToString() + '/' + (TotalFrame - 1).ToString();
                videoFileName.Text = ofd.SafeFileName;

                this.bntNextFrame.Enabled = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openVideo();
        }                
        
        private void bntPlay_Click(object sender, EventArgs e)
        {
            Play();
        }

        private void Play()
        {
            if (capture == null || currentFrameNum == TotalFrame - 1)
            {
                return;
            }            
            
            My_Timer.Start();
            this.bntPrevFrame.Enabled = true;
            status = 1;            
        }        

        private void My_Timer_Tick(object sender, EventArgs e)
        {
            if (currentFrameNum < TotalFrame)
            {
                // SetCaptureProperty could slow down, but avoid crash
                counterFrame.Text = (currentFrameNum).ToString() + '/' + (TotalFrame - 1).ToString();
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, currentFrameNum);
                m = new Mat();
                                
                capture.Read(m);
                if (m.GetData() != null) 
                { 
                    bp = m.ToBitmap();
                    if (rotated != null && rotated == 180)
                        bp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    pictureBox1.Image = bp;
                }
                currentFrameNum += 1;                
            }

            else
            {
                My_Timer.Stop();             
                status = 0;                
            }
                        
        }
        
        private void bntNextFrame_Click(object sender, EventArgs e)
        {
            NextFrame();              
        }
       

        private void NextFrame()
        {
            if (currentFrameNum < TotalFrame - 1 && capture != null)
            {
                // SetCaptureProperty could slow down, but avoid crash
                currentFrameNum += 1;
                setFrame(currentFrameNum);

                this.Invalidate();      
            }

            else
            {             
                return;
            }

            this.bntPrevFrame.Enabled = true;
            status = 0;

            if (currentFrameNum > this.nudStart.Value)
                this.nudEnd.Value = currentFrameNum;
            else
            {
                this.nudStart.Value = currentFrameNum;
                this.nudEnd.Value = currentFrameNum + 1;         
            }
        }

        private void bntPrevFrame_Click(object sender, EventArgs e)
        {
            PreviousFrame();
        }

        private void PreviousFrame()
        {
            if (currentFrameNum > 0 && currentFrameNum <= TotalFrame && capture != null)
            {                
                currentFrameNum -= 1;
                setFrame(currentFrameNum);
            }            
            status = 0;

            if (currentFrameNum > this.nudStart.Value)
                this.nudEnd.Value = currentFrameNum;
            else
            {
                this.nudStart.Value = currentFrameNum;
                this.nudEnd.Value = currentFrameNum + 1;
            }
        }

        private void buttonFirstFrame_Click(object sender, EventArgs e)
        {
            setFrame(0);
            currentFrameNum = 0;
        }

        private void buttonLastFrame_Click(object sender, EventArgs e)
        {
            setFrame((int)TotalFrame - 1);
            currentFrameNum = (int)TotalFrame - 1;
        }

        private void setFrame(int currentFrameNum)
        {
            // SetCaptureProperty could slow down, but avoid crash
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, currentFrameNum);
            try
            {
                //To avoid CRASH: Remove capture.QueryFrame()
                //pictureBox1.Image = capture.QueryFrame().ToBitmap();

                // Replaced by capture.Read(m)
                m = new Mat();
                capture.Read(m);
                if (m.GetData() != null)
                {
                    bp = m.ToBitmap();
                    if (rotated != null && rotated == 180)
                        bp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    pictureBox1.Image = bp;
                }
            }
            catch (NullReferenceException e)
            {
                throw new NullReferenceException(e.Message);
            }

            counterFrame.Text = (currentFrameNum).ToString() + '/' + (TotalFrame - 1).ToString();
        }

        private void bntPause_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void Pause()
        {            
            My_Timer.Stop();
            status = 0;
        }                
        
        private List<FrameObj> addActionFieldToJson(string jsonFile)
        {
            List<dynamic> listFramesFormatted = new List<dynamic>();

            var jsonSerializer = new JsonSerializer();
            var jsonReaderExport = new JsonTextReader(new StringReader(jsonFile))
            {
                SupportMultipleContent = true // This is important for multiple content JSON file reading!
            };
            while (jsonReaderExport.Read())
            {
                listFramesFormatted.Add(jsonSerializer.Deserialize<JObject>(jsonReaderExport));
            }            

            string jsonData = JsonConvert.SerializeObject(listFramesFormatted);
            List<FrameObj> framesAct = JsonConvert.DeserializeObject<List<FrameObj>>(jsonData);

            return framesAct;
        }

        private float getKeyPoint(List<FrameObj> listFrames, int i, int j, int index)
        {
            if (resizeImage == true)
            {
                if (resizeImage == true && index % 3 == 0 && rotated != null)                                       
                    return float.Parse(listFrames[i].predictions[j].keypoints[index].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                else
                    return listFrames[i].predictions[j].keypoints[index] * 2 / 3;
            }
            else
            {
                return listFrames[i].predictions[j].keypoints[index];
            }
        }

        private void processJson(string json, bool semilabeled, int x, int y, int height, int weight, int nrFrame)
        {            
            if (json[0] == '[' && json[json.Length - 1] == ']')
            {
                json = json.Substring(1, json.Length - 2);
                semilabeled = true;
            }
            listFrames = addActionFieldToJson(json);

            for (int i = 0; i < listFrames.Count; i++)
            {
                listRec.Add(new List<Rectangle>());
                listAction.Add(new List<string>());
                listKeypoints.Add(new List<Keypoints>());
                listPredict.Add(new List<string>());

                for (int j = 0; j < listFrames[i].predictions.Count; j++)
                {
                    if (resizeImage == false && rotated == null)
                    {
                        x = (int)listFrames[i].predictions[j].bbox[0];
                        y = (int)listFrames[i].predictions[j].bbox[1];
                        weight = (int)listFrames[i].predictions[j].bbox[2];
                        height = (int)listFrames[i].predictions[j].bbox[3];
                    }
                    else if (resizeImage == true && rotated != null)
                    {
                        //New version
                        // Different OS has different personalized Setting for number format, this parameter to use uniform number format                            
                        /*To get symmetric value of axis X and For some strange motivation*/
                        x = (int)double.Parse(listFrames[i].predictions[j].bbox[0].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                        y = (int)double.Parse(listFrames[i].predictions[j].bbox[1].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                        weight = (int)double.Parse(listFrames[i].predictions[j].bbox[2].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                        height = (int)double.Parse(listFrames[i].predictions[j].bbox[3].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                    }
                    else
                    {
                        //New version
                        // Different OS has different personalized Setting for number format, this parameter to use uniform number format                            
                        /*To get symmetric value of axis X and For some strange motivation*/
                        x = (int)double.Parse(listFrames[i].predictions[j].bbox[0].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                        y = (int)double.Parse(listFrames[i].predictions[j].bbox[1].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                        weight = (int)double.Parse(listFrames[i].predictions[j].bbox[2].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                        height = (int)double.Parse(listFrames[i].predictions[j].bbox[3].ToString(), CultureInfo.InvariantCulture) * 2 / 3;
                    }

                    listRec[listFrames[i].frame - 1].Add(new Rectangle(x, y, weight, height));

                    listKeypoints[listFrames[i].frame - 1].Add(new Keypoints(getKeyPoint(listFrames, i, j, 0), getKeyPoint(listFrames, i, j, 1), getKeyPoint(listFrames, i, j, 2),
                                                          getKeyPoint(listFrames, i, j, 3), getKeyPoint(listFrames, i, j, 4), getKeyPoint(listFrames, i, j, 5),
                                                          getKeyPoint(listFrames, i, j, 6), getKeyPoint(listFrames, i, j, 7), getKeyPoint(listFrames, i, j, 8),
                                                          getKeyPoint(listFrames, i, j, 9), getKeyPoint(listFrames, i, j, 10), getKeyPoint(listFrames, i, j, 11),
                                                          getKeyPoint(listFrames, i, j, 12), getKeyPoint(listFrames, i, j, 13), getKeyPoint(listFrames, i, j, 14),
                                                          getKeyPoint(listFrames, i, j, 15), getKeyPoint(listFrames, i, j, 16), getKeyPoint(listFrames, i, j, 17),
                                                          getKeyPoint(listFrames, i, j, 18), getKeyPoint(listFrames, i, j, 19), getKeyPoint(listFrames, i, j, 20),
                                                          getKeyPoint(listFrames, i, j, 21), getKeyPoint(listFrames, i, j, 22), getKeyPoint(listFrames, i, j, 23),
                                                          getKeyPoint(listFrames, i, j, 24), getKeyPoint(listFrames, i, j, 25), getKeyPoint(listFrames, i, j, 26),
                                                          getKeyPoint(listFrames, i, j, 27), getKeyPoint(listFrames, i, j, 28), getKeyPoint(listFrames, i, j, 29),
                                                          getKeyPoint(listFrames, i, j, 30), getKeyPoint(listFrames, i, j, 31), getKeyPoint(listFrames, i, j, 32),
                                                          getKeyPoint(listFrames, i, j, 33), getKeyPoint(listFrames, i, j, 34), getKeyPoint(listFrames, i, j, 35),
                                                          getKeyPoint(listFrames, i, j, 36), getKeyPoint(listFrames, i, j, 37), getKeyPoint(listFrames, i, j, 38),
                                                          getKeyPoint(listFrames, i, j, 39), getKeyPoint(listFrames, i, j, 40), getKeyPoint(listFrames, i, j, 41),
                                                          getKeyPoint(listFrames, i, j, 42), getKeyPoint(listFrames, i, j, 43), getKeyPoint(listFrames, i, j, 44),
                                                          getKeyPoint(listFrames, i, j, 45), getKeyPoint(listFrames, i, j, 46), getKeyPoint(listFrames, i, j, 47),
                                                          getKeyPoint(listFrames, i, j, 48), getKeyPoint(listFrames, i, j, 49), getKeyPoint(listFrames, i, j, 50)
                                                          ));

                    // Add new pen/color for plotting bounding box to new appeared person. Each person has only a color for all the frames
                    if (!listPersonColor.Any(a => a.personID == listFrames[i].predictions[j].id_))
                    {
                        penTemp = new Pen(Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
                        penTemp.Width = 3.0F;
                        listPersonColor.Add(new PersonColor { personID = listFrames[i].predictions[j].id_, pen = penTemp });
                    }
                    if (semilabeled == true)
                    {
                        listAction[nrFrame - 1].Add(listFrames[i].predictions[j].action);
                        listPredict[nrFrame - 1].Add(listFrames[i].predictions[j].predict);
                    }
                    else
                    {
                        listAction[nrFrame - 1].Add(null);
                        listPredict[nrFrame - 1].Add(null);
                    }
                }
                nrFrame++;
            }
        }

        private void bntLoadLabels_Click(object sender, EventArgs e)
        {            
            ofd = new OpenFileDialog();
            ofd.Filter = "JSON files|*.json|TXT files|*.txt|All files|*";

            int nrFrame = 1;
            lineByFrame = new List<List<string>>();
            lineByFrame.Add(new List<string>());
            listRec = new List<List<Rectangle>>();            
            listAction = new List<List<String>>();
            listPredict = new List<List<String>>();
            listPersonColor = new List<PersonColor>();
            listKeypoints = new List<List<Keypoints>>();
            int x = -1;
            int y = -1;
            int weight = -1;
            int height = -1;
            bool semilabeled = false;
            string searchJsonPath = Path.GetDirectoryName(Path.GetDirectoryName(openedVideoPath)) + "\\json_input\\" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";
            string json;
            
            if (File.Exists(searchJsonPath))
            {
                json = File.ReadAllText(searchJsonPath);
                annotationFileName.Text = Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";
                processJson(json, semilabeled, x, y, height, weight, nrFrame);
            }

            else 
            {
                MessageBox.Show("Couldn't automatically find the right JSON file:\n " + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json \n \n Please select it manually.", "Warning");
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    json = File.ReadAllText(ofd.FileName);
                    annotationFileName.Text = ofd.SafeFileName;
                    processJson(json, semilabeled, x, y, height, weight, nrFrame);
                }
            }
        }

        private void buttonEva_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "JSON files|*.json|TXT files|*.txt|All files|*";

            int nrFrame = 1;
            lineByFrame = new List<List<string>>();
            lineByFrame.Add(new List<string>());
            listRec = new List<List<Rectangle>>();
            listAction = new List<List<String>>();
            listPersonColor = new List<PersonColor>();
            listKeypoints = new List<List<Keypoints>>();
            listPredict = new List<List<String>>();
            int x = -1;
            int y = -1;
            int weight = -1;
            int height = -1;
            bool semilabeled = false;
            string searchJsonPath = Path.GetDirectoryName(Path.GetDirectoryName(openedVideoPath)) + "\\json_output\\action_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";
            string json;            

            if (File.Exists(searchJsonPath))
            {
                json = File.ReadAllText(searchJsonPath);
                annotationFileName.Text = "action_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";
                processJson(json, semilabeled, x, y, height, weight, nrFrame);
            }

            else
            {
                MessageBox.Show("Couldn't automatically find the right JSON file:\n action_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json \n \n Please select it manually.", "Warning");
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    json = File.ReadAllText(ofd.FileName);
                    annotationFileName.Text = ofd.SafeFileName;
                    processJson(json, semilabeled, x, y, height, weight, nrFrame);
                }
            }            
        }

        private void buttonPredict_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "JSON files|*.json|TXT files|*.txt|All files|*";

            int nrFrame = 1;
            lineByFrame = new List<List<string>>();
            lineByFrame.Add(new List<string>());
            listRec = new List<List<Rectangle>>();
            listAction = new List<List<String>>();
            listPersonColor = new List<PersonColor>();
            listKeypoints = new List<List<Keypoints>>();
            listPredict = new List<List<String>>();
            int x = -1;
            int y = -1;
            int weight = -1;
            int height = -1;
            bool semilabeled = false;
            string searchJsonPath = Path.GetDirectoryName(Path.GetDirectoryName(openedVideoPath)) + "\\json_output\\pred_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";
            string json;

            if (File.Exists(searchJsonPath))
            {
                json = File.ReadAllText(searchJsonPath);
                annotationFileName.Text = "pred_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";
                processJson(json, semilabeled, x, y, height, weight, nrFrame);
            }

            else
            {
                MessageBox.Show("Couldn't automatically find the right JSON file:\n pred_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json \n \n Please select it manually.", "Warning");
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    json = File.ReadAllText(ofd.FileName);
                    annotationFileName.Text = ofd.SafeFileName;
                    processJson(json, semilabeled, x, y, height, weight, nrFrame);
                }
            }
        }

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            if (currentFrameNum < TotalFrame && listRec != null)
            {
                if (listRec.Count != 0)
                {
                    foreach (Rectangle r in listRec[currentFrameNum])
                        if (r.Contains(e.Location) && !selectedPersonIndex.Any(idx => idx == listRec[currentFrameNum].IndexOf(r)))
                        {
                            // enter only if the index does not exist in selectedPersonIndex to ensure no duplicated value is inserted
                            selectedPersonIndex.Add(listRec[currentFrameNum].IndexOf(r));

                        }
                    foreach (int spi in selectedPersonIndex)
                    {
                        // Through "selectedPersonIndex" list to get "selectedPersonID" list                
                        if (!selectedPersonID.Any(idx => idx == listFrames[currentFrameNum].predictions[spi].id_))
                        {
                            selectedPersonID.Add(listFrames[currentFrameNum].predictions[spi].id_);
                            //Console.WriteLine("You have hit Rectangle Person ID.: " + selectedPersonID[selectedPersonID.Count - 1]);
                        }
                    }
                }
            }
            if (currentFrameNum == TotalFrame && listRec != null)
            {
                if (listRec.Count != 0)
                {
                    foreach (Rectangle r in listRec[currentFrameNum - 1])
                        if (r.Contains(e.Location) && !selectedPersonIndex.Any(idx => idx == listRec[currentFrameNum - 1].IndexOf(r)))
                        {
                            // enter only if the index does not exist in selectedPersonIndex to ensure no duplicated value is inserted
                            selectedPersonIndex.Add(listRec[currentFrameNum - 1].IndexOf(r));

                        }
                    foreach (int spi in selectedPersonIndex)
                    {
                        // Through "selectedPersonIndex" list to get "selectedPersonID" list                
                        if (!selectedPersonID.Any(idx => idx == listFrames[currentFrameNum - 1].predictions[spi].id_))
                        {
                            selectedPersonID.Add(listFrames[currentFrameNum - 1].predictions[spi].id_);                            
                        }
                    }
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (selectedPersonID.Count > 1)
            {
                FormSelection formPopup = new FormSelection(this);
                formPopup.ShowDialog(this);
                selectedPersonIndexUnique = listFrames[currentFrameNum].predictions.FindIndex(a => a.id_ == selectedPersonIDUnique);
            }
            else if (selectedPersonID.Count == 0)
            {
                MessageBox.Show("You should select a person to be deleted.", "Warning");
                return;
            }
            else
            {
                selectedPersonIDUnique = selectedPersonID[0];
                selectedPersonIndexUnique = selectedPersonIndex[0];
            }            

            DialogResult dialogResult = MessageBox.Show("Confirm to delete label of person " + selectedPersonIDUnique.ToString(), "Warning", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                for (int i = 0; i < listFrames.Count; i++)
                {
                    for (int j = 0; j < listFrames[i].predictions.Count; j++)
                    {
                        if (listFrames[i].predictions[j].id_ == selectedPersonIDUnique)
                        {
                            listAction[i][j] = null;
                        }
                    }
                }
                listPersonIDAssociated.Remove(selectedPersonIDUnique);
            }                        
        }

        private void actionAssociate(string actionLabel)
        {
            if (selectedPersonID.Count > 1)
            {
                FormSelection formPopup = new FormSelection(this);
                formPopup.ShowDialog(this);                
                selectedPersonIndexUnique = listFrames[currentFrameNum].predictions.FindIndex(a => a.id_ == selectedPersonIDUnique);
            }
            else if (selectedPersonID.Count == 0)
            {
                MessageBox.Show("You should select a person to be labeled.", "Warning");
                return;
            }            
            else
            {
                selectedPersonIDUnique = selectedPersonID[0];
                selectedPersonIndexUnique = selectedPersonIndex[0];
            }

            if (!listPersonIDAssociated.Contains(selectedPersonIDUnique))
            {
                if (this.cbInter.Checked == false)
                {                    
                    for (int i = 0; i < listFrames.Count; i++)
                    {
                        for (int j = 0; j < listFrames[i].predictions.Count; j++)
                        {                            
                            if (listFrames[i].predictions[j].id_ == selectedPersonIDUnique)
                            {
                                listAction[i][j] = actionLabel;
                            }
                        }                
                    }
                    listPersonIDAssociated.Add(selectedPersonIDUnique);
                }

                else
                {
                    for (int i = (int) nudStart.Value; i <= (int)nudEnd.Value; i++)
                    {                        
                        for (int j = 0; j < listFrames[i].predictions.Count; j++)
                        {                     
                            if (listFrames[i].predictions[j].id_ == selectedPersonIDUnique)
                            {
                                listAction[i][j] = actionLabel;
                            }
                        }
                    }                    
                }                
            }
            else
            {
                if (this.cbInter.Checked == false)
                {
                    listAction[currentFrameNum][selectedPersonIndexUnique] = actionLabel;
                }
                else
                {
                    for (int i = (int)nudStart.Value; i <= (int)nudEnd.Value; i++)
                    {                        
                        for (int j = 0; j < listFrames[i].predictions.Count; j++)
                        {                            
                            if (listFrames[i].predictions[j].id_ == selectedPersonIDUnique)
                            {
                                listAction[i][j] = actionLabel;
                            }
                        }
                    }
                }
            }

            selectedPersonID.Clear();
            selectedPersonIndex.Clear();
        }        

        private void bntExport_Click(object sender, EventArgs e)
        {                 
            string exportJsonPath = Path.GetDirectoryName(Path.GetDirectoryName(openedVideoPath)) + "\\json_output\\action_" + Path.GetFileNameWithoutExtension(openedVideoPath) + ".json";            
            Directory.CreateDirectory(Directory.GetParent(Path.GetDirectoryName(openedVideoPath)) + "\\json_output");
            using (StreamWriter sw = new StreamWriter(exportJsonPath, false))
            {
                for (int i = 0; i < listFrames.Count; i++)
                {
                    for (int j = 0; j < listFrames[i].predictions.Count; j++)
                    {
                        if (listAction[i][j] != null)
                        {
                            listFrames[i].predictions[j].action = listAction[i][j];
                        }
                    }
                }

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                };
                settings.Converters.Add(new MyConverter());
                sw.Write(JsonConvert.SerializeObject(listFrames, settings));

                MessageBox.Show("The labeled anntotaion is exported successfully!", "Export");
            }         
        }

        private void bntWalking_Click(object sender, EventArgs e)
        {
            actionAssociate("walking");
        }        

        private void bntDrinking_Click(object sender, EventArgs e)
        {
            actionAssociate("drinking");
        }        

        private void bntStanding_Click(object sender, EventArgs e)
        {
            actionAssociate("standing");
        }

        private void buttonSitting_Click(object sender, EventArgs e)
        {
            actionAssociate("sitting");
        }

        private void cbInter_CheckedChanged(object sender, EventArgs e)
        {
            if (cbInter.Checked == true)
            {
                this.nudStart.Enabled = true;
                this.nudEnd.Enabled = true;
                this.nudStart.Show();
                this.nudEnd.Show();
                this.labelTo.Show();
                this.nudStart.Value = currentFrameNum;
                this.nudEnd.Value = currentFrameNum + 1;
            }
            else
            {
                this.nudStart.Enabled = false;
                this.nudEnd.Enabled = false;
                this.labelTo.Hide();
                this.nudStart.Hide();
                this.nudEnd.Hide();                
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure to RESET?",
                                     "Warning",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {                
                Application.Restart();
                //Environment.Exit(0);
            }                        
        }

        private void walkingWhileCallingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileCalling");
        }

        private void walkingWhileDrinkingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileDrinking");
        }

        private void walkingWhileEatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileEating");
        }

        private void walkingWhileHoldingBabyInArmsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileHoldingBabyInArms");
        }

        private void walkingWhileHoldingCartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileHoldingCart");
        }

        private void walkingWhileHoldingStrollerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileHoldingStroller");
        }

        private void walkingWhileLookingAtShopsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileLookingAtShops");
        }

        private void walkingWhileLookingAtShowcaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileLookingAtShowcase");
        }

        private void walkingWhileSmokingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileSmoking");
        }
        private void walkingWhileTalkingTogetherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileTalkingTogether");
        }

        private void walkingWhileTalkingWithPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileTalkingWithPhone");
        }

        private void standingTogetherWhileLookingAtShopsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingTogetherWhileLookingAtShops");
        }
        

        private void standingTogetherWhileWatchingPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingTogetherWhileWatchingPhone");
        }

        private void standingWhileCallingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileCalling");
        }

        private void standingWhileDrinkingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileDrinking");
        }

        private void standingWhileEatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileEating");
        }

        private void standingWhileHoldingBabyInArmsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileHoldingBabyInArms");
        }

        private void standingWhileLookingAtShopsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileLookingAtShops");
        }
        private void standingWhileLookingAtShowcaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileLookingAtShowcase");
        }

        private void standingWhileHoldingCartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileHoldingCart");
        }

        private void standingWhileHoldingStrollerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileHoldingStroller");
        }
        private void standingWhileSmokingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileSmoking");
        }

        private void standingWhileTalkingTogetherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileTalkingTogether");
        }

        private void standingWhileTalkingWithPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileTalkingWithPhone");
        }

        private void standingWhileWatchingPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileWatchingPhone");
        }

        private void standingWhileWatchingPhoneTogetherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileWatchingPhoneTogether");
        }

        private void sittingWhileCallingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileCalling");
        }

        private void sittingWhileDrinkingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileDrinking");
        }

        private void sittingWhileEatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileEating");
        }

        private void sittingWhileHoldingBabyInArmsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileHoldingBabyInArms");
        }

        private void sittingWhileTalkingTogetherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileTalkingTogether");
        }

        private void sittingWhileTalkingWithPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileTalkingWithPhone");
        }

        private void sittingWhileWatchingPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileWatchingPhone");
        }

        private void sittingWhileWatchingPhoneTogetherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileWatchingPhoneTogether");
        }

        private void cleaningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("cleaning");
        }

        private void crouchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("crouching");
        }

        private void crouchingWhileEatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("crouchingWhileEating");
        }

        private void crouchingWhileWatchingPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("crouchingWhileWatchingPhone");
        }

        private void fallingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("fallingDown");
        }

        private void fightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("fighting");
        }

        private void jumpingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("jumping");
        }

        private void kickingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("kicking");
        }

        private void layingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("laying");
        }

        private void ridingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("riding");
        }

        private void runningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("running");
        }

        private void scooterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("scooter");
        }

        private void throwingTrashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("throwingTrash");
        }

        private void throwingSomethingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            actionAssociate("throwingSomething");
        }
        private void buttonSittingWhileWatchingPhone_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingWhileWatchingPhone");
        }

        private void buttonStandingWhileWatchingPhone_Click(object sender, EventArgs e)
        {
            actionAssociate("standingWhileWatchingPhone");
        }

        private void buttonWalkingWhileWatchingPhone_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingWhileWatchingPhone");
        }

        private void buttonSittingTogether_Click(object sender, EventArgs e)
        {
            actionAssociate("sittingTogether");
        }

        private void buttonStandingTogether_Click(object sender, EventArgs e)
        {
            actionAssociate("standingTogether");
        }

        private void buttonWalkingTogether_Click(object sender, EventArgs e)
        {
            actionAssociate("walkingTogether");
        }        

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormAddAction popupAddAction = new FormAddAction(this);
            popupAddAction.ShowDialog(this);
            ToolStripItem subItem = new ToolStripMenuItem(newActionName);            
            userDefinedActionsToolStripMenuItem.DropDownItems.Add(subItem);
            subItem.Click += (s, ea) => newActionStripItem_Click(s, ea, newActionName);
            //subItem.Click += new EventHandler(newActionStripItem_Click);            
        }

        protected void newActionStripItem_Click(object sender, EventArgs e, string actionText)
        {            
            actionAssociate(actionText);
        }

        private void checkBoxShowPose_CheckedChanged(object sender, EventArgs e)
        {
        }
        
    }
}


