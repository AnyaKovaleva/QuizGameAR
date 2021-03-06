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
            SuffleQuestions();
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
            FlipQuizCard();
        }

        if (canDisplayQuestion)
        {
            UnFlipQuizCard();
        }
    }

    private void DisplayQuestion()
    {
        questionPanel.SetActive(true);
        answerDescriptionPanel.SetActive(false);

        questionDisplay.text = questions.questions[currentQuestion].question;

        correctAnswerDescriptionText.text = questions.questions[currentQuestion].correctAnswerDescription;

        EnableAllOptionButtons();
        ResetColorOfAllOptionButtons();
        AssignAnswerOptionsToButtons();

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
        yield return new WaitWhile(() => audioManager.IsPlaying("Drum roll"));
        RevealCorrectAnswer(button);

        yield return new WaitWhile(() => audioManager.IsPlaying("Win") || audioManager.IsPlaying("Lose"));

        canDisplayQuestion = false;
        canDisplayAnswerDescription = true;
        yield return new WaitForSeconds(0.3f);


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
        canDisplayAnswerDescription = false;
        canDisplayQuestion = true;
        yield return new WaitForSeconds(0.3f);
        answerDescriptionPanel.SetActive(false);
        Debug.Log("finished waiting");
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

    private void SuffleQuestions()
    {
        if(questions != null)
        {
            Question tmp = new Question();
            int numOfQuestions = questions.questions.Length;
            for(int i=0; i < numOfQuestions; i++)
            {
                int questionNumber = Random.Range(i, questions.questions.Length);

                tmp = questions.questions[questionNumber];
                questions.questions[questionNumber] = questions.questions[i];
                questions.questions[i] = tmp;
            }

        }
    }

}
