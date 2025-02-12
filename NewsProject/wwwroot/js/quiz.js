$(document).ready(function () {
    $("#next-btn").hide();
    $("#submit-answer").show();
    $("#result-message").text("");

    $("#submit-answer").click(function () {
        var selectedAnswer = $("input[name='answer']:checked").val();
        if (!selectedAnswer) {
            $("#result-message").text("Please select an answer.");
            return;
        }

        var correctAnswer = $("#quiz-form").data("correct-answer");

        if (selectedAnswer.trim() === correctAnswer.trim()) {
            $("#result-message").text("Correct! Well done.").css("color", "green");
            $("#submit-answer").hide();
            $("#next-btn").show();
        } else {
            $("#result-message").text("Wrong! Try again.").css("color", "red");
            $("#submit-answer").show();
            $("#next-btn").hide();
        }
    });

    $("#next-btn").click(function () {
        $.ajax({
            url: '/Quiz/GetNextQuestion', // Use the relative URL for the endpoint
            method: 'GET',
            success: function (data) {
                if (data.completed) {
                    $("#complete").html(`
                        <h1 style="font-size: 2.5rem; color: green; text-align: center;">${data.message}</h1>
                        <a href="${data.retryQuizLink}" class="btn btn-primary" style="display: block; margin: 20px auto; text-align: center;">
                        Take Another Quiz
                       </a>
                      `);
                } else {
                    $("#question-number").text(`Question ${data.currentQuestionNumber} of ${data.totalQuestions}`);
                    $("#question-text").text(data.question);
                    $("label[for='option1']").html(`<input id="option1" type="radio" name="answer" value="${data.option1}" /> ${data.option1}`);
                    $("label[for='option2']").html(`<input id="option2" type="radio" name="answer" value="${data.option2}" /> ${data.option2}`);
                    $("label[for='option3']").html(`<input id="option3" type="radio" name="answer" value="${data.option3}" /> ${data.option3}`);
                    $(".quiz-image").attr("src", data.imageLink);
                    $("input[name='answer']").prop("checked", false);
                    $("#result-message").text("");
                    $("#submit-answer").show();
                    $("#next-btn").hide();
                    $("#quiz-form").data("correct-answer", data.correctAnswer);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading next question:', error);
                $("#result-message").text("An error occurred while fetching the next question.").css("color", "red");
            }
        });
    });
});

