using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class CheckSudoku : MonoBehaviour
{
    public Text CheckText;
    public Text ErrorText;
    const int PUZZLE_SIZE = 9;
    const int NUMBER_OF_THREADS = 11;
    public GameObject Box;
    public GameObject[,] box = new GameObject[9,9];
    public InputField inputField;
    public Text InputText;
    public Text InputStatusText;
    private int e = 0, f = 0;

    private int[] status_map = new int[NUMBER_OF_THREADS];
    private int count = 0;
    private int rv;
    public int[,] Sudoku = {
    {5, 3, 4, 6, 7, 8, 9, 1, 2},
    {6, 7, 2, 1, 9, 5, 3, 4, 8},
    {1, 9, 8, 3, 4, 2, 5, 6, 7},
    {8, 5, 9, 7, 6, 1, 4, 2, 3},
    {4, 2, 6, 8, 5, 3, 7, 9, 1},
    {7, 1, 3, 9, 2, 4, 8, 5, 6},
    {9, 6, 1, 5, 3, 7, 2, 8, 4},
    {2, 8, 7, 4, 1, 9, 6, 3, 5},
    {3, 4, 5, 2, 8, 6, 1, 7, 9}
        };

    void Awake()
    {
        Screen.SetResolution(1080, 1920, true);
    }

    void Start()
    {
        CheckText.text = " ";
        ErrorText.text = " ";
        InputStatusText.text = " ";

        for (int a = 0; a < PUZZLE_SIZE; a++)
        {
            for (int b = 0; b < PUZZLE_SIZE; b++)
            {
                box[a, b] = (GameObject)Instantiate(Box, new Vector3((float)((79 * b) + 225), (float)((-76 * a) + 1395), 1), Quaternion.identity);
                box[a, b].transform.parent = GameObject.Find("Canvas").transform;
                box[a, b].GetComponent<Text>().text = Sudoku[a, b].ToString();
            }
        }
    }
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
                Application.Quit();
        }
    }

    // 행 숫자 중복 체크
    private void row_worker()
    {
        int i, j, k, l, m;
        bool status = true;
        int[] map = new int[PUZZLE_SIZE];

        for (m = 0; m < PUZZLE_SIZE; m++)
            map[m] = 0;

        for (i = 0; i < PUZZLE_SIZE; i++)
        {
            for (j = 0; j < PUZZLE_SIZE; j++)
                map[Sudoku[i, j] - 1] = 1;

            status = true;
            for (k = 0; k < PUZZLE_SIZE; k++)
            {
                if (map[k] == 0)
                {
                    status = false;
                    break;
                }
            }
            if (status == false)
                break;
            else
            {
                for (l = 0; l < PUZZLE_SIZE; l++)
                    map[l] = 0;
            }
            
        }
        if (status)
            status_map[0] = 1;
    }

    // 열 숫자 중복 체크
    private void column_worker()
    {
        int i, j, k, l, m;
        bool status = true;
        int[] map = new int[PUZZLE_SIZE];

        for (m = 0; m < PUZZLE_SIZE; m++)
            map[m] = 0;

        for (i = 0; i < PUZZLE_SIZE; i++)
        {
            for (j = 0; j < PUZZLE_SIZE; j++)
                map[Sudoku[j, i] - 1] = 1;

            status = true;
            for (k = 0; k < PUZZLE_SIZE; k++)
            {
                if (map[k] == 0)
                {
                    status = false;
                    break;
                }
            }
            if (status == false)
                break;
            else
            {
                for (l = 0; l < PUZZLE_SIZE; l++)
                    map[l] = 0;
            }
        }
        if (status)
            status_map[1] = 1;
    }

    // 각 box마다 숫자 중복 체크
    private void subfiled_worker(object obj)
    {
        int[] index = new int[3];
        index = (int[])obj;
        int i, j, k, l;
        bool status;
        int[] map = new int[PUZZLE_SIZE];

        for (l = 0; l < PUZZLE_SIZE; l++)
            map[l] = 0;

        for (i = index[1]; i < index[1] + (PUZZLE_SIZE / 3); i++)
        {
            for (j = index[2]; j < index[2] + (PUZZLE_SIZE / 3); j++)
            {
                map[Sudoku[i, j] - 1] = 1;
            }
        }
        status = true;
        for (k = 0; k < PUZZLE_SIZE; k++)
        {
            if (map[k] == 0)
            {
                status = false;
                break;
            }
        }
        if (status)
            status_map[index[0]] = 1;
    }

    public void Click()
    {
        for (int i = 0; i < NUMBER_OF_THREADS; i++)
            status_map[i] = 0;

        rv = 1;

        //쓰레드 생성
        Thread[] workers = new Thread[NUMBER_OF_THREADS];

        workers[0] = new Thread(new ThreadStart(row_worker));
        //쓰레드 실행
        workers[0].Start();

        workers[1] = new Thread(new ThreadStart(column_worker));
        workers[1].Start();
        count = 2;

        for (int i = 0; i < PUZZLE_SIZE; i = i + (PUZZLE_SIZE / 3))
        {
            for (int j = 0; j < PUZZLE_SIZE; j = j + (PUZZLE_SIZE / 3))
            {
                int[] index = new int[3];
                index[0] = count;
                index[1] = i;
                index[2] = j;
                workers[count] = new Thread(subfiled_worker);
                workers[count].Start(index);
                ++count;
            }
        }
        //다른 쓰레드 기다림
        for (int i = 0; i < count; i++)
            workers[i].Join();

        for (int i = 0; i < NUMBER_OF_THREADS; i++)
        {
            if (status_map[i] == 0)
                rv = 0;
        }
        if (rv == 1)
        {
            CheckText.text = "Result : Sudoku puzzle is valid!\n";
            ErrorText.text = " ";
        }
        else
        {
            CheckText.text = "Result : Sudoku puzzle is invalid!\n";
            ErrorText.text = "Invalid Reason\n----------------------------";
            if (status_map[0] == 0)
                ErrorText.text += "\nRow test is wrong!";
            if (status_map[1] == 0)
                ErrorText.text += "\nColumn test is wrong!";
            for (int i = 0; i < PUZZLE_SIZE; i++)
            {
                if (status_map[i + 2] == 0)
                    ErrorText.text += "\nSubBox N0." + (i + 1) + " is wrong!";
            }
        }
    }

    public void EnterClick()
    {
        int value;
        if (inputField.text == "1" ||
           inputField.text == "2" ||
           inputField.text == "3" ||
           inputField.text == "4" ||
           inputField.text == "5" ||
           inputField.text == "6" ||
           inputField.text == "7" ||
           inputField.text == "8" ||
           inputField.text == "9")
        {
            value = int.Parse(inputField.text);
            InputStatusText.text = "Set!";
            Sudoku[e, f] = value;
            box[e, f].GetComponent<Text>().text = Sudoku[e, f].ToString();

            f++;
            if (f == 9)
            {
                e++;
                if (e == 9)
                    e = 0;
                f = 0;
            }
            InputText.text = "[" + e + ", " + f + "] ?";
        }
        else
            InputStatusText.text = "Input Error";
    }

    public void Invalid()
    {
        int[,] invalid = {
            { 4, 3, 4, 6, 7, 8, 5, 1, 2},
            { 6, 7, 2, 1, 9, 5, 3, 4, 8},
            { 1, 9, 8, 3, 4, 2, 5, 6, 7},
            { 8, 5, 9, 7, 8, 1, 4, 2, 3},
            { 4, 2, 6, 8, 5, 3, 7, 9, 1},
            { 7, 1, 3, 9, 2, 4, 8, 5, 6},
            { 1, 6, 2, 5, 3, 7, 2, 8, 4},
            { 2, 8, 7, 4, 1, 9, 6, 3, 5},
            { 3, 4, 5, 2, 8, 6, 1, 1, 9}
        };
        for (int i = 0; i < PUZZLE_SIZE; i++)
        {
            for (int j = 0; j < PUZZLE_SIZE; j++)
            {
                Sudoku[i, j] = invalid[i, j];
                box[i, j].GetComponent<Text>().text = Sudoku[i, j].ToString();
            }
        }
    }

    public void Valid()
    {
        int[,] valid = {
            { 5, 3, 4, 6, 7, 8, 9, 1, 2},
    { 6, 7, 2, 1, 9, 5, 3, 4, 8},
    { 1, 9, 8, 3, 4, 2, 5, 6, 7},
    { 8, 5, 9, 7, 6, 1, 4, 2, 3},
    { 4, 2, 6, 8, 5, 3, 7, 9, 1},
    { 7, 1, 3, 9, 2, 4, 8, 5, 6},
    { 9, 6, 1, 5, 3, 7, 2, 8, 4},
    { 2, 8, 7, 4, 1, 9, 6, 3, 5},
    { 3, 4, 5, 2, 8, 6, 1, 7, 9}
        };
        for (int i = 0; i < PUZZLE_SIZE; i++)
        {
            for (int j = 0; j < PUZZLE_SIZE; j++)
            {
                Sudoku[i, j] = valid[i, j];
                box[i, j].GetComponent<Text>().text = Sudoku[i, j].ToString();
            }
        }
    }
}