simulation = (() => {
    const btnSimulateId = "btn-simulate";
    const sltRoundId = "slt-round";
    const modalId = "myCustomModal";

    return {
        simulateRound: () => {
            document.getElementById(btnSimulateId).classList.add('disabled');
            let select = document.getElementById(sltRoundId);
            select.setAttribute("disabled", "");
            let round = parseInt(select.value);
            let tournamentId = select.dataset.tournamentId;

            let myHeaders = new Headers();
            myHeaders.append("Content-Type", "application/json");

            let raw = JSON.stringify(
                {
                    "TournamentId": tournamentId,
                    "Round": round
                }
            );

            let requestOptions = {
                method: 'POST',
                headers: myHeaders,
                body: raw
            };

            setCustomModalData(modalId, `Simulation - Simulating round ${round}`, `The round ${round} is being simulated`);
            showCustomModal(modalId);

            fetch("/admin/simulateRound", requestOptions)
                //.then(response => JSON.parse(response.text()))
                .then(response => response.json())
                .then(result => {
                    console.log(result);
                    let title = "";
                    let body = "";
                    switch (result.statusCode) {
                        // todo: implement rest of possible StatusCode cases
                        case 200:
                            title = "Simulation - Round simulated successfully";
                            select.dataset.currentRound = result.response.currentRound.toString()
                            break;
                        case 400:
                        case 404:
                        case 500:
                            title = "Simulation - Round simulation failed";
                            break;
                        default:
                            break;
                    }

                    if (result.message) {
                        body += result.message;
                    }

                    select.removeAttribute("disabled");
                    setCustomModalData(modalId, title, body);
                    showCustomModal(modalId);
                })
                .catch(error => {
                    document.getElementById(sltRoundId).removeAttribute("disabled");
                    setCustomModalData(modalId, 'Simulation - Round simulation failed', error || '');
                    showCustomModal(modalId);
                });
        },
        onChangeRound: (event) => {
            let target = event.target;
            let selectedRound = parseInt(target.value);
            let currentRound = parseInt(target.dataset.currentRound);

            if (selectedRound < currentRound) {
                document.getElementById(btnSimulateId).classList.add('disabled');
                
            } else {
                document.getElementById(btnSimulateId).classList.remove('disabled');
            }

            // TODO: Trigger the request to get the list of matches, disabling the dropdown until the backend response to avoid a request overlapping
        }
    }

})();
