using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizCard : MonoBehaviour
{
    [SerializeField]
    private string questionJsonFileName;
    [SerializeField]
    private TMP_Text questionDisplay;
    [SerializeField]
    private List<Button> answerOptionButtons;
    [SerializeField]
    private Sprite defaultButtonSprite;
    [SerializeField]
    private Sprite selectedAnswerSprite;
    [SerializeField]
    private Sprite correctAnswerSprite;
    [SerializeField]
    private Sprite wrongAnswerSprite;
    [SerializeField]
    private GameObject questionPanel;
    [SerializeField]
    private GameObject answerDescriptionPanel;
    [SerializeField]
    private TMP_Text correctAnswerDescriptionText;


    private QuestionCollection questions;
    private int currentQuestion;
    private int correctAnswerButtonIndex;
    private AudioManager audioManager;
    private bool canDisplayQuestion;
    private bool canDisplayAnswerDescription;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        canDisplayQuestion = false;
        canDisplayAnswerDescription = false;
        gameObject.GetComponentInChildren<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        audioManager = FindObjectOfType<AudioManager>();
        currentQuestion = 0;
        if (questionJsonFileName != null)
        {
            Debug.Log("Reading from json");

            questions = JSON_Reader.ReadQuestionCollection(questionJsonFileName);
            Debug.Log("Length = " + questions.questions.Length);
        }
        else
        {
            Debug.LogError("Json file path is null");
        }
        DisplayQuestion();
    }

    // Update is called once per frame
    void Update()
    {
        if (canDisplayAnswerDescription)
        {
            Debug.Log("flip");
            FlipQuizCard();
        }

        if (canDisplayQuestion)
        {
            Debug.Log("Unflip");
            UnFlipQuizCard();
        }
    }

    private void DisplayQuestion()
    {
        questionPanel.SetActive(true);
        answerDescriptionPanel.SetActive(false);

        questionDisplay.text = questions.questions[currentQuestion].question;
        Debug.Log("question: " + questions.questions[currentQuestion].question);

        correctAnswerDescriptionText.text = questions.questions[currentQuestion].correctAnswerDescription;
        Debug.Log("correct answer description: " + questions.questions[currentQuestion].correctAnswerDescription);


        EnableAllOptionButtons();
        Debug.Log("enabled buttons");
        ResetColorOfAllOptionButtons();
        Debug.Log("reset color");

        AssignAnswerOptionsToButtons();
        Debug.Log("assigned answer options to buttons");


        currentQuestion++;
        if (currentQuestion == questions.questions.Length)
        {
            currentQuestion = 0;
        }
    }

    public void OnAnswerOptionButtonPressed(Button button)
    {
        Debug.Log("button pressed");
        button.image.sprite = selectedAnswerSprite;
        DisableAllOptionButtons();

        StartCoroutine(AnswerButtonPressedProcessing(button));
    }

    private IEnumerator AnswerButtonPressedProcessing(Button button)
    {
        audioManager.Play("Drum roll");
        Debug.Log("playing drum roll");
        yield return new WaitWhile(() => audioManager.IsPlaying("Drum roll"));
        Debug.Log("playing drum roll");
        RevealCorrectAnswer(button);

        yield return new WaitWhile(() => audioManager.IsPlaying("Win") || audioManager.IsPlaying("Lose"));
        Debug.Log("Finished win/lose");

        canDisplayQuestion = false;
        canDisplayAnswerDescription = true;
        yield return new WaitForSeconds(0.5f);
        ShowAnswerDescriptionPanel();
    }

    private void RevealCorrectAnswer(Button button)
    {
        bool isCorrectAnswer = false;

        for (int i = 0; i < answerOptionButtons.Capacity; i++)
        {
            if (answerOptionButtons[i] != button)
            {
                answerOptionButtons[i].enabled = false;
            }
            else
            {
                if (i == correctAnswerButtonIndex)
                {
                    isCorrectAnswer = true;
                }
            }

        }

        if (isCorrectAnswer)
        {
            button.image.sprite = correctAnswerSprite;
            audioManager.Play("Win");
        }
        else
        {
            button.image.sprite = wrongAnswerSprite;
            answerOptionButtons[correctAnswerButtonIndex].image.sprite = correctAnswerSprite;
            audioManager.Play("Lose");
        }


    }

    public void OnDescriptionPanelClick()
    {
        Debug.Log("clicked button");
        StartCoroutine(UnflipToQuestion());
    }

    private IEnumerator UnflipToQuestion()
    {
        Debug.Log("in method");
        canDisplayAnswerDescription = false;
        canDisplayQuestion = true;
        yield return new WaitForSeconds(0.5f);
        answerDescriptionPanel.SetActive(false);
        Debug.Log("finished waitiong");
        DisplayQuestion();
    }

    //flip to show answer description
    private void FlipQuizCard()
    {
        Quaternion rot1 = Quaternion.AngleAxis(180, Vector3.up);
        Quaternion rot2 = Quaternion.AngleAxis(-35, Vector3.right);
        Quaternion rot = rot1 * rot2;

        if (transform.localRotation.eulerAngles.y != 180)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rot, 0.05f);
        }
        else
        {
            transform.localRotation = rot;
            canDisplayAnswerDescription = false;
        }
    }

    //unflip to show question
    private void UnFlipQuizCard()
    {
        Quaternion rot1 = Quaternion.AngleAxis(0, Vector3.up);
        Quaternion rot2 = Quaternion.AngleAxis(35, Vector3.right);
        Quaternion rot = rot1 * rot2;

        if (transform.localRotation.eulerAngles.y > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rot, 0.05f);
        }
        else
        {
            transform.localRotation = rot;
            canDisplayQuestion = false;
        }
    }

    private void ShowAnswerDescriptionPanel()
    {
        questionPanel.SetActive(false);
        answerDescriptionPanel.SetActive(true);
    }


    private void EnableAllOptionButtons()
    {
        foreach (Button button in answerOptionButtons)
        {
            button.enabled = true;
        }
    }
    private void DisableAllOptionButtons()
    {
        foreach (Button button in answerOptionButtons)
        {
            button.enabled = false;
        }
    }

    private void ResetColorOfAllOptionButtons()
    {
        foreach (Button button in answerOptionButtons)
        {
            button.image.sprite = defaultButtonSprite;
        }
    }


    private void AssignAnswerOptionsToButtons()
    {

        Stack<string> wrongAnswers = new Stack<string>();
        wrongAnswers.Push(questions.questions[currentQuestion].wrongAnswer1);
        wrongAnswers.Push(questions.questions[currentQuestion].wrongAnswer2);
        wrongAnswers.Push(questions.questions[currentQuestion].wrongAnswer3);

        //assigning correct answer
        correctAnswerButtonIndex = Random.Range(0, 4);
        answerOptionButtons[correctAnswerButtonIndex].GetComponentInChildren<TMP_Text>().text = questions.questions[currentQuestion].correctAnswer;

        //assigning wrong answers
        for (int i = 0; i < answerOptionButtons.Count; i++)
        {
            if (i != correctAnswerButtonIndex)
            {
                answerOptionButtons[i].GetComponentInChildren<TMP_Text>().text = wrongAnswers.Pop();
            }

        }

    }

}
