management = (() => {
    function insertCell(row, data) {
        var newCell = row.insertCell(-1);
        var cellText = document.createTextNode(data);
        newCell.appendChild(cellText);
    }
    function showMatches(modalId, round) {
        var tournamentId = $('#tournament').data('id');
        var numberOfRounds = $('#tournament').data('number-of-rounds');

        if (round > numberOfRounds) {
            setCustomModalData(modalId, 'Matchday - Get round Failed', `The tournament have ${numberOfRounds} rounds`);
            return showCustomModal(modalId);
        }

        if (round < 1) {
            setCustomModalData(modalId, 'Matchday - Get round Failed', 'Round 1 is the first.');
            return showCustomModal(modalId);
        }

        var buttonPreviews = document.getElementById("button-previews");
        var buttonNext = document.getElementById("button-next");
        var spinnerNavigation = document.getElementById("navigation-spinner");

        if (buttonPreviews) buttonPreviews.disabled = true;
        if (buttonNext) buttonNext.disabled = true;
        if (spinnerNavigation) spinnerNavigation.style.display = "inline-block";

        $.ajax({
            url: "/admin/getmatchesbyround",
            type: "get",
            data: {
                TournamentId: tournamentId,
                Round: round
            },
            success: function (response) {
                $('#displayed-round').data('value', round);
                $("#displayed-round").text('Round: ' + round);

                var currentRoundValue = $('#current-round').data('value');
                var currentRoundElement = document.getElementById("current-round");
                if (currentRoundElement && round == currentRoundValue) {
                    currentRoundElement.style.display = "inline";
                } else {
                    currentRoundElement.style.display = "none";
                }

                var tableBody = document.getElementById("table-body");
                while (tableBody.hasChildNodes()) {
                    tableBody.removeChild(tableBody.firstChild);
                }

                response.response.forEach(match => {
                    var newRow = tableBody.insertRow(-1);

                    insertCell(newRow, match.localFutbolTeamName);
                    insertCell(newRow, match.localGoals);
                    insertCell(newRow, match.visitorFutbolTeamName);
                    insertCell(newRow, match.visitorGoals);
                });
            },
            error: response => {
                setCustomModalData(modalId, 'Matchday - Get round Failed', response.message || '')
                showCustomModal(modalId);
            },
            complete: () => {
                if (buttonPreviews) buttonPreviews.disabled = false;
                if (buttonNext) buttonNext.disabled = false;
                if (spinnerNavigation) spinnerNavigation.style.display = "none";
            }
        });
    }
    return {
        cleanEntity: (modalId, entity) => {
            var buttonClean = document.getElementById("button-clean");
            if (buttonClean) buttonClean.disabled = true;

            var spinnerGroup = document.getElementById(`${entity}-spinner-group`);
            if (spinnerGroup) spinnerGroup.style.display = "inline-block";

            $.ajax({
                url: '/admin/clean' + entity + 's',
                type: 'POST',
                success: response => {
                    setCustomModalData(modalId, 'Tournament ' + entity + 's - Success', response.message);
                    showCustomModal(modalId);
                },
                error: response => {
                    setCustomModalData(modalId, 'Tournament ' + entity + 's - Cleaning Failed', response.message || '')
                    showCustomModal(modalId);
                },
                complete: () => {
                    if (buttonClean) buttonClean.disabled = false;
                    if (spinnerGroup) spinnerGroup.style.display = "none";
                }
            });
        },
        uploadEntity: (modalId, entity) => {
            var formData = new FormData();
            var fileInput = document.getElementById(`${entity}File`);
            if ('files' in fileInput) {
                if (fileInput.files.length > 0) {
                    formData.append('file', fileInput.files[0]);

                    fileInput.disabled = true;
                    var spinnerGroup = document.getElementById(`${entity}-spinner-group`);
                    if (spinnerGroup) spinnerGroup.style.display = "inline-block";

                    $.ajax({
                        url: '/admin/upload' + entity + 's',
                        type: 'POST',
                        data: formData,
                        processData: false,  // tell jQuery not to process the data
                        contentType: false,  // tell jQuery not to set contentType
                        success: response => {
                            setCustomModalData(modalId, 'Tournament ' + entity + 's - Success', response.message);
                            showCustomModal(modalId);
                        },
                        error: response => {
                            setCustomModalData(modalId, 'Tournament ' + entity + 's - Uploading Failed', response.message || '')
                            showCustomModal(modalId);
                        },
                        complete: () => {
                            fileInput.disabled = false;
                            if (spinnerGroup) spinnerGroup.style.display = "none";

                            var $el = $(`#${entity}File`);
                            $el.wrap('<form>').closest(
                                'form').get(0).reset();
                            $el.unwrap(); 
                        }
                    });
                }
            }
        },
        showPreviewsRound: (modalId) => {
            var previousRound = $('#displayed-round').data('value') - 1;
            showMatches(modalId, previousRound);
        },
        showNextRound: (modalId) => {
            var nextRound = $('#displayed-round').data('value') + 1;
            showMatches(modalId, nextRound);
        }        
    };
})();
