using System.IO;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Wave.OpenXR;
using TMPro;
using Wave.Essence.Eye;


[RequireComponent(typeof(AudioSource))]


public class LogMessage
{
    public string message;
}

public class CircleControl : MonoBehaviour
{

    //偵測眼睛往左或往右之函數，以combineEyeDerection的x軸來觀察
    FocusEyeData eyeData = new FocusEyeData();
    private FocusEyeDataManager _eyeDataManager;
    private float Eye_X;

    public AudioClip impact;
    AudioSource audiosource;

    public AudioSource _correctBi;
    public AudioSource _errorBi;


    public GameObject _initialUI;
    public GameObject _introP1;
    public GameObject _introP2;
    public GameObject Crosshair;
    public GameObject _circle_center;
    public GameObject _circle_right;
    public GameObject _circle_left;
    public GameObject _startWords;
    public GameObject _endWords;
    public TMP_Text _breakWords;


    public RectTransform _rightTarget;
    public RectTransform _leftTarget;

    //
    public Color _green;
    public Color _red;

    public Material material1;
    public Material material2;

    int count = 1;

    //UI數值輸入
    public TMP_InputField _inputTrialNum;//題數
    public TMP_InputField _targetLocation_X;
    public TMP_InputField _targetLocation_Y;


    public GameObject _rightHandContr;
    public GameObject _leftHandContr;
    public GameObject _intereactionMan;



    //題數
    int numberOfTrial = 180;

    //題型
    int Q1 = 2;
    int Q2 = 2;
    int Q3 = 2;
    int Q4 = 2;
    int Qtype = 0;
    bool Roundoff = false;
    //Answer
    int ans = 0;

    bool _gameStart = false;
    bool _gameReady = false;
    bool waitingforinput = false;

    public DataManager _dataManager; 
    public LogMessage _logMessage = new LogMessage();


    public RawImage _background;
    private string CoverPath = Application.persistentDataPath + "Image/Background.png";



    bool isGazeMid = false;




    private void Start()
    {
        _introP1.SetActive(true);
        //LoadImageFromFile(CoverPath, _background);
        numberOfTrial = int.Parse(_inputTrialNum.text);
        //_background.gameObject.SetActive(true); 
        //_initialUI.SetActive(true);
        //VR controller appear
        _rightHandContr.SetActive(false);
        _leftHandContr.SetActive(false);
        _intereactionMan.SetActive(false);
        BluetoothManager.instance.ConnectToDevice_Send("connect");
        _gameReady = true;


    }

    private void Update()
    {

        if ((Input.anyKey || InputDeviceControl.KeyDown(InputDeviceControl.ControlDevice.Right, CommonUsages.triggerButton)) && !_gameStart)
        //if (Input.anyKey &&!_gameStart)
        {
            _introP1.SetActive(false);
            _introP2.SetActive(true);
            
            _gameStart = true;
            StartCoroutine(ShowAndHideUI());
            
        }
        
    }


    private void LoadImageFromFile(string filePath, RawImage image)
    {

        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                image.texture = texture;
                image.SetNativeSize(); // 可選，根據圖片的原始尺寸調整 RawImage 的大小
            }
            else
            {
                Debug.LogError("Failed to load texture from " + filePath);
            }
        }
        else
        {
            Debug.LogError("File does not exist at " + filePath);
        }
    }



    //public void GameStart()
    //{
    //    //Target position setting
    //    _rightTarget.anchoredPosition = new Vector3(int.Parse(_targetLocation_X.text),
    //        int.Parse(_targetLocation_Y.text), 0);
    //    _leftTarget.anchoredPosition = new Vector3(-int.Parse(_targetLocation_X.text),
    //        int.Parse(_targetLocation_Y.text), 0);
    //    numberOfTrial = int.Parse(_inputTrialNum.text);

    //    _initialUI.SetActive(false);
    //    _introP1.SetActive(true);
    //    _gameReady = true;

    //}


    private IEnumerator ShowAndHideUI()
    {
        //press right controller trigger to start

        //turn off introduction
        yield return StartCoroutine(WaitUntilInput());

        _introP2.SetActive(false);

        PlayerPrefs.SetInt("GetData", 1);
        BluetoothManager.instance.ConnectToDevice_Send("start");

        _logMessage.message = "practice start";
        _dataManager.SaveLogMessage(_logMessage);

        while (count <= (numberOfTrial + 8))
        {
            
            Roundoff = false;

            //first 8 round are traning
            if (count == 9)
            {
                //message for traning finished
                Q1 = Q2 = Q3 = Q4 = 5;
                _logMessage.message = "practice over";
                _dataManager.SaveLogMessage(_logMessage);
                _startWords.SetActive(true);
                yield return new WaitForSeconds(10);
                _startWords.SetActive(false);
                yield return new WaitForSeconds(2);
            }

            //output message: round detail
            BluetoothManager.instance.ConnectToDevice_Send($"start,Round {count - 8} start.");
            _logMessage.message = "round" + (count - 8).ToString() + " start";
            _dataManager.SaveLogMessage(_logMessage);
            
            Qtype = Random.Range(0, 4);

            while(!Roundoff)
            {
                switch (Qtype)
                {
                    case 0:
                        if (Q1 > 0)
                        {
                            Q1--;
                            //Centor color : Purple
                            ColorDecision(0);

                            //Crosshair
                            yield return StartCoroutine(CrosshairShow());

                            //target show
                            TargetDecition(0);

                            Roundoff = true;
                        }
                        else
                        {
                            Qtype = (Qtype + 1) % 4;
                        }
                        break;

                    case 1:
                        if (Q2 > 0)
                        {
                            Q2--;
                            //Centor color : Purple
                            ColorDecision(0);

                            //Crosshair
                            yield return StartCoroutine(CrosshairShow());

                            //target show
                            TargetDecition(1);

                            Roundoff = true;
                        }
                        else
                        {
                            Qtype = (Qtype + 1) % 4;
                        }
                        break;


                    case 2:
                        if (Q3 > 0)
                        {
                            Q3--;
                            //Centor color : Brown
                            ColorDecision(1);

                            //Crosshair
                            yield return StartCoroutine(CrosshairShow());

                            //target show
                            TargetDecition(0);

                            Roundoff = true;
                        }
                        else
                        {
                            Qtype = (Qtype + 1) % 4;
                        }
                        break;


                    case 3:
                        if (Q4 > 0)
                        {
                            Q4--;
                            //Centor color : Brown
                            ColorDecision(1);

                            //Crosshair
                            yield return StartCoroutine(CrosshairShow());

                            //target show : 
                            TargetDecition(1);

                            Roundoff = true;
                        }
                        else
                        {
                            Qtype = (Qtype + 1) % 4;
                        }
                        break;
                }
            }

            
            yield return new WaitForSeconds(1.0f);


            //偵測是否答對
            eyeData = _eyeDataManager.GetEyeData();
            Eye_X = eyeData.CombindedEyeDirectionNormalized.x;
            DetectWrong(ans, Eye_X);


            _circle_right.SetActive(false);
            _circle_left.SetActive(false);
            BluetoothManager.instance.ConnectToDevice_Send($"Round {count - 8} over.");
            _logMessage.message = "round" + (count - 8).ToString() + " over";
            _dataManager.SaveLogMessage(_logMessage);

            yield return new WaitForSeconds(1.0f);

            //next round
            count++;

            //quit
            if (count > numberOfTrial + 8)
            {
                _endWords.SetActive(true);
                PlayerPrefs.SetInt("GetData", 0);

                yield return new WaitForSeconds(2.0f);
                Application.Quit();

             }


            if (count > 9 && (count - 8 - 1) % 20 == 0)
            {
                //暫停收集
                PlayerPrefs.SetInt("GetData", 0);
                BluetoothManager.instance.ConnectToDevice_Send("stop");

                _logMessage.message = "break time";              
                _dataManager.SaveLogMessage(_logMessage);

                Q1 = Q2 = Q3 = Q4 = 5;

                yield return StartCoroutine(Take_A_Break());

                _logMessage.message = "restart";
                _dataManager.SaveLogMessage(_logMessage);
                //開始
                PlayerPrefs.SetInt("GetData", 1);
                BluetoothManager.instance.ConnectToDevice_Send("start");
            }

        }
        //}
        PlayerPrefs.SetInt("GetData", 0);
        BluetoothManager.instance.ConnectToDevice_Send("stop");
    }


    private void ColorDecision(int c)
    {
        if(c == 0)
        {
            _circle_center.GetComponent<RawImage>().color = _green;
            _logMessage.message = "indicator color: Green";
            _dataManager.SaveLogMessage(_logMessage);

        }
        else
        {
            _circle_center.GetComponent<RawImage>().color = _red;
            _logMessage.message = "indicator color: Red";
            _dataManager.SaveLogMessage(_logMessage);
        }

    }

    private void TargetDecition(int t)
    {
        if (t == 0)
        {
            _circle_right.SetActive(true);
            _logMessage.message = "target showing: right";
            _dataManager.SaveLogMessage(_logMessage);



            if (_circle_center.GetComponent<RawImage>().color == _green)
            {
                _logMessage.message = "round" + (count - 8).ToString() + " answer: right";
                ans = 1;
                _dataManager.SaveLogMessage(_logMessage);
            }
            else
            {
                _logMessage.message = "round" + (count - 8).ToString() + " answer: left";
                ans = 0;
                _dataManager.SaveLogMessage(_logMessage);
            }

        }
        else
        {
            _circle_left.SetActive(true);
            _logMessage.message = "target showing: left";
            _dataManager.SaveLogMessage(_logMessage);

            //output to console

            if (_circle_center.GetComponent<RawImage>().color == _green)
            {
                _logMessage.message = "round" + (count - 8).ToString() + " answer: left";
                ans = 0;
                _dataManager.SaveLogMessage(_logMessage);
            }
            else
            {
                _logMessage.message = "round" + (count - 8).ToString() + " answer: right";
                ans = 1;
                _dataManager.SaveLogMessage(_logMessage);
            }
        }
    }


    private IEnumerator CrosshairShow()
    {
        Crosshair.SetActive(true);
        _logMessage.message = "crosshair showing";
        _dataManager.SaveLogMessage(_logMessage);
        yield return new WaitForSeconds(0.5f);

        //確認視線回歸中心點
        yield return StartCoroutine(WaitGazeMid());

        Crosshair.SetActive(false);
        yield return new WaitForSeconds(0.2f);


        //center dot
        _circle_center.SetActive(true);
        _logMessage.message = "indicator showing";
        _dataManager.SaveLogMessage(_logMessage);
        yield return new WaitForSeconds(1.0f);

        _circle_center.SetActive(false);
        yield return new WaitForSeconds(0.2f);

    }


    //find the vector(0,0,1) of R/L sight
    private IEnumerator OriginFind()
    {
        while (count <= 10)
        {
            PlayerPrefs.SetInt("GetData", 1);

            //R_check
            _circle_right.SetActive(true);
            _logMessage.message = "R_test";
            _dataManager.SaveLogMessage(_logMessage);
            //when active, dot move to get the user's R_origin



            yield return new WaitForSeconds(5.0f);


            //L_check
            _circle_left.SetActive(true);
            _logMessage.message = "L_test";
            _dataManager.SaveLogMessage(_logMessage);
            //when active, dot move to get the user's L_origin




            yield return new WaitForSeconds(5.0f);
            

            PlayerPrefs.SetInt("GetData", 0);
            yield return null;
        }
    }

    private IEnumerator WaitGazeMid()
    {
        IsGazeMid();
        while (!isGazeMid)
        {
            yield return new WaitForSeconds(1f);
            IsGazeMid();
        }
        yield return null;
    }

    private void IsGazeMid()
    {
        FocusEyeData data = new FocusEyeData();
        EyeManager.Instance.GetLeftEyePupilPositionInSensorArea(out data.LeftEyePupilPositionInSensorArea);
        EyeManager.Instance.GetRightEyePupilPositionInSensorArea(out data.RightEyePupilPositionInSensorArea);
        if (data.LeftEyePupilPositionInSensorArea.x > 0.46 
            && data.LeftEyePupilPositionInSensorArea.x < 0.54
            && data.RightEyePupilPositionInSensorArea.x > 0.46
            && data.RightEyePupilPositionInSensorArea.x < 0.54)
        {
            isGazeMid = true;
        }
        else
        {
            isGazeMid = false;
        }
    }
    public void UpdateText(string newText)
    {
        // 更新文本
        _breakWords.text = newText;

    }

    private IEnumerator Take_A_Break()
    {

        _breakWords.text = "休息下\n已經完成" + (count - 8 - 1).ToString() + "(剩" + (numberOfTrial - count + 9).ToString() + "題)\n按鍵繼續";
        waitingforinput = true;


        //while (!Input.anyKey)
        while (!InputDeviceControl.KeyDown(InputDeviceControl.ControlDevice.Right, CommonUsages.triggerButton) || !Input.anyKey)
        {
            yield return null;
        }
        _breakWords.text = "";
        waitingforinput = false;

    }

    private IEnumerator WaitUntilInput()
    {
        yield return new WaitForSeconds(2.0f);
        while (!InputDeviceControl.KeyDown(InputDeviceControl.ControlDevice.Right, CommonUsages.triggerButton) || !Input.anyKey)
        //while (!Input.anyKey)
        {
            _breakWords.text = "";
            yield return null;
        }

    }


    //錯誤發出聲音
    void DetectWrong(int Ans, float EyeDerection)
    {
        if(Ans == 0 && EyeDerection >0)
        {
            audiosource.PlayOneShot(impact);
        }
        else if(Ans == 1 && EyeDerection < 0)
        {
            audiosource.PlayOneShot(impact);
        }
    }

}
