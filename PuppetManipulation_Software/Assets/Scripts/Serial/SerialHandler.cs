using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class SerialHandler : MonoBehaviour
{
    // ��M�C�x���g�p�̃f���Q�[�g��`
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    // �����������̊��ɍ��킹�ĕύX
    // ��: "COM3", "COM5", "/dev/ttyUSB0", etc.
    public string portName = "COM3";
    public int baudRate = 115200;

    private SerialPort serialPort_;
    private Thread readThread_;
    private bool isRunning_ = false;

    // ��M�f�[�^���ꎞ�ۑ�����o�b�t�@
    private string message_;
    private bool isNewMessageReceived_ = false;

    void Awake()
    {
        Open();
    }

    void Update()
    {
        // �V�������b�Z�[�W������Ȃ�A�C�x���g���Ăяo��
        if (isNewMessageReceived_)
        {
            OnDataReceived?.Invoke(message_);
            isNewMessageReceived_ = false;
        }
    }

    void OnDestroy()
    {
        Close();
    }

    /// <summary>
    /// �V���A���|�[�g���J������
    /// </summary>
    private void Open()
    {
        // ���p�\�ȃ|�[�g�����O�\�� (�f�o�b�O�p)
        foreach (string port in SerialPort.GetPortNames())
        {
            Debug.Log("[SerialHandler] Found port: " + port);
        }

        //Debug.Log("[SerialHandler] Open() called for port: " + portName);

        // �V���A���|�[�g�̐ݒ�
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);

        // Arduino Uno �� "\r\n" ������
        // Pico (MicroPython) �� "\n" ������������Ȃ��̂ŗv�m�F
        serialPort_.NewLine = "\r\n";

        // �^�C���A�E�g�ݒ� (�~���b)
        serialPort_.ReadTimeout = 1000;
        serialPort_.WriteTimeout = 500;

        // ��Pico�Ȃ�CDC�f�o�C�X��DTR���K�v�ȏꍇ����
        serialPort_.DtrEnable = true;

        try
        {
            // �|�[�g�I�[�v��
            serialPort_.Open();
            //Debug.Log("[SerialHandler] Port opened successfully: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SerialHandler] Port open error: " + e.Message);
            return;
        }

        // ��M�X���b�h�J�n
        isRunning_ = true;
        readThread_ = new Thread(Read);
        readThread_.Start();
    }

    /// <summary>
    /// �V���A���|�[�g����鏈��
    /// </summary>
    private void Close()
    {
        isRunning_ = false;

        if (readThread_ != null && readThread_.IsAlive)
        {
            readThread_.Join(); // �X���b�h�I���҂�
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    /// <summary>
    /// ��M�X���b�h�̎���
    /// </summary>
    private void Read()
    {
        Debug.Log("[SerialHandler] Read thread started");

        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                // 1�s����M (���s�R�[�h������܂őҋ@)
                string line = serialPort_.ReadLine();

                // ��M�f�[�^��ϐ��Ɋi�[��Update() �� OnDataReceived ���Ă�
                message_ = line;
                isNewMessageReceived_ = true;

                // �f�o�b�O�p���O
                Debug.Log("[SerialHandler] Received line: " + line);
            }
            catch (System.TimeoutException)
            {
                // �f�[�^���Ȃ��܂܃^�C���A�E�g������X���[
                // �i���ɏ������Ȃ���OK�j
            }
            catch (System.Exception ex)
            {
                // ���̑���O�������̓��O�\��
                Debug.LogWarning("[SerialHandler] Read error: " + ex.Message);
            }
        }

        Debug.Log("[SerialHandler] Read thread exited");
    }

    /// <summary>
    /// �V���A���|�[�g�ւ̏������݊֐�
    /// </summary>
    public void Write(string msg)
    {
        if (serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                serialPort_.Write(msg);
                Debug.Log("[SerialHandler] Wrote: " + msg);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[SerialHandler] Write error: " + ex.Message);
            }
        }
    }
}