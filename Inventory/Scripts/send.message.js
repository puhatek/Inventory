

function showprogressbar() {
	$("#progressBar").toggle();
}

function hideprogressbar(result) {

    if (result.SendRespons === 'Sent') {
        $("#myTextArea").animate({
            marginLeft: "+=100px",
            opacity: 0
        }, 1000, function () {
            $("#progressBar").html('<p>Wysłano</p>').delay(4000).slideToggle();

            if (result.RedirectTo != null) {
                window.location.href = result.RedirectTo;
            }
        });
    }
    else {
        alert(result.SendRespons);
    }
}