using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay.Quiz.SortMode
{
    public class SortModeUI : BaseQuizModeUI
    {
        public class Info
        {
            public string[] AnswerTexts;
            public Action<Dictionary<int, DraggableAnswer>> OnSubmitAnswerOrder;
        }

        [SerializeField]
        private float answerMoveSpeed = 15;

        [SerializeField]
        private Button submitButton;

        [SerializeField]
        private DraggableAnswer[] answers;

        [SerializeField]
        private Transform[] answerPositions;

        private Dictionary<int, DraggableAnswer> answerByOrder;

        public void Init(Info information)
        {
            for (int i = 0; i < answers.Length; i++)
            {
                var answer = answers[i];
                answer.SetText(information.AnswerTexts[i]);
                answer.SetAnswerID(i);
                answer.SetOrder(answer.AnswerID);
            }

            answerByOrder = new Dictionary<int, DraggableAnswer>(answers.Length);
            foreach (var answer in answers)
            {
                answerByOrder.Add(answer.CurrentOrder, answer);
            }

            submitButton.onClick.AddListener(() =>
            {
                information.OnSubmitAnswerOrder(answerByOrder);
                submitButton.image.sprite = submitButton.spriteState.pressedSprite;
            });
        }

        void Update()
        {
            for (int i = 0; i < answers.Length; i++)
            {
                var answer = answers[i];

                if (answer.IsDragging)
                {
                    var oldIndex = answer.CurrentOrder;
                    var closestIndex = oldIndex;
                    var closestDistance = Vector2.Distance(answerPositions[oldIndex].position, answer.transform.position);

                    for (int x = 0; x < answerPositions.Length; x++)
                    {
                        var dist = Vector2.Distance(answerPositions[x].position, answer.transform.position);
                        if (dist < closestDistance)
                        {
                            closestIndex = x;
                            closestDistance = dist;
                        }
                    }

                    if (oldIndex == closestIndex)
                        continue;

                    if (closestIndex < oldIndex)
                    {
                        for (int x = oldIndex; x > closestIndex; x--)
                        {
                            answerByOrder[x] = answerByOrder[x - 1];
                            answerByOrder[x].SetOrder(x);
                        }
                    }
                    else
                    {
                        for (int x = oldIndex; x < closestIndex; x++)
                        {
                            answerByOrder[x] = answerByOrder[x + 1];
                            answerByOrder[x].SetOrder(x);
                        }
                    }
                    answerByOrder[closestIndex] = answer;
                    answer.SetOrder(closestIndex);
                }
                else
                {
                    var holderOwner = answerPositions[answer.CurrentOrder];
                    if (Vector2.Distance(holderOwner.position, answer.transform.position) > 0.01f)
                    {
                        var newPosition = Vector3.Lerp(answer.transform.position, holderOwner.position, answerMoveSpeed * Time.deltaTime);
                        answer.transform.position = newPosition;
                    }
                    else
                    {
                        answer.transform.position = holderOwner.position;
                    }
                }
            }
        }

        public override void SetInteractable(bool isEnable)
        {
            foreach (var answer in answers)
            {
                answer.SetDragEnable(isEnable);
            }
            submitButton.interactable = isEnable;
        }
    }
}
